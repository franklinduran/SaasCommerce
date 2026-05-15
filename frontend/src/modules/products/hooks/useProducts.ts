import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createProduct, getProducts, updateProduct } from '@/modules/products/services/productService'
import type { ProductFilters, UpdateProductRequest } from '@/modules/products/types'

export function useProductsQuery(filters: ProductFilters) {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: () => getProducts(filters),
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
