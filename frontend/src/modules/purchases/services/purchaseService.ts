import { useAuthStore } from '@/modules/auth/authStore'
import type { CreatePurchaseRequest, Purchase, PurchaseFilters, PurchaseListResponse } from '@/modules/purchases/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getPurchases(filters: PurchaseFilters): Promise<PurchaseListResponse> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  })

  if (filters.supplierId) {
    params.set('supplierId', filters.supplierId)
  }

  if (filters.status) {
    params.set('status', filters.status)
  }

  if (filters.query.trim()) {
    params.set('query', filters.query.trim())
  }

  if (filters.dateFrom) {
    params.set('dateFrom', filters.dateFrom)
  }

  if (filters.dateTo) {
    params.set('dateTo', filters.dateTo)
  }

  if (filters.sortBy) {
    params.set('sortBy', filters.sortBy)
  }

  if (filters.sortDirection) {
    params.set('sortDirection', filters.sortDirection)
  }

  const response = await httpClient<PurchaseListResponse>(`/api/purchases?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getPurchaseDetail(purchaseId: string): Promise<Purchase> {
  const response = await httpClient<Purchase>(`/api/purchases/${purchaseId}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function createPurchase(request: CreatePurchaseRequest) {
  const response = await httpClient<Purchase>('/api/purchases', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'POST',
  })

  return response.data
}

export async function receivePurchase(purchaseId: string) {
  const response = await httpClient<Purchase>(`/api/purchases/${purchaseId}/receive`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })

  return response.data
}

export async function cancelPurchase(purchaseId: string) {
  const response = await httpClient<Purchase>(`/api/purchases/${purchaseId}/cancel`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })

  return response.data
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
