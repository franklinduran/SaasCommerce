export type ProfitabilitySummary = {
  dateFrom: string
  dateTo: string
  totalSales: number
  totalCost: number
  grossProfit: number
  operatingExpenses: number
  estimatedNetProfit: number
  grossMarginPercent: number
  netMarginPercent: number
  salesCount: number
  warningCount: number
}

export type ProductProfitability = {
  productId: string
  productName: string
  sku: string
  totalQuantity: number
  totalSales: number
  totalCost: number
  grossProfit: number
  marginPercent: number
  hasMissingCost: boolean
  categoryId: string | null
  categoryName: string | null
}

export type BranchProfitability = {
  branchId: string
  branchName: string
  totalSales: number
  totalCost: number
  grossProfit: number
  operatingExpenses: number
  estimatedNetProfit: number
  netMarginPercent: number
  salesCount: number
}

export type ProfitabilityAlert = {
  alertType: 'MissingCost' | 'NegativeMargin' | 'HighVolumeLowMargin' | 'HighExpenses'
  message: string
  branchId: string | null
  branchName: string | null
  productId: string | null
  productName: string | null
  estimatedImpact: number | null
}

export type ProfitabilityFilters = {
  from: string
  to: string
  branchId?: string
}
