import { act } from 'react'
import { beforeEach, describe, expect, it } from 'vitest'
import { calculateCartSubtotal, usePOSCartStore } from '@/modules/pos/store/posCartStore'
import type { POSCartItem } from '@/modules/pos/types/posTypes'

describe('posCartStore', () => {
  beforeEach(() => {
    usePOSCartStore.getState().clearCart()
  })

  it('increases quantity when the same product is added twice', () => {
    act(() => {
      usePOSCartStore.getState().addItem(createCartItem())
      usePOSCartStore.getState().addItem(createCartItem())
    })

    expect(usePOSCartStore.getState().items).toEqual([
      expect.objectContaining({ productId: productId, quantity: 2 }),
    ])
  })

  it('removes item when quantity reaches zero', () => {
    act(() => {
      usePOSCartStore.getState().addItem(createCartItem())
      usePOSCartStore.getState().decreaseQuantity(productId)
    })

    expect(usePOSCartStore.getState().items).toEqual([])
  })

  it('calculates subtotal correctly', () => {
    const subtotal = calculateCartSubtotal([
      createCartItem({ quantity: 2, unitPrice: 125 }),
      createCartItem({
        name: 'Aceite 16 oz',
        productId: '66666666-6666-6666-6666-666666666666',
        quantity: 1,
        sku: 'ACE-16',
        unitPrice: 90,
      }),
    ])

    expect(subtotal).toBe(340)
  })
})

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
