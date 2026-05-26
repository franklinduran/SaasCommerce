import { beforeEach, describe, expect, it, vi } from 'vitest'
import { expensesApi } from '@/modules/expenses/services/expensesApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'expense-token' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('expensesApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'expense-result' } })
  })

  it('loads categories, expenses, details and summaries', async () => {
    await expensesApi.listCategories()
    await expensesApi.listExpenses({
      branchId: 'branch-1',
      categoryId: 'cat-1',
      dateFrom: '2026-05-01',
      dateTo: '2026-05-25',
      page: 2,
      pageSize: 50,
      paymentMethod: 'Cash',
      status: 'Paid',
    })
    await expensesApi.listExpenses()
    await expensesApi.getExpenseById('expense-1')
    await expensesApi.getSummary({ branchId: 'branch-1', dateFrom: '2026-05-01', dateTo: '2026-05-25' })

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/expense-categories', { accessToken: 'expense-token' })
    expect(httpClient).toHaveBeenNthCalledWith(
      2,
      '/api/expenses?branchId=branch-1&categoryId=cat-1&status=Paid&paymentMethod=Cash&dateFrom=2026-05-01&dateTo=2026-05-25&page=2&pageSize=50',
      { accessToken: 'expense-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/expenses', { accessToken: 'expense-token' })
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/expenses/expense-1', { accessToken: 'expense-token' })
    expect(httpClient).toHaveBeenNthCalledWith(
      5,
      '/api/expenses/summary?dateFrom=2026-05-01&dateTo=2026-05-25&branchId=branch-1',
      { accessToken: 'expense-token' },
    )
  })

  it('sends category and expense mutations', async () => {
    await expensesApi.createCategory({ name: 'Rent' })
    await expensesApi.createExpense({ amount: 250, categoryId: 'cat-1', description: 'Office', dueDate: '2026-05-25' })
    await expensesApi.payExpense('expense-1', { paidAt: '2026-05-25', paymentMethod: 'Cash' })
    await expensesApi.cancelExpense('expense-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/expense-categories', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/expenses', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/expenses/expense-1/pay', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/expenses/expense-1/cancel', expect.objectContaining({
      accessToken: 'expense-token',
      method: 'POST',
    }))
  })
})
