import { beforeEach, describe, expect, it, vi } from 'vitest'
import { onboardingApi } from '@/modules/onboarding/services/onboardingApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'onboarding-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('onboardingApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { completedSteps: [] } })
  })

  it('loads status and completes steps', async () => {
    await onboardingApi.getStatus()
    await onboardingApi.completeStep('inventory')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/onboarding/status', { accessToken: 'onboarding-token' })
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/onboarding/steps/inventory/complete', {
      accessToken: 'onboarding-token',
      method: 'POST',
    })
  })
})
