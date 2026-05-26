import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { CashRegisterPage } from '@/modules/cash-register/pages/CashRegisterPage'
import type { CashRegisterDetail, CloseCashRegisterResponse } from '@/modules/cash-register/types'

// ── Constants ──────────────────────────────────────────────────────────────────

const REGISTER_ID = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
const BRANCH_ID = '22222222-2222-2222-2222-222222222222'
const BUSINESS_ID = '11111111-1111-1111-1111-111111111111'

// ── Describe: CashRegisterPage ─────────────────────────────────────────────────

describe('CashRegisterPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows open register form when no active register', async () => {
    vi.stubGlobal('fetch', createFetchMock({ active: null }))

    renderCashRegisterPage()

    expect(await screen.findByText('Abrir caja avanzada')).toBeTruthy()
    expect(screen.getByLabelText('Monto inicial (RD$)')).toBeTruthy()
  })

  it('shows active register panel when register is open', async () => {
    vi.stubGlobal('fetch', createFetchMock({ active: createActiveRegister() }))

    renderCashRegisterPage()

    expect(await screen.findByText('Monto inicial')).toBeTruthy()
    expect(screen.getByText('Efectivo esperado')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Registrar movimiento/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Cerrar caja/i })).toBeTruthy()
  })

  it('shows sales breakdown when register is open', async () => {
    vi.stubGlobal('fetch', createFetchMock({ active: createActiveRegister({ cashSalesTotal: 3000 }) }))

    renderCashRegisterPage()

    expect(await screen.findByText('Ventas del turno')).toBeTruthy()
    expect(screen.getByText('Efectivo')).toBeTruthy()
    expect(screen.getByText('Tarjeta')).toBeTruthy()
  })

  it('shows movement form when register movement button clicked', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createFetchMock({ active: createActiveRegister() }))

    renderCashRegisterPage()

    await screen.findByRole('button', { name: /Registrar movimiento/i })
    await user.click(screen.getByRole('button', { name: /Registrar movimiento/i }))

    expect(screen.getByText('Nuevo movimiento')).toBeTruthy()
    expect(screen.getByLabelText('Motivo')).toBeTruthy()
  })

  it('shows close result panel after successful close', async () => {
    const user = userEvent.setup()
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        active: createActiveRegister(),
        closeResult: createCloseResult({ differenceType: 'Balanced' }),
      }),
    )

    renderCashRegisterPage()

    await screen.findByRole('button', { name: /Cerrar caja/i })
    await user.click(screen.getByRole('button', { name: /Cerrar caja/i }))

    await screen.findByLabelText('Monto contado (RD$)')
    await user.type(screen.getByLabelText('Monto contado (RD$)'), '1000')
    await user.click(screen.getByRole('button', { name: /Confirmar cierre/i }))

    expect(await screen.findByText('Caja cerrada')).toBeTruthy()
    expect(screen.getByText('Cuadrado')).toBeTruthy()
  })
})

// ── Render helpers ──────────────────────────────────────────────────────────────

function renderCashRegisterPage() {
  const router = createMemoryRouter(
    [
      { element: <CashRegisterPage />, path: '/cash-register' },
      { element: <div>Daily Summary</div>, path: '/cash-register/daily-summary' },
    ],
    { initialEntries: ['/cash-register'] },
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

function createActiveRegister(
  overrides: Partial<CashRegisterDetail> = {},
): CashRegisterDetail {
  return {
    id: REGISTER_ID,
    branchId: BRANCH_ID,
    userId: '44444444-4444-4444-4444-444444444444',
    status: 'Open',
    openingAmount: 1000,
    openedAt: '2026-05-26T08:00:00Z',
    closedAt: null,
    countedAmount: null,
    expectedCashAmount: null,
    difference: null,
    differenceType: null,
    cashSalesTotal: 2500,
    cardSalesTotal: 1000,
    transferSalesTotal: 500,
    creditSalesTotal: 200,
    cashReturnsTotal: 0,
    manualCashIn: 0,
    manualCashOut: 0,
    notes: null,
    closeNotes: null,
    movements: [],
    ...overrides,
  }
}

function createCloseResult(
  overrides: Partial<CloseCashRegisterResponse> = {},
): CloseCashRegisterResponse {
  return {
    cashRegisterId: REGISTER_ID,
    openingAmount: 1000,
    cashSales: 2500,
    cardSales: 1000,
    transferSales: 500,
    creditSales: 200,
    cashReturns: 0,
    manualCashIn: 0,
    manualCashOut: 0,
    expectedCashAmount: 3500,
    countedAmount: 3500,
    difference: 0,
    differenceType: 'Balanced',
    closedAt: '2026-05-26T17:00:00Z',
    ...overrides,
  }
}

// ── Fetch mock ──────────────────────────────────────────────────────────────────

function createFetchMock({
  active,
  closeResult,
  failActive = false,
}: {
  active?: CashRegisterDetail | null
  closeResult?: CloseCashRegisterResponse
  failActive?: boolean
} = {}) {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/cash-registers/active')) {
      if (failActive) {
        return createJsonResponse(null, false, 500, 'SERVER_ERROR', 'Failed.')
      }
      return createJsonResponse(active ?? null)
    }

    if (url.includes('/api/cash-registers/open')) {
      const opened = { cashRegisterId: REGISTER_ID, openedAt: '2026-05-26T08:00:00Z' }
      return createJsonResponse(opened, true, 201)
    }

    if (url.match(/\/api\/cash-registers\/[^/]+\/movements/)) {
      return createJsonResponse(
        { id: 'mov1', movementType: 'CashIn', amount: 100, reason: 'Test', createdAt: '2026-05-26T09:00:00Z' },
        true,
        201,
      )
    }

    if (url.match(/\/api\/cash-registers\/[^/]+\/close/)) {
      return createJsonResponse(closeResult ?? createCloseResult())
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
