import { useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  createInventoryAdjustment,
  getInventory,
  getInventoryMovements,
  getInventoryProductDetail,
  getStock,
} from '@/modules/inventory/services/inventoryApi'
import type { InventoryRealtimeNotification, MovementFilters, StockFilters } from '@/modules/inventory/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const inventoryKeys = {
  all: ['inventory'] as const,
  detail: (productId: string) => ['inventory', 'product', productId] as const,
  list: (filters: StockFilters) => ['inventory', 'list', filters] as const,
  movements: (filters: MovementFilters) => ['inventory', 'movements', filters] as const,
  stock: (filters: StockFilters) => ['inventory', 'stock', filters] as const,
}

export function useInventory(filters: StockFilters) {
  return useQuery({
    queryKey: inventoryKeys.list(filters),
    queryFn: () => getInventory(filters),
  })
}

export function useStockQuery(filters: StockFilters) {
  return useQuery({
    queryKey: inventoryKeys.stock(filters),
    queryFn: () => getStock(filters),
  })
}

export function useInventoryMovementsQuery(filters: MovementFilters) {
  return useQuery({
    queryKey: inventoryKeys.movements(filters),
    queryFn: () => getInventoryMovements(filters),
  })
}

export function useInventoryProductDetail(productId?: string) {
  return useQuery({
    enabled: Boolean(productId),
    queryKey: productId ? inventoryKeys.detail(productId) : ['inventory', 'product'],
    queryFn: () => getInventoryProductDetail(productId!),
  })
}

export function useCreateInventoryAdjustmentMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createInventoryAdjustment,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: inventoryKeys.all })
    },
  })
}

export function useInventoryRealtimeInvalidation(productId?: string) {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: InventoryRealtimeNotification) => {
      if (payload.businessId !== businessId) {
        return
      }

      void queryClient.invalidateQueries({ queryKey: inventoryKeys.all })

      if (productId && (!payload.productId || payload.productId === productId)) {
        void queryClient.invalidateQueries({ queryKey: inventoryKeys.detail(productId) })
      }
    }

    onRealtimeEvent('inventory.adjusted', handler)
    onRealtimeEvent('inventory.lowStockDetected', handler)
    onRealtimeEvent('inventory.stockChanged', handler)

    return () => {
      offRealtimeEvent('inventory.adjusted', handler)
      offRealtimeEvent('inventory.lowStockDetected', handler)
      offRealtimeEvent('inventory.stockChanged', handler)
    }
  }, [businessId, productId, queryClient])
}
