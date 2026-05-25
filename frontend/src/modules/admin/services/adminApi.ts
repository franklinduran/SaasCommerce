import { useAuthStore } from '@/modules/auth/authStore'
import type {
  CreatePilotBusinessRequest,
  CreatePilotBusinessResponse,
} from '@/modules/admin/types'
import { httpClient } from '@/shared/services/httpClient'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const adminApi = {
  async createPilotBusiness(
    request: CreatePilotBusinessRequest,
  ): Promise<CreatePilotBusinessResponse> {
    const response = await httpClient<CreatePilotBusinessResponse>(
      '/api/admin/pilot-businesses',
      {
        accessToken: getAccessToken(),
        body: JSON.stringify(request),
        method: 'POST',
      },
    )

    return response.data!
  },
}
