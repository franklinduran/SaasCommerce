import { useEffect, useMemo, useRef } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import { createSaleReturn, getSaleReturns, getSales } from '@/modules/sales/services/salesApi'
import type {
  CreateSaleReturnInput,
  SaleReturnChangedNotification,
  SalesFilters,
  SaleStatusChangedNotification,
} from '@/modules/sales/types/salesTypes'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const salesKeys = {
  all: ['sales'] as const,
  detail: (saleId: string) => ['sales', 'detail', saleId] as const,
  list: (filters: SalesFilters) => ['sales', 'list', filters] as const,
  returns: (saleId: string) => ['sales', 'detail', saleId, 'returns'] as const,
}

export function useSales(filters: SalesFilters) {
  return useQuery({
    queryFn: () => getSales(filters),
    queryKey: salesKeys.list(filters),
  })
}

export function useSaleReturns(saleId: string | undefined) {
  return useQuery({
    enabled: Boolean(saleId),
    queryFn: () => getSaleReturns(saleId!),
    queryKey: salesKeys.returns(saleId ?? ''),
  })
}

export function useCreateSaleReturn(saleId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: CreateSaleReturnInput) => createSaleReturn(saleId, input),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: salesKeys.detail(saleId) }),
        queryClient.invalidateQueries({ queryKey: salesKeys.returns(saleId) }),
        queryClient.invalidateQueries({ queryKey: salesKeys.all }),
      ])
    },
  })
}

export function useSaleStatusInvalidation(visibleSaleIds: string[]) {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()
  const visibleSaleIdsKey = useMemo(() => visibleSaleIds.join('|'), [visibleSaleIds])
  const visibleSaleIdsRef = useRef(new Set(visibleSaleIds))

  useEffect(() => {
    visibleSaleIdsRef.current = new Set(visibleSaleIds)
  }, [visibleSaleIds, visibleSaleIdsKey])

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: SaleStatusChangedNotification) => {
      if (payload.businessId !== businessId) {
        return
      }

      const visibleSaleIdsSet = visibleSaleIdsRef.current

      if (visibleSaleIdsSet.size > 0 && !visibleSaleIdsSet.has(payload.saleId)) {
        return
      }

      queryClient.invalidateQueries({ queryKey: salesKeys.all })
    }

    onRealtimeEvent('sale.statusChanged', handler)

    return () => {
      offRealtimeEvent('sale.statusChanged', handler)
    }
  }, [businessId, queryClient])
}

export function useSaleReturnInvalidation(saleId: string | undefined) {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId || !saleId) {
      return undefined
    }

    const handler = (payload: SaleReturnChangedNotification) => {
      if (payload.businessId !== businessId || payload.saleId !== saleId) {
        return
      }

      queryClient.invalidateQueries({ queryKey: salesKeys.detail(saleId) })
      queryClient.invalidateQueries({ queryKey: salesKeys.returns(saleId) })
      queryClient.invalidateQueries({ queryKey: salesKeys.all })
    }

    onRealtimeEvent('sale.returnChanged', handler)

    return () => {
      offRealtimeEvent('sale.returnChanged', handler)
    }
  }, [businessId, queryClient, saleId])
}
