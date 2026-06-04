import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useMemo, useState } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { CustomerSelector } from '@/modules/pos/components/CustomerSelector'
import { POSCart } from '@/modules/pos/components/POSCart'
import { ProductSearch } from '@/modules/pos/components/ProductSearch'
import { SaleStatusPanel } from '@/modules/pos/components/SaleStatusPanel'
import type { Customer, POSCartItem } from '@/modules/pos/types/posTypes'

describe('POS components', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows cart empty state when there are no items', () => {
    render(
      <POSCart
        itemCount={0}
        items={[]}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
        onRemove={vi.fn()}
        subtotal={0}
      />,
    )

    expect(screen.getByText('Carrito vacio')).toBeTruthy()
  })

  it('decreases quantity when user clicks decrease', async () => {
    const user = userEvent.setup()
    const onDecrease = vi.fn()

    render(
      <POSCart
        itemCount={2}
        items={[createCartItem({ quantity: 2 })]}
        onDecrease={onDecrease}
        onIncrease={vi.fn()}
        onRemove={vi.fn()}
        subtotal={500}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Disminuir Cafe molido' }))

    expect(onDecrease).toHaveBeenCalledWith(productId)
  })

  it('filters products by search term', async () => {
    const user = userEvent.setup()

    render(<ProductSearchHarness />)

    expect(screen.getByText('Cafe molido')).toBeTruthy()
    expect(screen.getByText('Aceite 16 oz')).toBeTruthy()

    await user.type(screen.getByLabelText('Buscar productos'), 'aceite')

    expect(screen.queryByText('Cafe molido')).toBeNull()
    expect(screen.getByText('Aceite 16 oz')).toBeTruthy()
  })

  it('renders completed sale state', () => {
    render(
      <SaleStatusPanel
        errorMessage={null}
        saleId="99999999-9999-9999-9999-999999999999"
        status="Completed"
        total={340}
      />,
    )

    expect(screen.getByText('Venta completada')).toBeTruthy()
  })

  it('renders failed sale state', () => {
    render(
      <SaleStatusPanel
        errorMessage="La venta fallo durante el procesamiento."
        reason="Stock insuficiente"
        saleId="99999999-9999-9999-9999-999999999999"
        status="Failed"
        total={340}
      />,
    )

    expect(screen.getByText('Venta fallida')).toBeTruthy()
    expect(screen.getByText('Stock insuficiente')).toBeTruthy()
  })
})

describe('CustomerSelector', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows Opcional label when isCreditPayment is false', () => {
    render(
      <CustomerSelector
        customers={[]}
        isError={false}
        isLoading={false}
        onCustomerChange={vi.fn()}
        onQueryChange={vi.fn()}
        query=""
        selectedCustomerId={null}
      />,
    )

    expect(screen.getByText('Opcional')).toBeTruthy()
  })

  it('shows Obligatorio label when isCreditPayment is true', () => {
    render(
      <CustomerSelector
        customers={[]}
        isCreditPayment
        isError={false}
        isLoading={false}
        onCustomerChange={vi.fn()}
        onQueryChange={vi.fn()}
        query=""
        selectedCustomerId={null}
      />,
    )

    expect(screen.getByText('Obligatorio')).toBeTruthy()
  })

  it('calls onQueryChange when typing in search input', async () => {
    const user = userEvent.setup()
    const onQueryChange = vi.fn()

    render(
      <CustomerSelector
        customers={[]}
        isError={false}
        isLoading={false}
        onCustomerChange={vi.fn()}
        onQueryChange={onQueryChange}
        query=""
        selectedCustomerId={null}
      />,
    )

    await user.type(screen.getByLabelText('Buscar clientes'), 'Mar')

    expect(onQueryChange).toHaveBeenCalled()
  })

  it('shows error message when isError is true', () => {
    render(
      <CustomerSelector
        customers={[]}
        isError
        isLoading={false}
        onCustomerChange={vi.fn()}
        onQueryChange={vi.fn()}
        query=""
        selectedCustomerId={null}
      />,
    )

    expect(screen.getByText('No se pudieron cargar clientes.')).toBeTruthy()
  })

  it('shows credit info for selected customer', () => {
    const customer = createCustomer({ creditLimit: 500, currentBalance: 100 })

    render(
      <CustomerSelector
        customers={[customer]}
        isError={false}
        isLoading={false}
        onCustomerChange={vi.fn()}
        onQueryChange={vi.fn()}
        query=""
        selectedCustomerId={customer.id}
      />,
    )

    expect(screen.getByText('Balance')).toBeTruthy()
    expect(screen.getByText('Límite')).toBeTruthy()
  })

  it('shows Sin limite when creditLimit is zero', () => {
    const customer = createCustomer({ creditLimit: 0, currentBalance: 0 })

    render(
      <CustomerSelector
        customers={[customer]}
        isError={false}
        isLoading={false}
        onCustomerChange={vi.fn()}
        onQueryChange={vi.fn()}
        query=""
        selectedCustomerId={customer.id}
      />,
    )

    expect(screen.getByText('Sin límite')).toBeTruthy()
  })

  it('shows Credito bloqueado badge for blocked customer', () => {
    const customer = createCustomer({ creditStatus: 'Blocked' })

    render(
      <CustomerSelector
        customers={[customer]}
        isError={false}
        isLoading={false}
        onCustomerChange={vi.fn()}
        onQueryChange={vi.fn()}
        query=""
        selectedCustomerId={customer.id}
      />,
    )

    expect(screen.getByText('Crédito bloqueado')).toBeTruthy()
  })
})

const customerId = '44444444-4444-4444-4444-444444444444'

function createCustomer(overrides: Partial<Customer> = {}): Customer {
  return {
    businessId: '11111111-1111-1111-1111-111111111111',
    createdAt: '2026-05-17T12:00:00Z',
    deactivatedAt: null,
    email: 'maria@example.com',
    fullName: 'Maria Perez',
    id: customerId,
    isActive: true,
    phone: '8095550101',
    updatedAt: null,
    ...overrides,
  }
}

function ProductSearchHarness() {
  const [query, setQuery] = useState('')
  const products = useMemo(
    () =>
      ['Cafe molido', 'Aceite 16 oz'].filter((product) =>
        product.toLowerCase().includes(query.toLowerCase()),
      ),
    [query],
  )

  return (
    <div>
      <ProductSearch
        isFetching={false}
        onQueryChange={setQuery}
        onRefresh={vi.fn()}
        query={query}
      />
      {products.map((product) => (
        <p key={product}>{product}</p>
      ))}
    </div>
  )
}

const productId = '55555555-5555-5555-5555-555555555555'

function createCartItem(overrides: Partial<POSCartItem> = {}): POSCartItem {
  return {
    name: 'Cafe molido',
    productId,
    quantity: 1,
    sku: 'CAF-001',
    unitPrice: 250,
    ...overrides,
  }
}
