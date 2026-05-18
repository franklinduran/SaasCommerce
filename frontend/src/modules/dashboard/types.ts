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
  saleNumber: string
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
  purchaseNumber: string
  supplierName: string | null
  status: string
  total: number
  createdAt: string
}

export type DashboardSummary = {
  salesToday: DashboardSalesToday
  invoicesToday: DashboardInvoicesToday
  receivables: DashboardReceivables
  lowStock: DashboardLowStock
  recentSales: DashboardRecentSale[]
  recentInvoices: DashboardRecentInvoice[]
  recentPurchases: DashboardRecentPurchase[]
}
