import { useQuery } from '@tanstack/react-query'
import { adminApi } from '@/modules/admin/services/adminApi'

export function usePilotMetrics() {
  return useQuery({
    queryKey: ['admin', 'pilot-metrics'],
    queryFn: () => adminApi.getPilotMetrics(),
    staleTime: 2 * 60 * 1000, // 2 min — metrics don't change that fast
  })
}
