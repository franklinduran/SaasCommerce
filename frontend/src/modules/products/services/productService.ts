import { useAuthStore } from '@/modules/auth/authStore'
import type {
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

  const response = await httpClient<ProductListResponse>(
    `/api/catalog/products?${params.toString()}`,
    { accessToken: token },
  )

  return response.data!
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

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
