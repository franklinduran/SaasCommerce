import { useAuthStore } from '@/modules/auth/authStore'
import type {
  SupplierFilters,
  SupplierListResponse,
  SupplierRequest,
  UpdateSupplierRequest,
} from '@/modules/suppliers/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getSuppliers(filters: SupplierFilters): Promise<SupplierListResponse> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  })

  if (filters.query.trim()) {
    params.set('query', filters.query.trim())
  }

  if (filters.isActive) {
    params.set('isActive', filters.isActive)
  }

  if (filters.sortBy) {
    params.set('sortBy', filters.sortBy)
  }

  if (filters.sortDirection) {
    params.set('sortDirection', filters.sortDirection)
  }

  const response = await httpClient<SupplierListResponse>(`/api/suppliers?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function createSupplier(request: SupplierRequest) {
  const response = await httpClient('/api/suppliers', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'POST',
  })

  return response.data
}

export async function updateSupplier(supplierId: string, request: UpdateSupplierRequest) {
  const response = await httpClient(`/api/suppliers/${supplierId}`, {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })

  return response.data
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
