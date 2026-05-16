import { useAuthStore } from '@/modules/auth/authStore'
import type {
  CreateInventoryAdjustmentRequest,
  InventoryMovementListResponse,
  MovementFilters,
  StockListResponse,
  StockFilters,
} from '@/modules/inventory/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getStock(filters: StockFilters): Promise<StockListResponse> {
  const response = await httpClient<StockListResponse>(`/api/inventory/stock?${toQueryString(filters)}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getInventoryMovements(filters: MovementFilters): Promise<InventoryMovementListResponse> {
  const response = await httpClient<InventoryMovementListResponse>(
    `/api/inventory/movements?${toQueryString(filters)}`,
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

function toQueryString(filters: Record<string, boolean | number | string>): string {
  const params = new URLSearchParams()

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== '' && value !== false) {
      params.set(key, String(value))
    }
  })

  return params.toString()
}
