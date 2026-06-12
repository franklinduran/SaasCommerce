import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  addCartItem,
  clearCart,
  getCart,
  removeCartItem,
  setCartItemQuantity,
  type Cart,
} from '@/modules/pos/services/cartApi'

const CART_KEY = ['pos-server-cart'] as const

// ── Helpers ───────────────────────────────────────────────────────────────────

function applyOptimistic(prev: Cart | undefined, updater: (cart: Cart) => Cart): Cart {
  const base: Cart = prev ?? { cartId: '', items: [], subtotal: 0 }
  const next = updater(base)
  next.subtotal = next.items.reduce((s, i) => s + i.unitPrice * i.quantity, 0)
  return next
}

// ── Query ─────────────────────────────────────────────────────────────────────

export function useServerCart() {
  return useQuery({
    queryKey: CART_KEY,
    queryFn: getCart,
    staleTime: Infinity, // only re-fetch when explicitly invalidated
  })
}

// ── Mutations with optimistic updates ────────────────────────────────────────

export function useAddToCart() {
  const qc = useQueryClient()

  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) =>
      addCartItem(productId, quantity),

    onMutate: async ({ productId, quantity }) => {
      await qc.cancelQueries({ queryKey: CART_KEY })
      const previous = qc.getQueryData<Cart>(CART_KEY)

      qc.setQueryData<Cart>(CART_KEY, (prev) =>
        applyOptimistic(prev, (cart) => {
          const existing = cart.items.find((i) => i.productId === productId)
          if (existing) {
            return {
              ...cart,
              items: cart.items.map((i) =>
                i.productId === productId ? { ...i, quantity: i.quantity + quantity } : i,
              ),
            }
          }
          // Optimistic item with placeholder data until server confirms
          return {
            ...cart,
            items: [
              ...cart.items,
              { productId, name: '…', sku: '', unitPrice: 0, quantity },
            ],
          }
        }),
      )

      return { previous }
    },

    onError: (_err, _vars, ctx) => {
      if (ctx?.previous) qc.setQueryData(CART_KEY, ctx.previous)
    },

    onSettled: () => qc.invalidateQueries({ queryKey: CART_KEY }),
  })
}

export function useSetCartItemQuantity() {
  const qc = useQueryClient()

  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) =>
      quantity <= 0 ? removeCartItem(productId) : setCartItemQuantity(productId, quantity),

    onMutate: async ({ productId, quantity }) => {
      await qc.cancelQueries({ queryKey: CART_KEY })
      const previous = qc.getQueryData<Cart>(CART_KEY)

      qc.setQueryData<Cart>(CART_KEY, (prev) =>
        applyOptimistic(prev, (cart) => ({
          ...cart,
          items:
            quantity <= 0
              ? cart.items.filter((i) => i.productId !== productId)
              : cart.items.map((i) =>
                  i.productId === productId ? { ...i, quantity } : i,
                ),
        })),
      )

      return { previous }
    },

    onError: (_err, _vars, ctx) => {
      if (ctx?.previous) qc.setQueryData(CART_KEY, ctx.previous)
    },

    onSettled: () => qc.invalidateQueries({ queryKey: CART_KEY }),
  })
}

export function useRemoveFromCart() {
  const qc = useQueryClient()

  return useMutation({
    mutationFn: (productId: string) => removeCartItem(productId),

    onMutate: async (productId) => {
      await qc.cancelQueries({ queryKey: CART_KEY })
      const previous = qc.getQueryData<Cart>(CART_KEY)

      qc.setQueryData<Cart>(CART_KEY, (prev) =>
        applyOptimistic(prev, (cart) => ({
          ...cart,
          items: cart.items.filter((i) => i.productId !== productId),
        })),
      )

      return { previous }
    },

    onError: (_err, _vars, ctx) => {
      if (ctx?.previous) qc.setQueryData(CART_KEY, ctx.previous)
    },

    onSettled: () => qc.invalidateQueries({ queryKey: CART_KEY }),
  })
}

export function useClearCart() {
  const qc = useQueryClient()

  return useMutation({
    mutationFn: clearCart,

    onMutate: async () => {
      await qc.cancelQueries({ queryKey: CART_KEY })
      const previous = qc.getQueryData<Cart>(CART_KEY)
      qc.setQueryData<Cart>(CART_KEY, (prev) =>
        applyOptimistic(prev, (cart) => ({ ...cart, items: [] })),
      )
      return { previous }
    },

    onError: (_err, _vars, ctx) => {
      if (ctx?.previous) qc.setQueryData(CART_KEY, ctx.previous)
    },

    onSettled: () => qc.invalidateQueries({ queryKey: CART_KEY }),
  })
}
