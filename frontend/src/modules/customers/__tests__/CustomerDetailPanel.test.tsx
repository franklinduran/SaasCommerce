import { act, cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { CustomerDetailPanel } from '@/modules/customers/components/CustomerDetailPanel'
import {
  useBlockCustomerCredit,
  useCustomerCredit,
  useCustomerCreditMovements,
  useUnblockCustomerCredit,
} from '@/modules/customers/hooks/useCustomerCredit'
import { useCustomerDetail } from '@/modules/customers/hooks/useCustomerDetail'
import {
  useDeactivateCustomer,
  useUpdateCustomer,
} from '@/modules/customers/hooks/useCustomers'
import { useRegisterCustomerPayment } from '@/modules/customers/hooks/useRegisterCustomerPayment'
import type {
  Customer,
  CustomerCreditMovementListResponse,
  CustomerCreditSummary,
} from '@/modules/customers/types'

const updateMutate = vi.fn()
const deactivateMutate = vi.fn()
const registerPaymentMutate = vi.fn()
const blockMutate = vi.fn()
const unblockMutate = vi.fn()

vi.mock('@/modules/customers/hooks/useCustomerDetail', () => ({
  useCustomerDetail: vi.fn(),
}))

vi.mock('@/modules/customers/hooks/useCustomerCredit', () => ({
  useBlockCustomerCredit: vi.fn(),
  useCustomerCredit: vi.fn(),
  useCustomerCreditMovements: vi.fn(),
  useUnblockCustomerCredit: vi.fn(),
}))

vi.mock('@/modules/customers/hooks/useCustomers', () => ({
  useDeactivateCustomer: vi.fn(),
  useUpdateCustomer: vi.fn(),
}))

vi.mock('@/modules/customers/hooks/useRegisterCustomerPayment', () => ({
  useRegisterCustomerPayment: vi.fn(),
}))

vi.mock('@/modules/customers/components/CustomerCreditMovementsTable', () => ({
  CustomerCreditMovementsTable: ({
    isError,
    isLoading,
    movements,
  }: {
    isError: boolean
    isLoading: boolean
    movements: unknown[]
  }) => (
    <div>
      Movimientos {movements.length} {isLoading ? 'cargando' : ''} {isError ? 'error' : ''}
    </div>
  ),
}))

vi.mock('@/modules/customers/components/RegisterCustomerPaymentDialog', () => ({
  RegisterCustomerPaymentDialog: ({
    isOpen,
    onClose,
    onSubmit,
  }: {
    isOpen: boolean
    onClose: () => void
    onSubmit: (amount: number, note?: string) => void
  }) => isOpen ? (
    <div role="dialog">
      <button onClick={() => onSubmit(125, 'abono')} type="button">Enviar abono</button>
      <button onClick={onClose} type="button">Cerrar abono</button>
    </div>
  ) : null,
}))

describe('CustomerDetailPanel', () => {
  const onClose = vi.fn()

  beforeEach(() => {
    vi.mocked(useCustomerDetail).mockReturnValue({ data: customer(), isLoading: false } as ReturnType<typeof useCustomerDetail>)
    vi.mocked(useCustomerCredit).mockReturnValue({ data: creditSummary(), isLoading: false } as ReturnType<typeof useCustomerCredit>)
    vi.mocked(useCustomerCreditMovements).mockReturnValue({
      data: movements(),
      isError: false,
      isLoading: false,
    } as ReturnType<typeof useCustomerCreditMovements>)
    vi.mocked(useUpdateCustomer).mockReturnValue({ isPending: false, mutate: updateMutate } as ReturnType<typeof useUpdateCustomer>)
    vi.mocked(useDeactivateCustomer).mockReturnValue({ isPending: false, mutate: deactivateMutate } as ReturnType<typeof useDeactivateCustomer>)
    vi.mocked(useRegisterCustomerPayment).mockReturnValue({ isPending: false, mutate: registerPaymentMutate } as ReturnType<typeof useRegisterCustomerPayment>)
    vi.mocked(useBlockCustomerCredit).mockReturnValue({ isPending: false, mutate: blockMutate } as ReturnType<typeof useBlockCustomerCredit>)
    vi.mocked(useUnblockCustomerCredit).mockReturnValue({ isPending: false, mutate: unblockMutate } as ReturnType<typeof useUnblockCustomerCredit>)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders customer credit data and updates the profile', async () => {
    const user = userEvent.setup()
    render(<CustomerDetailPanel customerId="customer-1" onClose={onClose} />)

    expect(screen.getByText('Maria Cliente')).toBeTruthy()
    expect(screen.getByText('RD$ 1,500.00')).toBeTruthy()
    expect(screen.getByText('RD$ 5,000.00')).toBeTruthy()
    expect(screen.getByText(/Movimientos 1/)).toBeTruthy()

    await user.clear(screen.getByLabelText('Nombre completo'))
    await user.type(screen.getByLabelText('Nombre completo'), 'Maria Nueva')
    await user.click(screen.getByRole('button', { name: 'Guardar cambios' }))

    expect(updateMutate).toHaveBeenCalledWith(
      {
        email: 'maria@test.com',
        fullName: 'Maria Nueva',
        isActive: true,
        phone: '8090000000',
      },
      expect.objectContaining({ onSuccess: expect.any(Function) }),
    )

    act(() => updateMutate.mock.calls[0][1].onSuccess())
    expect(await screen.findByText('Datos del cliente guardados.')).toBeTruthy()
  })

  it('registers payments, blocks credit and deactivates customers', async () => {
    const user = userEvent.setup()
    render(<CustomerDetailPanel customerId="customer-1" onClose={onClose} />)

    await user.click(screen.getByRole('button', { name: 'Registrar abono' }))
    await user.click(screen.getByRole('button', { name: 'Enviar abono' }))
    expect(registerPaymentMutate).toHaveBeenCalledWith(
      { amount: 125, customerId: 'customer-1', note: 'abono' },
      expect.objectContaining({ onSuccess: expect.any(Function) }),
    )
    act(() => registerPaymentMutate.mock.calls[0][1].onSuccess())
    expect(await screen.findByText('Abono de RD$ 125.00 registrado.')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Bloquear' }))
    expect(screen.getAllByText('Bloquear credito').length).toBeGreaterThan(1)
    await user.click(screen.getAllByRole('button', { name: /^Bloquear$/ }).at(-1)!)
    expect(blockMutate).toHaveBeenCalledWith(undefined, expect.objectContaining({ onSuccess: expect.any(Function) }))
    act(() => blockMutate.mock.calls[0][1].onSuccess())
    expect(await screen.findByText('Credito bloqueado.')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Desactivar' }))
    expect(screen.getAllByText('Desactivar cliente').length).toBeGreaterThan(1)
    await user.click(screen.getAllByRole('button', { name: /^Desactivar$/ }).at(-1)!)
    expect(deactivateMutate).toHaveBeenCalledWith('customer-1', expect.objectContaining({ onSuccess: expect.any(Function) }))
  })

  it('unblocks blocked credit and renders inactive state', async () => {
    const user = userEvent.setup()
    vi.mocked(useCustomerDetail).mockReturnValue({
      data: customer({ isActive: false }),
      isLoading: false,
    } as ReturnType<typeof useCustomerDetail>)
    vi.mocked(useCustomerCredit).mockReturnValue({
      data: creditSummary({ creditLimit: 0, currentBalance: 0, status: 'Blocked' }),
      isLoading: false,
    } as ReturnType<typeof useCustomerCredit>)

    render(<CustomerDetailPanel customerId="customer-1" onClose={onClose} />)

    expect(screen.getByText('Sin limite')).toBeTruthy()
    expect(screen.getAllByText('Inactivo').length).toBeGreaterThan(0)
    await user.click(screen.getByRole('button', { name: 'Desbloquear' }))
    expect(unblockMutate).toHaveBeenCalledWith(undefined, expect.objectContaining({ onSuccess: expect.any(Function) }))
  })

  it('renders skeleton while loading and validates profile input', async () => {
    vi.mocked(useCustomerDetail).mockReturnValue({ data: undefined, isLoading: true } as ReturnType<typeof useCustomerDetail>)
    const { rerender } = render(<CustomerDetailPanel customerId="customer-1" onClose={onClose} />)
    expect(document.querySelector('.bg-stone-100')).toBeTruthy()

    vi.mocked(useCustomerDetail).mockReturnValue({ data: customer(), isLoading: false } as ReturnType<typeof useCustomerDetail>)
    rerender(<CustomerDetailPanel customerId="customer-1" onClose={onClose} />)

    await userEvent.clear(screen.getByLabelText('Nombre completo'))
    await userEvent.click(screen.getByRole('button', { name: 'Guardar cambios' }))
    expect(await screen.findByText('El nombre es obligatorio.')).toBeTruthy()
  })
})

function customer(overrides: Partial<Customer> = {}): Customer {
  return {
    businessId: 'business-1',
    createdAt: '2026-05-25T12:00:00Z',
    creditLimit: 5000,
    creditStatus: 'Active',
    currentBalance: 1500,
    deactivatedAt: null,
    email: 'maria@test.com',
    fullName: 'Maria Cliente',
    id: 'customer-1',
    isActive: true,
    phone: '8090000000',
    updatedAt: '2026-05-25T13:00:00Z',
    ...overrides,
  }
}

function creditSummary(overrides: Partial<CustomerCreditSummary> = {}): CustomerCreditSummary {
  return {
    businessId: 'business-1',
    createdAt: '2026-05-25T12:00:00Z',
    creditLimit: 5000,
    currentBalance: 1500,
    customerId: 'customer-1',
    customerName: 'Maria Cliente',
    status: 'Active',
    updatedAt: '2026-05-25T13:00:00Z',
    ...overrides,
  }
}

function movements(): CustomerCreditMovementListResponse {
  return {
    hasNextPage: false,
    hasPreviousPage: false,
    items: [
      {
        amount: 250,
        businessId: 'business-1',
        createdAt: '2026-05-25T12:00:00Z',
        createdBy: 'Admin',
        customerId: 'customer-1',
        id: 'movement-1',
        newBalance: 1500,
        note: 'Venta',
        paymentId: null,
        previousBalance: 1250,
        saleId: 'sale-1',
        type: 'Debit',
      },
    ],
    page: 1,
    pageSize: 10,
    totalItems: 1,
    totalPages: 1,
  }
}
