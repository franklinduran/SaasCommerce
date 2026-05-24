import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { DailyClosingDetailPage } from '@/modules/daily-closing/pages/DailyClosingDetailPage'
import { DailyClosingHistoryPage } from '@/modules/daily-closing/pages/DailyClosingHistoryPage'
import { DailyClosingPage } from '@/modules/daily-closing/pages/DailyClosingPage'
import type {
  DailyClosingDetail,
  DailyClosingListItem,
  DailyClosingPreview,
} from '@/modules/daily-closing/types'

// ── Constants ─────────────────────────────────────────────────────────────────

const CLOSING_ID = 'cccccccc-cccc-cccc-cccc-cccccccccccc'
const BUSINESS_ID = '11111111-1111-1111-1111-111111111111'
const BRANCH_ID = '22222222-2222-2222-2222-222222222222'

// ── Describe: DailyClosingPage ─────────────────────────────────────────────

describe('DailyClosingPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders date and branch selectors', async () => {
    vi.stubGlobal('fetch', createFetchMock({ branches: [createBranch()], preview: null }))

    renderDailyClosingPage()

    // Date input always present
    const dateInput = await screen.findByLabelText('Fecha')
    expect(dateInput).toBeTruthy()
  })

  it('shows preview data when date and branch are selected', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({ branches: [createBranch()], preview: createPreview() }),
    )

    renderDailyClosingPage()

    expect(await screen.findByText('Total ventas')).toBeTruthy()
    expect(screen.getByText('Ganancia neta est.')).toBeTruthy()
    expect(screen.getAllByText(/RD\$/).length).toBeGreaterThan(0)
  })

  it('shows alert badge when preview has alerts', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        branches: [createBranch()],
        preview: createPreview({
          alerts: [
            {
              id: 'a1',
              alertType: 'NegativeMargin',
              message: 'El margen neto es negativo.',
              estimatedImpact: 500,
            },
          ],
        }),
      }),
    )

    renderDailyClosingPage()

    expect(await screen.findByText('Alertas (1)')).toBeTruthy()
    expect(screen.getByText('El margen neto es negativo.')).toBeTruthy()
  })

  it('shows create button after preview loads', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({ branches: [createBranch()], preview: createPreview() }),
    )

    renderDailyClosingPage()

    expect(await screen.findByRole('button', { name: /Crear cierre del día/i })).toBeTruthy()
  })

  it('shows preview error when API fails', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({ branches: [createBranch()], failPreview: true }),
    )

    renderDailyClosingPage()

    expect(
      await screen.findByText(/Error al cargar la vista previa/i),
    ).toBeTruthy()
  })
})

// ── Describe: DailyClosingHistoryPage ─────────────────────────────────────────

describe('DailyClosingHistoryPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows list of closings from API', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        list: {
          items: [
            createListItem({ id: 'c1', closingDate: '2026-05-20', status: 'Closed' }),
            createListItem({ id: 'c2', closingDate: '2026-05-21', status: 'Draft' }),
          ],
          totalCount: 2,
          page: 1,
          pageSize: 20,
        },
      }),
    )

    renderHistoryPage()

    expect(await screen.findByText('Cerrado')).toBeTruthy()
    expect(screen.getByText('Borrador')).toBeTruthy()
    expect(screen.getByText('2 cierres registrados')).toBeTruthy()
  })

  it('shows empty state when no closings exist', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        list: { items: [], totalCount: 0, page: 1, pageSize: 20 },
      }),
    )

    renderHistoryPage()

    expect(await screen.findByText('No hay cierres registrados.')).toBeTruthy()
  })

  it('shows alert count badge', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        list: {
          items: [createListItem({ alertCount: 3 })],
          totalCount: 1,
          page: 1,
          pageSize: 20,
        },
      }),
    )

    renderHistoryPage()

    expect(await screen.findByText('3 alertas')).toBeTruthy()
  })

  it('shows error state when API fails', async () => {
    vi.stubGlobal('fetch', createFetchMock({ failList: true }))

    renderHistoryPage()

    expect(await screen.findByText(/Error al cargar el historial/i)).toBeTruthy()
  })
})

// ── Describe: DailyClosingDetailPage ──────────────────────────────────────────

describe('DailyClosingDetailPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows draft closing details with close form', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({ detail: createDetail({ status: 'Draft' }) }),
    )

    renderDetailPage(CLOSING_ID)

    expect(await screen.findByText('Borrador')).toBeTruthy()
    expect(screen.getByText('Cerrar el día')).toBeTruthy()
    expect(screen.getByLabelText(/Efectivo contado/i)).toBeTruthy()
  })

  it('shows closed closing with cash reconciliation', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        detail: createDetail({
          status: 'Closed',
          cashCounted: 5000,
          cashExpected: 4800,
          cashDifference: 200,
          closedAt: '2026-05-23T15:00:00Z',
        }),
      }),
    )

    renderDetailPage(CLOSING_ID)

    // "Cerrado" appears in the status Badge and also as the "closedAt" label in metadata
    expect((await screen.findAllByText('Cerrado')).length).toBeGreaterThan(0)
    expect(screen.getByText('Cuadre de efectivo')).toBeTruthy()
    // Closed section should NOT show close form
    expect(screen.queryByText('Cerrar el día')).toBeNull()
  })

  it('shows validation error when cash counted is negative', async () => {
    const user = userEvent.setup()
    vi.stubGlobal(
      'fetch',
      createFetchMock({ detail: createDetail({ status: 'Draft' }) }),
    )

    renderDetailPage(CLOSING_ID)

    await screen.findByLabelText(/Efectivo contado/i)
    const input = screen.getByLabelText(/Efectivo contado/i)
    await user.type(input, '-100')

    await user.click(screen.getByRole('button', { name: /Cerrar día/i }))

    expect(await screen.findByText('El efectivo contado debe ser 0 o mayor.')).toBeTruthy()
  })

  it('shows alerts when present', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        detail: createDetail({
          alerts: [
            {
              id: 'a1',
              alertType: 'HighExpenseRatio',
              message: 'Gastos por encima del umbral.',
              estimatedImpact: 1200,
            },
          ],
        }),
      }),
    )

    renderDetailPage(CLOSING_ID)

    expect(await screen.findByText('Alertas (1)')).toBeTruthy()
    expect(screen.getByText('Gastos por encima del umbral.')).toBeTruthy()
  })

  it('shows not found when API returns error', async () => {
    vi.stubGlobal('fetch', createFetchMock({ failDetail: true }))

    renderDetailPage(CLOSING_ID)

    expect(await screen.findByText('No se encontró el cierre.')).toBeTruthy()
  })
})

// ── Render helpers ─────────────────────────────────────────────────────────────

function renderDailyClosingPage() {
  const router = createMemoryRouter(
    [
      { element: <DailyClosingPage />, path: '/daily-closing' },
      { element: <DailyClosingDetailPage />, path: '/daily-closing/:closingId' },
      { element: <DailyClosingHistoryPage />, path: '/daily-closing/history' },
    ],
    { initialEntries: ['/daily-closing'] },
  )
  renderWithQueryClient(<RouterProvider router={router} />)
}

function renderHistoryPage() {
  const router = createMemoryRouter(
    [
      { element: <DailyClosingHistoryPage />, path: '/daily-closing/history' },
      { element: <DailyClosingPage />, path: '/daily-closing' },
    ],
    { initialEntries: ['/daily-closing/history'] },
  )
  renderWithQueryClient(<RouterProvider router={router} />)
}

function renderDetailPage(id: string) {
  const router = createMemoryRouter(
    [
      { element: <DailyClosingDetailPage />, path: '/daily-closing/:closingId' },
      { element: <DailyClosingHistoryPage />, path: '/daily-closing/history' },
    ],
    { initialEntries: [`/daily-closing/${id}`] },
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

// ── Factory functions ──────────────────────────────────────────────────────────

function createBranch() {
  return {
    id: BRANCH_ID,
    businessId: BUSINESS_ID,
    name: 'Sucursal Principal',
    code: 'MAIN',
    address: null,
    phone: null,
    isMain: true,
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  }
}

function createPreview(overrides: Partial<DailyClosingPreview> = {}): DailyClosingPreview {
  return {
    branchId: BRANCH_ID,
    branchName: 'Sucursal Principal',
    closingDate: '2026-05-23',
    totalSales: 10000,
    cashSales: 6000,
    transferSales: 2000,
    cardSales: 1500,
    creditSales: 500,
    salesCount: 25,
    cashExpected: 6500,
    totalExpenses: 1500,
    totalCost: 5000,
    grossProfit: 5000,
    estimatedNetProfit: 3500,
    grossMarginPercent: 50,
    netMarginPercent: 35,
    newCreditsAmount: 500,
    newCreditsCount: 2,
    creditPaymentsReceived: 300,
    alerts: [],
    ...overrides,
  }
}

function createListItem(overrides: Partial<DailyClosingListItem> = {}): DailyClosingListItem {
  return {
    id: CLOSING_ID,
    branchId: BRANCH_ID,
    branchName: 'Sucursal Principal',
    closingDate: '2026-05-23',
    status: 'Draft',
    totalSales: 10000,
    estimatedNetProfit: 3500,
    netMarginPercent: 35,
    alertCount: 0,
    createdAt: '2026-05-23T10:00:00Z',
    closedAt: null,
    ...overrides,
  }
}

function createDetail(overrides: Partial<DailyClosingDetail> = {}): DailyClosingDetail {
  return {
    id: CLOSING_ID,
    businessId: BUSINESS_ID,
    branchId: BRANCH_ID,
    branchName: 'Sucursal Principal',
    closingDate: '2026-05-23',
    status: 'Draft',
    totalSales: 10000,
    cashSales: 6000,
    transferSales: 2000,
    cardSales: 1500,
    creditSales: 500,
    salesCount: 25,
    cashExpected: 6500,
    cashCounted: null,
    cashDifference: null,
    totalExpenses: 1500,
    totalCost: 5000,
    grossProfit: 5000,
    estimatedNetProfit: 3500,
    grossMarginPercent: 50,
    netMarginPercent: 35,
    newCreditsAmount: 500,
    newCreditsCount: 2,
    creditPaymentsReceived: 300,
    notes: null,
    createdAt: '2026-05-23T10:00:00Z',
    closedAt: null,
    closedByUserId: null,
    alerts: [],
    ...overrides,
  }
}

// ── Fetch mock ─────────────────────────────────────────────────────────────────

function createFetchMock({
  branches,
  preview,
  list,
  detail,
  failPreview = false,
  failList = false,
  failDetail = false,
}: {
  branches?: object[]
  preview?: DailyClosingPreview | null
  list?: { items: DailyClosingListItem[]; totalCount: number; page: number; pageSize: number }
  detail?: DailyClosingDetail
  failPreview?: boolean
  failList?: boolean
  failDetail?: boolean
} = {}) {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    // Branch list
    if (url.includes('/api/branches')) {
      return createJsonResponse({ items: branches ?? [], total: branches?.length ?? 0 })
    }

    // Daily closing preview
    if (url.includes('/api/daily-closing/preview')) {
      if (failPreview) {
        return createJsonResponse(null, false, 400, 'BAD_REQUEST', 'Preview failed.')
      }
      return createJsonResponse(preview ?? null)
    }

    // Daily closing list
    if (url.match(/\/api\/daily-closing\?/) || url === '/api/daily-closing') {
      if (failList) {
        return createJsonResponse(null, false, 500, 'SERVER_ERROR', 'List failed.')
      }
      return createJsonResponse(
        list ?? { items: [], totalCount: 0, page: 1, pageSize: 20 },
      )
    }

    // Daily closing detail
    if (url.match(/\/api\/daily-closing\/[^/]+$/) && !url.includes('/close')) {
      if (failDetail) {
        return createJsonResponse(null, false, 404, 'NOT_FOUND', 'Not found.')
      }
      return createJsonResponse(detail ?? createDetail())
    }

    // Close action
    if (url.includes('/close')) {
      return createJsonResponse(detail ?? createDetail({ status: 'Closed' }))
    }

    // Create closing
    if (url === '/api/daily-closing' || url.endsWith('/api/daily-closing')) {
      return createJsonResponse(detail ?? createDetail(), true, 201)
    }

    return createJsonResponse(null, false, 404, 'NOT_FOUND', 'Route not found.')
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
