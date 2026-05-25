import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { OnboardingPage } from '@/modules/onboarding/pages/OnboardingPage'
import type { OnboardingStatusResponse } from '@/modules/onboarding/types'

const businessId = '11111111-1111-1111-1111-111111111111'

describe('OnboardingPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders onboarding steps with progress bar', async () => {
    vi.stubGlobal('fetch', createOnboardingFetchMock(createIncompleteStatus()))
    renderOnboardingPage()

    expect(await screen.findByText('Configuración inicial')).toBeTruthy()
    expect(screen.getByText('Información del negocio')).toBeTruthy()
    expect(screen.getByText('Catálogo de productos')).toBeTruthy()
    expect(screen.getByText('Inventario inicial')).toBeTruthy()
    expect(screen.getByText('Primera sesión de caja')).toBeTruthy()
  })

  it('shows completed badge for finished steps', async () => {
    const status = createIncompleteStatus({
      businessInfoCompleted: true,
      completedCount: 1,
    })
    vi.stubGlobal('fetch', createOnboardingFetchMock(status))
    renderOnboardingPage()

    await screen.findByText('Configuración inicial')
    expect(screen.getByText('Completado')).toBeTruthy()
  })

  it('shows completion message when all steps are done', async () => {
    vi.stubGlobal('fetch', createOnboardingFetchMock(createCompleteStatus()))
    renderOnboardingPage()

    expect(
      await screen.findByText(/Tu negocio está listo para operar/),
    ).toBeTruthy()
  })

  it('shows CSV import shortcut when products not completed', async () => {
    vi.stubGlobal('fetch', createOnboardingFetchMock(createIncompleteStatus()))
    renderOnboardingPage()

    expect(await screen.findByText('¿Ya tienes una lista de productos?')).toBeTruthy()
  })

  it('does not show CSV import shortcut when products already completed', async () => {
    const status = createIncompleteStatus({
      productsCompleted: true,
      completedCount: 2,
    })
    vi.stubGlobal('fetch', createOnboardingFetchMock(status))
    renderOnboardingPage()

    await screen.findByText('Configuración inicial')

    expect(screen.queryByText('¿Ya tienes una lista de productos?')).toBeNull()
  })

  it('shows progress percentage', async () => {
    const status = createIncompleteStatus({
      completedCount: 2,
      businessInfoCompleted: true,
      productsCompleted: true,
    })
    vi.stubGlobal('fetch', createOnboardingFetchMock(status))
    renderOnboardingPage()

    expect(await screen.findByText('50%')).toBeTruthy()
  })
})

// ── Helpers ──────────────────────────────────────────────────────────────────

function createIncompleteStatus(overrides: Partial<OnboardingStatusResponse> = {}): OnboardingStatusResponse {
  return {
    businessInfoCompleted: false,
    cashSessionCompleted: false,
    completedCount: 0,
    inventoryCompleted: false,
    isComplete: false,
    productsCompleted: false,
    totalSteps: 4,
    ...overrides,
  }
}

function createCompleteStatus(): OnboardingStatusResponse {
  return {
    businessInfoCompleted: true,
    cashSessionCompleted: true,
    completedCount: 4,
    inventoryCompleted: true,
    isComplete: true,
    productsCompleted: true,
    totalSteps: 4,
  }
}

function createOnboardingFetchMock(status: OnboardingStatusResponse) {
  return vi.fn(async (url: string) => {
    if (String(url).includes('/api/onboarding/status')) {
      return new Response(
        JSON.stringify({ isSuccess: true, data: status, error: null }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      )
    }

    return new Response(JSON.stringify({ isSuccess: false, data: null, error: null }), {
      status: 404,
    })
  })
}

function renderOnboardingPage() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-24T23:59:00Z',
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

  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createMemoryRouter([{ path: '/', element: <OnboardingPage /> }])

  render(
    <QueryClientProvider client={qc}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}
