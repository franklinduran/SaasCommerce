import { useQueryClient } from '@tanstack/react-query'
import {
  useAddToCart,
  useClearCart,
  useRemoveFromCart,
  useServerCart,
  useSetCartItemQuantity,
} from '@/modules/pos/hooks/useServerCart'
import type { POSCartItem } from '@/modules/pos/types/posTypes'
import type { Cart } from '@/modules/pos/services/cartApi'

const CART_KEY = ['pos-server-cart'] as const

// Maps the server Cart shape to the POSCartItem shape used throughout the POS
function toPOSCartItems(cart: Cart | undefined): POSCartItem[] {
  return (cart?.items ?? []).map((i) => ({
    productId: i.productId,
    name: i.name,
    sku: i.sku,
    unitPrice: i.unitPrice,
    quantity: i.quantity,
  }))
}

export function usePOSCart() {
  const qc = useQueryClient()
  const cartQuery = useServerCart()
  const addMutation = useAddToCart()
  const setQtyMutation = useSetCartItemQuantity()
  const removeMutation = useRemoveFromCart()
  const clearMutation = useClearCart()

  const cart = cartQuery.data
  const items = toPOSCartItems(cart)
  const subtotal = cart?.subtotal ?? 0
  const itemCount = items.reduce((t, i) => t + i.quantity, 0)

  // ── addItem: also update the cache optimistically with full product info ──
  function addItem(item: POSCartItem) {
    // Update optimistic cache with full product info (name, sku, price)
    qc.setQueryData<Cart>(CART_KEY, (prev) => {
      const base: Cart = prev ?? { cartId: '', items: [], subtotal: 0 }
      const existing = base.items.find((i) => i.productId === item.productId)
      const newItems = existing
        ? base.items.map((i) =>
            i.productId === item.productId
              ? { ...i, quantity: i.quantity + item.quantity }
              : i,
          )
        : [
            ...base.items,
            {
              productId: item.productId,
              name: item.name,
              sku: item.sku,
              unitPrice: item.unitPrice,
              quantity: item.quantity,
            },
          ]
      return {
        ...base,
        items: newItems,
        subtotal: newItems.reduce((s, i) => s + i.unitPrice * i.quantity, 0),
      }
    })

    // Sync to server in background
    addMutation.mutate({ productId: item.productId, quantity: item.quantity })
  }

  function increaseQuantity(productId: string) {
    const current = items.find((i) => i.productId === productId)
    if (!current) return
    setQtyMutation.mutate({ productId, quantity: current.quantity + 1 })
  }

  function decreaseQuantity(productId: string) {
    const current = items.find((i) => i.productId === productId)
    if (!current) return
    setQtyMutation.mutate({ productId, quantity: current.quantity - 1 })
  }

  function setQuantity(productId: string, quantity: number) {
    setQtyMutation.mutate({ productId, quantity })
  }

  function removeItem(productId: string) {
    removeMutation.mutate(productId)
  }

  function clearCart() {
    clearMutation.mutate()
  }

  return {
    addItem,
    clearCart,
    decreaseQuantity,
    increaseQuantity,
    itemCount,
    items,
    removeItem,
    setQuantity,
    subtotal,
    isLoading: cartQuery.isLoading,
  }
}
