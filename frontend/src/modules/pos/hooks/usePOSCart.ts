import { useMemo } from 'react'
import { calculateCartSubtotal, usePOSCartStore } from '@/modules/pos/store/posCartStore'

export function usePOSCart() {
  const items = usePOSCartStore((state) => state.items)
  const addItem = usePOSCartStore((state) => state.addItem)
  const increaseQuantity = usePOSCartStore((state) => state.increaseQuantity)
  const decreaseQuantity = usePOSCartStore((state) => state.decreaseQuantity)
  const removeItem = usePOSCartStore((state) => state.removeItem)
  const clearCart = usePOSCartStore((state) => state.clearCart)
  const subtotal = useMemo(() => calculateCartSubtotal(items), [items])
  const itemCount = useMemo(
    () => items.reduce((total, item) => total + item.quantity, 0),
    [items],
  )

  return {
    addItem,
    clearCart,
    decreaseQuantity,
    increaseQuantity,
    itemCount,
    items,
    removeItem,
    subtotal,
  }
}
