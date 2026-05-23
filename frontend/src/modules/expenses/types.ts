export type ExpensePaymentMethod = 'Cash' | 'Transfer' | 'Card'
export type ExpenseStatus = 'Pending' | 'Paid' | 'Cancelled'

export type ExpenseCategory = {
  id: string
  businessId: string
  name: string
  isActive: boolean
  createdAt: string
}

export type OperatingExpense = {
  id: string
  businessId: string
  branchId: string
  userId: string
  categoryId: string
  categoryName: string
  description: string
  amount: number
  paymentMethod: ExpensePaymentMethod
  status: ExpenseStatus
  expenseDate: string
  notes: string | null
  cashSessionId: string | null
  cashMovementId: string | null
  paidAt: string | null
  paidByUserId: string | null
  cancelledAt: string | null
  createdAt: string
  updatedAt: string
}

export type OperatingExpenseListItem = {
  id: string
  branchId: string
  categoryId: string
  categoryName: string
  description: string
  amount: number
  paymentMethod: ExpensePaymentMethod
  status: ExpenseStatus
  expenseDate: string
  createdAt: string
}

export type OperatingExpensesListResult = {
  items: OperatingExpenseListItem[]
  totalCount: number
  page: number
  pageSize: number
}

export type ExpenseByCategoryResponse = {
  categoryId: string
  categoryName: string
  totalAmount: number
  count: number
}

export type ExpenseByPaymentMethodResponse = {
  paymentMethod: string
  totalAmount: number
  count: number
}

export type ExpenseSummaryResponse = {
  dateFrom: string
  dateTo: string
  totalAmount: number
  pendingAmount: number
  paidCount: number
  pendingCount: number
  cancelledCount: number
  byCategory: ExpenseByCategoryResponse[]
  byPaymentMethod: ExpenseByPaymentMethodResponse[]
}

export type CreateExpenseCategoryRequest = {
  name: string
}

export type CreateOperatingExpenseRequest = {
  branchId: string
  categoryId: string
  description: string
  amount: number
  paymentMethod: string
  status: string
  expenseDate: string
  notes?: string | null
}

export type PayOperatingExpenseRequest = {
  paymentMethod: string
}

export type ExpenseFilters = {
  branchId?: string
  categoryId?: string
  status?: string
  paymentMethod?: string
  dateFrom?: string
  dateTo?: string
  page?: number
  pageSize?: number
}
