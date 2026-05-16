import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createInventoryAdjustment,
  getInventoryMovements,
  getStock,
} from '@/modules/inventory/services/inventoryService'
import type { MovementFilters, StockFilters } from '@/modules/inventory/types'

export function useStockQuery(filters: StockFilters) {
  return useQuery({
    queryKey: ['inventory', 'stock', filters],
    queryFn: () => getStock(filters),
  })
}

export function useInventoryMovementsQuery(filters: MovementFilters) {
  return useQuery({
    queryKey: ['inventory', 'movements', filters],
    queryFn: () => getInventoryMovements(filters),
  })
}

export function useCreateInventoryAdjustmentMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createInventoryAdjustment,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['inventory'] })
    },
  })
}
