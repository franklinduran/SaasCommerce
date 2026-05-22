import { useAuthStore } from '@/modules/auth/authStore'
import type {
  BusinessSubscriptionResponse,
  SubscriptionPlanResponse,
  SubscriptionUsageResponse,
} from '@/modules/subscription/types'
import { httpClient } from '@/shared/services/httpClient'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const subscriptionApi = {
  async getPlans(): Promise<SubscriptionPlanResponse[]> {
    const response = await httpClient<SubscriptionPlanResponse[]>('/api/subscription-plans', {
      accessToken: getAccessToken(),
    })

    return response.data ?? []
  },

  async getPlanById(planId: string): Promise<SubscriptionPlanResponse> {
    const response = await httpClient<SubscriptionPlanResponse>(`/api/subscription-plans/${planId}`, {
      accessToken: getAccessToken(),
    })

    return response.data!
  },

  async getCurrentSubscription(): Promise<BusinessSubscriptionResponse> {
    const response = await httpClient<BusinessSubscriptionResponse>('/api/subscription/current', {
      accessToken: getAccessToken(),
    })

    return response.data!
  },

  async getSubscriptionUsage(): Promise<SubscriptionUsageResponse> {
    const response = await httpClient<SubscriptionUsageResponse>('/api/subscription/usage', {
      accessToken: getAccessToken(),
    })

    return response.data!
  },

  async startTrial(): Promise<BusinessSubscriptionResponse> {
    const response = await httpClient<BusinessSubscriptionResponse>('/api/subscription/start-trial', {
      accessToken: getAccessToken(),
      method: 'POST',
    })

    return response.data!
  },

  async changePlan(planId: string): Promise<BusinessSubscriptionResponse> {
    const response = await httpClient<BusinessSubscriptionResponse>('/api/subscription/change-plan', {
      accessToken: getAccessToken(),
      body: JSON.stringify({ planId }),
      method: 'POST',
    })

    return response.data!
  },

  async cancelSubscription(): Promise<BusinessSubscriptionResponse> {
    const response = await httpClient<BusinessSubscriptionResponse>('/api/subscription/cancel', {
      accessToken: getAccessToken(),
      method: 'POST',
    })

    return response.data!
  },

  async reactivateSubscription(): Promise<BusinessSubscriptionResponse> {
    const response = await httpClient<BusinessSubscriptionResponse>('/api/subscription/reactivate', {
      accessToken: getAccessToken(),
      method: 'POST',
    })

    return response.data!
  },
}
