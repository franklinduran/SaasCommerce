import { useQuery } from '@tanstack/react-query'
import { getProductsForPOS } from '@/modules/pos/services/salesApi'

export const POS_PAGE_SIZE = 25

export function useProductsForPOS(query: string, page: number) {
  return useQuery({
    queryKey: ['pos-products', query, page],
    queryFn: () => getProductsForPOS(query, page, POS_PAGE_SIZE),
  })
}
