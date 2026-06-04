import { create } from 'zustand'
import type { POSCartItem } from '@/modules/pos/types/posTypes'

type POSCartState = {
  items: POSCartItem[]
  addItem: (item: POSCartItem) => void
  increaseQuantity: (productId: string) => void
  decreaseQuantity: (productId: string) => void
  setQuantity: (productId: string, quantity: number) => void
  removeItem: (productId: string) => void
  clearCart: () => void
}

export const usePOSCartStore = create<POSCartState>()((set) => ({
  items: [],
  addItem: (item) =>
    set((state) => {
      const quantity = Math.max(1, Math.trunc(item.quantity || 1))
      const existingItem = state.items.find((current) => current.productId === item.productId)

      if (existingItem) {
        return {
          items: state.items.map((current) =>
            current.productId === item.productId
              ? { ...current, quantity: current.quantity + quantity }
              : current,
          ),
        }
      }

      return {
        items: [...state.items, { ...item, quantity }],
      }
    }),
  increaseQuantity: (productId) =>
    set((state) => ({
      items: state.items.map((item) =>
        item.productId === productId ? { ...item, quantity: item.quantity + 1 } : item,
      ),
    })),
  decreaseQuantity: (productId) =>
    set((state) => ({
      items: state.items
        .map((item) =>
          item.productId === productId ? { ...item, quantity: item.quantity - 1 } : item,
        )
        .filter((item) => item.quantity > 0),
    })),
  setQuantity: (productId, quantity) =>
    set((state) => {
      const validated = Math.trunc(quantity)
      if (validated <= 0) {
        return { items: state.items.filter((item) => item.productId !== productId) }
      }
      return {
        items: state.items.map((item) =>
          item.productId === productId ? { ...item, quantity: validated } : item,
        ),
      }
    }),
  removeItem: (productId) =>
    set((state) => ({
      items: state.items.filter((item) => item.productId !== productId),
    })),
  clearCart: () => set({ items: [] }),
}))

export function calculateCartSubtotal(items: POSCartItem[]) {
  return items.reduce((total, item) => total + item.unitPrice * item.quantity, 0)
}
