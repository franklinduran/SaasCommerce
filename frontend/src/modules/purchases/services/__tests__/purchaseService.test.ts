import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  cancelPurchase,
  createPurchase,
  getPurchaseDetail,
  getPurchases,
  receivePurchase,
} from '@/modules/purchases/services/purchaseService'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'purchase-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('purchaseService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'purchase-result' } })
  })

  it('builds purchase filters and loads details', async () => {
    await getPurchases({
      dateFrom: '2026-05-01',
      dateTo: '2026-05-25',
      page: 2,
      pageSize: 30,
      query: ' po ',
      sortBy: 'createdAt',
      sortDirection: 'desc',
      status: 'Received',
      supplierId: 'supplier-1',
    } as any)
    await getPurchaseDetail('purchase-1')

    expect(httpClient).toHaveBeenNthCalledWith(
      1,
      '/api/purchases?page=2&pageSize=30&supplierId=supplier-1&status=Received&query=po&dateFrom=2026-05-01&dateTo=2026-05-25&sortBy=createdAt&sortDirection=desc',
      { accessToken: 'purchase-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/purchases/purchase-1', { accessToken: 'purchase-token' })
  })

  it('sends purchase mutations', async () => {
    await createPurchase({ items: [], supplierId: 'supplier-1' } as any)
    await receivePurchase('purchase-1')
    await cancelPurchase('purchase-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/purchases', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/purchases/purchase-1/receive', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/purchases/purchase-1/cancel', expect.objectContaining({ method: 'POST' }))
  })
})
