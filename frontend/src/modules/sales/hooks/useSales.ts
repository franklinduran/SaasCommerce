import { useEffect, useMemo, useRef } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import { getSales } from '@/modules/sales/services/salesApi'
import type {
  SalesFilters,
  SaleStatusChangedNotification,
} from '@/modules/sales/types/salesTypes'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const salesKeys = {
  all: ['sales'] as const,
  detail: (saleId: string) => ['sales', 'detail', saleId] as const,
  list: (filters: SalesFilters) => ['sales', 'list', filters] as const,
}

export function useSales(filters: SalesFilters) {
  return useQuery({
    queryFn: () => getSales(filters),
    queryKey: salesKeys.list(filters),
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

      void queryClient.invalidateQueries({ queryKey: salesKeys.all })
    }

    onRealtimeEvent('sale.statusChanged', handler)

    return () => {
      offRealtimeEvent('sale.statusChanged', handler)
    }
  }, [businessId, queryClient])
}
