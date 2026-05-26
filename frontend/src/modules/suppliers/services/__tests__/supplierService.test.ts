import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  createSupplier,
  getSuppliers,
  updateSupplier,
} from '@/modules/suppliers/services/supplierService'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'supplier-token' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('supplierService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'supplier-result' } })
  })

  it('builds supplier filters', async () => {
    await getSuppliers({
      isActive: 'false',
      page: 2,
      pageSize: 20,
      query: ' acme ',
      sortBy: 'name',
      sortDirection: 'desc',
    })

    expect(httpClient).toHaveBeenCalledWith(
      '/api/suppliers?page=2&pageSize=20&query=acme&isActive=false&sortBy=name&sortDirection=desc',
      { accessToken: 'supplier-token' },
    )
  })

  it('sends supplier mutations', async () => {
    await createSupplier({ email: 'sales@acme.test', name: 'Acme', phone: '8090000000' })
    await updateSupplier('supplier-1', { email: null, isActive: false, name: 'Acme SRL', phone: null })

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/suppliers', expect.objectContaining({
      accessToken: 'supplier-token',
      method: 'POST',
    }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/suppliers/supplier-1', expect.objectContaining({
      accessToken: 'supplier-token',
      method: 'PUT',
    }))
  })
})
