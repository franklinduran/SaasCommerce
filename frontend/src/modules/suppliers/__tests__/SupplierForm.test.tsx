import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { SupplierFormDialog } from '@/modules/suppliers/components/SupplierFormDialog'

describe('SupplierFormDialog', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('should show validation errors', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', vi.fn())
    renderDialog()

    await user.click(screen.getByRole('button', { name: /crear proveedor/i }))

    expect(await screen.findByText('Nombre requerido')).toBeTruthy()
  })
})

function renderDialog() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-17T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: '22222222-2222-4222-8222-222222222222',
      businessId: '11111111-1111-4111-8111-111111111111',
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '33333333-3333-4333-8333-333333333333',
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
      <SupplierFormDialog
        onOpenChange={() => undefined}
        open
        supplier={null}
      />
    </QueryClientProvider>,
  )
}
