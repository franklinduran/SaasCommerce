import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'

export type CartItem = {
  productId: string
  name: string
  sku: string
  unitPrice: number
  quantity: number
}

export type Cart = {
  cartId: string
  items: CartItem[]
  subtotal: number
}

function getAccessToken() {
  return useAuthStore.getState().session?.accessToken
}

export async function getCart(): Promise<Cart> {
  const res = await httpClient<Cart>('/api/pos/cart', { accessToken: getAccessToken() })
  return res.data!
}

export async function addCartItem(productId: string, quantity: number): Promise<Cart> {
  const res = await httpClient<Cart>('/api/pos/cart/items', {
    accessToken: getAccessToken(),
    method: 'POST',
    body: JSON.stringify({ productId, quantity }),
  })
  return res.data!
}

export async function setCartItemQuantity(productId: string, quantity: number): Promise<Cart> {
  const res = await httpClient<Cart>(`/api/pos/cart/items/${productId}`, {
    accessToken: getAccessToken(),
    method: 'PUT',
    body: JSON.stringify({ quantity }),
  })
  return res.data!
}

export async function removeCartItem(productId: string): Promise<Cart> {
  const res = await httpClient<Cart>(`/api/pos/cart/items/${productId}`, {
    accessToken: getAccessToken(),
    method: 'DELETE',
  })
  return res.data!
}

export async function clearCart(): Promise<void> {
  await httpClient('/api/pos/cart', {
    accessToken: getAccessToken(),
    method: 'DELETE',
  })
}
