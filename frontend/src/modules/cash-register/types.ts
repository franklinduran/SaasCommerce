export type CashRegisterMovementItem = {
  id: string
  movementType: 'CashIn' | 'CashOut'
  amount: number
  reason: string
  createdAt: string
}

export type CashRegisterDetail = {
  id: string
  branchId: string
  userId: string
  status: 'Open' | 'Closed' | 'Cancelled'
  openingAmount: number
  openedAt: string
  closedAt: string | null
  countedAmount: number | null
  expectedCashAmount: number | null
  difference: number | null
  differenceType: 'Balanced' | 'Surplus' | 'Shortage' | null
  cashSalesTotal: number
  cardSalesTotal: number
  transferSalesTotal: number
  creditSalesTotal: number
  cashReturnsTotal: number
  manualCashIn: number
  manualCashOut: number
  notes: string | null
  closeNotes: string | null
  movements: CashRegisterMovementItem[]
}

export type OpenCashRegisterRequest = {
  branchId: string
  openingAmount: number
  notes?: string | null
}

export type RegisterMovementRequest = {
  type: 'CashIn' | 'CashOut'
  amount: number
  reason: string
}

export type CloseCashRegisterRequest = {
  countedAmount: number
  closeNotes?: string | null
}

export type OpenCashRegisterResponse = {
  cashRegisterId: string
  openedAt: string
}

export type CloseCashRegisterResponse = {
  cashRegisterId: string
  openingAmount: number
  cashSales: number
  cardSales: number
  transferSales: number
  creditSales: number
  cashReturns: number
  manualCashIn: number
  manualCashOut: number
  expectedCashAmount: number
  countedAmount: number
  difference: number
  differenceType: 'Balanced' | 'Surplus' | 'Shortage'
  closedAt: string
}

export type DailyCashRegisterSummaryItem = {
  cashRegisterId: string
  branchId: string
  userId: string
  openingAmount: number
  status: 'Open' | 'Closed' | 'Cancelled'
  openedAt: string
  closedAt: string | null
  expectedCashAmount: number | null
  countedAmount: number | null
  difference: number | null
  differenceType: string | null
  cashSalesTotal: number
  cardSalesTotal: number
  transferSalesTotal: number
  creditSalesTotal: number
  cashReturnsTotal: number
  manualCashIn: number
  manualCashOut: number
}

export type CashRegisterHistoryResponse = {
  items: DailyCashRegisterSummaryItem[]
  page: number
  pageSize: number
  totalCount: number
}

export type DailyCashRegisterSummary = {
  date: string
  openRegisters: number
  closedRegisters: number
  totalExpectedCash: number
  totalCountedCash: number
  totalDifference: number
  totalCashSales: number
  totalCardSales: number
  totalTransferSales: number
  totalCreditSales: number
  totalCashReturns: number
  registers: DailyCashRegisterSummaryItem[]
}
