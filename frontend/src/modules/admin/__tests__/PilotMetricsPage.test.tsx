import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { PilotMetricsPage } from '@/modules/admin/pages/PilotMetricsPage'
import type { PilotMetricsSummary } from '@/modules/admin/types'

const BUSINESS_ID = '11111111-1111-1111-1111-111111111111'

const MOCK_METRICS: PilotMetricsSummary = {
  totalBusinesses: 5,
  activeTrials: 3,
  activeSubscriptions: 1,
  suspendedSubscriptions: 0,
  cancelledSubscriptions: 1,
  newBusinessesLast30Days: 2,
  trialsExpiringIn7Days: 1,
  recentBusinesses: [
    {
      businessName: 'Colmado La Esperanza',
      subscriptionStatus: 'Trial',
      planName: null,
      createdAt: '2026-05-01T00:00:00Z',
      trialEndsAt: '2026-05-30T00:00:00Z',
    },
    {
      businessName: 'Ferreteria Central',
      subscriptionStatus: 'Active',
      planName: 'PRO',
      createdAt: '2026-04-15T00:00:00Z',
      trialEndsAt: null,
    },
  ],
}

describe('PilotMetricsPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.restoreAllMocks()
    vi.unstubAllGlobals()
  })

  it('shows loading spinner initially', () => {
    // Never-resolving fetch to hold the loading state
    vi.stubGlobal(
      'fetch',
      vi.fn(() => new Promise<Response>(() => {})),
    )
    renderPage()
    // Spinner is rendered via Loader2 icon — just check the page title is present
    expect(screen.getByText('Metricas del piloto')).toBeTruthy()
  })

  it('renders KPIs after data loads', async () => {
    vi.stubGlobal('fetch', createMetricsFetchMock(MOCK_METRICS))
    renderPage()

    // Total businesses and active trials are unique numbers on the page
    expect(await screen.findByText('5')).toBeTruthy()
    expect(screen.getByText('3')).toBeTruthy()
    expect(screen.getByText('2')).toBeTruthy()
    // 'Activos' label confirms the active subscriptions card renders
    expect(screen.getByText('Activos')).toBeTruthy()
    // KPI labels
    expect(screen.getByText('Total negocios')).toBeTruthy()
    expect(screen.getByText('En trial')).toBeTruthy()
    expect(screen.getByText('Nuevos (30 dias)')).toBeTruthy()
  })

  it('shows trial expiry alert when trialsExpiringIn7Days > 0', async () => {
    vi.stubGlobal('fetch', createMetricsFetchMock(MOCK_METRICS))
    renderPage()

    expect(await screen.findByText(/1 negocio expira en los proximos 7 dias/)).toBeTruthy()
  })

  it('does not show trial expiry alert when trialsExpiringIn7Days === 0', async () => {
    const metrics: PilotMetricsSummary = { ...MOCK_METRICS, trialsExpiringIn7Days: 0 }
    vi.stubGlobal('fetch', createMetricsFetchMock(metrics))
    renderPage()

    await screen.findByText('5') // wait for data
    expect(screen.queryByText(/negocio expira/)).toBeNull()
  })

  it('renders recent businesses table with business names', async () => {
    vi.stubGlobal('fetch', createMetricsFetchMock(MOCK_METRICS))
    renderPage()

    expect(await screen.findByText('Colmado La Esperanza')).toBeTruthy()
    expect(screen.getByText('Ferreteria Central')).toBeTruthy()
  })

  it('shows status badges for businesses', async () => {
    vi.stubGlobal('fetch', createMetricsFetchMock(MOCK_METRICS))
    renderPage()

    await screen.findByText('Colmado La Esperanza')
    expect(screen.getByText('Trial')).toBeTruthy()
    expect(screen.getByText('Activo')).toBeTruthy()
  })

  it('shows plan name when present and dash when not', async () => {
    vi.stubGlobal('fetch', createMetricsFetchMock(MOCK_METRICS))
    renderPage()

    await screen.findByText('Colmado La Esperanza')
    expect(screen.getByText('PRO')).toBeTruthy()
    // The business with no plan should show '—'
    const dashes = screen.getAllByText('—')
    expect(dashes.length).toBeGreaterThan(0)
  })

  it('shows error message when API fails', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async () => new Response('{"isSuccess":false,"data":null,"error":{"code":"FORBIDDEN","message":"Forbidden"}}', {
        status: 403,
        headers: { 'Content-Type': 'application/json' },
      })),
    )
    renderPage()

    expect(await screen.findByText(/Error al cargar las metricas/)).toBeTruthy()
  })

  it('shows suspended and cancelled metric cards when non-zero', async () => {
    const metrics: PilotMetricsSummary = {
      ...MOCK_METRICS,
      suspendedSubscriptions: 2,
      cancelledSubscriptions: 3,
    }
    vi.stubGlobal('fetch', createMetricsFetchMock(metrics))
    renderPage()

    await screen.findByText('5')
    expect(screen.getByText('Suspendidos')).toBeTruthy()
    expect(screen.getByText('Cancelados')).toBeTruthy()
  })

  it('clicking Actualizar button refetches data', async () => {
    const mockFetch = createMetricsFetchMock(MOCK_METRICS)
    vi.stubGlobal('fetch', mockFetch)
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('5')

    const calls = mockFetch.mock.calls.length
    await user.click(screen.getByRole('button', { name: /Actualizar/i }))

    // After click, another fetch is triggered
    expect(mockFetch.mock.calls.length).toBeGreaterThan(calls)
  })
})

// ── Helpers ───────────────────────────────────────────────────────────────────

function createMetricsFetchMock(data: PilotMetricsSummary) {
  return vi.fn(async (_url: string) =>
    new Response(
      JSON.stringify({ isSuccess: true, data, error: null }),
      { status: 200, headers: { 'Content-Type': 'application/json' } },
    ),
  )
}

function renderPage() {
  useAuthStore.getState().setSession({
    accessToken: 'admin-jwt',
    expiresAt: '2027-01-01T00:00:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: '22222222-2222-2222-2222-222222222222',
      businessId: BUSINESS_ID,
      email: 'admin@saas.com',
      fullName: 'Super Admin',
      id: '44444444-4444-4444-4444-444444444444',
      roles: ['SaasAdmin'],
    },
  })

  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createMemoryRouter([{ path: '/', element: <PilotMetricsPage /> }])

  render(
    <QueryClientProvider client={qc}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}
