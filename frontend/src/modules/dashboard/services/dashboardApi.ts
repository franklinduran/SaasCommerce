import { useAuthStore } from '@/modules/auth/authStore'
import type { DashboardSummary } from '@/modules/dashboard/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getDashboardSummary(): Promise<DashboardSummary> {
  const response = await httpClient<DashboardSummary>('/api/dashboard/summary', {
    accessToken: getAccessToken(),
  })

  return response.data!
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
