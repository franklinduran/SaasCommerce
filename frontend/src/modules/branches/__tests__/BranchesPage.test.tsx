import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { BranchesPage } from '@/modules/branches/pages/BranchesPage'

const branchId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
const businessId = '11111111-1111-1111-1111-111111111111'

describe('BranchesPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('should render branch list', async () => {
    vi.stubGlobal('fetch', createBranchFetchMock())
    renderBranchesPage()

    expect(await screen.findByText('Sucursal Principal')).toBeTruthy()
    expect(screen.getByText('PRINCIPAL')).toBeTruthy()
  })

  it('should show principal badge for main branch', async () => {
    vi.stubGlobal('fetch', createBranchFetchMock())
    renderBranchesPage()

    expect(await screen.findByText('Principal')).toBeTruthy()
  })

  it('should disable deactivate button for main branch', async () => {
    vi.stubGlobal('fetch', createBranchFetchMock())
    renderBranchesPage()

    await screen.findByText('Sucursal Principal')
    const deactivateButtons = screen.queryAllByRole('button', { name: 'Desactivar' })

    // Main branch should not have a deactivate button (or it is disabled)
    deactivateButtons.forEach((btn) => {
      expect((btn as HTMLButtonElement).disabled).toBe(true)
    })
  })

  it('should open create dialog when clicking Nueva sucursal', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createBranchFetchMock())
    renderBranchesPage()

    await user.click(await screen.findByRole('button', { name: /Nueva sucursal/ }))

    // "Nueva sucursal" appears in button and dialog title — check dialog is present
    expect(screen.getAllByText('Nueva sucursal').length).toBeGreaterThanOrEqual(2)
    expect(await screen.findByLabelText('Nombre *')).toBeTruthy()
    expect(screen.getByLabelText('Código *')).toBeTruthy()
  })

  it('should validate required fields in create form', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createBranchFetchMock())
    renderBranchesPage()

    await user.click(await screen.findByRole('button', { name: /Nueva sucursal/ }))
    // Dialog description uniquely identifies the open dialog
    await screen.findByText('Completa los datos para registrar una nueva sucursal.')
    await user.click(screen.getByRole('button', { name: 'Crear sucursal' }))

    expect(await screen.findByText('El nombre es obligatorio')).toBeTruthy()
    expect(screen.getByText('El código es obligatorio')).toBeTruthy()
  })
})

vi.mock('@/shared/services/signalrClient', () => ({
  onRealtimeEvent: vi.fn(),
  offRealtimeEvent: vi.fn(),
}))

function renderBranchesPage() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-21T23:59:00Z',
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

  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  const router = createMemoryRouter(
    [{ element: <BranchesPage />, path: '/branches' }],
    { initialEntries: ['/branches'] },
  )

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

function createBranchFetchMock() {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/branches')) {
      return createJsonResponse({
        items: [
          {
            address: 'Calle Principal 123',
            businessId,
            code: 'PRINCIPAL',
            createdAt: '2026-01-01T00:00:00Z',
            id: branchId,
            isActive: true,
            isMain: true,
            name: 'Sucursal Principal',
            phone: '809-555-0100',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ],
        total: 1,
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
