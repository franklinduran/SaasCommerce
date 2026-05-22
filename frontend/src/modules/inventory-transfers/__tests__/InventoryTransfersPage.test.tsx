import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { InventoryTransfersPage } from '@/modules/inventory-transfers/pages/InventoryTransfersPage'

const transferId = 'tttttttt-tttt-tttt-tttt-tttttttttttt'
const sourceBranchId = '11111111-1111-1111-1111-111111111111'
const targetBranchId = '22222222-2222-2222-2222-222222222222'
const businessId = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'

describe('InventoryTransfersPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('should render transfer list', async () => {
    vi.stubGlobal('fetch', createFetchMock())
    renderTransfersPage()

    expect(await screen.findByText('Sucursal Norte')).toBeTruthy()
    expect(screen.getByText('Pendiente')).toBeTruthy()
  })

  it('should show cancel button for pending transfers', async () => {
    vi.stubGlobal('fetch', createFetchMock())
    renderTransfersPage()

    await screen.findByText('Sucursal Norte')
    expect(screen.getAllByRole('button', { name: /Cancelar/ }).length).toBeGreaterThan(0)
  })

  it('should open detail panel when clicking Detalle', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createFetchMock())
    renderTransfersPage()

    await user.click(await screen.findByRole('button', { name: /Detalle/ }))

    expect(await screen.findByText('Detalle de transferencia')).toBeTruthy()
    // Note appears in both table row and detail panel
    expect(screen.getAllByText('Motivo de traslado').length).toBeGreaterThanOrEqual(1)
    // Detail-specific label only visible in panel
    expect(screen.getByText('Nota')).toBeTruthy()
  })

  it('should invalidate query when SignalR event arrives', async () => {
    const fetchMock = createFetchMock()
    vi.stubGlobal('fetch', fetchMock)
    renderTransfersPage()

    await screen.findByText('Sucursal Norte')

    window.dispatchEvent(
      new CustomEvent('test-transfer-completed', {
        detail: {
          businessId,
          occurredAt: new Date().toISOString(),
          sourceBranchId,
          status: 'Completed',
          targetBranchId,
          transferId,
        },
      }),
    )

    await waitFor(() => {
      expect(fetchMock.mock.calls.length).toBeGreaterThan(1)
    })
  })

  it('should open create dialog and validate required fields', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createFetchMock())
    renderTransfersPage()

    await user.click(await screen.findByRole('button', { name: /Nueva transferencia/ }))
    expect(await screen.findByText('Nueva transferencia de inventario')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Crear transferencia' }))
    expect(await screen.findByText('Selecciona la sucursal de origen')).toBeTruthy()
  })
})

vi.mock('@/shared/services/signalrClient', () => ({
  onRealtimeEvent: (_eventName: string, handler: (payload: unknown) => void) => {
    window.addEventListener('test-transfer-completed', (event) => {
      handler((event as CustomEvent).detail)
    })
  },
  offRealtimeEvent: vi.fn(),
}))

function renderTransfersPage() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-21T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: sourceBranchId,
      businessId,
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
    [{ element: <InventoryTransfersPage />, path: '/inventory-transfers' }],
    { initialEntries: ['/inventory-transfers'] },
  )

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

function createFetchMock() {
  let transferCalls = 0

  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/inventory-transfers') && !url.includes('/cancel')) {
      transferCalls += 1
      return createJsonResponse({
        hasNextPage: false,
        hasPreviousPage: false,
        items: [
          {
            businessId,
            createdAt: '2026-05-21T10:00:00Z',
            createdByUserId: '44444444-4444-4444-4444-444444444444',
            failureReason: null,
            id: `tttttttt-tttt-tttt-tttt-${String(transferCalls).padStart(12, '0')}`,
            items: [
              {
                productId: 'pppppppp-pppp-pppp-pppp-pppppppppppp',
                productName: 'Cafe molido',
                quantity: 5,
              },
            ],
            note: 'Motivo de traslado',
            sourceBranchId,
            sourceBranchName: 'Sucursal Norte',
            status: 'Pending',
            targetBranchId,
            targetBranchName: 'Sucursal Sur',
            updatedAt: '2026-05-21T10:00:00Z',
          },
        ],
        page: 1,
        pageSize: 20,
        total: 1,
        totalPages: 1,
      })
    }

    if (url.includes('/api/branches')) {
      return createJsonResponse({
        items: [
          { id: sourceBranchId, isActive: true, isMain: true, name: 'Sucursal Norte', code: 'NORTE' },
          { id: targetBranchId, isActive: true, isMain: false, name: 'Sucursal Sur', code: 'SUR' },
        ],
        total: 2,
      })
    }

    if (url.includes('/api/catalog/products')) {
      return createJsonResponse({
        hasNextPage: false,
        hasPreviousPage: false,
        items: [
          { id: 'pppppppp-pppp-pppp-pppp-pppppppppppp', isActive: true, name: 'Cafe molido', sku: 'SKU-001' },
        ],
        page: 1,
        pageSize: 200,
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
