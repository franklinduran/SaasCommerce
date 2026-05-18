export type InvoiceStatus = 'Draft' | 'Issued' | 'Cancelled'

export type Invoice = {
  id: string
  invoiceId: string
  businessId: string
  branchId: string
  saleId: string
  customerId: string | null
  invoiceNumber: string
  subtotal: number
  discountTotal: number
  taxTotal: number
  total: number
  status: InvoiceStatus
  createdAt: string
  updatedAt: string
  cancelledAt: string | null
}

export type InvoiceListResponse = {
  items: Invoice[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type InvoiceFilters = {
  dateFrom: string
  dateTo: string
  page: number
  pageSize: number
  query: string
  status: string
}

export type InvoiceRealtimeNotification = {
  businessId: string
  invoiceId: string
  saleId: string
}
