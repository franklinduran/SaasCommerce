import { useQuery } from '@tanstack/react-query'
import { getCurrentBranchForPOS } from '@/modules/pos/services/salesApi'

export function useCurrentBranchForPOS(enabled: boolean) {
  return useQuery({
    enabled,
    queryKey: ['pos-current-branch'],
    queryFn: getCurrentBranchForPOS,
  })
}
