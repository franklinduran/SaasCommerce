import { useAuthStore } from '@/modules/auth/authStore'
import type {
  CreateInventoryTransferRequest,
  InventoryTransfer,
  InventoryTransferFilters,
  InventoryTransferListResponse,
} from '@/modules/inventory-transfers/types'
import { httpClient } from '@/shared/services/httpClient'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

function toQueryString(filters: Record<string, boolean | number | string | null | undefined>): string {
  const params = new URLSearchParams()

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== '' && value !== false && value !== null && value !== undefined) {
      params.set(key, String(value))
    }
  })

  return params.toString()
}

export async function getInventoryTransfers(
  filters: InventoryTransferFilters,
): Promise<InventoryTransferListResponse> {
  const qs = toQueryString(filters as Record<string, boolean | number | string | null | undefined>)
  const querySuffix = qs ? `?${qs}` : ''
  const response = await httpClient<InventoryTransferListResponse>(
    `/api/inventory-transfers${querySuffix}`,
    { accessToken: getAccessToken() },
  )

  return response.data!
}

export async function getInventoryTransferById(transferId: string): Promise<InventoryTransfer> {
  const response = await httpClient<InventoryTransfer>(
    `/api/inventory-transfers/${transferId}`,
    { accessToken: getAccessToken() },
  )

  return response.data!
}

export async function createInventoryTransfer(
  request: CreateInventoryTransferRequest,
): Promise<InventoryTransfer> {
  const response = await httpClient<InventoryTransfer>('/api/inventory-transfers', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'POST',
  })

  return response.data!
}

export async function cancelInventoryTransfer(transferId: string): Promise<void> {
  await httpClient(`/api/inventory-transfers/${transferId}/cancel`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })
}
