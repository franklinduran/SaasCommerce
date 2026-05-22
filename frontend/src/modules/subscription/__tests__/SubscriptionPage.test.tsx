import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
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
