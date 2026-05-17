export type StockItem = {
  id: string
  businessId: string
  branchId: string
  branchName: string | null
  productId: string
  productName: string
  sku: string
  barcode: string | null
  unitOfMeasure: string
  quantity: number
  minimumStock: number | null
  reorderPoint: number | null
  isLowStock: boolean
  isOutOfStock: boolean
  status: StockStatus
  lastUpdatedAt: string | null
}

export type StockStatus = 'Available' | 'LowStock' | 'OutOfStock'

export type InventoryMovement = {
  id: string
  businessId: string
  branchId: string
  branchName: string | null
  productId: string
  productName: string | null
  previousStock: number
  newStock: number
  quantity: number
  reason: string
  saleId: string | null
  purchaseId: string | null
  note: string | null
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

export type InventoryProductDetail = {
  productId: string
  productName: string
  sku: string
  barcode: string | null
  unitOfMeasure: string
  minimumStock: number | null
  reorderPoint: number | null
  branches: InventoryBranchStock[]
  recentMovements: InventoryMovement[]
  alerts: InventoryAlert[]
}

export type InventoryBranchStock = {
  branchId: string
  branchName: string
  currentStock: number
  minimumStock: number | null
  isLowStock: boolean
  isOutOfStock: boolean
  status: StockStatus
  lastUpdatedAt: string | null
}

export type InventoryAlert = {
  branchId: string
  branchName: string
  productId: string
  productName: string
  currentStock: number
  minimumStock: number
  severity: string
  detectedAt: string
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
  branchId?: string | null
  note?: string | null
}

export type StockFilters = {
  productId: string
  branchId: string
  search: string
  lowStockOnly: boolean
  outOfStockOnly: boolean
  productType: string
  categoryId: string
  page: number
  pageSize: number
  sortBy: string
  sortDirection: string
}

export type InventoryRealtimeNotification = {
  businessId: string
  branchId: string
  productId?: string | null
  saleId?: string | null
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
