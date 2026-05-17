import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useMemo, useState } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { POSCart } from '@/modules/pos/components/POSCart'
import { ProductSearch } from '@/modules/pos/components/ProductSearch'
import { SaleStatusPanel } from '@/modules/pos/components/SaleStatusPanel'
import type { POSCartItem } from '@/modules/pos/types/posTypes'

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
