import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  activateProduct,
  createProduct,
  deactivateProduct,
  getCategories,
  getProducts,
  updateProduct,
} from '@/modules/products/services/productService'
import type { ProductFilters, UpdateProductRequest } from '@/modules/products/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export function useProductsQuery(filters: ProductFilters) {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: () => getProducts(filters),
  })
}

export function useCategoriesQuery() {
  return useQuery({
    queryKey: ['catalog-categories'],
    queryFn: getCategories,
  })
}

export function useCreateProductMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createProduct,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
}

export function useUpdateProductMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ productId, request }: { productId: string; request: UpdateProductRequest }) =>
      updateProduct(productId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
}

export function useActivateProductMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: activateProduct,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
}

export function useDeactivateProductMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deactivateProduct,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
}

export function useProductsRealtimeInvalidation() {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: { businessId?: string }) => {
      if (payload.businessId === businessId) {
        queryClient.invalidateQueries({ queryKey: ['products'] })
      }
    }

    onRealtimeEvent('product.costUpdated', handler)
    onRealtimeEvent('inventory.updated', handler)

    return () => {
      offRealtimeEvent('product.costUpdated', handler)
      offRealtimeEvent('inventory.updated', handler)
    }
  }, [businessId, queryClient])
}
