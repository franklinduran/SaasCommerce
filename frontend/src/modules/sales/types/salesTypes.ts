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
  productId: string
  productName: string
  sku: string | null
  quantity: number
  unitPrice: number
  subtotal: number
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
