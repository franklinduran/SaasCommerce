import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { PermissionRoute } from '@/modules/auth/components/PermissionRoute'
import type { CurrentUserPermissionsResponse } from '@/shared/types/permissions'
import { Permission } from '@/shared/types/permissions'

const SESSION = {
  accessToken: 'jwt',
  expiresAt: '2099-01-01T00:00:00Z',
  refreshToken: 'refresh',
  user: {
    branchId: '22222222-2222-2222-2222-222222222222',
    businessId: '11111111-1111-1111-1111-111111111111',
    email: 'owner@test.com',
    fullName: 'Owner',
    id: '33333333-3333-3333-3333-333333333333',
    roles: ['Owner'],
  },
}

function stubPermissions(permissions: string[]) {
  const payload: CurrentUserPermissionsResponse = {
    businessId: SESSION.user.businessId,
    permissions,
    role: 'Owner',
    userId: SESSION.user.id,
  }

  vi.stubGlobal('fetch', vi.fn(async () => ({
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => ({ correlationId: 'test', data: payload, error: null, isSuccess: true }),
    ok: true,
    status: 200,
  })))
}

function renderWithPermission(
  requiredPermission: Parameters<typeof PermissionRoute>[0]['permissions'],
) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route
            element={
              <PermissionRoute permissions={requiredPermission}>
                <div>Protected content</div>
              </PermissionRoute>
            }
            path="/protected"
          />
          <Route element={<div>Forbidden page</div>} path="/forbidden" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PermissionRoute', () => {
  beforeEach(() => {
    useAuthStore.getState().setSession(SESSION)
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders children when user holds the required permission', async () => {
    stubPermissions([Permission.UsersView, Permission.BranchesView])

    renderWithPermission(Permission.UsersView)

    expect(await screen.findByText('Protected content')).toBeTruthy()
    expect(screen.queryByText('Forbidden page')).toBeNull()
  })

  it('redirects to /forbidden when user lacks the required permission', async () => {
    stubPermissions([Permission.SalesView]) // does NOT include UsersView

    renderWithPermission(Permission.UsersView)

    expect(await screen.findByText('Forbidden page')).toBeTruthy()
    expect(screen.queryByText('Protected content')).toBeNull()
  })

  it('accepts an array of permissions and grants access if user holds any one', async () => {
    // User only has ReportsView, not AuditView
    stubPermissions([Permission.ReportsView])

    renderWithPermission([Permission.AuditView, Permission.ReportsView])

    expect(await screen.findByText('Protected content')).toBeTruthy()
  })

  it('redirects to /forbidden when user holds none of the required array permissions', async () => {
    stubPermissions([Permission.SalesView])

    renderWithPermission([Permission.AuditView, Permission.UsersView])

    expect(await screen.findByText('Forbidden page')).toBeTruthy()
  })
})
