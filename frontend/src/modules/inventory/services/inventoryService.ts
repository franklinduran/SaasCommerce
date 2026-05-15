import { useAuthStore } from '@/modules/auth/authStore'
import type {
  CreateInventoryAdjustmentRequest,
  InventoryMovementListResponse,
  StockListResponse,
} from '@/modules/inventory/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getStock(): Promise<StockListResponse> {
  const response = await httpClient<StockListResponse>('/api/inventory/stock?page=1&pageSize=20', {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getInventoryMovements(): Promise<InventoryMovementListResponse> {
  const response = await httpClient<InventoryMovementListResponse>(
    '/api/inventory/movements?page=1&pageSize=20',
    { accessToken: getAccessToken() },
  )

  return response.data!
}

export async function createInventoryAdjustment(request: CreateInventoryAdjustmentRequest) {
  const response = await httpClient('/api/inventory/adjustments', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'POST',
  })

  return response.data
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
