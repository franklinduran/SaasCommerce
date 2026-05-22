import { useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  cancelInventoryTransfer,
  createInventoryTransfer,
  getInventoryTransferById,
  getInventoryTransfers,
} from '@/modules/inventory-transfers/services/inventoryTransfersApi'
import type {
  CreateInventoryTransferRequest,
  InventoryTransferFilters,
  InventoryTransferRealtimeNotification,
} from '@/modules/inventory-transfers/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const transferKeys = {
  all: ['inventory-transfers'] as const,
  detail: (transferId: string) => ['inventory-transfers', 'detail', transferId] as const,
  list: (filters: InventoryTransferFilters) => ['inventory-transfers', 'list', filters] as const,
}

export function useInventoryTransfers(filters: InventoryTransferFilters) {
  return useQuery({
    queryKey: transferKeys.list(filters),
    queryFn: () => getInventoryTransfers(filters),
  })
}

export function useInventoryTransferById(transferId?: string) {
  return useQuery({
    enabled: Boolean(transferId),
    queryKey: transferId ? transferKeys.detail(transferId) : ['inventory-transfers', 'detail'],
    queryFn: () => getInventoryTransferById(transferId!),
  })
}

export function useCreateInventoryTransferMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateInventoryTransferRequest) => createInventoryTransfer(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: transferKeys.all })
    },
  })
}

export function useCancelInventoryTransferMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (transferId: string) => cancelInventoryTransfer(transferId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: transferKeys.all })
    },
  })
}

export function useInventoryTransferRealtimeInvalidation(transferId?: string) {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: InventoryTransferRealtimeNotification) => {
      if (payload.businessId !== businessId) {
        return
      }

      queryClient.invalidateQueries({ queryKey: transferKeys.all })

      if (transferId && payload.transferId === transferId) {
        queryClient.invalidateQueries({ queryKey: transferKeys.detail(transferId) })
      }
    }

    onRealtimeEvent('inventoryTransfer.completed', handler)
    onRealtimeEvent('inventoryTransfer.failed', handler)
    onRealtimeEvent('inventoryTransfer.cancelled', handler)
    onRealtimeEvent('inventoryTransfer.created', handler)

    return () => {
      offRealtimeEvent('inventoryTransfer.completed', handler)
      offRealtimeEvent('inventoryTransfer.failed', handler)
      offRealtimeEvent('inventoryTransfer.cancelled', handler)
      offRealtimeEvent('inventoryTransfer.created', handler)
    }
  }, [businessId, queryClient, transferId])
}
