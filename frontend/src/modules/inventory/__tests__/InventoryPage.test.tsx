import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { InventoryPage } from '@/modules/inventory/InventoryPage'

describe('InventoryPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows stock rows and pagination metadata', async () => {
    vi.stubGlobal('fetch', createInventoryFetchMock())
    renderInventoryPage()

    expect(await screen.findByText('Cafe molido')).toBeTruthy()
    expect(screen.getByText('SKU-001')).toBeTruthy()
    expect(screen.getByText('Pagina 1 de 2')).toBeTruthy()
  })

  it('requests the next stock page when pagination changes', async () => {
    const user = userEvent.setup()
    const fetchMock = createInventoryFetchMock()
    vi.stubGlobal('fetch', fetchMock)
    renderInventoryPage()

    await screen.findByText('Cafe molido')
    await user.click(screen.getAllByRole('button', { name: 'Siguiente' })[0])

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith(
        expect.stringContaining('/api/inventory/stock?page=2'),
        expect.anything(),
      )
    })
  })

  it('opens adjustment drawer and validates required product', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createInventoryFetchMock())
    renderInventoryPage()

    await user.click(await screen.findByRole('button', { name: 'Ajustar inventario' }))
    await user.click(screen.getByRole('button', { name: 'Ajustar' }))

    expect(await screen.findByText('Producto invalido')).toBeTruthy()
  })
})

function renderInventoryPage() {
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
      <InventoryPage />
    </QueryClientProvider>,
  )
}

function createInventoryFetchMock() {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/inventory/stock')) {
      return createJsonResponse({
        hasNextPage: true,
        hasPreviousPage: false,
        items: [
          {
            barcode: '1234567890',
            branchId: '22222222-2222-2222-2222-222222222222',
            businessId: '11111111-1111-1111-1111-111111111111',
            id: '55555555-5555-5555-5555-555555555555',
            isLowStock: false,
            minimumStock: 1,
            productId: '66666666-6666-6666-6666-666666666666',
            productName: 'Cafe molido',
            quantity: 12,
            reorderPoint: 5,
            sku: 'SKU-001',
            unitOfMeasure: 'Unit',
          },
        ],
        page: Number(new URL(url).searchParams.get('page') ?? '1'),
        pageSize: 10,
        total: 11,
        totalItems: 11,
        totalPages: 2,
      })
    }

    if (url.includes('/api/inventory/movements')) {
      return createJsonResponse({
        hasNextPage: false,
        hasPreviousPage: false,
        items: [],
        page: 1,
        pageSize: 10,
        total: 0,
        totalItems: 0,
        totalPages: 0,
      })
    }

    return createJsonResponse(null, false, 404)
  })
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
