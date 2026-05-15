import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createInventoryAdjustment,
  getInventoryMovements,
  getStock,
} from '@/modules/inventory/services/inventoryService'

export function useStockQuery() {
  return useQuery({
    queryKey: ['inventory', 'stock'],
    queryFn: getStock,
  })
}

export function useInventoryMovementsQuery() {
  return useQuery({
    queryKey: ['inventory', 'movements'],
    queryFn: getInventoryMovements,
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
