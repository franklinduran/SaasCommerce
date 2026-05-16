import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { ProductsPage } from '@/modules/products/ProductsPage'

describe('ProductsPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders products and category filters from the API', async () => {
    vi.stubGlobal('fetch', createProductsFetchMock())
    renderProductsPage()

    expect(await screen.findByText('Cafe molido')).toBeTruthy()
    expect(screen.getByRole('option', { name: 'Bebidas' })).toBeTruthy()
    expect(screen.getByText('Pagina 1 de 2')).toBeTruthy()
  })

  it('opens the product drawer for create flow', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createProductsFetchMock())
    renderProductsPage()

    await user.click(await screen.findByRole('button', { name: 'Crear producto' }))

    expect(screen.getByRole('heading', { name: 'Crear producto' })).toBeTruthy()
    expect(screen.getByText('Datos generales')).toBeTruthy()
  })

  it('requests deactivate endpoint after confirmation', async () => {
    const user = userEvent.setup()
    const fetchMock = createProductsFetchMock()
    vi.stubGlobal('fetch', fetchMock)
    vi.stubGlobal('confirm', vi.fn(() => true))
    renderProductsPage()

    await user.click(await screen.findByRole('button', { name: 'Desactivar' }))

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith(
        expect.stringContaining('/api/catalog/products/55555555-5555-5555-5555-555555555555/deactivate'),
        expect.objectContaining({ method: 'PUT' }),
      )
    })
  })
})

function renderProductsPage() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-15T23:59:00Z',
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

  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <ProductsPage />
    </QueryClientProvider>,
  )
}

function createProductsFetchMock() {
  return vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = input.toString()

    if (url.includes('/api/catalog/categories')) {
      return createJsonResponse([
        {
          businessId: '11111111-1111-1111-1111-111111111111',
          description: null,
          id: '77777777-7777-7777-7777-777777777777',
          isActive: true,
          name: 'Bebidas',
        },
      ])
    }

    if (url.includes('/api/catalog/products/') && init?.method === 'PUT') {
      return createJsonResponse(null)
    }

    if (url.includes('/api/catalog/products')) {
      return createJsonResponse({
        hasNextPage: true,
        hasPreviousPage: false,
        items: [createProduct()],
        page: Number(new URL(url).searchParams.get('page') ?? '1'),
        pageSize: 10,
        totalItems: 11,
        totalPages: 2,
      })
    }

    return createJsonResponse(null, false, 404)
  })
}

function createProduct() {
  return {
    allowNegativeStock: false,
    allowsDiscount: true,
    attributesJson: null,
    barcode: '1234567890',
    brandId: null,
    businessId: '11111111-1111-1111-1111-111111111111',
    categoryId: '77777777-7777-7777-7777-777777777777',
    costPrice: 150,
    description: 'Producto de prueba',
    id: '55555555-5555-5555-5555-555555555555',
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
    sku: 'SKU-001',
    supplierCode: null,
    taxCategory: 'Itbis18',
    taxRate: 18,
    trackInventory: true,
    unitOfMeasure: 'Unit',
    variantName: null,
    wholesalePrice: null,
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
