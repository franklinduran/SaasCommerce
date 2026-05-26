import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  cancelInventoryTransfer,
  createInventoryTransfer,
  getInventoryTransferById,
  getInventoryTransfers,
} from '@/modules/inventory-transfers/services/inventoryTransfersApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'transfer-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('inventoryTransfersApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'transfer-result' } })
  })

  it('builds transfer filters and loads details', async () => {
    await getInventoryTransfers({ fromBranchId: 'from-1', page: 2, pageSize: 15, status: 'Pending', toBranchId: 'to-1' } as any)
    await getInventoryTransfers({ fromBranchId: '', includeCancelled: false, page: 1, pageSize: 10 } as any)
    await getInventoryTransferById('transfer-1')

    expect(httpClient).toHaveBeenNthCalledWith(
      1,
      '/api/inventory-transfers?fromBranchId=from-1&page=2&pageSize=15&status=Pending&toBranchId=to-1',
      { accessToken: 'transfer-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/inventory-transfers?page=1&pageSize=10', { accessToken: 'transfer-token' })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/inventory-transfers/transfer-1', { accessToken: 'transfer-token' })
  })

  it('sends transfer mutations', async () => {
    await createInventoryTransfer({ fromBranchId: 'from-1', items: [], toBranchId: 'to-1' } as any)
    await cancelInventoryTransfer('transfer-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/inventory-transfers', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/inventory-transfers/transfer-1/cancel', expect.objectContaining({ method: 'POST' }))
  })
})
