import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { ProductImportPage } from '@/modules/products/pages/ProductImportPage'
import type { ImportProductsResponse } from '@/modules/products/services/productImportApi'

const businessId = '11111111-1111-1111-1111-111111111111'

describe('ProductImportPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders the import page with upload section', async () => {
    renderProductImportPage()

    expect(await screen.findByText('Importar productos')).toBeTruthy()
    expect(screen.getByText('1. Descarga la plantilla')).toBeTruthy()
    expect(screen.getByText('2. Sube tu archivo CSV')).toBeTruthy()
    expect(screen.getByText('Descargar plantilla CSV')).toBeTruthy()
  })

  it('shows download button for template', async () => {
    vi.stubGlobal('fetch', createTemplateFetchMock())
    renderProductImportPage()

    const downloadBtn = await screen.findByRole('button', { name: /Descargar plantilla/ })

    expect(downloadBtn).toBeTruthy()
  })

  it('shows import results after uploading a valid CSV', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createImportFetchMock(createSuccessResult()))

    renderProductImportPage()

    await screen.findByText('Importar productos')

    const file = new File(
      ['Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity\nArroz,ARR-001,Víveres,100,80,50'],
      'products.csv',
      { type: 'text/csv' },
    )

    const input = screen.getByLabelText('Seleccionar archivo', { selector: 'input' })
    await user.upload(input, file)

    expect(await screen.findByText('Importados')).toBeTruthy()
    expect(screen.getByText('Omitidos')).toBeTruthy()
  })

  it('shows error rows when some imports fail', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createImportFetchMock(createPartialResult()))
    renderProductImportPage()

    await screen.findByText('Importar productos')

    const file = new File(
      ['Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity\n,BAD-SKU,Bebidas,50,30,5'],
      'bad.csv',
      { type: 'text/csv' },
    )

    const input = screen.getByLabelText('Seleccionar archivo', { selector: 'input' })
    await user.upload(input, file)

    expect(await screen.findByText('Filas con errores (1)')).toBeTruthy()
    expect(screen.getByText('Name is required.')).toBeTruthy()
  })

  it('shows error message when API returns an error', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createErrorFetchMock())
    renderProductImportPage()

    await screen.findByText('Importar productos')

    const file = new File([''], 'empty.csv', { type: 'text/csv' })
    const input = screen.getByLabelText('Seleccionar archivo', { selector: 'input' })
    await user.upload(input, file)

    await waitFor(() => {
      expect(screen.getByText(/Error/)).toBeTruthy()
    })
  })

  it('shows link to catalog after successful import', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createImportFetchMock(createSuccessResult()))
    renderProductImportPage()

    await screen.findByText('Importar productos')

    const file = new File(['Name,Sku\nTest,TST-001'], 'products.csv', { type: 'text/csv' })
    const input = screen.getByLabelText('Seleccionar archivo', { selector: 'input' })
    await user.upload(input, file)

    expect(await screen.findByText(/Ver catálogo/)).toBeTruthy()
  })
})

// ── Helpers ──────────────────────────────────────────────────────────────────

function createSuccessResult(): ImportProductsResponse {
  return {
    errors: [],
    importedCount: 2,
    skippedCount: 0,
    totalRows: 2,
  }
}

function createPartialResult(): ImportProductsResponse {
  return {
    errors: [{ messages: ['Name is required.'], name: '', rowNumber: 2 }],
    importedCount: 0,
    skippedCount: 1,
    totalRows: 1,
  }
}

function createImportFetchMock(result: ImportProductsResponse) {
  return vi.fn(async (url: string) => {
    if (String(url).includes('/api/products/import')) {
      return new Response(
        JSON.stringify({ isSuccess: true, data: result, error: null }),
        { status: 201, headers: { 'Content-Type': 'application/json' } },
      )
    }

    return new Response('', { status: 404 })
  })
}

function createTemplateFetchMock() {
  return vi.fn(async () =>
    new Response('Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity\r\n', {
      status: 200,
      headers: { 'Content-Type': 'text/csv' },
    }),
  )
}

function createErrorFetchMock() {
  return vi.fn(async () =>
    new Response(
      JSON.stringify({
        isSuccess: false,
        data: null,
        error: { code: 'IMPORT_EMPTY_FILE', message: 'El archivo está vacío.' },
      }),
      { status: 400, headers: { 'Content-Type': 'application/json' } },
    ),
  )
}

function renderProductImportPage() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-24T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: '22222222-2222-2222-2222-222222222222',
      businessId,
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '44444444-4444-4444-4444-444444444444',
      roles: ['Admin'],
    },
  })

  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createMemoryRouter([{ path: '/', element: <ProductImportPage /> }])

  render(
    <QueryClientProvider client={qc}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}
