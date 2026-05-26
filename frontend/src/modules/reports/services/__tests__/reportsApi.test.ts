import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  getAccountsReceivableReport,
  getArExportUrl,
  getInvoiceExportUrl,
  getInvoiceReport,
  getLowStockExportUrl,
  getLowStockReport,
  getPurchaseExportUrl,
  getPurchaseReport,
  getSalesExportUrl,
  getSalesReport,
} from '@/modules/reports/services/reportsApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'report-token' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('reportsApi', () => {
  beforeEach(() => {
    vi.mocked(httpClient).mockResolvedValue({ data: { items: [] } })
  })

  it('requests every report endpoint with filters', async () => {
    await getSalesReport({ branchId: 'b1', dateFrom: '2026-05-01', dateTo: '2026-05-25', page: 1, pageSize: 10, paymentMethod: 'Cash', search: ' sale ', status: 'Completed' })
    await getInvoiceReport({ customerId: 'c1', dateFrom: '2026-05-01', dateTo: '2026-05-25', page: 2, pageSize: 20, search: ' inv ', status: 'Issued' })
    await getAccountsReceivableReport({ customerId: 'c1', dateFrom: '2026-05-01', dateTo: '2026-05-25', page: 1, pageSize: 50, status: 'Overdue' })
    await getLowStockReport({ branchId: 'b1', categoryId: 'cat1', page: 1, pageSize: 25, search: ' cafe ' })
    await getPurchaseReport({ branchId: 'b1', dateFrom: '2026-05-01', dateTo: '2026-05-25', page: 1, pageSize: 10, status: 'Received', supplierId: 's1' })

    expect(httpClient).toHaveBeenCalledWith(expect.stringContaining('/api/reports/sales?'), { accessToken: 'report-token' })
    expect(httpClient).toHaveBeenCalledWith(expect.stringContaining('/api/reports/invoices?'), { accessToken: 'report-token' })
    expect(httpClient).toHaveBeenCalledWith(expect.stringContaining('/api/reports/accounts-receivable?'), { accessToken: 'report-token' })
    expect(httpClient).toHaveBeenCalledWith(expect.stringContaining('/api/reports/inventory-low-stock?'), { accessToken: 'report-token' })
    expect(httpClient).toHaveBeenCalledWith(expect.stringContaining('/api/reports/purchases?'), { accessToken: 'report-token' })
  })

  it('builds authenticated export URLs', () => {
    const base = { dateFrom: '2026-05-01', dateTo: '2026-05-25', page: 1, pageSize: 10 }

    expect(getSalesExportUrl({ ...base, branchId: 'b1', paymentMethod: 'Cash', search: 'sale', status: 'Completed' })).toContain('/api/reports/sales/export?')
    expect(getInvoiceExportUrl({ ...base, customerId: 'c1', search: 'inv', status: 'Issued' })).toContain('/api/reports/invoices/export?')
    expect(getArExportUrl({ ...base, customerId: 'c1', status: 'Overdue' })).toContain('/api/reports/accounts-receivable/export?')
    expect(getLowStockExportUrl({ branchId: 'b1', categoryId: 'cat1', page: 1, pageSize: 10, search: 'cafe' })).toContain('/api/reports/inventory-low-stock/export?')
    expect(getPurchaseExportUrl({ ...base, branchId: 'b1', status: 'Received', supplierId: 's1' })).toContain('/api/reports/purchases/export?')
    expect(getPurchaseExportUrl({ ...base, branchId: '', status: '', supplierId: '' })).toContain('access_token=report-token')
  })
})
