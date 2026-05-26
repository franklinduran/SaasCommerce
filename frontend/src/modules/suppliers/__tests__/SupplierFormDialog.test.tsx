import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SupplierFormDialog } from '@/modules/suppliers/components/SupplierFormDialog'
import { useCreateSupplier, useUpdateSupplier } from '@/modules/suppliers/hooks/useSuppliers'
import type { Supplier } from '@/modules/suppliers/types'
import { HttpClientError } from '@/shared/services/httpClient'

vi.mock('@/modules/suppliers/hooks/useSuppliers', () => ({
  useCreateSupplier: vi.fn(),
  useUpdateSupplier: vi.fn(),
}))

describe('SupplierFormDialog behavior', () => {
  const createSupplier = vi.fn()
  const updateSupplier = vi.fn()
  const onOpenChange = vi.fn()
  const onSaved = vi.fn()

  beforeEach(() => {
    vi.mocked(useCreateSupplier).mockReturnValue({
      isPending: false,
      mutateAsync: createSupplier,
    } as never)
    vi.mocked(useUpdateSupplier).mockReturnValue({
      isPending: false,
      mutateAsync: updateSupplier,
    } as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('creates suppliers with normalized optional fields', async () => {
    const user = userEvent.setup()
    const created = supplierFixture({ id: 'created' })
    createSupplier.mockResolvedValueOnce(created)

    renderDialog(null)

    await user.type(screen.getByLabelText(/Nombre/), '  Proveedor Nuevo  ')
    await user.type(screen.getByLabelText(/RNC/), '  123456789  ')
    await user.type(screen.getByLabelText(/Telefono/), '   ')
    await user.type(screen.getByLabelText(/Correo/), 'nuevo@proveedor.test')
    await user.type(screen.getByLabelText(/Direccion/), '  Calle 10  ')
    await user.click(screen.getByRole('button', { name: 'Crear proveedor' }))

    await waitFor(() => {
      expect(createSupplier).toHaveBeenCalledWith({
        address: 'Calle 10',
        email: 'nuevo@proveedor.test',
        name: 'Proveedor Nuevo',
        phone: null,
        rnc: '123456789',
      })
    })
    expect(onSaved).toHaveBeenCalledWith(created)
  })

  it('updates suppliers and includes active state', async () => {
    const user = userEvent.setup()
    updateSupplier.mockResolvedValueOnce(undefined)

    renderDialog(supplierFixture())

    const name = screen.getByLabelText(/Nombre/)
    await user.clear(name)
    await user.type(name, '  Proveedor Editado  ')
    await user.click(screen.getByLabelText('Proveedor activo'))
    await user.click(screen.getByRole('button', { name: 'Guardar cambios' }))

    await waitFor(() => {
      expect(updateSupplier).toHaveBeenCalledWith({
        request: {
          address: 'Calle 1',
          email: 'proveedor@test.com',
          isActive: false,
          name: 'Proveedor Editado',
          phone: '809-555-1111',
          rnc: '101010101',
        },
        supplierId: 'supplier-1',
      })
    })
    expect(onSaved).toHaveBeenCalledWith(supplierFixture())
  })

  it('shows api errors and disables inputs while pending', async () => {
    const user = userEvent.setup()
    createSupplier.mockRejectedValueOnce(
      new HttpClientError('Bad request', 400, { code: 'SupplierExists', message: 'Proveedor duplicado' }),
    )

    const { rerender } = renderDialog(null)
    await user.type(screen.getByLabelText(/Nombre/), 'Proveedor')
    await user.click(screen.getByRole('button', { name: 'Crear proveedor' }))
    expect(await screen.findByText('Proveedor duplicado')).toBeTruthy()

    vi.mocked(useCreateSupplier).mockReturnValueOnce({
      isPending: true,
      mutateAsync: createSupplier,
    } as never)
    rerender(
      <SupplierFormDialog
        onOpenChange={onOpenChange}
        onSaved={onSaved}
        open
        supplier={null}
      />,
    )

    expect(screen.getByRole('button', { name: 'Guardando...' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Cancelar' })).toBeDisabled()
    expect(screen.getByLabelText(/Nombre/)).toBeDisabled()
  })

  function renderDialog(supplier: Supplier | null) {
    return render(
      <SupplierFormDialog
        onOpenChange={onOpenChange}
        onSaved={onSaved}
        open
        supplier={supplier}
      />,
    )
  }
})

function supplierFixture(overrides: Partial<Supplier> = {}): Supplier {
  return {
    address: 'Calle 1',
    businessId: 'business-1',
    createdAt: '2026-01-01T00:00:00.000Z',
    email: 'proveedor@test.com',
    id: 'supplier-1',
    isActive: true,
    name: 'Proveedor Uno',
    phone: '809-555-1111',
    rnc: '101010101',
    updatedAt: '2026-01-01T00:00:00.000Z',
    ...overrides,
  }
}
