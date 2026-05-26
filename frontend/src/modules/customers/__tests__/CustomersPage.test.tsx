import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { CustomersPage } from '@/modules/customers/pages/CustomersPage'
import {
  useCreateCustomer,
  useCustomers,
} from '@/modules/customers/hooks/useCustomers'
import type { Customer, CustomerFilters, CustomerListResponse } from '@/modules/customers/types'

const refetch = vi.fn()
const createMutate = vi.fn()
let customersState: {
  data?: CustomerListResponse
  isError?: boolean
  isFetching?: boolean
  isLoading?: boolean
}
let canExport = true

vi.mock('@/modules/customers/hooks/useCustomers', () => ({
  useCreateCustomer: vi.fn(),
  useCustomerCreditInvalidation: vi.fn(),
  useCustomers: vi.fn(),
}))

vi.mock('@/modules/customers/components/CreateCustomerDialog', () => ({
  CreateCustomerDialog: ({
    onOpenChange,
    onSubmit,
    open,
  }: {
    onOpenChange: (open: boolean) => void
    onSubmit: (request: { fullName: string }) => void
    open: boolean
  }) => open ? (
    <div role="dialog">
      <button onClick={() => onSubmit({ fullName: 'Nuevo Cliente' })} type="button">Guardar cliente</button>
      <button onClick={() => onOpenChange(false)} type="button">Cerrar crear cliente</button>
    </div>
  ) : null,
}))

vi.mock('@/modules/customers/components/CustomerDetailPanel', () => ({
  CustomerDetailPanel: ({
    customerId,
    onClose,
  }: {
    customerId: string
    onClose: () => void
  }) => (
    <div>
      <p>Detalle cliente {customerId}</p>
      <button onClick={onClose} type="button">Cerrar detalle cliente</button>
    </div>
  ),
}))

vi.mock('@/modules/customers/components/CustomerListItem', () => ({
  CustomerListItem: ({
    customer,
    onClick,
    selected,
  }: {
    customer: Customer
    onClick: () => void
    selected: boolean
  }) => (
    <li>
      <button aria-pressed={selected} onClick={onClick} type="button">
        {customer.fullName} {customer.phone} {customer.isActive ? 'Activo' : 'Inactivo'}
      </button>
    </li>
  ),
}))

vi.mock('@/shared/components/CsvExportButton', () => ({
  CsvExportButton: ({ endpoint, filename }: { endpoint: string; filename: string }) => (
    <a href={endpoint}>{filename}</a>
  ),
}))

vi.mock('@/shared/hooks/usePermissions', () => ({
  useHasPermission: () => canExport,
}))

describe('CustomersPage', () => {
  beforeEach(() => {
    customersState = { data: customerList(), isError: false, isFetching: false, isLoading: false }
    vi.mocked(useCustomers).mockImplementation((filters: CustomerFilters) => ({
      data: customersState.data,
      isError: Boolean(customersState.isError),
      isFetching: Boolean(customersState.isFetching),
      isLoading: Boolean(customersState.isLoading),
      refetch,
      filters,
    }) as unknown as ReturnType<typeof useCustomers>)
    vi.mocked(useCreateCustomer).mockReturnValue({
      isPending: false,
      mutate: createMutate,
    } as unknown as ReturnType<typeof useCreateCustomer>)
    createMutate.mockImplementation((_request, options) => options?.onSuccess?.({ id: 'created-1' }))
    canExport = true
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders customers, stats, export and opens selected detail', async () => {
    const user = userEvent.setup()
    renderPage('/customers')

    expect(screen.getByText('Clientes')).toBeTruthy()
    expect(screen.getByText('RD$ 1,500')).toBeTruthy()
    expect(screen.getByRole('link', { name: /clientes_/ })).toHaveAttribute('href', '/api/customers/export')
    expect(screen.getByRole('button', { name: /Maria Cliente/ })).toBeTruthy()

    await user.click(screen.getByRole('button', { name: /Maria Cliente/ }))
    expect(await screen.findByText('Detalle cliente customer-1')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Cerrar detalle cliente' }))
    expect(await screen.findByText('Selecciona un cliente')).toBeTruthy()
  })

  it('updates filters, refreshes and creates customers', async () => {
    const user = userEvent.setup()
    renderPage('/customers')

    await user.type(screen.getByPlaceholderText('Buscar por nombre o telefono'), 'maria')
    expect(useCustomers).toHaveBeenLastCalledWith(expect.objectContaining({ query: 'maria' }))

    await user.click(screen.getByRole('combobox', { name: 'Filtrar por estado' }))
    await user.click(screen.getByRole('option', { name: 'Activos' }))
    expect(useCustomers).toHaveBeenLastCalledWith(expect.objectContaining({ isActive: 'true' }))

    await user.click(screen.getByRole('button', { name: 'Refrescar' }))
    expect(refetch).toHaveBeenCalled()

    await user.click(screen.getAllByRole('button', { name: 'Crear cliente' })[0])
    await user.click(screen.getByRole('button', { name: 'Guardar cliente' }))
    expect(createMutate).toHaveBeenCalledWith({ fullName: 'Nuevo Cliente' }, expect.objectContaining({ onSuccess: expect.any(Function) }))
    expect(await screen.findByText('Detalle cliente created-1')).toBeTruthy()
  })

  it('renders loading, error and empty states', async () => {
    const user = userEvent.setup()
    customersState = { isLoading: true }
    const { rerender } = renderPage('/customers')
    expect(document.querySelectorAll('.bg-stone-100').length).toBeGreaterThan(0)

    customersState = { isError: true }
    rerenderWithRouter(rerender, '/customers')
    expect(screen.getByText('No se pudieron cargar los clientes.')).toBeTruthy()
    await user.click(screen.getByRole('button', { name: 'Reintentar' }))
    expect(refetch).toHaveBeenCalled()

    customersState = { data: { ...customerList(), items: [], totalItems: 0 } }
    rerenderWithRouter(rerender, '/customers')
    expect(screen.getByText('Aun no hay clientes')).toBeTruthy()

    await user.type(screen.getByPlaceholderText('Buscar por nombre o telefono'), 'x')
    expect(screen.getByText('Sin resultados')).toBeTruthy()
  })
})

function renderPage(path: string) {
  return render(routes(path))
}

function rerenderWithRouter(rerender: (ui: React.ReactNode) => void, path: string) {
  rerender(routes(path))
}

function routes(path: string) {
  return (
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/customers" element={<CustomersPage />} />
        <Route path="/customers/:customerId" element={<CustomersPage />} />
      </Routes>
    </MemoryRouter>
  )
}

function customerList(): CustomerListResponse {
  return {
    hasNextPage: false,
    hasPreviousPage: false,
    items: [
      customer('customer-1', 'Maria Cliente', true, 1500, 'Active'),
      customer('customer-2', 'Jose Bloqueado', true, 0, 'Blocked'),
      customer('customer-3', 'Ana Inactiva', false, 0, 'Closed'),
    ],
    page: 1,
    pageSize: 50,
    totalItems: 3,
    totalPages: 1,
  }
}

function customer(
  id: string,
  fullName: string,
  isActive: boolean,
  currentBalance: number,
  creditStatus: Customer['creditStatus'],
): Customer {
  return {
    businessId: 'business-1',
    createdAt: '2026-05-25T12:00:00Z',
    creditLimit: 5000,
    creditStatus,
    currentBalance,
    deactivatedAt: null,
    email: `${id}@test.com`,
    fullName,
    id,
    isActive,
    phone: '8090000000',
    updatedAt: null,
  }
}
