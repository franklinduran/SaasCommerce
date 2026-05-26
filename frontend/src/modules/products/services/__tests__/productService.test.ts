import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  activateProduct,
  createProduct,
  deactivateProduct,
  getCategories,
  getProducts,
  updateProduct,
} from '@/modules/products/services/productService'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'product-token' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('productService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'product-result' } })
  })

  it('builds product list filters and loads categories', async () => {
    await getProducts({
      categoryId: ' cat-1 ',
      isActive: 'true',
      page: 3,
      pageSize: 15,
      productType: 'Inventory',
      query: ' cafe ',
      sortBy: 'name',
      sortDirection: 'asc',
    })
    await getCategories()

    expect(httpClient).toHaveBeenNthCalledWith(
      1,
      '/api/catalog/products?page=3&pageSize=15&query=cafe&productType=Inventory&isActive=true&categoryId=cat-1&sortBy=name&sortDirection=asc',
      { accessToken: 'product-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/catalog/categories', { accessToken: 'product-token' })
  })

  it('sends product mutations', async () => {
    await createProduct({ name: 'Coffee', price: 10, productType: 'Inventory', sku: 'COF' })
    await updateProduct('product-1', { name: 'Coffee XL', price: 12 })
    await activateProduct('product-1')
    await deactivateProduct('product-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/catalog/products', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/catalog/products/product-1', expect.objectContaining({ method: 'PUT' }))
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/catalog/products/product-1/activate', expect.objectContaining({ method: 'PUT' }))
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/catalog/products/product-1/deactivate', expect.objectContaining({ method: 'PUT' }))
  })
})
