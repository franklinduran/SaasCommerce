import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'
import type {
  CreateExpenseCategoryRequest,
  CreateOperatingExpenseRequest,
  ExpenseCategory,
  ExpenseFilters,
  ExpenseSummaryResponse,
  OperatingExpense,
  OperatingExpensesListResult,
  PayOperatingExpenseRequest,
} from '../types'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const expensesApi = {
  // ── Categories ────────────────────────────────────────────────────────────

  async listCategories(): Promise<ExpenseCategory[]> {
    const response = await httpClient<ExpenseCategory[]>('/api/expense-categories', {
      accessToken: getAccessToken(),
    })
    return response.data ?? []
  },

  async createCategory(request: CreateExpenseCategoryRequest): Promise<ExpenseCategory> {
    const response = await httpClient<ExpenseCategory>('/api/expense-categories', {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify(request),
    })
    return response.data!
  },

  // ── Expenses ──────────────────────────────────────────────────────────────

  async listExpenses(filters?: ExpenseFilters): Promise<OperatingExpensesListResult> {
    const query = new URLSearchParams()
    if (filters?.branchId) query.set('branchId', filters.branchId)
    if (filters?.categoryId) query.set('categoryId', filters.categoryId)
    if (filters?.status) query.set('status', filters.status)
    if (filters?.paymentMethod) query.set('paymentMethod', filters.paymentMethod)
    if (filters?.dateFrom) query.set('dateFrom', filters.dateFrom)
    if (filters?.dateTo) query.set('dateTo', filters.dateTo)
    if (filters?.page) query.set('page', String(filters.page))
    if (filters?.pageSize) query.set('pageSize', String(filters.pageSize))

    const qs = query.toString()
    const response = await httpClient<OperatingExpensesListResult>(
      `/api/expenses${qs ? `?${qs}` : ''}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },

  async getExpenseById(id: string): Promise<OperatingExpense> {
    const response = await httpClient<OperatingExpense>(`/api/expenses/${id}`, {
      accessToken: getAccessToken(),
    })
    return response.data!
  },

  async createExpense(request: CreateOperatingExpenseRequest): Promise<OperatingExpense> {
    const response = await httpClient<OperatingExpense>('/api/expenses', {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify(request),
    })
    return response.data!
  },

  async payExpense(id: string, request: PayOperatingExpenseRequest): Promise<OperatingExpense> {
    const response = await httpClient<OperatingExpense>(`/api/expenses/${id}/pay`, {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify(request),
    })
    return response.data!
  },

  async cancelExpense(id: string): Promise<OperatingExpense> {
    const response = await httpClient<OperatingExpense>(`/api/expenses/${id}/cancel`, {
      accessToken: getAccessToken(),
      method: 'POST',
    })
    return response.data!
  },

  async getSummary(params: {
    dateFrom: string
    dateTo: string
    branchId?: string
  }): Promise<ExpenseSummaryResponse> {
    const query = new URLSearchParams()
    query.set('dateFrom', params.dateFrom)
    query.set('dateTo', params.dateTo)
    if (params.branchId) query.set('branchId', params.branchId)

    const response = await httpClient<ExpenseSummaryResponse>(
      `/api/expenses/summary?${query.toString()}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },
}
