import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ReportsPage } from '@/modules/reports/ReportsPage'
import {
  getArExportUrl,
  getInvoiceExportUrl,
  getLowStockExportUrl,
  getPurchaseExportUrl,
  getSalesExportUrl,
} from '@/modules/reports/services/reportsApi'
import { useAuthStore } from '@/modules/auth/authStore'

vi.mock('@/shared/components/PermissionGate', () => ({
  PermissionGate: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}))

vi.mock('@/modules/reports/hooks/useReports', () => ({
  useAccountsReceivableReport: vi.fn(),
  useInvoiceReport: vi.fn(),
  useLowStockReport: vi.fn(),
  usePurchaseReport: vi.fn(),
  useSalesReport: vi.fn(),
}))

import {
  useAccountsReceivableReport,
  useInvoiceReport,
  useLowStockReport,
  usePurchaseReport,
  useSalesReport,
} from '@/modules/reports/hooks/useReports'

describe('ReportsPage', () => {
  beforeEach(() => {
    useAuthStore.getState().setSession({
      accessToken: 'token-123',
      expiresAt: '2026-05-25T23:59:00Z',
      refreshToken: 'refresh',
      user: {
        branchId: 'branch-1',
        businessId: 'business-1',
        email: 'admin@test.com',
        fullName: 'Admin',
        id: 'user-1',
        roles: ['Admin'],
      },
    })

    vi.mocked(useSalesReport).mockReturnValue(queryResult(salesReport()) as never)
    vi.mocked(useInvoiceReport).mockReturnValue(queryResult(invoiceReport()) as never)
    vi.mocked(useAccountsReceivableReport).mockReturnValue(queryResult(accountsReceivableReport()) as never)
    vi.mocked(useLowStockReport).mockReturnValue(queryResult(lowStockReport()) as never)
    vi.mocked(usePurchaseReport).mockReturnValue(queryResult(purchaseReport()) as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    useAuthStore.getState().clearSession()
  })

  it('renders sales report with summary, rows, export link and pagination', () => {
    render(<ReportsPage />)

    expect(screen.getByText('Reportes')).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Ventas' })).toBeTruthy()
    expect(screen.getByText('V-001')).toBeTruthy()
    expect(screen.getByText('Cliente Uno')).toBeTruthy()
    expect(screen.getByText('Pagina 1 de 2')).toBeTruthy()
    expect(screen.getByRole('link', { name: 'Exportar ventas' }).getAttribute('href'))
      .toContain('/api/reports/sales/export?')
  })

  it('updates sales filters, pagination and reset state', async () => {
    const user = userEvent.setup()
    render(<ReportsPage />)

    await user.type(screen.getByPlaceholderText('Cliente, codigo o venta'), 'maria')
    expect(vi.mocked(useSalesReport).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({
      page: 1,
      search: 'maria',
    }))

    await user.click(screen.getByRole('button', { name: 'Siguiente' }))
    expect(vi.mocked(useSalesReport).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({
      page: 2,
      search: 'maria',
    }))

    await user.click(screen.getByRole('button', { name: 'Limpiar' }))
    expect(vi.mocked(useSalesReport).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({
      page: 1,
      search: '',
    }))
  })

  it('renders every report tab with populated data', async () => {
    const user = userEvent.setup()
    render(<ReportsPage />)

    await user.click(screen.getByRole('tab', { name: 'Facturas' }))
    expect(screen.getByText('F-001')).toBeTruthy()
    expect(screen.getByRole('link', { name: 'Exportar facturas' })).toBeTruthy()

    await user.click(screen.getByRole('tab', { name: 'Cuentas por cobrar' }))
    expect(screen.getByText('Cliente Credito')).toBeTruthy()
    expect(screen.getByRole('link', { name: 'Exportar C/C' })).toBeTruthy()

    await user.click(screen.getByRole('tab', { name: 'Bajo stock' }))
    expect(screen.getByText('Cafe')).toBeTruthy()
    expect(screen.getByText('SKU-1')).toBeTruthy()

    await user.click(screen.getByRole('tab', { name: 'Compras' }))
    expect(screen.getByText('P-001')).toBeTruthy()
    expect(screen.getByText('Proveedor Uno')).toBeTruthy()
  })

  it('shows loading, error and empty report states', async () => {
    const user = userEvent.setup()
    vi.mocked(useSalesReport).mockReturnValue(queryResult(undefined, { isLoading: true }) as never)
    vi.mocked(useInvoiceReport).mockReturnValue(queryResult(undefined, { isError: true }) as never)
    vi.mocked(useAccountsReceivableReport).mockReturnValue(queryResult({
      ...accountsReceivableReport(),
      items: [],
      summary: { totalCustomers: 0, totalOverdue: 0, totalPending: 0 },
      totalItems: 0,
      totalPages: 1,
      hasNextPage: false,
    }) as never)

    render(<ReportsPage />)
    expect(screen.getByText('Cargando reporte...')).toBeTruthy()

    await user.click(screen.getByRole('tab', { name: 'Facturas' }))
    expect(screen.getByText('Error al cargar el reporte.')).toBeTruthy()

    await user.click(screen.getByRole('tab', { name: 'Cuentas por cobrar' }))
    expect(screen.getByText('Sin saldos pendientes para los filtros seleccionados.')).toBeTruthy()
  })

  it('builds export urls with filters and token', () => {
    const salesUrl = getSalesExportUrl({
      branchId: 'b1',
      dateFrom: '2026-05-01',
      dateTo: '2026-05-25',
      page: 1,
      pageSize: 25,
      paymentMethod: 'Cash',
      search: '  cliente  ',
      status: 'Completed',
    })

    expect(salesUrl).toContain('branchId=b1')
    expect(salesUrl).toContain('paymentMethod=Cash')
    expect(salesUrl).toContain('search=cliente')
    expect(salesUrl).toContain('access_token=token-123')

    expect(getInvoiceExportUrl(baseInvoiceFilters())).toContain('/api/reports/invoices/export?')
    expect(getArExportUrl(baseArFilters())).toContain('/api/reports/accounts-receivable/export?')
    expect(getLowStockExportUrl({ branchId: 'b1', categoryId: 'c1', page: 1, pageSize: 25, search: 'sku' }))
      .toContain('/api/reports/inventory-low-stock/export?')
    expect(getPurchaseExportUrl(basePurchaseFilters())).toContain('/api/reports/purchases/export?')
  })
})

function queryResult(data: unknown, overrides: Partial<{ isLoading: boolean; isError: boolean }> = {}) {
  return {
    data,
    isError: false,
    isLoading: false,
    ...overrides,
  }
}

function paged<T>(items: T[]) {
  return {
    hasNextPage: true,
    hasPreviousPage: false,
    items,
    page: 1,
    pageSize: 25,
    totalItems: items.length + 1,
    totalPages: 2,
  }
}

function salesReport() {
  return {
    ...paged([
      {
        branchName: 'Principal',
        createdAt: '2026-05-25T12:00:00Z',
        customerName: 'Cliente Uno',
        discountTotal: 0,
        paymentMethod: 'Cash',
        saleId: 'sale-1',
        saleNumber: 'V-001',
        status: 'Completed',
        subtotal: 100,
        taxTotal: 18,
        total: 118,
      },
      {
        branchName: null,
        createdAt: '2026-05-24T12:00:00Z',
        customerName: null,
        discountTotal: 0,
        paymentMethod: 'Card',
        saleId: 'sale-2',
        saleNumber: 'V-002',
        status: 'Mystery',
        subtotal: 50,
        taxTotal: 9,
        total: 59,
      },
    ]),
    summary: { averageAmount: 118, totalAmount: 118, totalCount: 1 },
  }
}

function invoiceReport() {
  return {
    ...paged([
      {
        createdAt: '2026-05-25T12:00:00Z',
        customerName: 'Cliente Factura',
        discountTotal: 0,
        invoiceId: 'invoice-1',
        invoiceNumber: 'F-001',
        status: 'Issued',
        subtotal: 100,
        taxTotal: 18,
        total: 118,
      },
      {
        createdAt: '2026-05-24T12:00:00Z',
        customerName: null,
        discountTotal: 0,
        invoiceId: 'invoice-2',
        invoiceNumber: 'F-002',
        status: 'Draft',
        subtotal: 10,
        taxTotal: 0,
        total: 10,
      },
      {
        createdAt: '2026-05-23T12:00:00Z',
        customerName: 'Cliente Cancelado',
        discountTotal: 0,
        invoiceId: 'invoice-3',
        invoiceNumber: 'F-003',
        status: 'Cancelled',
        subtotal: 15,
        taxTotal: 0,
        total: 15,
      },
    ]),
    summary: { totalAmount: 118, totalCount: 1 },
  }
}

function accountsReceivableReport() {
  return {
    ...paged([
      {
        creditAccountId: 'credit-1',
        creditLimit: 1000,
        currentBalance: 250,
        customerId: 'customer-1',
        customerName: 'Cliente Credito',
        lastMovementAt: '2026-05-24T12:00:00Z',
        overdueAmount: 50,
        status: 'Active',
      },
      {
        creditAccountId: 'credit-2',
        creditLimit: 500,
        currentBalance: 0,
        customerId: 'customer-2',
        customerName: 'Cliente Bloqueado',
        lastMovementAt: null,
        overdueAmount: 0,
        status: 'Blocked',
      },
    ]),
    summary: { totalCustomers: 1, totalOverdue: 50, totalPending: 250 },
  }
}

function lowStockReport() {
  return paged([
    {
      branchName: 'Principal',
      categoryName: 'Bebidas',
      currentStock: 2,
      lastMovementAt: null,
      minimumStock: 5,
      productId: 'product-1',
      productName: 'Cafe',
      sku: 'SKU-1',
      suggestedRestock: 3,
      unitCost: 25,
    },
    {
      branchName: null,
      categoryName: null,
      currentStock: 0,
      lastMovementAt: null,
      minimumStock: 2,
      productId: 'product-2',
      productName: 'Azucar',
      sku: 'SKU-2',
      suggestedRestock: 2,
      unitCost: 10,
    },
  ])
}

function purchaseReport() {
  return {
    ...paged([
      {
        createdAt: '2026-05-25T12:00:00Z',
        itemCount: 3,
        purchaseId: 'purchase-1',
        purchaseNumber: 'P-001',
        receivedAt: '2026-05-25T13:00:00Z',
        status: 'Received',
        supplierName: 'Proveedor Uno',
        total: 500,
      },
      {
        createdAt: '2026-05-24T12:00:00Z',
        itemCount: 1,
        purchaseId: 'purchase-2',
        purchaseNumber: 'P-002',
        receivedAt: null,
        status: 'Pending',
        supplierName: null,
        total: 25,
      },
    ]),
    summary: { totalAmount: 500, totalCount: 1 },
  }
}

function baseInvoiceFilters() {
  return { customerId: 'c1', dateFrom: '', dateTo: '', page: 1, pageSize: 25, search: 'f', status: 'Issued' }
}

function baseArFilters() {
  return { customerId: 'c1', dateFrom: '', dateTo: '', page: 1, pageSize: 25, status: 'Active' }
}

function basePurchaseFilters() {
  return { branchId: 'b1', dateFrom: '', dateTo: '', page: 1, pageSize: 25, status: 'Received', supplierId: 's1' }
}
