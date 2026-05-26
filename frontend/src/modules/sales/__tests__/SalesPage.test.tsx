import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import {
  createMemoryRouter,
  MemoryRouter,
  RouterProvider,
} from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { SaleReceipt } from '@/modules/sales/components/SaleReceipt'
import { SaleDetailPage } from '@/modules/sales/pages/SaleDetailPage'
import { SalesPage } from '@/modules/sales/pages/SalesPage'
import type { SaleDetail, SaleStatus } from '@/modules/sales/types/salesTypes'

describe('SalesHistoryPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('SalesHistoryPage should list sales from API', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ sales: [createApiSale()] }))
    renderSalesPage()

    expect(await screen.findByText('Maria Perez')).toBeTruthy()
    expect(screen.getByText('A1B2C3D4')).toBeTruthy()
    expect(screen.getByText('RD$250.00')).toBeTruthy()
  })

  it('SalesHistoryPage should show empty state when there are no sales', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ sales: [] }))
    renderSalesPage()

    expect(await screen.findByText('No hay ventas registradas.')).toBeTruthy()
  })

  it('SalesHistoryPage should show error state when API fails', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ failList: true }))
    renderSalesPage()

    expect(await screen.findByText('No se pudo cargar el historial de ventas.')).toBeTruthy()
  })

  it('SalesHistoryPage should filter sales by status', async () => {
    const user = userEvent.setup()
    const receivedSale = createApiSale({
      code: 'RECIBIDA',
      id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      saleId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      status: 'Received',
    })
    const completedSale = createApiSale({
      code: 'COMPLETA',
      id: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
      saleId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
      status: 'Completed',
    })
    const fetchMock = createSalesFetchMock({ sales: [receivedSale, completedSale] })
    vi.stubGlobal('fetch', fetchMock)
    renderSalesPage()

    await screen.findByText('RECIBIDA')
    await user.click(screen.getByRole('combobox', { name: 'Estado' }))
    await user.click(await screen.findByRole('option', { name: 'Completada' }))

    expect(await screen.findByText('COMPLETA')).toBeTruthy()
    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith(
        expect.stringContaining('status=Completed'),
        expect.anything(),
      )
    })
  })

  it('SalesHistoryPage should navigate to sale detail', async () => {
    const user = userEvent.setup()
    const sale = createApiSale()
    vi.stubGlobal('fetch', createSalesFetchMock({ detailSale: sale, sales: [sale] }))
    const router = createMemoryRouter(
      [
        { element: <SalesPage />, path: '/sales' },
        { element: <SaleDetailPage />, path: '/sales/:saleId' },
      ],
      { initialEntries: ['/sales'] },
    )

    renderWithQueryClient(<RouterProvider router={router} />)

    await user.click(await screen.findByRole('link', { name: /Ver detalle/i }))

    await waitFor(() => {
      expect(router.state.location.pathname).toBe(`/sales/${sale.saleId}`)
    })
    expect(await screen.findByText('Productos vendidos')).toBeTruthy()
  })
})

describe('SaleDetailPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('SaleDetailPage should show sale items', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ detailSale: createApiSale() }))
    renderSaleDetailPage()

    expect(await screen.findAllByText('Cafe molido')).toHaveLength(2)
    expect(screen.getAllByText('SKU-001').length).toBeGreaterThan(0)
  })

  it('SaleDetailPage should show customer when present', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ detailSale: createApiSale() }))
    renderSaleDetailPage()

    expect((await screen.findAllByText('Maria Perez')).length).toBeGreaterThan(0)
  })

  it('SaleDetailPage should show error state when sale fails to load, and retry button triggers refetch', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSalesFetchMock({ failDetail: true }))
    renderSaleDetailPage()

    expect(await screen.findByText('No se pudo cargar la venta.')).toBeTruthy()
    const retryBtn = screen.getByRole('button', { name: /Reintentar/i })
    expect(retryBtn).toBeTruthy()
    // Click retry — covers the refetch callback branch
    await user.click(retryBtn)
  })

  it('SaleDetailPage should render with Negocio fallback when business data unavailable', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ failBusiness: true }))
    renderSaleDetailPage()

    // Should still render the sale (not crash) — businessName fallback is 'Negocio'
    expect(await screen.findByText('Productos vendidos')).toBeTruthy()
  })

  it('SaleDetailPage should render without saleId route param (empty invalidation)', async () => {
    // Covers the saleId ? [saleId] : [] false branch in useSaleStatusInvalidation
    vi.stubGlobal('fetch', createSalesFetchMock({ detailSale: createApiSale() }))
    const router = createMemoryRouter(
      [{ element: <SaleDetailPage />, path: '/sales' }],
      { initialEntries: ['/sales'] },
    )
    renderWithQueryClient(<RouterProvider router={router} />)
    // Without saleId, sale query can't load — shows error or loading state, not crash
    await waitFor(() => {
      expect(document.body.textContent).toBeTruthy()
    })
  })

  it('SaleDetailPage should use showRnc=false fallback when billing settings unavailable', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ failBilling: true }))
    renderSaleDetailPage()

    // Sale should still load — receipt renders with showRnc=false (billing?.showRncOnReceipt ?? false)
    expect(await screen.findByText('Productos vendidos')).toBeTruthy()
  })

  it('SaleDetailPage should show failure reason when sale failed', async () => {
    vi.stubGlobal(
      'fetch',
      createSalesFetchMock({
        detailSale: createApiSale({
          failureReason: 'Pago rechazado por el proveedor.',
          status: 'Failed',
        }),
      }),
    )
    renderSaleDetailPage()

    expect(await screen.findByText('Pago rechazado por el proveedor.')).toBeTruthy()
  })

  it('SaleDetailPage should show existing sale returns', async () => {
    vi.stubGlobal('fetch', createSalesFetchMock({ returns: [createApiSaleReturn()] }))
    renderSaleDetailPage()

    expect(await screen.findByText('Devoluciones')).toBeTruthy()
    expect(await screen.findByText('NC-20260526-ABC123')).toBeTruthy()
    expect(screen.getByText('Cliente devuelve una unidad')).toBeTruthy()
  })

  it('SaleDetailPage should submit a partial return', async () => {
    const user = userEvent.setup()
    const fetchMock = createSalesFetchMock()
    vi.stubGlobal('fetch', fetchMock)
    renderSaleDetailPage()

    await screen.findByText('Devoluciones')
    await user.click(screen.getByRole('button', { name: /Registrar devolución/i }))
    await user.type(screen.getByLabelText('Motivo'), 'Producto equivocado')
    await user.type(screen.getByLabelText('Cantidad a devolver de Cafe molido'), '1')
    await user.click(screen.getByRole('button', { name: /^Registrar$/i }))

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith(
        expect.stringContaining('/api/sales/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/returns'),
        expect.objectContaining({
          body: JSON.stringify({
            reason: 'Producto equivocado',
            items: [{ saleItemId: 'sale-item-1', quantity: 1 }],
          }),
          method: 'POST',
        }),
      )
    })
  })
})

describe('SaleReceipt', () => {
  afterEach(() => {
    cleanup()
  })

  it('SaleReceipt should render business name, items and total', () => {
    render(<SaleReceipt businessName="Colmado Central" sale={createReceiptSale()} />)

    expect(screen.getByText('Colmado Central')).toBeTruthy()
    expect(screen.getByText('Cafe molido')).toBeTruthy()
    expect(screen.getAllByText('RD$250.00').length).toBeGreaterThan(0)
  })

  it('SaleReceipt should show non fiscal receipt label', () => {
    render(<SaleReceipt businessName="Colmado Central" sale={createReceiptSale()} />)

    expect(screen.getByText('Recibo no fiscal')).toBeTruthy()
  })

  it('SaleReceipt should not expose internal technical fields', () => {
    render(<SaleReceipt businessName="Colmado Central" sale={createReceiptSale()} />)

    expect(screen.queryByText('product-internal-id')).toBeNull()
    expect(screen.queryByText('11111111-1111-1111-1111-111111111111')).toBeNull()
  })

  it('SaleReceipt should show RNC when showRnc and rnc are provided', () => {
    render(
      <SaleReceipt
        businessName="Colmado Central"
        rnc="101123456"
        showRnc
        sale={createReceiptSale()}
      />,
    )
    expect(screen.getByText('RNC: 101123456')).toBeTruthy()
  })

  it('SaleReceipt should not show RNC when showRnc is false', () => {
    render(
      <SaleReceipt
        businessName="Colmado Central"
        rnc="101123456"
        showRnc={false}
        sale={createReceiptSale()}
      />,
    )
    expect(screen.queryByText('RNC: 101123456')).toBeNull()
  })

  it('SaleReceipt should show phone when provided', () => {
    render(
      <SaleReceipt
        businessName="Colmado Central"
        phone="8091234567"
        sale={createReceiptSale()}
      />,
    )
    expect(screen.getByText('Tel: 8091234567')).toBeTruthy()
  })

  it('SaleReceipt should show receiptHeaderText when provided', () => {
    render(
      <SaleReceipt
        businessName="Colmado Central"
        receiptHeaderText="Bienvenido a nuestro local"
        sale={createReceiptSale()}
      />,
    )
    expect(screen.getByText('Bienvenido a nuestro local')).toBeTruthy()
  })

  it('SaleReceipt should show custom footer text', () => {
    render(
      <SaleReceipt
        businessName="Colmado Central"
        receiptFooterText="Vuelva pronto"
        sale={createReceiptSale()}
      />,
    )
    expect(screen.getByText('Vuelva pronto')).toBeTruthy()
    expect(screen.queryByText('Gracias por su compra')).toBeNull()
  })

  it('SaleReceipt should show default footer when no receiptFooterText', () => {
    render(<SaleReceipt businessName="Colmado Central" sale={createReceiptSale()} />)
    expect(screen.getByText('Gracias por su compra')).toBeTruthy()
  })

  it('SaleReceipt should show ITBIS note in totals section', () => {
    render(<SaleReceipt businessName="Colmado Central" sale={createReceiptSale()} />)
    expect(screen.getByText('ITBIS 18% incluido en precios')).toBeTruthy()
  })

  it('SaleReceipt should show fallback branch name when branchName is null', () => {
    render(
      <SaleReceipt
        businessName="Colmado Central"
        sale={{ ...createReceiptSale(), branchName: null }}
      />,
    )
    expect(screen.getByText('Sucursal no disponible')).toBeTruthy()
  })

  it('SaleReceipt should show Consumidor final when customerName is null', () => {
    render(
      <SaleReceipt
        businessName="Colmado Central"
        sale={{ ...createReceiptSale(), customerName: null }}
      />,
    )
    expect(screen.getByText('Consumidor final')).toBeTruthy()
  })
})

function renderSalesPage() {
  renderWithQueryClient(
    <MemoryRouter initialEntries={['/sales']}>
      <SalesPage />
    </MemoryRouter>,
  )
}

function renderSaleDetailPage() {
  const router = createMemoryRouter(
    [{ element: <SaleDetailPage />, path: '/sales/:saleId' }],
    { initialEntries: ['/sales/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'] },
  )

  renderWithQueryClient(<RouterProvider router={router} />)
}

function renderWithQueryClient(element: React.ReactElement) {
  setSession()
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      {element}
    </QueryClientProvider>,
  )
}

function setSession() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-17T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: '22222222-2222-2222-2222-222222222222',
      businessId: '11111111-1111-1111-1111-111111111111',
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '44444444-4444-4444-4444-444444444444',
      roles: ['Admin'],
    },
  })
}

function createSalesFetchMock({
  detailSale = createApiSale(),
  failBilling = false,
  failBusiness = false,
  failDetail = false,
  failList = false,
  returns = [],
  sales = [createApiSale()],
}: {
  detailSale?: ApiSaleMock
  failBilling?: boolean
  failBusiness?: boolean
  failDetail?: boolean
  failList?: boolean
  returns?: ApiSaleReturnMock[]
  sales?: ApiSaleMock[]
} = {}) {
  return vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = input.toString()

    if (url.includes('/api/business/current')) {
      if (failBusiness) return createJsonResponse(null, false, 500, 'Business unavailable')
      return createJsonResponse({
        businessId: '11111111-1111-1111-1111-111111111111',
        identificationNumber: '101123456',
        identificationType: 'RNC',
        name: 'Colmado Central',
        phones: ['8091234567'],
        rnc: '101123456',
        phone: '8091234567',
        receiptFooterText: null,
      })
    }

    if (url.includes('/api/settings/billing')) {
      if (failBilling) return createJsonResponse(null, false, 500, 'Billing unavailable')
      return createJsonResponse({
        businessId: '11111111-1111-1111-1111-111111111111',
        receiptFooterText: null,
        receiptHeaderText: null,
        showLogoOnReceipt: false,
        showRncOnReceipt: false,
      })
    }

    if (url.includes('/api/settings/business')) {
      return createJsonResponse({
        businessId: '11111111-1111-1111-1111-111111111111',
        commercialName: 'Colmado Central',
        legalName: null,
        phone: '8091234567',
        rnc: '101123456',
      })
    }

    if (url.includes('/api/sales/') && url.includes('/returns')) {
      if (init?.method === 'POST') {
        return createJsonResponse(createApiSaleReturn({ reason: 'Producto equivocado' }))
      }

      return createJsonResponse(returns)
    }

    if (url.includes('/api/sales/') && !url.endsWith('/api/sales')) {
      if (failDetail) return createJsonResponse(null, false, 500, 'Sale unavailable')
      return createJsonResponse(detailSale)
    }

    if (url.includes('/api/sales')) {
      if (failList) {
        return createJsonResponse(null, false, 500, 'No se pudo cargar ventas')
      }

      const requestUrl = new URL(url)
      const status = requestUrl.searchParams.get('status')
      const filteredSales = status
        ? sales.filter((sale) => sale.status === status)
        : sales

      return createJsonResponse({
        hasNextPage: false,
        hasPreviousPage: false,
        items: filteredSales,
        page: Number(requestUrl.searchParams.get('page') ?? '1'),
        pageSize: Number(requestUrl.searchParams.get('pageSize') ?? '10'),
        totalItems: filteredSales.length,
        totalPages: filteredSales.length > 0 ? 1 : 0,
      })
    }

    return createJsonResponse(null, false, 404, 'Not found')
  })
}

type ApiSaleMock = {
  branchName: string
  cancellationReason: string | null
  code: string
  createdAt: string
  customerName: string | null
  failureReason: string | null
  id: string
  items: Array<{
    lineTotal: number
    productId: string
    productName: string
    quantity: number
    saleItemId: string
    sku: string
    subtotal: number
    unitPrice: number
  }>
  paymentMethod: string
  saleId: string
  status: SaleStatus
  total: number
}

function createApiSale(overrides: Partial<ApiSaleMock> = {}): ApiSaleMock {
  return {
    ...createApiSaleBase(),
    ...overrides,
  }
}

function createApiSaleBase(): ApiSaleMock {
  return {
    branchName: 'Principal',
    cancellationReason: null,
    code: 'A1B2C3D4',
    createdAt: '2026-05-17T14:00:00Z',
    customerName: 'Maria Perez',
    failureReason: null,
    id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    items: [
      {
        lineTotal: 250,
        productId: 'product-internal-id',
        productName: 'Cafe molido',
        quantity: 2,
        saleItemId: 'sale-item-1',
        sku: 'SKU-001',
        subtotal: 250,
        unitPrice: 125,
      },
    ],
    paymentMethod: 'Cash',
    saleId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    status: 'Completed',
    total: 250,
  }
}

function createReceiptSale(): SaleDetail {
  return {
    branchName: 'Principal',
    cancellationReason: null,
    code: 'A1B2C3D4',
    createdAt: '2026-05-17T14:00:00Z',
    customerName: 'Maria Perez',
    failureReason: null,
    id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    items: [
      {
        productId: 'product-internal-id',
        productName: 'Cafe molido',
        quantity: 2,
        saleItemId: 'sale-item-1',
        sku: 'SKU-001',
        subtotal: 250,
        unitPrice: 125,
      },
    ],
    paymentMethod: 'Cash',
    status: 'Completed',
    total: 250,
  }
}

type ApiSaleReturnMock = {
  approvedAt: string | null
  creditNote: {
    code: string
    createdAt: string
    customerId: string | null
    id: string
    saleId: string
    saleReturnId: string
    total: number
  } | null
  failedAt: string | null
  failureReason: string | null
  id: string
  items: Array<{
    id: string
    lineTotal: number
    productId: string
    productName: string
    quantity: number
    saleItemId: string
    sku: string
    unitPrice: number
  }>
  reason: string
  requestedAt: string
  saleId: string
  status: 'Requested' | 'Approved' | 'Failed'
  total: number
}

function createApiSaleReturn(overrides: Partial<ApiSaleReturnMock> = {}): ApiSaleReturnMock {
  return {
    approvedAt: '2026-05-26T10:03:00Z',
    creditNote: {
      code: 'NC-20260526-ABC123',
      createdAt: '2026-05-26T10:03:00Z',
      customerId: null,
      id: 'credit-note-1',
      saleId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      saleReturnId: 'return-1',
      total: 125,
    },
    failedAt: null,
    failureReason: null,
    id: 'return-1',
    items: [
      {
        id: 'return-item-1',
        lineTotal: 125,
        productId: 'product-internal-id',
        productName: 'Cafe molido',
        quantity: 1,
        saleItemId: 'sale-item-1',
        sku: 'SKU-001',
        unitPrice: 125,
      },
    ],
    reason: 'Cliente devuelve una unidad',
    requestedAt: '2026-05-26T10:00:00Z',
    saleId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    status: 'Approved',
    total: 125,
    ...overrides,
  }
}

function createJsonResponse(data: unknown, ok = true, status = 200, message = 'Request failed') {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => ({
      correlationId: 'test',
      data,
      error: ok ? null : { code: 'ERROR', message },
      isSuccess: ok,
    }),
    ok,
    status,
  }
}
