import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { expensesApi } from '../services/expensesApi'
import type {
  CreateExpenseCategoryRequest,
  CreateOperatingExpenseRequest,
  ExpenseFilters,
  PayOperatingExpenseRequest,
} from '../types'

export const expenseQueryKeys = {
  categories: ['expenses', 'categories'] as const,
  expenses: (filters?: ExpenseFilters) => ['expenses', 'list', filters] as const,
  expense: (id: string) => ['expenses', 'detail', id] as const,
  summary: (params?: object) => ['expenses', 'summary', params] as const,
}

// ── Categories ────────────────────────────────────────────────────────────────

export function useExpenseCategories() {
  return useQuery({
    queryKey: expenseQueryKeys.categories,
    queryFn: expensesApi.listCategories,
    staleTime: 1000 * 60 * 5, // 5 minutes
  })
}

export function useCreateExpenseCategory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateExpenseCategoryRequest) => expensesApi.createCategory(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: expenseQueryKeys.categories })
    },
  })
}

// ── Expenses ──────────────────────────────────────────────────────────────────

export function useExpenses(filters?: ExpenseFilters) {
  return useQuery({
    queryKey: expenseQueryKeys.expenses(filters),
    queryFn: () => expensesApi.listExpenses(filters),
    staleTime: 1000 * 30,
  })
}

export function useExpense(id: string) {
  return useQuery({
    queryKey: expenseQueryKeys.expense(id),
    queryFn: () => expensesApi.getExpenseById(id),
    enabled: Boolean(id),
    staleTime: 1000 * 60,
  })
}

export function useExpenseSummary(params?: { dateFrom: string; dateTo: string; branchId?: string }) {
  return useQuery({
    queryKey: expenseQueryKeys.summary(params),
    queryFn: () => expensesApi.getSummary(params!),
    enabled: Boolean(params?.dateFrom && params?.dateTo),
    staleTime: 1000 * 60,
  })
}

export function useCreateExpense() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateOperatingExpenseRequest) => expensesApi.createExpense(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['expenses', 'list'] })
      void queryClient.invalidateQueries({ queryKey: ['expenses', 'summary'] })
    },
  })
}

export function usePayExpense() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: PayOperatingExpenseRequest }) =>
      expensesApi.payExpense(id, request),
    onSuccess: (expense) => {
      queryClient.setQueryData(expenseQueryKeys.expense(expense.id), expense)
      void queryClient.invalidateQueries({ queryKey: ['expenses', 'list'] })
      void queryClient.invalidateQueries({ queryKey: ['expenses', 'summary'] })
    },
  })
}

export function useCancelExpense() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => expensesApi.cancelExpense(id),
    onSuccess: (expense) => {
      queryClient.setQueryData(expenseQueryKeys.expense(expense.id), expense)
      void queryClient.invalidateQueries({ queryKey: ['expenses', 'list'] })
      void queryClient.invalidateQueries({ queryKey: ['expenses', 'summary'] })
    },
  })
}
