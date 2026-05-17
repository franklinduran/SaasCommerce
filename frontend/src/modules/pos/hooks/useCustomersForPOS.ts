import { useQuery } from '@tanstack/react-query'
import { getCustomersForPOS } from '@/modules/pos/services/salesApi'

export function useCustomersForPOS(query: string) {
  return useQuery({
    queryKey: ['pos-customers', query],
    queryFn: () => getCustomersForPOS(query),
  })
}
