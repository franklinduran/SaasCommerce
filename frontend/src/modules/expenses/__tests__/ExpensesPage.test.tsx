import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, MemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { ExpenseCategoriesPage } from '../pages/ExpenseCategoriesPage'
import { ExpenseDetailPage } from '../pages/ExpenseDetailPage'
import { ExpensesPage } from '../pages/ExpensesPage'
import type { ExpenseCategory, OperatingExpense, OperatingExpensesListResult } from '../types'

const EXPENSE_ID = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'
const CATEGORY_ID = 'cccccccc-cccc-cccc-cccc-cccccccccccc'

// ── Test data factories ───────────────────────────────────────────────────────

function createCategory(overrides?: Partial<ExpenseCategory>): ExpenseCategory {
  return {
    id: CATEGORY_ID,
    businessId: '11111111-1111-1111-1111-111111111111',
    name: 'Servicios',
    isActive: true,
    createdAt: '2026-05-22T10:00:00Z',
    ...overrides,
  }
}

function createExpense(overrides?: Partial<OperatingExpense>): OperatingExpense {
  return {
    id: EXPENSE_ID,
    businessId: '11111111-1111-1111-1111-111111111111',
    branchId: '22222222-2222-2222-2222-222222222222',
    userId: '33333333-3333-3333-3333-333333333333',
    categoryId: CATEGORY_ID,
    categoryName: 'Servicios',
    description: 'Factura de electricidad',
    amount: 2500,
    paymentMethod: 'Transfer',
    status: 'Pending',
    expenseDate: '2026-05-20T00:00:00Z',
    notes: null,
    cashSessionId: null,
    cashMovementId: null,
    paidAt: null,
    paidByUserId: null,
    cancelledAt: null,
    createdAt: '2026-05-22T10:00:00Z',
    updatedAt: '2026-05-22T10:00:00Z',
    ...overrides,
  }
}

function createListResult(overrides?: Partial<OperatingExpensesListResult>): OperatingExpensesListResult {
  return {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    ...overrides,
  }
}

// ── API mock helpers ──────────────────────────────────────────────────────────

function createJsonResponse<T>(data: T, ok = true, status = 200) {
  return {
    ok: ok && status >= 200 && status < 300,
    status,
    json: async () => ({
      isSuccess: ok,
      data,
      error: ok ? null : { code: 'ERROR', message: 'failed' },
      correlationId: 'test-correlation-id',
    }),
    headers: new Headers({ 'content-type': 'application/json' }),
  }
}

function createFetchMock({
  categories = [createCategory()],
  listResult = createListResult(),
  expense = createExpense(),
}: {
  categories?: ExpenseCategory[]
  listResult?: OperatingExpensesListResult
  expense?: OperatingExpense
} = {}) {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/expense-categories')) {
      return createJsonResponse(categories)
    }
    if (url.match(/\/api\/expenses\/[^/]+$/) && !url.includes('pay') && !url.includes('cancel')) {
      return createJsonResponse(expense)
    }
    if (url.includes('/api/expenses')) {
      return createJsonResponse(listResult)
    }

    return createJsonResponse(null, false, 404)
  })
}

// ── Render helpers ────────────────────────────────────────────────────────────

function setSession() {
  useAuthStore.getState().setSession({
    accessToken: 'test-token',
    refreshToken: 'refresh-token',
    expiresAt: '2099-01-01T00:00:00.000Z',
    user: {
      id: '33333333-3333-3333-3333-333333333333',
      businessId: '11111111-1111-1111-1111-111111111111',
      branchId: '22222222-2222-2222-2222-222222222222',
      fullName: 'Test User',
      email: 'test@test.com',
      roles: ['Owner'],
    },
  })
}

function createQueryClient() {
  return new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
}

function renderExpensesPage() {
  const qc = createQueryClient()
  return render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <ExpensesPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

function renderExpenseDetailPage(id = EXPENSE_ID) {
  const qc = createQueryClient()
  const router = createMemoryRouter(
    [{ path: '/expenses/:id', element: <ExpenseDetailPage /> }],
    { initialEntries: [`/expenses/${id}`] },
  )
  return render(
    <QueryClientProvider client={qc}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

function renderCategoriesPage() {
  const qc = createQueryClient()
  return render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <ExpenseCategoriesPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('ExpensesPage', () => {
  beforeEach(() => { setSession() })
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows empty state when no expenses exist', async () => {
    vi.stubGlobal('fetch', createFetchMock({ listResult: createListResult() }))

    renderExpensesPage()

    await waitFor(() => {
      expect(screen.queryByText('No hay gastos registrados.')).toBeTruthy()
    })
  })

  it('shows expense list items', async () => {
    const expense = createExpense()
    const listResult = createListResult({
      items: [{
        id: expense.id,
        branchId: expense.branchId,
        categoryId: expense.categoryId,
        categoryName: expense.categoryName,
        description: expense.description,
        amount: expense.amount,
        paymentMethod: expense.paymentMethod,
        status: expense.status,
        expenseDate: expense.expenseDate,
        createdAt: expense.createdAt,
      }],
      totalCount: 1,
    })
    vi.stubGlobal('fetch', createFetchMock({ listResult }))

    renderExpensesPage()

    expect(await screen.findByText('Factura de electricidad')).toBeTruthy()
    expect(screen.getByText(/Pendiente/)).toBeTruthy()
  })

  it('shows header with action buttons', async () => {
    vi.stubGlobal('fetch', createFetchMock())

    renderExpensesPage()

    await waitFor(() => {
      expect(screen.getByText('Gastos operativos')).toBeTruthy()
      expect(screen.getByRole('button', { name: /nuevo gasto/i })).toBeTruthy()
      expect(screen.getByRole('button', { name: /categorías/i })).toBeTruthy()
    })
  })
})

describe('ExpenseDetailPage', () => {
  beforeEach(() => { setSession() })
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows expense detail for a pending expense', async () => {
    vi.stubGlobal('fetch', createFetchMock({ expense: createExpense() }))

    renderExpenseDetailPage()

    expect(await screen.findByText('Factura de electricidad')).toBeTruthy()
    expect(screen.getByText(/Pendiente/)).toBeTruthy()
    expect(screen.getByText('Servicios')).toBeTruthy()
  })

  it('shows pay and cancel actions for pending expense', async () => {
    vi.stubGlobal('fetch', createFetchMock({ expense: createExpense({ status: 'Pending' }) }))

    renderExpenseDetailPage()

    await waitFor(() => {
      expect(screen.getByText('Registrar pago')).toBeTruthy()
      expect(screen.getByRole('button', { name: /pagar/i })).toBeTruthy()
      expect(screen.getByRole('button', { name: /cancelar gasto/i })).toBeTruthy()
    })
  })

  it('does not show actions for a paid expense', async () => {
    vi.stubGlobal('fetch', createFetchMock({ expense: createExpense({ status: 'Paid' }) }))

    renderExpenseDetailPage()

    await waitFor(() => {
      expect(screen.getByText('Factura de electricidad')).toBeTruthy()
    })

    expect(screen.queryByText('Registrar pago')).toBeNull()
    expect(screen.queryByRole('button', { name: /cancelar gasto/i })).toBeNull()
  })

  it('does not show actions for a cancelled expense', async () => {
    vi.stubGlobal('fetch', createFetchMock({ expense: createExpense({ status: 'Cancelled' }) }))

    renderExpenseDetailPage()

    await waitFor(() => {
      expect(screen.getByText('Factura de electricidad')).toBeTruthy()
    })

    expect(screen.queryByText('Registrar pago')).toBeNull()
  })
})

describe('ExpenseCategoriesPage', () => {
  beforeEach(() => { setSession() })
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows categories list', async () => {
    vi.stubGlobal('fetch', createFetchMock({ categories: [createCategory()] }))

    renderCategoriesPage()

    expect(await screen.findByText('Servicios')).toBeTruthy()
    expect(screen.getByText('Activa')).toBeTruthy()
  })

  it('shows empty state when no categories exist', async () => {
    vi.stubGlobal('fetch', createFetchMock({ categories: [] }))

    renderCategoriesPage()

    await waitFor(() => {
      expect(screen.queryByText('No hay categorías aún.')).toBeTruthy()
    })
  })

  it('toggles the new category form', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createFetchMock({ categories: [createCategory()] }))

    renderCategoriesPage()

    await screen.findByText('Servicios')

    await user.click(screen.getByRole('button', { name: /nueva categoría/i }))

    expect(screen.getByPlaceholderText(/Ej\. Servicios, Alquiler/)).toBeTruthy()
  })
})
