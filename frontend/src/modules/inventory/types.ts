export type StockItem = {
  id: string
  businessId: string
  branchId: string
  productId: string
  quantity: number
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
}

export type InventoryMovementListResponse = {
  items: InventoryMovement[]
  page: number
  pageSize: number
  total: number
}

export type CreateInventoryAdjustmentRequest = {
  productId: string
  quantity: number
  reason: string
}
