import { useEffect, useRef } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { getSaleByIdForPOS } from '@/modules/pos/services/salesApi'
import type { SaleResponse } from '@/modules/pos/types/posTypes'

type SaleStatusSyncOptions = {
  enabled: boolean
  onSaleLoaded: (sale: SaleResponse) => void
  saleId: string | null
}

export function useSaleStatusSync({
  enabled,
  onSaleLoaded,
  saleId,
}: Readonly<SaleStatusSyncOptions>) {
  const queryClient = useQueryClient()
  const onSaleLoadedRef = useRef(onSaleLoaded)

  useEffect(() => {
    onSaleLoadedRef.current = onSaleLoaded
  }, [onSaleLoaded])

  useEffect(() => {
    if (!enabled || !saleId) {
      return undefined
    }

    let isActive = true

    const syncSale = async () => {
      const activeSaleId = saleId
      const sale = await queryClient
        .fetchQuery({
          queryFn: () => getSaleByIdForPOS(activeSaleId),
          queryKey: ['pos-sale', activeSaleId],
          staleTime: 0,
        })
        .catch(() => null)

      if (isActive && sale?.saleId === activeSaleId) {
        onSaleLoadedRef.current(sale)
      }
    }

    void syncSale()
    const intervalId = window.setInterval(() => void syncSale(), 2000)

    return () => {
      isActive = false
      window.clearInterval(intervalId)
    }
  }, [enabled, queryClient, saleId])
}
