// ── Dashboard summary ──────────────────────────────────────────────────────

export type DashboardSalesToday = {
  count: number
  totalAmount: number
}

export type DashboardInvoicesToday = {
  count: number
  totalAmount: number
}

export type DashboardReceivables = {
  customerCount: number
  totalPending: number
}

export type DashboardLowStock = {
  productCount: number
}

export type DashboardRecentSale = {
  saleId: string
  code: string
  status: string
  paymentMethod: string
  total: number
  createdAt: string
}

export type DashboardRecentInvoice = {
  invoiceId: string
  invoiceNumber: string
  status: string
  total: number
  createdAt: string
}

export type DashboardRecentPurchase = {
  purchaseId: string
  supplierName: string | null
  status: string
  total: number
  createdAt: string
}

export type DashboardDailySalesPoint = {
  /** ISO date string, e.g. "2026-05-20" */
  date: string
  totalSales: number
  saleCount: number
}

export type DashboardDailyPurchasesPoint = {
  /** ISO date string, e.g. "2026-05-20" */
  date: string
  totalPurchases: number
  purchaseCount: number
}

export type DashboardPaymentMethodTotal = {
  method: string
  count: number
  total: number
}

export type DashboardSaleStatusBreakdown = {
  completed: number
  cancelled: number
  pending: number
  failed: number
  other: number
}

export type DashboardSummary = {
  salesToday: DashboardSalesToday
  invoicesToday: DashboardInvoicesToday
  receivables: DashboardReceivables
  lowStock: DashboardLowStock
  recentSales: DashboardRecentSale[]
  recentInvoices: DashboardRecentInvoice[]
  recentPurchases: DashboardRecentPurchase[]
  /** Daily aggregated sales for the last 30 days (server-computed) */
  dailySales: DashboardDailySalesPoint[]
  /** Daily aggregated completed purchases for the last 30 days */
  dailyPurchases: DashboardDailyPurchasesPoint[]
  /** Payment method totals for the last 30 days (completed sales) */
  paymentMethodTotals: DashboardPaymentMethodTotal[]
  /** Sale status breakdown for the last 30 days */
  saleStatusBreakdown: DashboardSaleStatusBreakdown
}

// ── Chart / filter types ───────────────────────────────────────────────────

export type DateRangeFilter = '7d' | '14d' | '30d'

export type ChartDataPoint = {
  label: string   // e.g. "Lun", "Mar" or "01/06"
  ventas: number
  facturas: number
  gastos: number
}
