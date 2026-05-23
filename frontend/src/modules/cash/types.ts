export type CashMovement = {
  id: string
  cashSessionId: string
  userId: string
  type: 'CashIn' | 'CashOut'
  amount: number
  description: string
  createdAt: string
}

export type CashSession = {
  id: string
  businessId: string
  branchId: string
  userId: string
  status: 'Open' | 'Closed'
  openingBalance: number
  closingBalance: number | null
  systemBalance: number
  notes: string | null
  openedAt: string
  closedAt: string | null
  updatedAt: string
  movements: CashMovement[]
}

export type CashSessionListItem = {
  id: string
  branchId: string
  userId: string
  status: 'Open' | 'Closed'
  openingBalance: number
  closingBalance: number | null
  systemBalance: number
  openedAt: string
  closedAt: string | null
}

export type CashSessionsListResult = {
  items: CashSessionListItem[]
  totalCount: number
  page: number
  pageSize: number
}

export type CashClosingResult = {
  cashSessionId: string
  openingBalance: number
  systemBalance: number
  closingBalance: number
  difference: number
  outcome: 'Balanced' | 'Surplus' | 'Shortage'
}

export type OpenCashSessionRequest = {
  openingBalance: number
  notes?: string | null
}

export type CloseCashSessionRequest = {
  closingBalance: number
}

export type RegisterCashMovementRequest = {
  type: 'CashIn' | 'CashOut'
  amount: number
  description: string
}
