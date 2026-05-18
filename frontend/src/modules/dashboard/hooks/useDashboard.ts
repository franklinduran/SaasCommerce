import { useEffect } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import { getDashboardSummary } from '@/modules/dashboard/services/dashboardApi'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const dashboardKeys = {
  all: ['dashboard'] as const,
  summary: () => ['dashboard', 'summary'] as const,
}

export function useDashboardSummary() {
  return useQuery({
    queryFn: getDashboardSummary,
    queryKey: dashboardKeys.summary(),
    refetchInterval: 5 * 60 * 1000, // refresh every 5 min as fallback
    staleTime: 30 * 1000, // 30 seconds
  })
}

export function useDashboardRealtimeInvalidation() {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const invalidate = (payload: { businessId?: string }) => {
      if (payload.businessId && payload.businessId !== businessId) {
        return
      }
      void queryClient.invalidateQueries({ queryKey: dashboardKeys.summary() })
    }

    onRealtimeEvent('sale.statusChanged', invalidate)
    onRealtimeEvent('invoice.generated', invalidate)
    onRealtimeEvent('invoice.cancelled', invalidate)
    onRealtimeEvent('payment.registered', invalidate)
    onRealtimeEvent('inventory.lowStock', invalidate)
    onRealtimeEvent('purchase.received', invalidate)

    return () => {
      offRealtimeEvent('sale.statusChanged', invalidate)
      offRealtimeEvent('invoice.generated', invalidate)
      offRealtimeEvent('invoice.cancelled', invalidate)
      offRealtimeEvent('payment.registered', invalidate)
      offRealtimeEvent('inventory.lowStock', invalidate)
      offRealtimeEvent('purchase.received', invalidate)
    }
  }, [businessId, queryClient])
}
