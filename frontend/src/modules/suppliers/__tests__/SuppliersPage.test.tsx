import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SuppliersPage } from '@/modules/suppliers/SuppliersPage'
import { useSuppliers } from '@/modules/suppliers/hooks/useSuppliers'

vi.mock('@/modules/suppliers/hooks/useSuppliers', () => ({
  useSuppliers: vi.fn(),
}))

vi.mock('@/modules/suppliers/components/SupplierListItem', () => ({
  SupplierListItem: ({ onClick, selected, supplier }: any) => (
    <button data-selected={selected} onClick={onClick} type="button">
      {supplier.name}
    </button>
  ),
}))

vi.mock('@/modules/suppliers/components/SupplierDetailPanel', () => ({
  SupplierDetailPanel: ({ onClose, supplier }: any) => (
    <div>
      Supplier detail {supplier?.name}
      <button onClick={onClose} type="button">Close supplier</button>
    </div>
  ),
}))

vi.mock('@/modules/suppliers/components/SupplierFormDialog', () => ({
  SupplierFormDialog: ({ onOpenChange, onSaved, open }: any) => (
    open ? (
      <div>
        Supplier form
        <button onClick={() => onSaved({ id: 'supplier-3' })} type="button">Save supplier</button>
        <button onClick={() => onOpenChange(false)} type="button">Dismiss supplier</button>
      </div>
    ) : null
  ),
}))

describe('SuppliersPage', () => {
  const refetch = vi.fn()

  beforeEach(() => {
    vi.mocked(useSuppliers).mockReturnValue(result(supplierData()) as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders supplier stats, selection and create dialog flow', async () => {
    const user = userEvent.setup()
    render(<SuppliersPage />)

    expect(screen.getByText('Proveedores')).toBeTruthy()
    expect(screen.getByText('Proveedor Uno')).toBeTruthy()
    expect(screen.getByText('Proveedor Dos')).toBeTruthy()

    await user.click(screen.getByText('Proveedor Uno'))
    expect(screen.getByText('Supplier detail Proveedor Uno')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: /Crear proveedor/i }))
    expect(screen.getByText('Supplier form')).toBeTruthy()
    await user.click(screen.getByText('Save supplier'))
    expect(screen.queryByText('Supplier form')).toBeNull()
  })

  it('updates filters and retries failed loads', async () => {
    const user = userEvent.setup()
    render(<SuppliersPage />)

    await user.type(screen.getByPlaceholderText(/Buscar por nombre/i), 'acme')
    expect(vi.mocked(useSuppliers).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({ query: 'acme' }))

    await user.click(screen.getByRole('button', { name: /Refrescar/i }))
    expect(refetch).toHaveBeenCalled()
  })

  it('renders loading, error and empty states', () => {
    vi.mocked(useSuppliers).mockReturnValueOnce(result(undefined, { isLoading: true }) as never)
    const { rerender } = render(<SuppliersPage />)
    expect(document.querySelectorAll('.bg-stone-100').length).toBeGreaterThan(0)

    vi.mocked(useSuppliers).mockReturnValueOnce(result(undefined, { isError: true }) as never)
    rerender(<SuppliersPage />)
    expect(screen.getByText('No se pudieron cargar los proveedores.')).toBeTruthy()

    vi.mocked(useSuppliers).mockReturnValueOnce(result({ items: [], totalItems: 0 }) as never)
    rerender(<SuppliersPage />)
    expect(screen.getByText('Aun no hay proveedores')).toBeTruthy()
  })

  function result(data: unknown, overrides: Record<string, unknown> = {}) {
    return {
      data,
      isError: false,
      isFetching: false,
      isLoading: false,
      refetch,
      ...overrides,
    }
  }
})

function supplierData() {
  return {
    items: [
      { email: 'uno@test.com', id: 'supplier-1', isActive: true, name: 'Proveedor Uno', phone: '809', rnc: '123' },
      { email: null, id: 'supplier-2', isActive: false, name: 'Proveedor Dos', phone: null, rnc: null },
    ],
    totalItems: 2,
  }
}
