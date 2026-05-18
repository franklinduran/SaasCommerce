import { useAuthStore } from '@/modules/auth/authStore'
import type {
  AccountsReceivableFilters,
  AccountsReceivableReport,
  InvoiceReport,
  InvoiceReportFilters,
  LowStockFilters,
  LowStockReport,
  PurchaseReport,
  PurchaseReportFilters,
  SalesReport,
  SalesReportFilters,
} from '@/modules/reports/types'
import { httpClient } from '@/shared/services/httpClient'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

// ── Sales ──────────────────────────────────────────────────────────────────

export async function getSalesReport(filters: SalesReportFilters): Promise<SalesReport> {
  const params = buildDateRangeParams(filters)

  if (filters.branchId) params.set('branchId', filters.branchId)
  if (filters.status) params.set('status', filters.status)
  if (filters.paymentMethod) params.set('paymentMethod', filters.paymentMethod)
  if (filters.search.trim()) params.set('search', filters.search.trim())

  const response = await httpClient<SalesReport>(`/api/reports/sales?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export function getSalesExportUrl(filters: SalesReportFilters): string {
  const params = buildDateRangeParams(filters)

  if (filters.branchId) params.set('branchId', filters.branchId)
  if (filters.status) params.set('status', filters.status)
  if (filters.paymentMethod) params.set('paymentMethod', filters.paymentMethod)
  if (filters.search.trim()) params.set('search', filters.search.trim())

  const token = getAccessToken()
  if (token) params.set('access_token', token)

  return `${apiBaseUrl.replace(/\/$/, '')}/api/reports/sales/export?${params.toString()}`
}

// ── Invoices ───────────────────────────────────────────────────────────────

export async function getInvoiceReport(filters: InvoiceReportFilters): Promise<InvoiceReport> {
  const params = buildDateRangeParams(filters)

  if (filters.status) params.set('status', filters.status)
  if (filters.customerId) params.set('customerId', filters.customerId)
  if (filters.search.trim()) params.set('search', filters.search.trim())

  const response = await httpClient<InvoiceReport>(`/api/reports/invoices?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export function getInvoiceExportUrl(filters: InvoiceReportFilters): string {
  const params = buildDateRangeParams(filters)

  if (filters.status) params.set('status', filters.status)
  if (filters.customerId) params.set('customerId', filters.customerId)
  if (filters.search.trim()) params.set('search', filters.search.trim())

  const token = getAccessToken()
  if (token) params.set('access_token', token)

  return `${apiBaseUrl.replace(/\/$/, '')}/api/reports/invoices/export?${params.toString()}`
}

// ── Accounts Receivable ────────────────────────────────────────────────────

export async function getAccountsReceivableReport(
  filters: AccountsReceivableFilters,
): Promise<AccountsReceivableReport> {
  const params = buildDateRangeParams(filters)

  if (filters.customerId) params.set('customerId', filters.customerId)
  if (filters.status) params.set('status', filters.status)

  const response = await httpClient<AccountsReceivableReport>(
    `/api/reports/accounts-receivable?${params.toString()}`,
    { accessToken: getAccessToken() },
  )

  return response.data!
}

export function getArExportUrl(filters: AccountsReceivableFilters): string {
  const params = buildDateRangeParams(filters)

  if (filters.customerId) params.set('customerId', filters.customerId)
  if (filters.status) params.set('status', filters.status)

  const token = getAccessToken()
  if (token) params.set('access_token', token)

  return `${apiBaseUrl.replace(/\/$/, '')}/api/reports/accounts-receivable/export?${params.toString()}`
}

// ── Low Stock ──────────────────────────────────────────────────────────────

export async function getLowStockReport(filters: LowStockFilters): Promise<LowStockReport> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  })

  if (filters.branchId) params.set('branchId', filters.branchId)
  if (filters.categoryId) params.set('categoryId', filters.categoryId)
  if (filters.search.trim()) params.set('search', filters.search.trim())

  const response = await httpClient<LowStockReport>(
    `/api/reports/inventory-low-stock?${params.toString()}`,
    { accessToken: getAccessToken() },
  )

  return response.data!
}

export function getLowStockExportUrl(filters: LowStockFilters): string {
  const params = new URLSearchParams()

  if (filters.branchId) params.set('branchId', filters.branchId)
  if (filters.categoryId) params.set('categoryId', filters.categoryId)
  if (filters.search.trim()) params.set('search', filters.search.trim())

  const token = getAccessToken()
  if (token) params.set('access_token', token)

  return `${apiBaseUrl.replace(/\/$/, '')}/api/reports/inventory-low-stock/export?${params.toString()}`
}

// ── Purchases ──────────────────────────────────────────────────────────────

export async function getPurchaseReport(filters: PurchaseReportFilters): Promise<PurchaseReport> {
  const params = buildDateRangeParams(filters)

  if (filters.supplierId) params.set('supplierId', filters.supplierId)
  if (filters.status) params.set('status', filters.status)
  if (filters.branchId) params.set('branchId', filters.branchId)

  const response = await httpClient<PurchaseReport>(`/api/reports/purchases?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export function getPurchaseExportUrl(filters: PurchaseReportFilters): string {
  const params = buildDateRangeParams(filters)

  if (filters.supplierId) params.set('supplierId', filters.supplierId)
  if (filters.status) params.set('status', filters.status)
  if (filters.branchId) params.set('branchId', filters.branchId)

  const token = getAccessToken()
  if (token) params.set('access_token', token)

  return `${apiBaseUrl.replace(/\/$/, '')}/api/reports/purchases/export?${params.toString()}`
}

// ── Helpers ────────────────────────────────────────────────────────────────

function buildDateRangeParams(filters: { dateFrom: string; dateTo: string; page: number; pageSize: number }) {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  })

  const dateFrom = filters.dateFrom ? new Date(`${filters.dateFrom}T00:00:00`).toISOString() : null
  const dateTo = filters.dateTo ? new Date(`${filters.dateTo}T23:59:59.999`).toISOString() : null

  if (dateFrom) params.set('dateFrom', dateFrom)
  if (dateTo) params.set('dateTo', dateTo)

  return params
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
