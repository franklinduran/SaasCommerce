import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { DailyCashRegisterSummaryPage } from '@/modules/cash-register/pages/DailyCashRegisterSummaryPage'
import type { DailyCashRegisterSummary, DailyCashRegisterSummaryItem } from '@/modules/cash-register/types'

// ── Constants ──────────────────────────────────────────────────────────────────

const REGISTER_ID = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
const BRANCH_ID = '22222222-2222-2222-2222-222222222222'
const BUSINESS_ID = '11111111-1111-1111-1111-111111111111'

// ── Describe: DailyCashRegisterSummaryPage ─────────────────────────────────────

describe('DailyCashRegisterSummaryPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders date and branch selectors', async () => {
    vi.stubGlobal('fetch', createFetchMock({ summary: createSummary() }))

    renderSummaryPage()

    const dateInput = await screen.findByLabelText('Fecha')
    expect(dateInput).toBeTruthy()
  })

  it('shows KPI cards when summary is loaded', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        summary: createSummary({ openRegisters: 2, closedRegisters: 3, totalExpectedCash: 8000 }),
      }),
    )

    renderSummaryPage()

    expect(await screen.findByText('Cajas abiertas')).toBeTruthy()
    expect(screen.getByText('Cajas cerradas')).toBeTruthy()
    expect(screen.getByText('Efectivo esperado')).toBeTruthy()
    expect(screen.getByText('Efectivo contado')).toBeTruthy()
  })

  it('shows register list when registers are present', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        summary: createSummary({ registers: [createRegisterItem({ status: 'Closed', differenceType: 'Balanced' })] }),
      }),
    )

    renderSummaryPage()

    expect(await screen.findByText(/Detalle por caja/i)).toBeTruthy()
    expect(screen.getByText('Cerrada')).toBeTruthy()
    expect(screen.getByText('Cuadrado')).toBeTruthy()
  })

  it('shows empty state when no registers for the day', async () => {
    vi.stubGlobal('fetch', createFetchMock({ summary: createSummary({ registers: [] }) }))

    renderSummaryPage()

    expect(await screen.findByText('No hay cajas registradas para esta fecha.')).toBeTruthy()
  })

  it('shows error state when API fails', async () => {
    vi.stubGlobal('fetch', createFetchMock({ failSummary: true }))

    renderSummaryPage()

    expect(await screen.findByText(/Error al cargar el arqueo/i)).toBeTruthy()
  })
})

// ── Render helpers ──────────────────────────────────────────────────────────────

function renderSummaryPage() {
  const router = createMemoryRouter(
    [{ element: <DailyCashRegisterSummaryPage />, path: '/cash-register/daily-summary' }],
    { initialEntries: ['/cash-register/daily-summary'] },
  )
  renderWithQueryClient(<RouterProvider router={router} />)
}

function renderWithQueryClient(element: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })
  return render(<QueryClientProvider client={queryClient}>{element}</QueryClientProvider>)
}

function setSession() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-12-31T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: BRANCH_ID,
      businessId: BUSINESS_ID,
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '44444444-4444-4444-4444-444444444444',
      roles: ['Admin'],
    },
  })
}

// ── Factory functions ───────────────────────────────────────────────────────────

function createSummary(overrides: Partial<DailyCashRegisterSummary> = {}): DailyCashRegisterSummary {
  return {
    date: '2026-05-26',
    openRegisters: 0,
    closedRegisters: 1,
    totalExpectedCash: 5000,
    totalCountedCash: 5000,
    totalDifference: 0,
    totalCashSales: 3000,
    totalCardSales: 1000,
    totalTransferSales: 500,
    totalCreditSales: 200,
    totalCashReturns: 0,
    registers: [createRegisterItem()],
    ...overrides,
  }
}

function createRegisterItem(
  overrides: Partial<DailyCashRegisterSummaryItem> = {},
): DailyCashRegisterSummaryItem {
  return {
    cashRegisterId: REGISTER_ID,
    branchId: BRANCH_ID,
    userId: '44444444-4444-4444-4444-444444444444',
    openingAmount: 1000,
    status: 'Closed',
    openedAt: '2026-05-26T08:00:00Z',
    closedAt: '2026-05-26T17:00:00Z',
    expectedCashAmount: 4000,
    countedAmount: 4000,
    difference: 0,
    differenceType: 'Balanced',
    cashSalesTotal: 3000,
    cardSalesTotal: 1000,
    transferSalesTotal: 500,
    creditSalesTotal: 200,
    cashReturnsTotal: 0,
    manualCashIn: 0,
    manualCashOut: 0,
    ...overrides,
  }
}

// ── Fetch mock ──────────────────────────────────────────────────────────────────

function createFetchMock({
  summary,
  failSummary = false,
}: {
  summary?: DailyCashRegisterSummary
  failSummary?: boolean
} = {}) {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/cash-registers/daily-summary')) {
      if (failSummary) {
        return createJsonResponse(null, false, 500, 'SERVER_ERROR', 'Failed.')
      }
      return createJsonResponse(summary ?? createSummary())
    }

    if (url.includes('/api/branches')) {
      return createJsonResponse({ items: [], total: 0 })
    }

    return createJsonResponse(null, false, 404, 'NOT_FOUND', 'Not found.')
  })
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
