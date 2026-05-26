import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SupplierDetailPanel } from '@/modules/suppliers/components/SupplierDetailPanel'
import { useUpdateSupplier } from '@/modules/suppliers/hooks/useSuppliers'
import type { Supplier } from '@/modules/suppliers/types'

vi.mock('@/modules/suppliers/hooks/useSuppliers', () => ({
  useUpdateSupplier: vi.fn(),
}))

vi.mock('@/modules/suppliers/components/SupplierFormDialog', () => ({
  SupplierFormDialog: ({ onOpenChange, onSaved, open, supplier }: any) => (
    open ? (
      <div>
        Supplier form {supplier?.name}
        <button onClick={() => onOpenChange(false)} type="button">Close supplier form</button>
        <button onClick={() => onSaved?.(supplier)} type="button">Save supplier form</button>
      </div>
    ) : null
  ),
}))

describe('SupplierDetailPanel', () => {
  const mutate = vi.fn()
  const onClose = vi.fn()

  beforeEach(() => {
    vi.mocked(useUpdateSupplier).mockReturnValue({
      isPending: false,
      mutate,
    } as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders supplier details, edits and deactivates active suppliers', async () => {
    const user = userEvent.setup()
    mutate.mockImplementationOnce((_payload, options) => options?.onSuccess?.())

    render(<SupplierDetailPanel onClose={onClose} supplier={supplierFixture()} />)

    expect(screen.getAllByText('Proveedor Uno').length).toBeGreaterThan(0)
    expect(screen.getByText('RNC 101010101')).toBeTruthy()
    expect(screen.getByText('Activo')).toBeTruthy()
    expect(screen.getAllByText('compras@proveedor.test').length).toBeGreaterThan(0)
    expect(screen.getByText('Calle 1')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: /Editar/i }))
    expect(screen.getByText('Supplier form Proveedor Uno')).toBeTruthy()
    await user.click(screen.getByText('Save supplier form'))
    expect(screen.getByText('Cambios guardados.')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Desactivar' }))
    expect(mutate).toHaveBeenCalledWith(
      {
        request: {
          address: 'Calle 1',
          email: 'compras@proveedor.test',
          isActive: false,
          name: 'Proveedor Uno',
          phone: '809-555-2222',
          rnc: '101010101',
        },
        supplierId: 'supplier-1',
      },
      expect.any(Object),
    )
    expect(screen.getByText('Proveedor desactivado.')).toBeTruthy()
  })

  it('renders empty values and reactivates inactive suppliers', async () => {
    const user = userEvent.setup()
    mutate.mockImplementationOnce((_payload, options) => options?.onSuccess?.())

    render(
      <SupplierDetailPanel
        onClose={onClose}
        supplier={{
          ...supplierFixture(),
          address: null,
          createdAt: 'not-a-date',
          email: null,
          isActive: false,
          phone: null,
          rnc: null,
          updatedAt: null,
        }}
      />,
    )

    expect(screen.getByText('Inactivo')).toBeTruthy()
    expect(screen.getAllByText('-').length).toBeGreaterThan(0)
    expect(screen.getByText('not-a-date')).toBeTruthy()
    expect(screen.getByText('Sin registro')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Reactivar' }))

    expect(mutate).toHaveBeenCalledWith(
      expect.objectContaining({
        request: expect.objectContaining({ isActive: true }),
        supplierId: 'supplier-1',
      }),
      expect.any(Object),
    )
    expect(screen.getByText('Proveedor reactivado.')).toBeTruthy()
  })

  it('closes the mobile panel and renders nothing without a supplier', async () => {
    const user = userEvent.setup()
    const { container, rerender } = render(
      <SupplierDetailPanel onClose={onClose} supplier={supplierFixture()} />,
    )

    await user.click(screen.getByRole('button', { name: 'Cerrar panel' }))
    expect(onClose).toHaveBeenCalled()

    rerender(<SupplierDetailPanel onClose={onClose} supplier={null} />)
    expect(container).toBeEmptyDOMElement()
  })
})

function supplierFixture(): Supplier {
  return {
    address: 'Calle 1',
    businessId: 'business-1',
    createdAt: '2026-01-02T10:30:00.000Z',
    email: 'compras@proveedor.test',
    id: 'supplier-1',
    isActive: true,
    name: 'Proveedor Uno',
    phone: '809-555-2222',
    rnc: '101010101',
    updatedAt: '2026-01-03T11:45:00.000Z',
  }
}
