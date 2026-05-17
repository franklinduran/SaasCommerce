import { useQuery } from '@tanstack/react-query'
import { getSaleDetail } from '@/modules/sales/services/salesApi'
import { salesKeys } from '@/modules/sales/hooks/useSales'

export function useSaleDetail(saleId: string | undefined) {
  return useQuery({
    enabled: Boolean(saleId),
    queryFn: () => getSaleDetail(saleId!),
    queryKey: salesKeys.detail(saleId ?? ''),
  })
}
