import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import type { ReactNode } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { PurchaseForm } from '@/modules/purchases/components/PurchaseForm'
import { PurchaseStatusBadge } from '@/modules/purchases/components/PurchaseStatusBadge'
import { PurchaseDetailPage } from '@/modules/purchases/pages/PurchaseDetailPage'
import { PurchasesPage } from '@/modules/purchases/pages/PurchasesPage'

describe('Purchases module', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('PurchasesPage should render purchases', async () => {
    vi.stubGlobal('fetch', createPurchasingFetchMock())
    renderWithProviders(<PurchasesPage />)

    expect(await screen.findByText('Distribuidora Norte')).toBeTruthy()
    expect(screen.getByText('FAC-001')).toBeTruthy()
    expect(screen.getAllByText('RD$ 250.00').length).toBeGreaterThan(0)
  })

  it('PurchaseForm should prevent empty purchase', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createPurchasingFetchMock())
    renderWithProviders(<PurchaseForm />)

    await user.click(screen.getByRole('button', { name: 'Guardar compra' }))

    expect(await screen.findByText('Selecciona un proveedor.')).toBeTruthy()
  })

  it('PurchaseForm should calculate totals', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createPurchasingFetchMock())
    renderWithProviders(<PurchaseForm />)

    await user.click(screen.getByRole('button', { name: 'Agregar producto' }))
    const productSelect = (await screen.findAllByRole('combobox'))[1]
    await user.click(productSelect)
    await user.click(await screen.findByRole('option', { name: 'Cafe molido / CAF-001' }))

    expect(await screen.findAllByText('RD$ 125.00')).toHaveLength(2)
  })

  it('PurchaseDetail should show completed status', async () => {
    vi.stubGlobal('fetch', createPurchasingFetchMock())
    renderWithProviders(
      <Routes>
        <Route element={<PurchaseDetailPage />} path="/purchases/:purchaseId" />
      </Routes>,
      `/purchases/${purchaseId}`,
    )

    expect(await screen.findByText('Completada')).toBeTruthy()
    expect(screen.getByText('Cafe molido')).toBeTruthy()
  })

  it('PurchaseStatusBadge should render completed and failed states', () => {
    const { rerender } = renderWithProviders(<PurchaseStatusBadge status="Completed" />)

    expect(screen.getByText('Completada')).toBeTruthy()

    rerender(<PurchaseStatusBadge status="Failed" />)

    expect(screen.getByText('Fallida')).toBeTruthy()
  })
})

const businessId = '11111111-1111-4111-8111-111111111111'
const branchId = '22222222-2222-4222-8222-222222222222'
const userId = '33333333-3333-4333-8333-333333333333'
const supplierId = '44444444-4444-4444-8444-444444444444'
const productId = '55555555-5555-4555-8555-555555555555'
const purchaseId = '66666666-6666-4666-8666-666666666666'

function renderWithProviders(element: ReactNode, initialPath = '/') {
  useAuthStore.getState().setSession({
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
  })

  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>{element}</MemoryRouter>
    </QueryClientProvider>,
  )
}

function createPurchasingFetchMock() {
  return vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = input.toString()

    if (url.includes('/api/suppliers')) {
      return createJsonResponse(createPagedResponse([createSupplier()]))
    }

    if (url.includes('/api/catalog/categories')) {
      return createJsonResponse([])
    }

    if (url.includes('/api/catalog/products')) {
      return createJsonResponse(createPagedResponse([createProduct()]))
    }

    if (url.includes(`/api/purchases/${purchaseId}`)) {
      return createJsonResponse(createPurchase())
    }

    if (url.includes('/api/purchases') && init?.method === 'POST') {
      return createJsonResponse(createPurchase(), true, 201)
    }

    if (url.includes('/api/purchases')) {
      return createJsonResponse({
        ...createPagedResponse([createPurchase()]),
        totalPurchased: 250,
      })
    }

    return createJsonResponse(null, false, 404)
  })
}

function createSupplier() {
  return {
    address: 'Santiago',
    businessId,
    createdAt: '2026-05-17T12:00:00Z',
    email: 'ventas@norte.test',
    id: supplierId,
    isActive: true,
    name: 'Distribuidora Norte',
    phone: '8095550101',
    rnc: '101111111',
    updatedAt: null,
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

function createPurchase() {
  return {
    branchId,
    businessId,
    cancelledAt: null,
    code: '66666666',
    createdAt: '2026-05-17T12:00:00Z',
    id: purchaseId,
    items: [
      {
        id: '77777777-7777-4777-8777-777777777777',
        productId,
        productName: 'Cafe molido',
        quantity: 2,
        sku: 'CAF-001',
        subtotal: 250,
        unitCost: 125,
      },
    ],
    movements: [
      {
        createdAt: '2026-05-17T12:00:00Z',
        id: '88888888-8888-4888-8888-888888888888',
        newStock: 12,
        previousStock: 10,
        productId,
        quantity: 2,
        reason: 'PurchaseReceived',
      },
    ],
    notes: null,
    purchaseDate: '2026-05-17T12:00:00Z',
    purchaseId,
    receivedAt: '2026-05-17T12:00:00Z',
    status: 'Completed',
    supplierId,
    supplierInvoiceNumber: 'FAC-001',
    supplierName: 'Distribuidora Norte',
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
    pageSize: 10,
    totalItems: items.length,
    totalPages: items.length > 0 ? 1 : 0,
  }
}

function createJsonResponse(data: unknown, ok = true, status = 200) {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => ({
      correlationId: 'test',
      data,
      error: null,
      isSuccess: ok,
    }),
    ok,
    status,
  }
}
