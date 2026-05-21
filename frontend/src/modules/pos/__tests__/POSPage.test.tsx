import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { POSPage } from '@/modules/pos/pages/POSPage'
import { usePOSCartStore } from '@/modules/pos/store/posCartStore'
import type { SaleStatusChangedNotification } from '@/modules/pos/types/posTypes'

const realtimeMock = vi.hoisted(() => ({
  handlers: new Map<string, Set<(payload: unknown) => void>>(),
}))

vi.mock('@/shared/services/signalrClient', () => ({
  offRealtimeEvent: vi.fn(
    (eventName: string, handler: (payload: unknown) => void) => {
      realtimeMock.handlers.get(eventName)?.delete(handler)
    },
  ),
  onRealtimeEvent: vi.fn(
    (eventName: string, handler: (payload: unknown) => void) => {
      const handlers = realtimeMock.handlers.get(eventName) ?? new Set()

      handlers.add(handler)
      realtimeMock.handlers.set(eventName, handlers)
    },
  ),
}))

const server = setupServer(
  http.get('http://localhost:5000/api/catalog/products', ({ request }) => {
    productRequests += 1
    const url = new URL(request.url)
    const query = url.searchParams.get('query')?.toLowerCase() ?? ''
    const items = [createProduct()].filter((product) =>
      `${product.name} ${product.sku}`.toLowerCase().includes(query),
    )

    return HttpResponse.json(createApiResponse(createPagedResponse(items)))
  }),
  http.get('http://localhost:5000/api/customers', () =>
    HttpResponse.json(createApiResponse(createPagedResponse([createCustomer()]))),
  ),
  http.post('http://localhost:5000/api/sales', async ({ request }) => {
    lastSaleRequest = await request.json()

    return HttpResponse.json(createApiResponse(createSaleResponse('Received')), { status: 201 })
  }),
  http.get('http://localhost:5000/api/sales/:saleId', () =>
    HttpResponse.json(createApiResponse(createSaleResponse('Received'))),
  ),
)

describe('POSPage', () => {
  beforeAll(() => server.listen())

  beforeEach(() => {
    lastSaleRequest = null
    productRequests = 0
    realtimeMock.handlers.clear()
    usePOSCartStore.getState().clearCart()
    useAuthStore.getState().setSession(createSession())
  })

  afterEach(() => {
    cleanup()
    server.resetHandlers()
    useAuthStore.getState().clearSession()
    usePOSCartStore.getState().clearCart()
  })

  afterAll(() => server.close())

  it('loads products and customers from API', async () => {
    const user = userEvent.setup()
    renderPOSPage()

    expect(await screen.findByText('Cafe molido')).toBeTruthy()
    await user.click(await screen.findByRole('combobox', { name: 'Seleccionar cliente' }))
    expect(await screen.findByRole('option', { name: 'Maria Perez' })).toBeTruthy()
  })

  it('creates sale when cart is valid', async () => {
    const user = userEvent.setup()
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    await waitFor(() => {
      expect(lastSaleRequest).toMatchObject({
        branchId,
        customerId: null,
        items: [{ productId, quantity: 1 }],
        paymentMethod: 'Cash',
      })
    })
    expect(JSON.stringify(lastSaleRequest)).not.toContain('businessId')
    expect(await screen.findByText('Venta recibida')).toBeTruthy()
  })

  it('shows validation error when cart is empty', async () => {
    const user = userEvent.setup()
    renderPOSPage()

    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('El carrito esta vacio.')).toBeTruthy()
  })

  it('shows API error when create sale fails', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          {
            correlationId: 'test',
            data: null,
            error: {
              code: 'PRODUCT_NOT_FOUND',
              message: 'Product was not found.',
            },
            isSuccess: false,
          },
          { status: 404 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Uno de los productos no esta disponible.')).toBeTruthy()
  })

  it('updates to Completed when SignalR event arrives and clears cart', async () => {
    const user = userEvent.setup()
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))
    expect(await screen.findByText('Venta recibida')).toBeTruthy()

    act(() => {
      emitSaleStatus('Completed')
    })

    expect(await screen.findByText('Venta completada')).toBeTruthy()
    expect(screen.getByText('Carrito vacio')).toBeTruthy()
  })

  it('keeps cart when sale fails', async () => {
    const user = userEvent.setup()
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))
    expect(await screen.findByText('Venta recibida')).toBeTruthy()

    act(() => {
      emitSaleStatus('Failed', 'Stock insuficiente')
    })

    expect(await screen.findByText('Venta fallida')).toBeTruthy()
    expect(screen.queryByText('Carrito vacio')).toBeNull()
    expect(screen.getAllByText('Stock insuficiente')).toHaveLength(2)
  })

  it('updates sale status from API polling when realtime is not received', async () => {
    const user = userEvent.setup()
    server.use(
      http.get('http://localhost:5000/api/sales/:saleId', () =>
        HttpResponse.json(createApiResponse(createSaleResponse('Failed', 'Stock insuficiente'))),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Venta fallida')).toBeTruthy()
    expect(screen.queryByText('Carrito vacio')).toBeNull()
    expect(screen.getAllByText('Stock insuficiente')).toHaveLength(2)
  })

  it('refreshes products when inventory.updated arrives', async () => {
    renderPOSPage()

    expect(await screen.findByText('Cafe molido')).toBeTruthy()
    expect(productRequests).toBe(1)

    act(() => {
      realtimeMock.handlers.get('inventory.updated')?.forEach((handler) =>
        handler({
          branchId,
          businessId,
          productId,
        }),
      )
    })

    await waitFor(() => {
      expect(productRequests).toBeGreaterThan(1)
    })
  })
})

let lastSaleRequest: unknown = null
let productRequests = 0

const businessId = '11111111-1111-4111-8111-111111111111'
const branchId = '22222222-2222-4222-8222-222222222222'
const userId = '33333333-3333-4333-8333-333333333333'
const productId = '55555555-5555-4555-8555-555555555555'
const saleId = '99999999-9999-4999-8999-999999999999'

function renderPOSPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <POSPage />
    </QueryClientProvider>,
  )
}

function emitSaleStatus(status: SaleStatusChangedNotification['status'], reason: string | null = null) {
  const payload: SaleStatusChangedNotification = {
    branchId,
    businessId,
    correlationId: '88888888-8888-4888-8888-888888888888',
    createdAt: '2026-05-17T12:00:00Z',
    eventId: '77777777-7777-4777-8777-777777777777',
    reason,
    saleId,
    status,
    userId,
    version: 1,
  }

  realtimeMock.handlers.get('sale.statusChanged')?.forEach((handler) => handler(payload))
}

function createSession() {
  return {
    accessToken: 'jwt',
    expiresAt: '2026-05-17T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId,
      businessId,
      email: 'admin@test.com',
      fullName: 'Admin',
      id: userId,
      roles: ['Admin'],
    },
  }
}

function createProduct() {
  return {
    allowNegativeStock: false,
    allowsDiscount: true,
    attributesJson: null,
    barcode: '1234567890',
    brandId: null,
    businessId,
    categoryId: null,
    costPrice: 125,
    description: 'Producto de prueba',
    id: productId,
    internalCode: null,
    isActive: true,
    isTaxIncluded: true,
    maximumStock: 100,
    minSalePrice: null,
    minimumStock: 1,
    name: 'Cafe molido',
    parentProductId: null,
    productType: 'Simple',
    profitMargin: 40,
    reorderPoint: 5,
    salePrice: 250,
    sku: 'CAF-001',
    supplierCode: null,
    taxCategory: 'Itbis18',
    taxRate: 18,
    trackInventory: true,
    unitOfMeasure: 'Unit',
    variantName: null,
    wholesalePrice: null,
  }
}

function createCustomer() {
  return {
    businessId,
    createdAt: '2026-05-17T12:00:00Z',
    deactivatedAt: null,
    email: 'maria@example.com',
    fullName: 'Maria Perez',
    id: '44444444-4444-4444-8444-444444444444',
    isActive: true,
    phone: '8095550101',
    updatedAt: null,
  }
}

function createSaleResponse(
  status: SaleStatusChangedNotification['status'],
  failureReason: string | null = null,
) {
  return {
    branchId,
    businessId,
    cancelledAt: null,
    cancellationReason: null,
    completedAt: null,
    createdAt: '2026-05-17T12:00:00Z',
    customerId: null,
    failedAt: status === 'Failed' ? '2026-05-17T12:00:03Z' : null,
    failureReason,
    items: [
      {
        lineTotal: 250,
        productId,
        productName: 'Cafe molido',
        quantity: 1,
        sku: 'CAF-001',
        unitPrice: 250,
      },
    ],
    paymentMethod: 'Cash',
    saleId,
    status,
    total: 250,
    updatedAt: '2026-05-17T12:00:00Z',
    userId,
  }
}

function createPagedResponse<TItem>(items: TItem[]) {
  return {
    hasNextPage: false,
    hasPreviousPage: false,
    items,
    page: 1,
    pageSize: 20,
    totalItems: items.length,
    totalPages: items.length > 0 ? 1 : 0,
  }
}

function createApiResponse<TData>(data: TData) {
  return {
    correlationId: 'test',
    data,
    error: null,
    isSuccess: true,
  }
}
