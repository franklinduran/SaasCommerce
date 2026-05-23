import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, MemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { CashHistoryPage } from '@/modules/cash/pages/CashHistoryPage'
import { CashPage } from '@/modules/cash/pages/CashPage'
import { CashSessionDetailPage } from '@/modules/cash/pages/CashSessionDetailPage'
import type { CashSession, CashSessionListItem } from '@/modules/cash/types'

const SESSION_ID = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
const BUSINESS_ID = '11111111-1111-1111-1111-111111111111'
const BRANCH_ID = '22222222-2222-2222-2222-222222222222'

describe('CashPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows the open session form when there is no active session', async () => {
    vi.stubGlobal('fetch', createFetchMock({ currentSession: null }))

    renderCashPage()

    // Both the heading and button say "Abrir caja" — use findAllByText
    expect((await screen.findAllByText('Abrir caja')).length).toBeGreaterThan(0)
    expect(screen.getByText('No hay una caja abierta para esta sucursal.')).toBeTruthy()
  })

  it('shows the active session panel when there is an open session', async () => {
    vi.stubGlobal('fetch', createFetchMock({ currentSession: createSession() }))

    renderCashPage()

    expect(await screen.findByText('Balance sistema')).toBeTruthy()
    expect(screen.getByText('Balance inicial')).toBeTruthy()
    expect(screen.getAllByText('RD$1,000.00').length).toBeGreaterThan(0)
  })

  it('shows movements in the active session panel', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        currentSession: createSession({
          movements: [
            createMovement({ amount: 300, description: 'Fondo inicial extra', type: 'CashIn' }),
            createMovement({ amount: 50, description: 'Pago de servicio', type: 'CashOut' }),
          ],
          systemBalance: 1250,
        }),
      }),
    )

    renderCashPage()

    expect(await screen.findByText('Fondo inicial extra')).toBeTruthy()
    expect(screen.getByText('Pago de servicio')).toBeTruthy()
    expect(screen.getByText('Movimientos de caja')).toBeTruthy()
  })

  it('shows action buttons when session is open', async () => {
    vi.stubGlobal('fetch', createFetchMock({ currentSession: createSession() }))

    renderCashPage()

    expect(await screen.findByRole('button', { name: /Registrar movimiento/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Cerrar caja/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Ver historial/i })).toBeTruthy()
  })

  it('shows the movement form when Registrar movimiento is clicked', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createFetchMock({ currentSession: createSession() }))

    renderCashPage()

    await user.click(await screen.findByRole('button', { name: /Registrar movimiento/i }))

    // After opening form: "Nuevo movimiento" card title appears
    expect(screen.getByText('Nuevo movimiento')).toBeTruthy()
    // The cancel button inside the form
    expect(screen.getByRole('button', { name: /Cancelar/i })).toBeTruthy()
  })

  it('shows the close session form when Cerrar caja is clicked', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createFetchMock({ currentSession: createSession() }))

    renderCashPage()

    await user.click(await screen.findByRole('button', { name: /Cerrar caja/i }))

    // Multiple "Cerrar caja" elements appear (button + form title) — check form-specific label
    expect(screen.getAllByText(/Cerrar caja/i).length).toBeGreaterThan(0)
    expect(screen.getByLabelText(/Balance contado/i)).toBeTruthy()
  })

  it('shows validation error when opening balance is not entered', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createFetchMock({ currentSession: null }))

    renderCashPage()

    await screen.findAllByText('Abrir caja') // wait for render
    // Submitting with empty balance (NaN) triggers the same validation error as negative
    await user.click(screen.getByRole('button', { name: 'Abrir caja', exact: true }))

    expect(await screen.findByText('El balance inicial debe ser 0 o mayor.')).toBeTruthy()
  })
})

describe('CashHistoryPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows list of sessions from API', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        sessions: {
          items: [
            createSessionListItem({ id: 'aaa-1', openingBalance: 500, status: 'Closed', systemBalance: 750 }),
            createSessionListItem({ id: 'aaa-2', openingBalance: 1000, status: 'Open', systemBalance: 1000 }),
          ],
          page: 1,
          pageSize: 20,
          totalCount: 2,
        },
      }),
    )

    renderCashHistoryPage()

    expect(await screen.findByText('Abierta')).toBeTruthy()
    expect(screen.getByText('Cerrada')).toBeTruthy()
    expect(screen.getAllByText(/RD\$/).length).toBeGreaterThan(0)
  })

  it('shows empty state when no sessions exist', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        sessions: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      }),
    )

    renderCashHistoryPage()

    expect(await screen.findByText('No hay sesiones de caja registradas.')).toBeTruthy()
  })

  it('shows error state when API fails', async () => {
    vi.stubGlobal('fetch', createFetchMock({ failSessions: true }))

    renderCashHistoryPage()

    expect(await screen.findByText('Error al cargar el historial. Intenta nuevamente.')).toBeTruthy()
  })
})

describe('CashSessionDetailPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows session details with movements', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        sessionDetail: createSession({
          movements: [createMovement({ amount: 200, description: 'Pago en efectivo', type: 'CashIn' })],
          systemBalance: 1200,
        }),
      }),
    )

    renderCashSessionDetailPage(SESSION_ID)

    // Movement description appears; the title is dynamic "Movimientos (1)"
    expect(await screen.findByText('Pago en efectivo')).toBeTruthy()
    expect(screen.getByText(/Movimientos \(1\)/)).toBeTruthy()
  })

  it('shows closed session with difference when closingBalance is set', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        sessionDetail: createSession({
          closedAt: '2026-05-22T14:00:00Z',
          closingBalance: 1000,
          status: 'Closed',
          systemBalance: 1000,
        }),
      }),
    )

    renderCashSessionDetailPage(SESSION_ID)

    // Detail page shows "Balance cierre" card and "Cerrada" badge for closed sessions
    expect(await screen.findByText('Balance cierre')).toBeTruthy()
    expect(screen.getByText('Cerrada')).toBeTruthy()
    // Difference = 0 → formatted as RD$0.00
    expect(screen.getByText(/Diferencia:/)).toBeTruthy()
  })
})

// ── Render helpers ────────────────────────────────────────────────────────────

function renderCashPage() {
  const router = createMemoryRouter(
    [
      { element: <CashPage />, path: '/cash' },
      { element: <CashHistoryPage />, path: '/cash/history' },
    ],
    { initialEntries: ['/cash'] },
  )

  renderWithQueryClient(<RouterProvider router={router} />)
}

function renderCashHistoryPage() {
  renderWithQueryClient(
    <MemoryRouter initialEntries={['/cash/history']}>
      <CashHistoryPage />
    </MemoryRouter>,
  )
}

function renderCashSessionDetailPage(id: string) {
  const router = createMemoryRouter(
    [{ element: <CashSessionDetailPage />, path: '/cash/:cashSessionId' }],
    { initialEntries: [`/cash/${id}`] },
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
    expiresAt: '2026-05-22T23:59:00Z',
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

// ── Factory functions ─────────────────────────────────────────────────────────

function createSession(overrides: Partial<CashSession> = {}): CashSession {
  return {
    branchId: BRANCH_ID,
    businessId: BUSINESS_ID,
    closedAt: null,
    closingBalance: null,
    id: SESSION_ID,
    movements: [],
    notes: null,
    openedAt: '2026-05-22T08:00:00Z',
    openingBalance: 1000,
    status: 'Open',
    systemBalance: 1000,
    updatedAt: '2026-05-22T08:00:00Z',
    userId: '44444444-4444-4444-4444-444444444444',
    ...overrides,
  }
}

function createMovement(
  overrides: Partial<{ id: string; amount: number; description: string; type: 'CashIn' | 'CashOut' }> = {},
) {
  return {
    amount: overrides.amount ?? 100,
    cashSessionId: SESSION_ID,
    createdAt: '2026-05-22T09:00:00Z',
    description: overrides.description ?? 'Movimiento',
    id: overrides.id ?? `mov-${Math.random()}`,
    type: overrides.type ?? ('CashIn' as const),
    userId: '44444444-4444-4444-4444-444444444444',
  }
}

function createSessionListItem(overrides: Partial<CashSessionListItem> = {}): CashSessionListItem {
  return {
    branchId: BRANCH_ID,
    closedAt: null,
    closingBalance: null,
    id: SESSION_ID,
    openedAt: '2026-05-22T08:00:00Z',
    openingBalance: 1000,
    status: 'Open',
    systemBalance: 1000,
    userId: '44444444-4444-4444-4444-444444444444',
    ...overrides,
  }
}

// ── Fetch mock ────────────────────────────────────────────────────────────────

function createFetchMock({
  currentSession,
  sessionDetail,
  sessions,
  failSessions = false,
}: {
  currentSession?: CashSession | null
  sessionDetail?: CashSession
  sessions?: { items: CashSessionListItem[]; page: number; pageSize: number; totalCount: number }
  failSessions?: boolean
} = {}) {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/cash-sessions/current')) {
      return createJsonResponse(currentSession ?? null)
    }

    if (url.match(/\/api\/cash-sessions\/[^/]+$/) && !url.includes('movements') && !url.includes('close')) {
      return createJsonResponse(sessionDetail ?? createSession())
    }

    if (url.includes('/api/cash-sessions')) {
      if (failSessions) {
        return createJsonResponse(null, false, 500, 'SERVER_ERROR', 'Error al cargar sesiones.')
      }

      return createJsonResponse(
        sessions ?? { items: [], page: 1, pageSize: 20, totalCount: 0 },
      )
    }

    return createJsonResponse(null, false, 404, 'NOT_FOUND', 'Ruta no encontrada.')
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
