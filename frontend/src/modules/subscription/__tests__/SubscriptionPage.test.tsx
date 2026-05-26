import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { SubscriptionPage } from '@/modules/subscription/pages/SubscriptionPage'
import type {
  BusinessSubscriptionResponse,
  SubscriptionPlanResponse,
  SubscriptionUsageResponse,
} from '@/modules/subscription/types'

const businessId = '11111111-1111-1111-1111-111111111111'
const subscriptionId = '66666666-6666-6666-6666-666666666666'

describe('SubscriptionPage', () => {
  beforeEach(() => {
    vi.setSystemTime(new Date('2026-05-22T12:00:00Z'))
    useAuthStore.getState().setSession({
      accessToken: 'jwt',
      expiresAt: '2026-05-22T23:59:00Z',
      refreshToken: 'refresh',
      user: {
        branchId: '22222222-2222-2222-2222-222222222222',
        businessId,
        email: 'admin@test.com',
        fullName: 'Admin',
        id: '44444444-4444-4444-4444-444444444444',
        roles: ['Admin'],
      },
    })
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
    vi.useRealTimers()
  })

  it('renders the active subscription dashboard', async () => {
    vi.stubGlobal('fetch', createFetchMock())

    renderSubscriptionPage()

    expect((await screen.findAllByText('Plan actual')).length).toBeGreaterThan(0)
    expect(screen.getAllByText('Pro').length).toBeGreaterThan(0)
    expect(screen.getByText('Operativo')).toBeTruthy()
    expect(screen.getByText('Uso contra limites')).toBeTruthy()
    expect(screen.getByText('Comparativa de planes')).toBeTruthy()
  })

  it('shows a warning when trial ends soon', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      subscription: createSubscription({
        currentPeriodEnd: null,
        status: 'Trial',
        trialEndsAt: '2026-05-25T12:00:00Z',
      }),
      usage: createUsage({
        status: 'Trial',
        trialEndsAt: '2026-05-25T12:00:00Z',
      }),
    }))

    renderSubscriptionPage()

    expect(await screen.findByText('Trial por vencer')).toBeTruthy()
    expect(screen.getAllByText('Trial').length).toBeGreaterThan(0)
  })

  it('shows blocked state for expired subscriptions', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      subscription: createSubscription({
        currentPeriodEnd: '2026-05-20T12:00:00Z',
        status: 'Expired',
      }),
      usage: createUsage({ status: 'Expired' }),
    }))

    renderSubscriptionPage()

    expect(await screen.findByText('Suscripcion vencida')).toBeTruthy()
    expect(screen.getByText('Bloqueado')).toBeTruthy()
  })

  it('shows limit reached warning', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      usage: createUsage({
        products: { current: 2000, isAtLimit: true, maximum: 2000 },
      }),
    }))

    renderSubscriptionPage()

    expect((await screen.findAllByText('Limite alcanzado')).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/productos/i).length).toBeGreaterThan(0)
  })

  it('renders empty state when the business has no subscription', async () => {
    vi.stubGlobal('fetch', createFetchMock({ subscription: null, usage: null }))

    renderSubscriptionPage()

    expect(await screen.findByText('Sin suscripcion activa')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Iniciar trial de 14 dias/ })).toBeTruthy()
    expect(screen.getByText('Planes disponibles')).toBeTruthy()
  })

  it('clicking Refrescar in empty state triggers refetch', async () => {
    const user = userEvent.setup()
    const fetchMock = createFetchMock({ subscription: null, usage: null })
    vi.stubGlobal('fetch', fetchMock)

    renderSubscriptionPage()
    await screen.findByText('Sin suscripcion activa')

    const btn = screen.getByRole('button', { name: 'Refrescar' })
    const callCount = fetchMock.mock.calls.length
    expect(btn).toBeTruthy()
    await user.click(btn)
    await waitFor(() => {
      expect(fetchMock.mock.calls.length).toBeGreaterThan(callCount)
    })
  })

  it('clicking Refrescar in main state triggers refetch', async () => {
    const user = userEvent.setup()
    const fetchMock = createFetchMock()
    vi.stubGlobal('fetch', fetchMock)

    renderSubscriptionPage()
    // "Operativo" is unique to the main dashboard (Acceso comercial metric)
    await screen.findByText('Operativo')

    const btn = screen.getByRole('button', { name: 'Refrescar' })
    const callCount = fetchMock.mock.calls.length
    await user.click(btn)
    await waitFor(() => {
      expect(fetchMock.mock.calls.length).toBeGreaterThan(callCount)
    })
  })

  it('starts trial when Iniciar trial button is clicked', async () => {
    const user = userEvent.setup()
    let callCount = 0
    vi.stubGlobal('fetch', vi.fn(async (input: RequestInfo | URL) => {
      const url = input.toString()

      if (url.includes('/api/subscription/start-trial')) {
        callCount += 1
        return createJsonResponse(createSubscription({ status: 'Trial', trialEndsAt: '2026-06-05T12:00:00Z' }))
      }

      return createFetchMock({ subscription: callCount === 0 ? null : createSubscription({ status: 'Trial' }), usage: null })(input)
    }))

    renderSubscriptionPage()
    await screen.findByText('Sin suscripcion activa')

    await user.click(screen.getByRole('button', { name: /Iniciar trial de 14 dias/ }))

    await waitFor(() => {
      expect(callCount).toBe(1)
    })
  })

  it('changes plan when Cambiar button is clicked', async () => {
    const user = userEvent.setup()
    let changePlanCalled = false
    vi.stubGlobal('fetch', vi.fn(async (input: RequestInfo | URL) => {
      const url = input.toString()

      if (url.includes('/api/subscription/change-plan')) {
        changePlanCalled = true
        return createJsonResponse(createSubscription())
      }

      return createFetchMock()(input)
    }))

    renderSubscriptionPage()
    await screen.findByText('Operativo')

    // Click "Cambiar" on a non-current plan (Basico or Premium)
    const cambiarBtns = await screen.findAllByRole('button', { name: 'Cambiar' })
    expect(cambiarBtns.length).toBeGreaterThan(0)
    await user.click(cambiarBtns[0]!)

    await waitFor(() => {
      expect(changePlanCalled).toBe(true)
    })
    expect(await screen.findByText('Plan actualizado correctamente.')).toBeTruthy()
  })

  it('cancels subscription when Cancelar button is clicked and confirmed', async () => {
    const user = userEvent.setup()
    let cancelCalled = false
    vi.stubGlobal('confirm', vi.fn(() => true))
    vi.stubGlobal('fetch', vi.fn(async (input: RequestInfo | URL) => {
      const url = input.toString()

      if (url.includes('/api/subscription/cancel')) {
        cancelCalled = true
        return createJsonResponse(createSubscription({ status: 'Cancelled' }))
      }

      return createFetchMock()(input)
    }))

    renderSubscriptionPage()
    await screen.findByText('Operativo')

    await user.click(screen.getByRole('button', { name: 'Cancelar suscripcion' }))

    await waitFor(() => {
      expect(cancelCalled).toBe(true)
    })
    expect(await screen.findByText('Suscripcion cancelada correctamente.')).toBeTruthy()
  })

  it('does not cancel when window.confirm is rejected', async () => {
    const user = userEvent.setup()
    let cancelCalled = false
    vi.stubGlobal('confirm', vi.fn(() => false))
    vi.stubGlobal('fetch', vi.fn(async (input: RequestInfo | URL) => {
      const url = input.toString()

      if (url.includes('/api/subscription/cancel')) {
        cancelCalled = true
        return createJsonResponse(createSubscription({ status: 'Cancelled' }))
      }

      return createFetchMock()(input)
    }))

    renderSubscriptionPage()
    await screen.findByText('Operativo')

    await user.click(screen.getByRole('button', { name: 'Cancelar suscripcion' }))

    // Wait a tick to ensure the handler ran
    await waitFor(() => {
      expect(cancelCalled).toBe(false)
    })
  })

  it('reactivates subscription from Cancelled state', async () => {
    const user = userEvent.setup()
    let reactivateCalled = false
    vi.stubGlobal('fetch', vi.fn(async (input: RequestInfo | URL) => {
      const url = input.toString()

      if (url.includes('/api/subscription/reactivate')) {
        reactivateCalled = true
        return createJsonResponse(createSubscription({ status: 'Active' }))
      }

      return createFetchMock({ subscription: createSubscription({ status: 'Cancelled' }) })(input)
    }))

    renderSubscriptionPage()
    // Cancelled shows "Bloqueado" in the Acceso comercial metric
    await screen.findByText('Bloqueado')

    // In Cancelled state ActionsPanel shows 'Reactivar' button (may also appear in alert banner)
    const reactivarBtns = screen.getAllByRole('button', { name: 'Reactivar' })
    await user.click(reactivarBtns[0]!)

    await waitFor(() => {
      expect(reactivateCalled).toBe(true)
    })
    expect(await screen.findByText('Suscripcion reactivada correctamente.')).toBeTruthy()
  })

  it('shows Cuenta suspendida alert for Suspended subscription', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      subscription: createSubscription({ status: 'Suspended' }),
      usage: createUsage({ status: 'Suspended' }),
    }))

    renderSubscriptionPage()

    // Unique message text for Suspended
    expect(await screen.findByText(/Puedes consultar tus datos/)).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Soporte' })).toBeTruthy()
  })

  it('shows Suscripcion cancelada alert for Cancelled subscription', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      subscription: createSubscription({ status: 'Cancelled' }),
      usage: createUsage({ status: 'Cancelled' }),
    }))

    renderSubscriptionPage()

    // Unique message text for Cancelled
    expect(await screen.findByText(/La suscripcion esta cancelada/)).toBeTruthy()
    const reactivarBtns = screen.getAllByRole('button', { name: 'Reactivar' })
    expect(reactivarBtns.length).toBeGreaterThan(0)
  })

  it('shows Pago pendiente alert for PastDue subscription', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      subscription: createSubscription({ status: 'PastDue' }),
      usage: createUsage({ status: 'PastDue' }),
    }))

    renderSubscriptionPage()

    // Unique message text for PastDue
    expect(await screen.findByText(/Hay un pago pendiente/)).toBeTruthy()
  })

  it('shows Renovacion cercana when active period ends within 7 days', async () => {
    vi.setSystemTime(new Date('2026-05-22T12:00:00Z'))
    vi.stubGlobal('fetch', createFetchMock({
      subscription: createSubscription({
        currentPeriodEnd: '2026-05-26T12:00:00Z', // 4 days from now
        status: 'Active',
      }),
    }))

    renderSubscriptionPage()

    // Unique message text for period-near-end
    expect(await screen.findByText(/El periodo actual vence en/)).toBeTruthy()
  })

  it('shows Limite alcanzado for sales at limit', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      usage: createUsage({
        sales: { current: 10000, isAtLimit: true, maximum: 10000 },
      }),
    }))

    renderSubscriptionPage()

    expect(await screen.findByText(/Alcanzaste el limite de ventas mensuales/)).toBeTruthy()
  })

  it('shows Limite alcanzado for users at limit', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      usage: createUsage({
        users: { current: 10, isAtLimit: true, maximum: 10 },
      }),
    }))

    renderSubscriptionPage()

    expect(await screen.findByText(/Alcanzaste el limite de usuarios/)).toBeTruthy()
  })

  it('shows Limite alcanzado for branches at limit', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      usage: createUsage({
        branches: { current: 3, isAtLimit: true, maximum: 3 },
      }),
    }))

    renderSubscriptionPage()

    expect(await screen.findByText(/Alcanzaste el limite de sucursales/)).toBeTruthy()
  })

  it('shows UpgradeBanner when a resource is near the limit (>=80%)', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      usage: createUsage({
        products: { current: 1700, isAtLimit: false, maximum: 2000 }, // 85%
      }),
    }))

    renderSubscriptionPage()

    expect(await screen.findByText('Uso cercano al limite')).toBeTruthy()
    // Shows single resource label
    expect(screen.getByText(/productos/)).toBeTruthy()
  })

  it('shows UpgradeBanner with two near-limit resources (covers formatList 2-item branch)', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      usage: createUsage({
        products: { current: 1700, isAtLimit: false, maximum: 2000 }, // 85%
        users: { current: 9, isAtLimit: false, maximum: 10 }, // 90%
      }),
    }))

    renderSubscriptionPage()

    expect(await screen.findByText('Uso cercano al limite')).toBeTruthy()
    // formatList with 2 items: "productos y usuarios"
    expect(screen.getByText(/productos y usuarios/)).toBeTruthy()
  })

  it('clicking Ver planes in trial-expiring banner scrolls to plans', async () => {
    const user = userEvent.setup()
    const scrollIntoView = vi.fn()
    Object.defineProperty(HTMLElement.prototype, 'scrollIntoView', {
      configurable: true,
      value: scrollIntoView,
    })
    vi.setSystemTime(new Date('2026-05-22T12:00:00Z'))
    vi.stubGlobal('fetch', createFetchMock({
      subscription: createSubscription({
        currentPeriodEnd: null,
        status: 'Trial',
        trialEndsAt: '2026-05-25T12:00:00Z', // 3 days left
      }),
      usage: createUsage({ status: 'Trial', trialEndsAt: '2026-05-25T12:00:00Z' }),
    }))

    renderSubscriptionPage()
    await screen.findByText('Trial por vencer')

    const verPlanes = screen.getByRole('button', { name: 'Ver planes' })
    await user.click(verPlanes)
    expect(scrollIntoView).toHaveBeenCalled()
  })

  it('clicking Contactar soporte triggers contactSupport callback', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('location', { href: '' })
    vi.stubGlobal('fetch', createFetchMock())

    renderSubscriptionPage()
    await screen.findByText('Operativo')

    await user.click(screen.getByRole('button', { name: 'Contactar soporte' }))
    // contactSupport sets window.location.href — covers the callback line
    expect((window.location as { href: string }).href).toContain('mailto:')
  })
})

function renderSubscriptionPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <SubscriptionPage />
    </QueryClientProvider>,
  )
}

function createFetchMock(overrides: {
  plans?: SubscriptionPlanResponse[]
  subscription?: BusinessSubscriptionResponse | null
  usage?: SubscriptionUsageResponse | null
} = {}) {
  const plans = overrides.plans ?? createPlans()
  const subscription = overrides.subscription === undefined ? createSubscription() : overrides.subscription
  const usage = overrides.usage === undefined ? createUsage() : overrides.usage

  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/subscription/current')) {
      if (!subscription) {
        return createJsonResponse(null, false, 404, 'subscription.not_found', 'No hay suscripcion activa.')
      }

      return createJsonResponse(subscription)
    }

    if (url.includes('/api/subscription/usage')) {
      if (!usage) {
        return createJsonResponse(null, false, 404, 'subscription.usage_not_found', 'No hay uso disponible.')
      }

      return createJsonResponse(usage)
    }

    if (url.includes('/api/subscription-plans')) {
      return createJsonResponse(plans)
    }

    return createJsonResponse(null, false, 404, 'not_found', 'Ruta no encontrada.')
  })
}

function createPlans(): SubscriptionPlanResponse[] {
  return [
    createPlan({
      code: 'basic',
      id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      maxBranches: 1,
      maxProducts: 300,
      maxSalesPerMonth: 1000,
      maxUsers: 2,
      monthlyPrice: 995,
      name: 'Basico',
    }),
    createPlan({
      code: 'pro',
      id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      maxBranches: 3,
      maxProducts: 2000,
      maxSalesPerMonth: 10000,
      maxUsers: 10,
      monthlyPrice: 2495,
      name: 'Pro',
    }),
    createPlan({
      code: 'premium',
      id: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
      maxBranches: 999,
      maxProducts: 999999,
      maxSalesPerMonth: 999999,
      maxUsers: 999,
      monthlyPrice: 4995,
      name: 'Premium',
    }),
  ]
}

function createPlan(overrides: Partial<SubscriptionPlanResponse> = {}): SubscriptionPlanResponse {
  return {
    code: 'pro',
    createdAt: '2026-05-01T12:00:00Z',
    description: 'Plan comercial para negocios en crecimiento.',
    features: ['Sales', 'Products', 'Branches', 'Users', 'InventoryTransfers', 'Reports'],
    id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    isActive: true,
    maxBranches: 3,
    maxProducts: 2000,
    maxSalesPerMonth: 10000,
    maxUsers: 10,
    monthlyPrice: 2495,
    name: 'Pro',
    updatedAt: '2026-05-01T12:00:00Z',
    ...overrides,
  }
}

function createSubscription(
  overrides: Partial<BusinessSubscriptionResponse> = {},
): BusinessSubscriptionResponse {
  return {
    businessId,
    cancelledAt: null,
    cancellationReason: null,
    createdAt: '2026-05-01T12:00:00Z',
    currentPeriodEnd: '2026-06-01T12:00:00Z',
    currentPeriodStart: '2026-05-01T12:00:00Z',
    id: subscriptionId,
    plan: createPlan(),
    startedAt: '2026-05-01T12:00:00Z',
    status: 'Active',
    suspendedAt: null,
    trialEndsAt: null,
    updatedAt: '2026-05-01T12:00:00Z',
    ...overrides,
  }
}

function createUsage(overrides: Partial<SubscriptionUsageResponse> = {}): SubscriptionUsageResponse {
  return {
    branches: { current: 1, isAtLimit: false, maximum: 3 },
    features: [
      { isEnabled: true, name: 'Sales' },
      { isEnabled: true, name: 'Products' },
      { isEnabled: true, name: 'InventoryTransfers' },
      { isEnabled: false, name: 'AuditLogs' },
    ],
    periodEndsAt: '2026-06-01T12:00:00Z',
    planName: 'Pro',
    products: { current: 120, isAtLimit: false, maximum: 2000 },
    sales: { current: 450, isAtLimit: false, maximum: 10000 },
    status: 'Active',
    subscriptionId,
    trialEndsAt: null,
    users: { current: 4, isAtLimit: false, maximum: 10 },
    ...overrides,
  }
}

function createJsonResponse(
  data: unknown,
  ok = true,
  status = 200,
  code = 'ERROR',
  message = 'Request failed',
) {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => ({
      correlationId: 'test',
      data,
      error: ok ? null : { code, message },
      isSuccess: ok,
    }),
    ok,
    status,
  }
}
