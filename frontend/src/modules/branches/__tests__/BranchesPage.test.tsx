import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { BranchesPage } from '@/modules/branches/pages/BranchesPage'
import {
  useActivateBranchMutation,
  useBranches,
  useDeactivateBranchMutation,
} from '@/modules/branches/hooks/useBranches'

vi.mock('@/modules/branches/hooks/useBranches', () => ({
  useActivateBranchMutation: vi.fn(),
  useBranches: vi.fn(),
  useDeactivateBranchMutation: vi.fn(),
}))

vi.mock('@/modules/branches/components/BranchFormDialog', () => ({
  BranchFormDialog: ({ branch, onOpenChange, onSaved, open }: any) => (
    open ? (
      <div>
        Branch form {branch?.name ?? 'new'}
        <button onClick={onSaved} type="button">Save branch</button>
        <button onClick={() => onOpenChange(false)} type="button">Close branch form</button>
      </div>
    ) : null
  ),
}))

vi.mock('@/modules/branches/components/BranchStatusBadge', () => ({
  BranchStatusBadge: ({ isActive, isMain }: any) => (
    <span>{isMain ? 'Principal' : isActive ? 'Activa' : 'Inactiva'}</span>
  ),
}))

describe('BranchesPage', () => {
  const refetch = vi.fn()
  const activate = vi.fn()
  const deactivate = vi.fn()

  beforeEach(() => {
    vi.mocked(useBranches).mockReturnValue(result(branchData()) as never)
    vi.mocked(useActivateBranchMutation).mockReturnValue({ isPending: false, mutateAsync: activate } as never)
    vi.mocked(useDeactivateBranchMutation).mockReturnValue({ isPending: false, mutateAsync: deactivate } as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders branch rows and opens create/edit dialogs', async () => {
    const user = userEvent.setup()
    render(<BranchesPage />)

    expect(screen.getByText('Sucursales')).toBeTruthy()
    expect(screen.getAllByText('Principal').length).toBeGreaterThan(0)
    expect(screen.getByText('Almacen')).toBeTruthy()
    await user.click(screen.getByRole('button', { name: /Nueva sucursal/i }))
    expect(screen.getByText('Branch form new')).toBeTruthy()
    await user.click(screen.getByText('Close branch form'))

    await user.click(screen.getAllByRole('button', { name: /Editar/i })[1])
    expect(screen.getByText('Branch form Almacen')).toBeTruthy()
    await user.click(screen.getByText('Save branch'))
    expect(refetch).toHaveBeenCalled()
  })

  it('activates, deactivates and surfaces mutation errors', async () => {
    const user = userEvent.setup()
    deactivate.mockRejectedValueOnce(new Error('fail'))
    render(<BranchesPage />)

    await user.click(screen.getAllByRole('button', { name: /Desactivar/i })[1])
    expect(await screen.findByText('No se pudo desactivar la sucursal.')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: /^Activar$/i }))
    expect(activate).toHaveBeenCalledWith('branch-3')
  })

  it('renders loading, error and empty states', () => {
    vi.mocked(useBranches).mockReturnValueOnce(result(undefined, { isLoading: true }) as never)
    const { rerender } = render(<BranchesPage />)
    expect(screen.getByText('Cargando sucursales...')).toBeTruthy()

    vi.mocked(useBranches).mockReturnValueOnce(result(undefined, { isError: true }) as never)
    rerender(<BranchesPage />)
    expect(screen.getByText('No se pudieron cargar las sucursales.')).toBeTruthy()

    vi.mocked(useBranches).mockReturnValueOnce(result({ items: [], total: 0 }) as never)
    rerender(<BranchesPage />)
    expect(screen.getByText('No hay sucursales')).toBeTruthy()
  })

  function result(data: unknown, overrides: Record<string, unknown> = {}) {
    return {
      data,
      isError: false,
      isLoading: false,
      refetch,
      ...overrides,
    }
  }
})

function branchData() {
  return {
    items: [
      { address: null, code: 'MAIN', id: 'branch-1', isActive: true, isMain: true, name: 'Principal', phone: null },
      { address: 'Calle 1', code: 'ALM', id: 'branch-2', isActive: true, isMain: false, name: 'Almacen', phone: '809' },
      { address: null, code: 'OLD', id: 'branch-3', isActive: false, isMain: false, name: 'Vieja', phone: null },
    ],
    total: 3,
  }
}
