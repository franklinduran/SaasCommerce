export type StockItem = {
  id: string
  businessId: string
  branchId: string
  productId: string
  productName: string
  sku: string
  barcode: string | null
  unitOfMeasure: string
  quantity: number
  minimumStock: number | null
  reorderPoint: number | null
  isLowStock: boolean
}

export type InventoryMovement = {
  id: string
  businessId: string
  branchId: string
  productId: string
  previousStock: number
  newStock: number
  quantity: number
  reason: string
  userId: string
  createdAt: string
}

export type StockListResponse = {
  items: StockItem[]
  page: number
  pageSize: number
  total: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type InventoryMovementListResponse = {
  items: InventoryMovement[]
  page: number
  pageSize: number
  total: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type CreateInventoryAdjustmentRequest = {
  productId: string
  quantity: number
  reason: string
}

export type StockFilters = {
  search: string
  lowStockOnly: boolean
  productType: string
  categoryId: string
  page: number
  pageSize: number
  sortBy: string
  sortDirection: string
}

export type MovementFilters = {
  productId: string
  movementType: string
  dateFrom: string
  dateTo: string
  page: number
  pageSize: number
  sortBy: string
  sortDirection: string
}
