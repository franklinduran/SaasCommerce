import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createSupplier,
  getSuppliers,
  updateSupplier,
} from '@/modules/suppliers/services/supplierService'
import type { SupplierFilters, UpdateSupplierRequest } from '@/modules/suppliers/types'

export const supplierKeys = {
  all: ['suppliers'] as const,
  list: (filters: SupplierFilters) => ['suppliers', 'list', filters] as const,
}

export function useSuppliers(filters: SupplierFilters) {
  return useQuery({
    queryKey: supplierKeys.list(filters),
    queryFn: () => getSuppliers(filters),
  })
}

export function useCreateSupplier() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createSupplier,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: supplierKeys.all })
    },
  })
}

export function useUpdateSupplier() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ supplierId, request }: { supplierId: string; request: UpdateSupplierRequest }) =>
      updateSupplier(supplierId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: supplierKeys.all })
    },
  })
}
