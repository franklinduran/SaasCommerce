import type { Product, ProductListResponse } from '@/modules/products/types'

export type POSProduct = Product
export type POSProductListResponse = ProductListResponse

export type POSCartItem = {
  productId: string
  name: string
  sku: string
  unitPrice: number
  quantity: number
}

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

export type CurrentBranchForPOS = {
  branchId: string
  businessId: string
  name: string
  address: string | null
  phone: string | null
}

export type PaymentMethod = 'Cash'

export type CreateSaleItemRequest = {
  productId: string
  quantity: number
}

export type CreateSaleRequest = {
  branchId: string
  customerId?: string | null
  paymentMethod: PaymentMethod
  items: CreateSaleItemRequest[]
}

export type SaleItemResponse = {
  productId: string
  productName: string
  sku: string | null
  quantity: number
  unitPrice: number
  lineTotal: number
}

export type SaleStatus =
  | 'Received'
  | 'Processing'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'

export type SaleResponse = {
  saleId: string
  businessId: string
  branchId: string
  userId: string
  customerId: string | null
  status: SaleStatus
  paymentMethod: PaymentMethod
  total: number
  items: SaleItemResponse[]
  createdAt: string
  updatedAt: string
  completedAt: string | null
  failedAt: string | null
  cancelledAt: string | null
  failureReason: string | null
  cancellationReason: string | null
}

export type SaleStatusChangedNotification = {
  eventId: string
  correlationId: string
  saleId: string
  businessId: string
  branchId: string
  userId: string
  status: SaleStatus
  reason: string | null
  createdAt: string
  version: number
}
