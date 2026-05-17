export type CustomerCreditStatus = 'Active' | 'Blocked' | 'Closed'

export type Customer = {
  id: string
  businessId: string
  fullName: string
  phone: string | null
  email: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
  deactivatedAt: string | null
  currentBalance: number
  creditLimit: number
  creditStatus: CustomerCreditStatus
}

export type CustomerListResponse = {
  items: Customer[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type CustomerFilters = {
  isActive: string
  page: number
  pageSize: number
  query: string
  sortBy: string
  sortDirection: string
}

export type CustomerUpsertRequest = {
  fullName: string
  phone?: string | null
  email?: string | null
  isActive?: boolean
}

export type CustomerCreditSummary = {
  businessId: string
  customerId: string
  customerName: string
  creditLimit: number
  currentBalance: number
  status: CustomerCreditStatus
  createdAt: string
  updatedAt: string
}

export type CustomerCreditMovement = {
  id: string
  businessId: string
  customerId: string
  saleId: string | null
  paymentId: string | null
  type: 'Debit' | 'Payment' | 'Adjustment' | 'Cancellation'
  amount: number
  previousBalance: number
  newBalance: number
  note: string | null
  createdAt: string
  createdBy: string | null
}

export type CustomerCreditMovementListResponse = {
  items: CustomerCreditMovement[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type RegisterCustomerPaymentRequest = {
  customerId: string
  amount: number
  note?: string | null
}

export type RegisterCustomerPaymentResponse = {
  customerId: string
  paymentId: string
  amount: number
  newBalance: number
}

export type CustomerCreditRealtimeNotification = {
  businessId: string
  customerId: string
  amount?: number
  newBalance?: number
  status?: CustomerCreditStatus
}
