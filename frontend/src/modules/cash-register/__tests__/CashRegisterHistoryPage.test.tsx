import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { CashRegisterHistoryPage } from '@/modules/cash-register/pages/CashRegisterHistoryPage'
import type { CashRegisterHistoryResponse, DailyCashRegisterSummaryItem } from '@/modules/cash-register/types'

const REGISTER_ID = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
const BRANCH_ID = '22222222-2222-2222-2222-222222222222'

describe('CashRegisterHistoryPage', () => {
  beforeEach(() => {
    useAuthStore.getState().setSession({
      accessToken: 'token',
      expiresAt: '2026-05-27T12:00:00Z',
      refreshToken: 'refresh',
      user: {
        businessId: '11111111-1111-1111-1111-111111111111',
        email: 'cashier@example.com',
        fullName: 'Cashier',
        id: '44444444-4444-4444-4444-444444444444',
        roles: ['Cashier'],
      },
    })
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows closed cash registers from history endpoint', async () => {
    vi.stubGlobal('fetch', createFetchMock({ history: createHistory() }))

    renderHistoryPage()

    expect(await screen.findByText('Historial de arqueos')).toBeTruthy()
    expect(await screen.findByText('Cerrada')).toBeTruthy()
    expect(screen.getByText('Cuadrado')).toBeTruthy()
  })

  it('shows empty state when history has no registers', async () => {
    vi.stubGlobal('fetch', createFetchMock({ history: createHistory({ items: [] }) }))

    renderHistoryPage()

    expect(await screen.findByText('No hay cajas para los filtros seleccionados.')).toBeTruthy()
  })

  it('renders status, difference and nullable values for mixed registers', async () => {
    vi.stubGlobal('fetch', createFetchMock({
      history: createHistory({
        items: [
          createRegisterItem({ status: 'Open', closedAt: null, countedAmount: null, difference: null, differenceType: null }),
          createRegisterItem({ cashRegisterId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', difference: 125, differenceType: 'Surplus' }),
          createRegisterItem({ cashRegisterId: 'cccccccc-cccc-cccc-cccc-cccccccccccc', status: 'Cancelled', difference: -75, differenceType: 'Shortage' }),
        ],
        totalCount: 3,
      }),
    }))

    renderHistoryPage()

    expect(await screen.findByText('Abierta')).toBeTruthy()
    expect(screen.getByText('Cancelada')).toBeTruthy()
    expect(screen.getByText('Sobrante')).toBeTruthy()
    expect(screen.getByText('Faltante')).toBeTruthy()
    expect(screen.getAllByText('-').length).toBeGreaterThan(0)
  })

  it('sends date filters to history endpoint', async () => {
    const fetchMock = createFetchMock({ history: createHistory({ items: [] }) })
    vi.stubGlobal('fetch', fetchMock)

    renderHistoryPage()
    const from = await screen.findByLabelText('Desde')
    const to = screen.getByLabelText('Hasta')

    fireEvent.change(from, { target: { value: '2026-05-26T08:00' } })
    fireEvent.change(to, { target: { value: '2026-05-26T18:00' } })

    await waitFor(() => {
      const calls = fetchMock.mock.calls.map(([input]) => input.toString()).join('\n')
      expect(calls).toContain('dateFrom=2026-05-26T08%3A00')
      expect(calls).toContain('dateTo=2026-05-26T18%3A00')
    })
  })

  it('shows controlled error state when history request fails', async () => {
    vi.stubGlobal('fetch', createFetchMock({ fail: true }))

    renderHistoryPage()

    expect(await screen.findByText('No se pudo cargar el historial.')).toBeTruthy()
  })
})

function renderHistoryPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  const router = createMemoryRouter(
    [{ element: <CashRegisterHistoryPage />, path: '/cash-register/history' }],
    { initialEntries: ['/cash-register/history'] },
  )

  render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

function createHistory(overrides: Partial<CashRegisterHistoryResponse> = {}): CashRegisterHistoryResponse {
  return {
    items: [createRegisterItem()],
    page: 1,
    pageSize: 20,
    totalCount: 1,
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

function createFetchMock({ history, fail = false }: { history?: CashRegisterHistoryResponse; fail?: boolean } = {}) {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/cash-registers?')) {
      if (fail) {
        return createJsonResponse(null, false, 500, 'SERVER_ERROR', 'Server error.')
      }

      return createJsonResponse(history ?? createHistory())
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
