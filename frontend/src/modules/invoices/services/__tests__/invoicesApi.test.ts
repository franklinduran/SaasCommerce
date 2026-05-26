import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  cancelInvoice,
  getInvoice,
  getInvoiceBySale,
  getInvoices,
} from '@/modules/invoices/services/invoicesApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'invoice-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('invoicesApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'invoice-result' } })
  })

  it('builds invoice filters and loads invoice details', async () => {
    await getInvoices({ dateFrom: '2026-05-01', dateTo: '2026-05-25', page: 2, pageSize: 20, query: ' ri ', status: 'Issued' } as any)
    await getInvoices({ dateFrom: '', dateTo: '', page: 1, pageSize: 10, query: '', status: '' } as any)
    await getInvoice('invoice-1')
    await getInvoiceBySale('sale-1')

    const firstUrl = vi.mocked(httpClient).mock.calls[0][0]
    expect(firstUrl).toContain('/api/invoices?page=2&pageSize=20&status=Issued&query=ri&dateFrom=')
    expect(firstUrl).toContain('&dateTo=')
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/invoices?page=1&pageSize=10', { accessToken: 'invoice-token' })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/invoices/invoice-1', { accessToken: 'invoice-token' })
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/sales/sale-1/invoice', { accessToken: 'invoice-token' })
  })

  it('cancels invoices', async () => {
    await cancelInvoice('invoice-1')

    expect(httpClient).toHaveBeenCalledWith('/api/invoices/invoice-1/cancel', {
      accessToken: 'invoice-token',
      method: 'POST',
    })
  })
})
