export type SaleStatus = 'Received' | 'Processing' | 'Completed' | 'Failed' | 'Cancelled'

export type SalesFilters = {
  dateFrom: string
  dateTo: string
  page: number
  pageSize: number
  query: string
  status: '' | SaleStatus
}

export type SaleListItem = {
  id: string
  code: string
  customerName: string | null
  status: SaleStatus
  paymentMethod: string
  total: number
  createdAt: string
}

export type SaleDetail = SaleListItem & {
  branchName: string | null
  failureReason: string | null
  cancellationReason: string | null
  items: SaleDetailItem[]
}

export type SaleDetailItem = {
  saleItemId: string
  productId: string
  productName: string
  sku: string | null
  quantity: number
  unitPrice: number
  subtotal: number
}

export type SaleReturnStatus = 'Requested' | 'Approved' | 'Failed'

export type SaleReturnItem = {
  id: string
  saleItemId: string
  productId: string
  productName: string
  sku: string | null
  quantity: number
  unitPrice: number
  lineTotal: number
}

export type CreditNote = {
  id: string
  saleId: string
  saleReturnId: string
  customerId: string | null
  code: string
  total: number
  createdAt: string
}

export type SaleReturn = {
  id: string
  saleId: string
  status: SaleReturnStatus
  reason: string
  total: number
  items: SaleReturnItem[]
  creditNote: CreditNote | null
  requestedAt: string
  approvedAt: string | null
  failedAt: string | null
  failureReason: string | null
}

export type CreateSaleReturnItemInput = {
  saleItemId: string
  quantity: number
}

export type CreateSaleReturnInput = {
  reason: string
  items: CreateSaleReturnItemInput[]
}

export type SaleListResponse = {
  items: SaleListItem[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type SaleStatusChangedNotification = {
  saleId: string
  businessId: string
  status: SaleStatus
}

export type SaleReturnChangedNotification = {
  saleId: string
  saleReturnId: string
  businessId: string
  status: SaleReturnStatus
}
