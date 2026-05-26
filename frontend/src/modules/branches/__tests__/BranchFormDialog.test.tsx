import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { BranchFormDialog } from '@/modules/branches/components/BranchFormDialog'
import {
  useCreateBranchMutation,
  useUpdateBranchMutation,
} from '@/modules/branches/hooks/useBranches'
import type { Branch } from '@/modules/branches/types'
import { HttpClientError } from '@/shared/services/httpClient'

vi.mock('@/modules/branches/hooks/useBranches', () => ({
  useCreateBranchMutation: vi.fn(),
  useUpdateBranchMutation: vi.fn(),
}))

describe('BranchFormDialog', () => {
  const createBranch = vi.fn()
  const updateBranch = vi.fn()
  const onOpenChange = vi.fn()
  const onSaved = vi.fn()

  beforeEach(() => {
    vi.mocked(useCreateBranchMutation).mockReturnValue({
      isPending: false,
      mutateAsync: createBranch,
    } as never)
    vi.mocked(useUpdateBranchMutation).mockReturnValue({
      isPending: false,
      mutateAsync: updateBranch,
    } as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('creates a branch with normalized values', async () => {
    const user = userEvent.setup()
    createBranch.mockResolvedValueOnce(undefined)

    renderDialog(null)

    await user.type(screen.getByLabelText(/^Nombre/), '  Almacen Norte  ')
    await user.type(screen.getByLabelText(/C.*digo/), 'ALM02')
    await user.type(screen.getByLabelText(/Tel/), '   ')
    await user.type(screen.getByLabelText(/Direcci/), '  Calle 2  ')
    await user.click(screen.getByLabelText('Sucursal principal'))
    await user.click(screen.getByRole('button', { name: 'Crear sucursal' }))

    await waitFor(() => {
      expect(createBranch).toHaveBeenCalledWith({
        address: 'Calle 2',
        code: 'ALM02',
        isMain: true,
        name: 'Almacen Norte',
        phone: null,
      })
    })
    expect(onSaved).toHaveBeenCalled()
    expect(onOpenChange).toHaveBeenCalledWith(false)
  })

  it('updates an existing branch and keeps its id in the request', async () => {
    const user = userEvent.setup()
    updateBranch.mockResolvedValueOnce(undefined)

    renderDialog(branchFixture())

    const name = screen.getByLabelText(/^Nombre/)
    await user.clear(name)
    await user.type(name, '  Principal renovada  ')
    await user.clear(screen.getByLabelText(/Tel/))
    await user.type(screen.getByLabelText(/Tel/), '809-555-0000')
    await user.clear(screen.getByLabelText(/Direcci/))
    await user.click(screen.getByRole('button', { name: 'Guardar cambios' }))

    await waitFor(() => {
      expect(updateBranch).toHaveBeenCalledWith({
        branchId: 'branch-1',
        request: {
          address: null,
          name: 'Principal renovada',
          phone: '809-555-0000',
        },
      })
    })
    expect(onSaved).toHaveBeenCalled()
    expect(onOpenChange).toHaveBeenCalledWith(false)
  })

  it('shows api validation errors on create and update', async () => {
    const user = userEvent.setup()
    createBranch.mockRejectedValueOnce(
      new HttpClientError('Bad request', 400, { code: 'BranchExists', message: 'CÃ³digo duplicado' }),
    )

    const { rerender } = renderDialog(null)
    await user.type(screen.getByLabelText(/^Nombre/), 'Sucursal')
    await user.type(screen.getByLabelText(/C.*digo/), 'DUP')
    await user.click(screen.getByRole('button', { name: 'Crear sucursal' }))

    expect(await screen.findByText('CÃ³digo duplicado')).toBeTruthy()

    updateBranch.mockRejectedValueOnce(new Error('boom'))
    rerender(
      <BranchFormDialog
        branch={branchFixture()}
        onOpenChange={onOpenChange}
        onSaved={onSaved}
        open
      />,
    )
    await user.click(screen.getByRole('button', { name: 'Guardar cambios' }))

    expect(await screen.findByText('No se pudo actualizar la sucursal.')).toBeTruthy()
  })

  it('disables controls while a mutation is pending', () => {
    vi.mocked(useCreateBranchMutation).mockReturnValue({
      isPending: true,
      mutateAsync: createBranch,
    } as never)

    renderDialog(null)

    expect(screen.getByRole('button', { name: 'Guardando...' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Cancelar' })).toBeDisabled()
    expect(screen.getByLabelText(/^Nombre/)).toBeDisabled()
  })

  function renderDialog(branch: Branch | null) {
    return render(
      <BranchFormDialog
        branch={branch}
        onOpenChange={onOpenChange}
        onSaved={onSaved}
        open
      />,
    )
  }
})

function branchFixture(): Branch {
  return {
    address: 'Calle 1',
    businessId: 'business-1',
    code: 'MAIN',
    createdAt: '2026-01-01T00:00:00.000Z',
    id: 'branch-1',
    isActive: true,
    isMain: true,
    name: 'Principal',
    phone: '809-555-1111',
    updatedAt: '2026-01-01T00:00:00.000Z',
  }
}
