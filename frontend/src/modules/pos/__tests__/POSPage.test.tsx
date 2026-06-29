import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import { MemoryRouter } from 'react-router-dom'
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
  http.get('http://localhost:5000/api/cash-sessions/current', () =>
    HttpResponse.json(createApiResponse(createOpenCashSession())),
  ),
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

  it('shows API error when create sale fails with 404', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          { correlationId: 'test', data: null, error: { code: 'PRODUCT_NOT_FOUND', message: 'Not found.' }, isSuccess: false },
          { status: 404 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Uno de los productos no esta disponible.')).toBeTruthy()
  })

  it('shows session expired message on 401', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          { correlationId: 'test', data: null, error: { code: 'UNAUTHORIZED', message: 'Unauthorized.' }, isSuccess: false },
          { status: 401 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('La sesion expiro. Inicia sesion nuevamente.')).toBeTruthy()
  })

  it('shows permission error on 403', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          { correlationId: 'test', data: null, error: { code: 'FORBIDDEN', message: 'Forbidden.' }, isSuccess: false },
          { status: 403 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Sin permiso para esta operacion. Verifica tu suscripcion o permisos.')).toBeTruthy()
  })

  it('shows cash session closed message when NO_OPEN_CASH_SESSION', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          { correlationId: 'test', data: null, error: { code: 'NO_OPEN_CASH_SESSION', message: 'No open session.' }, isSuccess: false },
          { status: 409 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('No hay caja abierta. Ve a Caja y abre una sesion primero.')).toBeTruthy()
  })

  it('shows bad request message on 400', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          { correlationId: 'test', data: null, error: { code: 'VALIDATION_ERROR', message: 'Datos invalidos.' }, isSuccess: false },
          { status: 400 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Datos invalidos.')).toBeTruthy()
  })

  it('shows NoCashSessionBanner when no open cash session', async () => {
    server.use(
      http.get('http://localhost:5000/api/cash-sessions/current', () =>
        HttpResponse.json(createApiResponse(null)),
      ),
    )
    renderPOSPage()

    expect(await screen.findByText('Caja cerrada')).toBeTruthy()
    expect(screen.getByText('Ir a Caja')).toBeTruthy()
  })

  it('clears carrito-vacio message when a product is added', async () => {
    const user = userEvent.setup()
    renderPOSPage()

    // Trigger empty cart validation first
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))
    expect(await screen.findByText('El carrito esta vacio.')).toBeTruthy()

    // Adding a product should clear it
    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await waitFor(() => {
      expect(screen.queryByText('El carrito esta vacio.')).toBeNull()
    })
  })

  it('shows credit validation when Credit payment has no customer', async () => {
    const user = userEvent.setup()
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    // Switch to credit payment
    await user.click(screen.getByRole('button', { name: 'Crédito' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Selecciona un cliente para ventas a crédito.')).toBeTruthy()
  })

  it('shows subscription limit message when SUBSCRIPTION_LIMIT_REACHED', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          { correlationId: 'test', data: null, error: { code: 'SUBSCRIPTION_LIMIT_REACHED', message: 'Limite de ventas alcanzado.' }, isSuccess: false },
          { status: 422 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Limite de ventas alcanzado.')).toBeTruthy()
  })

  it('shows generic error for unhandled status codes', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('http://localhost:5000/api/sales', () =>
        HttpResponse.json(
          { correlationId: 'test', data: null, error: { code: 'SERVER_ERROR', message: 'Error inesperado.' }, isSuccess: false },
          { status: 500 },
        ),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('Error inesperado.')).toBeTruthy()
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

  it('shows blocked credit message when customer credit is Blocked', async () => {
    const user = userEvent.setup()
    const blockedCustomerId = 'bb000000-bb00-4b00-8b00-bbbbbbbbbbbb'
    server.use(
      http.get('http://localhost:5000/api/customers', () =>
        HttpResponse.json(createApiResponse(createPagedResponse([
          {
            ...createCustomer(),
            creditStatus: 'Blocked',
            id: blockedCustomerId,
            fullName: 'Juan Morales',
          },
        ]))),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Crédito' }))
    await user.click(await screen.findByRole('combobox', { name: 'Seleccionar cliente' }))
    await user.click(await screen.findByRole('option', { name: 'Juan Morales' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('El cliente tiene el credito bloqueado.')).toBeTruthy()
  })

  it('shows credit limit exceeded message when total exceeds customer limit', async () => {
    const user = userEvent.setup()
    const limitedCustomerId = 'cc000000-cc00-4c00-8c00-cccccccccccc'
    // Cart subtotal will be 250 (one Cafe molido at 250), customer currentBalance=100, creditLimit=200
    // 100 + 250 = 350 > 200 → exceeds limit
    server.use(
      http.get('http://localhost:5000/api/customers', () =>
        HttpResponse.json(createApiResponse(createPagedResponse([
          {
            ...createCustomer(),
            creditLimit: 200,
            creditStatus: 'Active',
            currentBalance: 100,
            id: limitedCustomerId,
            fullName: 'Ana Lopez',
          },
        ]))),
      ),
    )
    renderPOSPage()

    await user.click(await screen.findByRole('button', { name: 'Agregar Cafe molido' }))
    await user.click(screen.getByRole('button', { name: 'Crédito' }))
    await user.click(await screen.findByRole('combobox', { name: 'Seleccionar cliente' }))
    await user.click(await screen.findByRole('option', { name: 'Ana Lopez' }))
    await user.click(screen.getByRole('button', { name: 'Procesar venta' }))

    expect(await screen.findByText('La venta supera el limite de credito del cliente.')).toBeTruthy()
  })

  it('clicking Actualizar triggers product refresh callback', async () => {
    renderPOSPage()

    await screen.findByText('Cafe molido')
    const refreshBtn = screen.getByRole('button', { name: 'Actualizar productos' })
    expect(refreshBtn).toBeTruthy()

    const user = userEvent.setup()
    await user.click(refreshBtn)

    // After clicking, products are refetched — requestCount increases
    await waitFor(() => {
      expect(productRequests).toBeGreaterThan(1)
    })
  })

  it('shows error state and Reintentar button when products fail to load', async () => {
    const user = userEvent.setup()
    server.use(
      http.get('http://localhost:5000/api/catalog/products', () =>
        HttpResponse.json({ correlationId: 'test', data: null, error: { code: 'SERVER_ERROR', message: 'Error' }, isSuccess: false }, { status: 500 }),
      ),
    )
    renderPOSPage()

    expect(await screen.findByText('No se pudo cargar el catalogo.')).toBeTruthy()
    const retryBtn = screen.getByRole('button', { name: 'Reintentar' })
    expect(retryBtn).toBeTruthy()
    // Click Reintentar — covers onRetry callback
    await user.click(retryBtn)
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
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>
        <POSPage />
      </QueryClientProvider>
    </MemoryRouter>,
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

function createOpenCashSession() {
  return {
    id: 'cc000000-0000-4000-8000-000000000001',
    branchId,
    businessId,
    openedAt: '2026-05-25T08:00:00Z',
    openingBalance: 1000,
    status: 'Open',
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
