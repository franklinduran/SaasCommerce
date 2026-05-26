import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { UserDetailPanel } from '@/modules/users/components/UserDetailPanel'
import { usersApi } from '@/modules/users/services/usersApi'
import type { ResetPasswordResponse, UserDetail } from '@/modules/users/types'

vi.mock('@/modules/users/components/RoleSelect', () => ({
  RoleSelect: ({
    disabled,
    onValueChange,
    value,
  }: {
    disabled?: boolean
    onValueChange: (value: string) => void
    value: string
  }) => (
    <select
      aria-label="Rol asignado"
      disabled={disabled}
      onChange={(event) => onValueChange(event.target.value)}
      value={value}
    >
      <option value="Admin">Admin</option>
      <option value="Supervisor">Supervisor</option>
      <option value="Cashier">Cashier</option>
    </select>
  ),
}))

vi.mock('@/modules/users/components/ResetPasswordDialog', () => ({
  ResetPasswordDialog: ({
    onConfirm,
    open,
    temporaryPassword,
  }: {
    onConfirm: () => void
    open: boolean
    temporaryPassword: string
  }) => open ? (
    <div role="dialog">
      <p>Clave temporal: {temporaryPassword}</p>
      <button onClick={onConfirm} type="button">Confirmar clave</button>
    </div>
  ) : null,
}))

vi.mock('@/modules/users/services/usersApi', () => ({
  usersApi: {
    activateUser: vi.fn(),
    disableUser: vi.fn(),
    getUserById: vi.fn(),
    resetPassword: vi.fn(),
    updateUser: vi.fn(),
    updateUserRole: vi.fn(),
  },
}))

describe('UserDetailPanel', () => {
  const onClose = vi.fn()
  const onUpdated = vi.fn()

  beforeEach(() => {
    vi.mocked(usersApi.getUserById).mockResolvedValue(userDetail())
    vi.mocked(usersApi.updateUser).mockResolvedValue(undefined)
    vi.mocked(usersApi.updateUserRole).mockResolvedValue(undefined)
    vi.mocked(usersApi.disableUser).mockResolvedValue(undefined)
    vi.mocked(usersApi.activateUser).mockResolvedValue(undefined)
    vi.mocked(usersApi.resetPassword).mockResolvedValue({
      temporaryPassword: 'Temp123!',
      userId: 'user-1',
    } as ResetPasswordResponse)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('loads a user, updates profile data and changes role', async () => {
    const user = userEvent.setup()
    render(<UserDetailPanel onClose={onClose} onUpdated={onUpdated} userId="user-1" />)

    expect(await screen.findByText('Ana Admin')).toBeTruthy()
    expect(screen.getByText('Administrador')).toBeTruthy()

    await user.clear(screen.getByLabelText('Nombre completo'))
    await user.type(screen.getByLabelText('Nombre completo'), 'Ana Maria')
    await user.click(screen.getByRole('button', { name: 'Guardar cambios' }))

    await waitFor(() => {
      expect(usersApi.updateUser).toHaveBeenCalledWith('user-1', {
        fullName: 'Ana Maria',
        phone: '8090000000',
      })
    })
    expect(onUpdated).toHaveBeenCalled()

    await user.selectOptions(screen.getByLabelText('Rol asignado'), 'Supervisor')
    expect(screen.getByText('Cambio pendiente: Supervisor')).toBeTruthy()
    await user.click(screen.getByRole('button', { name: 'Aplicar' }))

    await waitFor(() => {
      expect(usersApi.updateUserRole).toHaveBeenCalledWith('user-1', 'Supervisor')
    })
  })

  it('resets password, disables active users and closes from the header', async () => {
    const user = userEvent.setup()
    render(<UserDetailPanel onClose={onClose} onUpdated={onUpdated} userId="user-1" />)
    await screen.findByText('Ana Admin')

    await user.click(screen.getByRole('button', { name: 'Generar nueva' }))
    expect(await screen.findByText('Clave temporal: Temp123!')).toBeTruthy()
    await user.click(screen.getByRole('button', { name: 'Confirmar clave' }))
    expect(screen.getByText('Contrasena temporal generada.')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Desactivar' }))
    expect(screen.getByText('Desactivar usuario')).toBeTruthy()
    await user.click(screen.getByRole('button', { name: /^Desactivar$/ }))

    await waitFor(() => expect(usersApi.disableUser).toHaveBeenCalledWith('user-1'))

    await user.click(screen.getByLabelText('Cerrar panel'))
    expect(onClose).toHaveBeenCalled()
  })

  it('reactivates inactive users and shows load errors', async () => {
    const user = userEvent.setup()
    vi.mocked(usersApi.getUserById).mockResolvedValue(userDetail({ isActive: false }))

    render(<UserDetailPanel onClose={onClose} onUpdated={onUpdated} userId="user-1" />)

    expect(await screen.findByText('Inactivo')).toBeTruthy()
    await user.click(screen.getByRole('button', { name: 'Reactivar' }))

    await waitFor(() => expect(usersApi.activateUser).toHaveBeenCalledWith('user-1'))
  })

  it('shows an empty/error state when loading fails', async () => {
    vi.mocked(usersApi.getUserById).mockRejectedValue(new Error('No existe'))

    render(<UserDetailPanel onClose={onClose} onUpdated={onUpdated} userId="missing" />)

    expect(await screen.findByText('No existe')).toBeTruthy()
    await userEvent.click(screen.getByRole('button', { name: 'Volver' }))
    expect(onClose).toHaveBeenCalled()
  })
})

function userDetail(overrides: Partial<UserDetail> = {}): UserDetail {
  return {
    businessId: 'business-1',
    createdAt: '2026-05-25T12:00:00Z',
    email: 'ana@test.com',
    fullName: 'Ana Admin',
    isActive: true,
    mustChangePassword: true,
    phone: '8090000000',
    role: 'Admin',
    updatedAt: '2026-05-25T13:00:00Z',
    userId: 'user-1',
    ...overrides,
  }
}
