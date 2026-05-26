import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import UsersPage from '@/modules/users/UsersPage'
import { usersApi } from '@/modules/users/services/usersApi'
import type { UserListResponse, UserSummary } from '@/modules/users/types'

vi.mock('@/modules/users/components/CreateUserDialog', () => ({
  CreateUserDialog: ({
    onOpenChange,
    onSuccess,
    open,
  }: {
    onOpenChange: (open: boolean) => void
    onSuccess: (userId?: string) => void
    open: boolean
  }) => open ? (
    <div role="dialog">
      <button onClick={() => onSuccess('new-user')} type="button">Guardar usuario</button>
      <button onClick={() => onOpenChange(false)} type="button">Cerrar crear usuario</button>
    </div>
  ) : null,
}))

vi.mock('@/modules/users/components/UserDetailPanel', () => ({
  UserDetailPanel: ({
    onClose,
    onUpdated,
    userId,
  }: {
    onClose: () => void
    onUpdated: () => void
    userId: string
  }) => (
    <div>
      <p>Detalle {userId}</p>
      <button onClick={onClose} type="button">Cerrar detalle</button>
      <button onClick={onUpdated} type="button">Actualizar detalle</button>
    </div>
  ),
}))

vi.mock('@/modules/users/components/UserListItem', () => ({
  UserListItem: ({
    onClick,
    selected,
    user,
  }: {
    onClick: () => void
    selected: boolean
    user: UserSummary
  }) => (
    <li>
      <button aria-pressed={selected} onClick={onClick} type="button">
        {user.fullName} {user.email} {user.role} {user.isActive ? 'Activo' : 'Inactivo'}
      </button>
    </li>
  ),
}))

vi.mock('@/modules/users/services/usersApi', () => ({
  usersApi: {
    getUsers: vi.fn(),
  },
}))

describe('UsersPage', () => {
  beforeEach(() => {
    vi.mocked(usersApi.getUsers).mockResolvedValue(userList())
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('loads users, shows stats and selects a user', async () => {
    const user = userEvent.setup()
    render(<UsersPage />)

    expect(await screen.findByText('Usuarios')).toBeTruthy()
    expect(screen.getByText('Total de usuarios')).toBeTruthy()
    expect(screen.getByText('Administradores')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Ana Admin/ })).toBeTruthy()
    expect(screen.getByText('Mostrando 3 de 3 usuarios')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: /Ana Admin/ }))
    expect(screen.getByText('Detalle user-1')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Cerrar detalle' }))
    expect(screen.getByText('Selecciona un usuario')).toBeTruthy()
  })

  it('filters by search, role and status', async () => {
    const user = userEvent.setup()
    render(<UsersPage />)
    await screen.findByRole('button', { name: /Ana Admin/ })

    await user.type(screen.getByPlaceholderText('Buscar por nombre o correo'), 'caja')
    expect(screen.getByText('Mostrando 1 de 3 usuarios')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Carlos Caja/ })).toBeTruthy()

    await user.clear(screen.getByPlaceholderText('Buscar por nombre o correo'))
    await user.click(screen.getByRole('combobox', { name: 'Filtrar por rol' }))
    await user.click(screen.getByRole('option', { name: 'Supervisor' }))
    expect(screen.getByText('Mostrando 1 de 3 usuarios')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Sofia Supervisor/ })).toBeTruthy()

    await user.click(screen.getByRole('combobox', { name: 'Filtrar por estado' }))
    await user.click(screen.getByRole('option', { name: 'Inactivos' }))
    expect(screen.getByText('Sin resultados')).toBeTruthy()
  })

  it('shows empty state and opens create dialog', async () => {
    const user = userEvent.setup()
    vi.mocked(usersApi.getUsers).mockResolvedValue({ items: [], totalItems: 0 })

    render(<UsersPage />)

    expect(await screen.findByText('Aun no hay usuarios')).toBeTruthy()
    await user.click(screen.getAllByRole('button', { name: 'Crear usuario' })[0])
    expect(screen.getByRole('dialog')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Guardar usuario' }))
    await waitFor(() => expect(usersApi.getUsers).toHaveBeenCalledTimes(2))
  })

  it('shows error state and retries loading', async () => {
    const user = userEvent.setup()
    vi.mocked(usersApi.getUsers)
      .mockRejectedValueOnce(new Error('No se pudo cargar'))
      .mockResolvedValueOnce(userList())

    render(<UsersPage />)

    expect(await screen.findByText('No se pudo cargar')).toBeTruthy()
    await user.click(screen.getByRole('button', { name: 'Reintentar' }))
    expect(await screen.findByRole('button', { name: /Ana Admin/ })).toBeTruthy()
  })
})

function userList(): UserListResponse {
  return {
    items: [
      userSummary('user-1', 'Ana Admin', 'ana@test.com', 'Admin', true),
      userSummary('user-2', 'Sofia Supervisor', 'sofia@test.com', 'Supervisor', true),
      userSummary('user-3', 'Carlos Caja', 'caja@test.com', 'Cashier', false),
    ],
    totalItems: 3,
  }
}

function userSummary(
  userId: string,
  fullName: string,
  email: string,
  role: string,
  isActive: boolean,
): UserSummary {
  return {
    createdAt: '2026-05-25T12:00:00Z',
    email,
    fullName,
    isActive,
    phone: '8090000000',
    role,
    userId,
  }
}
