export type PurchaseStatus = 'Draft' | 'Received' | 'Cancelled'

export type PurchaseItem = {
  id: string
  productId: string
  productName: string
  sku: string | null
  quantity: number
  unitCost: number
  subtotal: number
}

export type PurchaseMovement = {
  id: string
  productId: string
  previousStock: number
  newStock: number
  quantity: number
  reason: string
  createdAt: string
}

export type Purchase = {
  id: string
  purchaseId: string
  businessId: string
  branchId: string
  supplierId: string
  supplierName: string
  userId: string
  status: PurchaseStatus
  supplierInvoiceNumber: string | null
  purchaseDate: string
  notes: string | null
  total: number
  items: PurchaseItem[]
  movements: PurchaseMovement[]
  createdAt: string
  updatedAt: string
  receivedAt: string | null
  cancelledAt: string | null
  code: string
}

export type PurchaseListResponse = {
  items: Purchase[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  totalPurchased: number
}

export type PurchaseFilters = {
  supplierId: string
  status: string
  query: string
  dateFrom: string
  dateTo: string
  page: number
  pageSize: number
  sortBy: string
  sortDirection: string
}

export type CreatePurchaseRequest = {
  supplierId: string
  branchId: string | null
  items: CreatePurchaseItemRequest[]
  supplierInvoiceNumber: string | null
  purchaseDate: string | null
  notes: string | null
  receiveNow: boolean
}

export type CreatePurchaseItemRequest = {
  productId: string
  quantity: number
  unitCost: number
}

export type PurchaseRealtimeNotification = {
  businessId: string
  branchId: string
  purchaseId?: string
  productId?: string
}
