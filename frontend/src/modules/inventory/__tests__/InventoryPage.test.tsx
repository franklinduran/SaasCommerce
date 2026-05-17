import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { InventoryPage } from '@/modules/inventory/pages/InventoryPage'
import { InventoryProductDetailPage } from '@/modules/inventory/pages/InventoryProductDetailPage'

const productId = '66666666-6666-6666-6666-666666666666'

describe('InventoryPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('InventoryPage should render inventory rows', async () => {
    vi.stubGlobal('fetch', createInventoryFetchMock())
    renderInventoryRoutes()

    expect(await screen.findByText('Cafe molido')).toBeTruthy()
    expect(screen.getByText('Sucursal Principal')).toBeTruthy()
    expect(screen.getByText('12')).toBeTruthy()
  })

  it('InventoryPage should show low stock badge', async () => {
    vi.stubGlobal('fetch', createInventoryFetchMock({ status: 'LowStock', quantity: 2 }))
    renderInventoryRoutes()

    expect(await screen.findByText('Stock bajo')).toBeTruthy()
  })

  it('AdjustInventoryDialog should validate required fields', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createInventoryFetchMock())
    renderInventoryRoutes()

    await user.click(await screen.findByRole('button', { name: 'Ajustar inventario' }))
    await user.click(screen.getByRole('button', { name: 'Ajustar' }))

    expect(await screen.findByText('Producto invalido')).toBeTruthy()
  })

  it('InventoryPage should refresh when SignalR stock event arrives', async () => {
    const fetchMock = createInventoryFetchMock()
    vi.stubGlobal('fetch', fetchMock)
    renderInventoryRoutes()

    await screen.findByText('Cafe molido')
    window.dispatchEvent(new CustomEvent('test-inventory-stock-changed'))

    await waitFor(() => {
      expect(fetchMock.mock.calls.length).toBeGreaterThan(1)
    })
  })

  it('InventoryProductDetailPage should show stock by branch and movements', async () => {
    vi.stubGlobal('fetch', createInventoryFetchMock())
    renderInventoryRoutes(`/inventory/products/${productId}`)

    expect(await screen.findByText('Cafe molido')).toBeTruthy()
    expect(screen.getAllByText('Sucursal Principal').length).toBeGreaterThan(0)
    expect(screen.getByText('Historial de movimientos')).toBeTruthy()
    expect(screen.getByText('InitialStock')).toBeTruthy()
  })
})

vi.mock('@/shared/services/signalrClient', () => ({
  onRealtimeEvent: (_eventName: string, handler: (payload: unknown) => void) => {
    window.addEventListener('test-inventory-stock-changed', () => {
      handler({
        branchId: '22222222-2222-2222-2222-222222222222',
        businessId: '11111111-1111-1111-1111-111111111111',
        productId,
      })
    })
  },
  offRealtimeEvent: vi.fn(),
}))

function renderInventoryRoutes(initialEntry = '/inventory') {
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

  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })
  const router = createMemoryRouter(
    [
      { element: <InventoryPage />, path: '/inventory' },
      { element: <InventoryProductDetailPage />, path: '/inventory/products/:productId' },
    ],
    { initialEntries: [initialEntry] },
  )

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

function createInventoryFetchMock(overrides: Partial<{ quantity: number; status: 'Available' | 'LowStock' | 'OutOfStock' }> = {}) {
  let inventoryCalls = 0

  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/inventory/products/')) {
      return createJsonResponse({
        alerts: [],
        barcode: null,
        branches: [
          {
            branchId: '22222222-2222-2222-2222-222222222222',
            branchName: 'Sucursal Principal',
            currentStock: overrides.quantity ?? 12,
            isLowStock: overrides.status === 'LowStock',
            isOutOfStock: overrides.status === 'OutOfStock',
            lastUpdatedAt: '2026-05-17T12:00:00Z',
            minimumStock: 5,
            status: overrides.status ?? 'Available',
          },
        ],
        minimumStock: 5,
        productId,
        productName: 'Cafe molido',
        recentMovements: [
          {
            branchId: '22222222-2222-2222-2222-222222222222',
            branchName: 'Sucursal Principal',
            businessId: '11111111-1111-1111-1111-111111111111',
            createdAt: '2026-05-17T12:00:00Z',
            id: '77777777-7777-7777-7777-777777777777',
            newStock: 12,
            note: null,
            previousStock: 0,
            productId,
            productName: 'Cafe molido',
            quantity: 12,
            reason: 'InitialStock',
            saleId: null,
            userId: '44444444-4444-4444-4444-444444444444',
          },
        ],
        reorderPoint: 5,
        sku: 'SKU-001',
        unitOfMeasure: 'Unit',
      })
    }

    if (url.includes('/api/inventory?')) {
      inventoryCalls += 1
      return createJsonResponse({
        hasNextPage: false,
        hasPreviousPage: false,
        items: [
          {
            barcode: null,
            branchId: '22222222-2222-2222-2222-222222222222',
            branchName: 'Sucursal Principal',
            businessId: '11111111-1111-1111-1111-111111111111',
            id: `55555555-5555-5555-5555-${String(inventoryCalls).padStart(12, '0')}`,
            isLowStock: overrides.status === 'LowStock',
            isOutOfStock: overrides.status === 'OutOfStock',
            lastUpdatedAt: '2026-05-17T12:00:00Z',
            minimumStock: 5,
            productId,
            productName: 'Cafe molido',
            quantity: overrides.quantity ?? 12,
            reorderPoint: 5,
            sku: 'SKU-001',
            status: overrides.status ?? 'Available',
            unitOfMeasure: 'Unit',
          },
        ],
        page: 1,
        pageSize: 10,
        total: 1,
        totalItems: 1,
        totalPages: 1,
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
      error: ok ? null : { code: 'ERROR', message: 'failed' },
      isSuccess: ok,
    }),
    ok,
    status,
  }
}
