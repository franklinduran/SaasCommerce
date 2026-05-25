import { useAuthStore } from '@/modules/auth/authStore'
import type { OnboardingStatusResponse } from '@/modules/onboarding/types'
import { httpClient } from '@/shared/services/httpClient'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const onboardingApi = {
  async getStatus(): Promise<OnboardingStatusResponse> {
    const response = await httpClient<OnboardingStatusResponse>('/api/onboarding/status', {
      accessToken: getAccessToken(),
    })

    return response.data!
  },

  async completeStep(step: string): Promise<OnboardingStatusResponse> {
    const response = await httpClient<OnboardingStatusResponse>(
      `/api/onboarding/steps/${step}/complete`,
      {
        accessToken: getAccessToken(),
        method: 'POST',
      },
    )

    return response.data!
  },
}
