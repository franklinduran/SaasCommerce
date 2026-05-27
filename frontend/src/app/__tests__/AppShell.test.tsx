import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { AppShell } from '@/app/AppShell'
import { logout as serverLogout } from '@/modules/auth/services/authService'
import { Permission } from '@/shared/types/permissions'

const clearSession = vi.fn()
const toggleSidebar = vi.fn()
let sidebarCollapsed = false
let mustChangePassword = false

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: (selector: (state: unknown) => unknown) => selector({
    clearSession,
    session: {
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      user: {
        businessId: 'business-1',
        fullName: 'Ana Admin',
        mustChangePassword,
      },
    },
  }),
}))

vi.mock('@/modules/auth/services/authService', () => ({
  logout: vi.fn(),
}))

vi.mock('@/modules/notifications/components/NotificationBell', () => ({
  NotificationBell: () => <button type="button">Notificaciones</button>,
}))

vi.mock('@/modules/subscription/components/SubscriptionAlertBanner', () => ({
  SubscriptionAlertBanner: ({
    onChoosePlan,
    onContactSupport,
    onDismiss,
    onReactivateClick,
  }: {
    onChoosePlan: () => void
    onContactSupport: () => void
    onDismiss: () => void
    onReactivateClick: () => void
  }) => (
    <div>
      <p>Banner suscripcion</p>
      <button onClick={onChoosePlan} type="button">Elegir plan</button>
      <button onClick={onReactivateClick} type="button">Reactivar plan</button>
      <button onClick={onContactSupport} type="button">Contactar soporte</button>
      <button onClick={onDismiss} type="button">Ocultar banner</button>
    </div>
  ),
}))

vi.mock('@/shared/hooks/useAppStore', () => ({
  useAppStore: (selector: (state: unknown) => unknown) => selector({
    businessName: 'Colmado Test',
    sidebarCollapsed,
    toggleSidebar,
  }),
}))

vi.mock('@/shared/hooks/usePermissions', () => ({
  useCurrentUserPermissions: () => ({
    data: {
      permissions: [
        Permission.DashboardView,
        Permission.SalesCreate,
        Permission.CashView,
        Permission.UsersView,
      ],
    },
  }),
}))

describe('AppShell', () => {
  beforeEach(() => {
    vi.mocked(serverLogout).mockResolvedValue(undefined)
    sidebarCollapsed = false
    mustChangePassword = false
    sessionStorage.clear()
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders navigation, banner and outlet content', async () => {
    const user = userEvent.setup()
    renderShell('/')

    expect(screen.getAllByText('Colmado Test').length).toBeGreaterThan(0)
    expect(screen.getByRole('link', { name: /Inicio/ })).toBeTruthy()
    expect(screen.getByRole('link', { name: /POS/ })).toBeTruthy()
    expect(screen.queryByRole('link', { name: /Productos/ })).toBeNull()
    expect(screen.getByText('Banner suscripcion')).toBeTruthy()
    expect(screen.getByText('Outlet /')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Ocultar banner' }))
    expect(sessionStorage.getItem('subscription-banner-dismissed')).toBe('true')
  })

  it('toggles sidebar, navigates from the banner and logs out', async () => {
    const user = userEvent.setup()
    renderShell('/')

    await user.click(screen.getByRole('button', { name: 'Contraer menu' }))
    expect(toggleSidebar).toHaveBeenCalled()

    await user.click(screen.getByRole('button', { name: 'Elegir plan' }))
    expect(await screen.findByText('Outlet /subscription')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Abrir menu de usuario' }))
    await user.click(screen.getByRole('menuitem', { name: 'Cerrar sesion' }))
    await waitFor(() => {
      expect(serverLogout).toHaveBeenCalledWith('access-token', 'refresh-token')
      expect(clearSession).toHaveBeenCalled()
    })
    expect(await screen.findByText('Login page')).toBeTruthy()
  })

  it('redirects users that must change password', async () => {
    mustChangePassword = true

    renderShell('/cash/session-1')

    expect(await screen.findByText('Change password page')).toBeTruthy()
  })

  it('supports collapsed navigation', () => {
    sidebarCollapsed = true

    renderShell('/cash/session-1')

    expect(screen.getByRole('button', { name: 'Expandir menu' })).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Abrir menu de usuario' })).toBeTruthy()
    expect(screen.getByText('Detalle de caja')).toBeTruthy()
  })

  it('opens the user menu and navigates to account settings', async () => {
    const user = userEvent.setup()

    renderShell('/')

    await user.click(screen.getByRole('button', { name: 'Abrir menu de usuario' }))

    expect(screen.getByRole('menu', { name: 'Menu de usuario' })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: 'Usuarios y roles' })).toBeTruthy()

    await user.click(screen.getByRole('menuitem', { name: 'Ajustes de cuenta' }))

    expect(await screen.findByText('Outlet /settings')).toBeTruthy()
  })

  it('keeps logout local even if server logout fails', async () => {
    vi.mocked(serverLogout).mockRejectedValue(new Error('offline'))
    const user = userEvent.setup()

    renderShell('/users')
    expect(screen.getAllByText('Usuarios').length).toBeGreaterThan(0)

    await user.click(screen.getByRole('button', { name: 'Abrir menu de usuario' }))
    await user.click(screen.getByRole('menuitem', { name: 'Cerrar sesion' }))
    await waitFor(() => expect(clearSession).toHaveBeenCalled())
    expect(await screen.findByText('Login page')).toBeTruthy()
  })
})

function renderShell(initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route element={<AppShell />}>
          <Route path="/" element={<p>Outlet /</p>} />
          <Route path="/cash/:id" element={<p>Outlet cash detail</p>} />
          <Route path="/settings" element={<p>Outlet /settings</p>} />
          <Route path="/subscription" element={<p>Outlet /subscription</p>} />
          <Route path="/users" element={<p>Outlet /users</p>} />
        </Route>
        <Route path="/change-password" element={<p>Change password page</p>} />
        <Route path="/login" element={<p>Login page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}
