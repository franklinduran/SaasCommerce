// ── Shared ─────────────────────────────────────────────────────────────────

export type ReportDateFilters = {
  dateFrom: string
  dateTo: string
  page: number
  pageSize: number
}

// ── Sales Report ───────────────────────────────────────────────────────────

export type SalesReportItem = {
  saleId: string
  saleNumber: string
  branchName: string | null
  customerName: string | null
  status: string
  paymentMethod: string
  subtotal: number
  discountTotal: number
  taxTotal: number
  total: number
  createdAt: string
}

export type SalesReportSummary = {
  totalCount: number
  totalAmount: number
  averageAmount: number
}

export type SalesReport = {
  items: SalesReportItem[]
  summary: SalesReportSummary
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type SalesReportFilters = ReportDateFilters & {
  branchId: string
  status: string
  paymentMethod: string
  search: string
}

// ── Invoice Report ─────────────────────────────────────────────────────────

export type InvoiceReportItem = {
  invoiceId: string
  invoiceNumber: string
  customerName: string | null
  status: string
  subtotal: number
  discountTotal: number
  taxTotal: number
  total: number
  createdAt: string
}

export type InvoiceReportSummary = {
  totalCount: number
  totalAmount: number
}

export type InvoiceReport = {
  items: InvoiceReportItem[]
  summary: InvoiceReportSummary
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type InvoiceReportFilters = ReportDateFilters & {
  status: string
  customerId: string
  search: string
}

// ── Accounts Receivable Report ─────────────────────────────────────────────

export type AccountsReceivableItem = {
  creditAccountId: string
  customerId: string
  customerName: string
  currentBalance: number
  creditLimit: number
  overdueAmount: number
  lastMovementAt: string | null
  status: string
}

export type AccountsReceivableSummary = {
  totalPending: number
  totalCustomers: number
  totalOverdue: number
}

export type AccountsReceivableReport = {
  items: AccountsReceivableItem[]
  summary: AccountsReceivableSummary
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type AccountsReceivableFilters = ReportDateFilters & {
  customerId: string
  status: string
}

// ── Low Stock Report ───────────────────────────────────────────────────────

export type LowStockItem = {
  productId: string
  productName: string
  sku: string
  branchName: string | null
  categoryName: string | null
  currentStock: number
  minimumStock: number
  suggestedRestock: number
  unitCost: number
  lastMovementAt: string | null
}

export type LowStockReport = {
  items: LowStockItem[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type LowStockFilters = {
  branchId: string
  categoryId: string
  search: string
  page: number
  pageSize: number
}

// ── Purchase Report ────────────────────────────────────────────────────────

export type PurchaseReportItem = {
  purchaseId: string
  purchaseNumber: string
  supplierName: string | null
  status: string
  itemCount: number
  total: number
  receivedAt: string | null
  createdAt: string
}

export type PurchaseReportSummary = {
  totalCount: number
  totalAmount: number
}

export type PurchaseReport = {
  items: PurchaseReportItem[]
  summary: PurchaseReportSummary
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type PurchaseReportFilters = ReportDateFilters & {
  supplierId: string
  status: string
  branchId: string
}
