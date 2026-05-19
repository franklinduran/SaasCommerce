import { useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  cancelPurchase,
  createPurchase,
  getPurchaseDetail,
  getPurchases,
  receivePurchase,
} from '@/modules/purchases/services/purchaseService'
import type { PurchaseFilters, PurchaseRealtimeNotification } from '@/modules/purchases/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const purchaseKeys = {
  all: ['purchases'] as const,
  detail: (purchaseId: string) => ['purchases', 'detail', purchaseId] as const,
  list: (filters: PurchaseFilters) => ['purchases', 'list', filters] as const,
}

export function usePurchases(filters: PurchaseFilters) {
  return useQuery({
    queryKey: purchaseKeys.list(filters),
    queryFn: () => getPurchases(filters),
  })
}

export function usePurchaseDetail(purchaseId?: string) {
  return useQuery({
    enabled: Boolean(purchaseId),
    queryKey: purchaseId ? purchaseKeys.detail(purchaseId) : ['purchases', 'detail'],
    queryFn: () => getPurchaseDetail(purchaseId!),
  })
}

export function useCreatePurchase() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createPurchase,
    onSuccess: async () => {
      await invalidatePurchaseData(queryClient)
    },
  })
}

export function useReceivePurchase() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: receivePurchase,
    onSuccess: async (purchase) => {
      await invalidatePurchaseData(queryClient)
      if (purchase?.purchaseId) {
        await queryClient.invalidateQueries({ queryKey: purchaseKeys.detail(purchase.purchaseId) })
      }
    },
  })
}

export function useCancelPurchase() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: cancelPurchase,
    onSuccess: async (purchase) => {
      await invalidatePurchaseData(queryClient)
      if (purchase?.purchaseId) {
        await queryClient.invalidateQueries({ queryKey: purchaseKeys.detail(purchase.purchaseId) })
      }
    },
  })
}

export function usePurchaseRealtimeInvalidation(purchaseId?: string) {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: PurchaseRealtimeNotification) => {
      if (payload.businessId !== businessId) {
        return
      }

      invalidatePurchaseData(queryClient)

      if (purchaseId && (!payload.purchaseId || payload.purchaseId === purchaseId)) {
        queryClient.invalidateQueries({ queryKey: purchaseKeys.detail(purchaseId) })
      }
    }

    onRealtimeEvent('purchase.received', handler)
    onRealtimeEvent('inventory.updated', handler)
    onRealtimeEvent('product.costUpdated', handler)

    return () => {
      offRealtimeEvent('purchase.received', handler)
      offRealtimeEvent('inventory.updated', handler)
      offRealtimeEvent('product.costUpdated', handler)
    }
  }, [businessId, purchaseId, queryClient])
}

async function invalidatePurchaseData(queryClient: ReturnType<typeof useQueryClient>) {
  await Promise.all([
    queryClient.invalidateQueries({ queryKey: purchaseKeys.all }),
    queryClient.invalidateQueries({ queryKey: ['inventory'] }),
    queryClient.invalidateQueries({ queryKey: ['products'] }),
  ])
}
