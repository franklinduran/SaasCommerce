import { useEffect, useRef } from 'react'
import { useAuthStore } from '@/modules/auth/authStore'
import type { SaleStatusChangedNotification } from '@/modules/pos/types/posTypes'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

type SaleStatusSubscriptionOptions = {
  saleId: string | null
  onStatusChanged: (payload: SaleStatusChangedNotification) => void
}

export function useSaleStatusSubscription({
  onStatusChanged,
  saleId,
}: Readonly<SaleStatusSubscriptionOptions>) {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const saleIdRef = useRef(saleId)
  const onStatusChangedRef = useRef(onStatusChanged)

  useEffect(() => {
    saleIdRef.current = saleId
  }, [saleId])

  useEffect(() => {
    onStatusChangedRef.current = onStatusChanged
  }, [onStatusChanged])

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: SaleStatusChangedNotification) => {
      const activeSaleId = saleIdRef.current

      if (!activeSaleId || payload.saleId !== activeSaleId || payload.businessId !== businessId) {
        return
      }

      onStatusChangedRef.current(payload)
    }

    onRealtimeEvent('sale.statusChanged', handler)

    return () => {
      offRealtimeEvent('sale.statusChanged', handler)
    }
  }, [businessId])
}
