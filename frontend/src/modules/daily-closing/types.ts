export type DailyClosingAlertType =
  | 'OpenCashSession'
  | 'MissingProductCost'
  | 'NegativeMargin'
  | 'HighExpenseRatio'
  | 'CreditSalesHigh'

export type DailyClosingStatus = 'Draft' | 'Closed'

export type DailyClosingAlert = {
  id: string
  alertType: DailyClosingAlertType
  message: string
  estimatedImpact: number | null
}

export type DailyClosingListItem = {
  id: string
  branchId: string
  branchName: string
  closingDate: string
  status: DailyClosingStatus
  totalSales: number
  estimatedNetProfit: number
  netMarginPercent: number
  alertCount: number
  createdAt: string
  closedAt: string | null
}

export type DailyClosingDetail = {
  id: string
  businessId: string
  branchId: string
  branchName: string
  closingDate: string
  status: DailyClosingStatus
  // Sales by payment method
  totalSales: number
  cashSales: number
  transferSales: number
  cardSales: number
  creditSales: number
  salesCount: number
  // Cash
  cashExpected: number
  cashCounted: number | null
  cashDifference: number | null
  // Expenses
  totalExpenses: number
  // Profitability
  totalCost: number
  grossProfit: number
  estimatedNetProfit: number
  grossMarginPercent: number
  netMarginPercent: number
  // Credits
  newCreditsAmount: number
  newCreditsCount: number
  creditPaymentsReceived: number
  // Metadata
  notes: string | null
  createdAt: string
  closedAt: string | null
  closedByUserId: string | null
  alerts: DailyClosingAlert[]
}

export type DailyClosingPreview = Omit<
  DailyClosingDetail,
  'id' | 'businessId' | 'cashCounted' | 'cashDifference' | 'notes' | 'createdAt' | 'closedAt' | 'closedByUserId' | 'status'
>

export type DailyClosingPagedResult = {
  items: DailyClosingListItem[]
  totalCount: number
  page: number
  pageSize: number
}
