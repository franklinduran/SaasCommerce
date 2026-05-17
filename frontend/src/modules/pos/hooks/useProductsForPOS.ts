import { useQuery } from '@tanstack/react-query'
import { getProductsForPOS } from '@/modules/pos/services/salesApi'

export function useProductsForPOS(query: string) {
  return useQuery({
    queryKey: ['pos-products', query],
    queryFn: () => getProductsForPOS(query),
  })
}
