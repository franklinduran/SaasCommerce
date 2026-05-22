export type InventoryTransferStatus = 'Pending' | 'Completed' | 'Failed' | 'Cancelled'

export type InventoryTransferItem = {
  productId: string
  productName: string | null
  quantity: number
}

export type InventoryTransfer = {
  id: string
  businessId: string
  sourceBranchId: string
  sourceBranchName: string | null
  targetBranchId: string
  targetBranchName: string | null
  createdByUserId: string
  status: InventoryTransferStatus
  note: string | null
  failureReason: string | null
  createdAt: string
  updatedAt: string
  items: InventoryTransferItem[]
}

export type InventoryTransferListResponse = {
  items: InventoryTransfer[]
  total: number
  page: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type CreateInventoryTransferItemRequest = {
  productId: string
  quantity: number
}

export type CreateInventoryTransferRequest = {
  sourceBranchId: string
  targetBranchId: string
  items: CreateInventoryTransferItemRequest[]
  note?: string | null
}

export type InventoryTransferFilters = {
  sourceBranchId?: string
  targetBranchId?: string
  status?: string
  page: number
  pageSize: number
}

export type InventoryTransferRealtimeNotification = {
  transferId: string
  businessId: string
  sourceBranchId: string
  targetBranchId: string
  status: InventoryTransferStatus
  reason?: string | null
  occurredAt: string
}
