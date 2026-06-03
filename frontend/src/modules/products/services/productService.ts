import { useAuthStore } from '@/modules/auth/authStore'
import type {
  Category,
  CreateProductRequest,
  ProductFilters,
  ProductListResponse,
  UpdateProductRequest,
} from '@/modules/products/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getProducts(filters: ProductFilters): Promise<ProductListResponse> {
  const token = getAccessToken()
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  })

  if (filters.query.trim().length > 0) {
    params.set('query', filters.query.trim())
  }

  if (filters.productType) {
    params.set('productType', filters.productType)
  }

  if (filters.isActive) {
    params.set('isActive', filters.isActive)
  }

  if (filters.categoryId.trim().length > 0) {
    params.set('categoryId', filters.categoryId.trim())
  }

  if (filters.sortBy) {
    params.set('sortBy', filters.sortBy)
  }

  if (filters.sortDirection) {
    params.set('sortDirection', filters.sortDirection)
  }

  const response = await httpClient<ProductListResponse>(
    `/api/catalog/products?${params.toString()}`,
    { accessToken: token },
  )

  return response.data!
}

export async function getCategories(): Promise<Category[]> {
  const response = await httpClient<Category[]>('/api/catalog/categories', {
    accessToken: getAccessToken(),
  })

  return response.data ?? []
}

export async function createProduct(request: CreateProductRequest) {
  const response = await httpClient('/api/catalog/products', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'POST',
  })

  return response.data
}

export async function updateProduct(productId: string, request: UpdateProductRequest) {
  const response = await httpClient(`/api/catalog/products/${productId}`, {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })

  return response.data
}

export async function activateProduct(productId: string) {
  const response = await httpClient(`/api/catalog/products/${productId}/activate`, {
    accessToken: getAccessToken(),
    method: 'PUT',
  })

  return response.data
}

export async function deactivateProduct(productId: string) {
  const response = await httpClient(`/api/catalog/products/${productId}/deactivate`, {
    accessToken: getAccessToken(),
    method: 'PUT',
  })

  return response.data
}

export async function uploadProductImage(productId: string, file: File): Promise<void> {
  const token = getAccessToken()
  const formData = new FormData()
  formData.append('file', file)

  const response = await fetch(`/api/catalog/products/${productId}/image`, {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: formData,
  })

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Error al subir la imagen.' }))
    throw new Error(error?.error?.message ?? error?.message ?? 'Error al subir la imagen.')
  }
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
