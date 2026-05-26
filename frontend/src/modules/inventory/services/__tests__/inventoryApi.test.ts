import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  createInventoryAdjustment,
  getInventory,
  getInventoryMovements,
  getInventoryProductDetail,
  getStock,
} from '@/modules/inventory/services/inventoryApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'inventory-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('inventoryApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'inventory-result' } })
  })

  it('builds inventory and movement query strings', async () => {
    await getInventory({ branchId: 'branch-1', includeInactive: false, page: 2, pageSize: 10, query: ' sku ' } as any)
    await getStock({ branchId: 'branch-1', page: 1, pageSize: 20, query: '' } as any)
    await getInventoryMovements({ branchId: 'branch-1', movementType: 'Adjustment', page: 1, pageSize: 10 } as any)
    await getInventoryProductDetail('product-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/inventory?branchId=branch-1&page=2&pageSize=10&query=+sku+', { accessToken: 'inventory-token' })
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/inventory/stock?branchId=branch-1&page=1&pageSize=20', { accessToken: 'inventory-token' })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/inventory/movements?branchId=branch-1&movementType=Adjustment&page=1&pageSize=10', { accessToken: 'inventory-token' })
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/inventory/products/product-1', { accessToken: 'inventory-token' })
  })

  it('creates inventory adjustments', async () => {
    await createInventoryAdjustment({ productId: 'product-1', quantityDelta: 3, reason: 'count' } as any)

    expect(httpClient).toHaveBeenCalledWith('/api/inventory/adjustments', expect.objectContaining({
      accessToken: 'inventory-token',
      method: 'POST',
    }))
  })
})
