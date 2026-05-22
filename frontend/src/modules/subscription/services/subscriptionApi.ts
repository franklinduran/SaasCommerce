import { apiClient } from '@/lib/apiClient';
import {
  SubscriptionPlanResponse,
  BusinessSubscriptionResponse,
  SubscriptionUsageResponse,
} from '../types';

export const subscriptionApi = {
  /**
   * Get all active subscription plans
   */
  getPlans: async (): Promise<SubscriptionPlanResponse[]> => {
    const response = await apiClient.get<SubscriptionPlanResponse[]>('/api/subscription-plans');
    return response.data;
  },

  /**
   * Get a specific subscription plan by ID
   */
  getPlanById: async (planId: string): Promise<SubscriptionPlanResponse> => {
    const response = await apiClient.get<SubscriptionPlanResponse>(
      `/api/subscription-plans/${planId}`
    );
    return response.data;
  },

  /**
   * Get current business subscription
   */
  getCurrentSubscription: async (): Promise<BusinessSubscriptionResponse> => {
    const response = await apiClient.get<BusinessSubscriptionResponse>(
      '/api/subscription/current'
    );
    return response.data;
  },

  /**
   * Get current subscription usage and limits
   */
  getSubscriptionUsage: async (): Promise<SubscriptionUsageResponse> => {
    const response = await apiClient.get<SubscriptionUsageResponse>(
      '/api/subscription/usage'
    );
    return response.data;
  },

  /**
   * Start a trial subscription for a new business
   */
  startTrial: async (): Promise<BusinessSubscriptionResponse> => {
    const response = await apiClient.post<BusinessSubscriptionResponse>(
      '/api/subscription/start-trial'
    );
    return response.data;
  },

  /**
   * Change the business subscription plan
   */
  changePlan: async (newPlanId: string): Promise<BusinessSubscriptionResponse> => {
    const response = await apiClient.post<BusinessSubscriptionResponse>(
      '/api/subscription/change-plan',
      { planId: newPlanId }
    );
    return response.data;
  },

  /**
   * Cancel the current subscription
   */
  cancelSubscription: async (): Promise<BusinessSubscriptionResponse> => {
    const response = await apiClient.post<BusinessSubscriptionResponse>(
      '/api/subscription/cancel'
    );
    return response.data;
  },

  /**
   * Reactivate a cancelled subscription
   */
  reactivateSubscription: async (): Promise<BusinessSubscriptionResponse> => {
    const response = await apiClient.post<BusinessSubscriptionResponse>(
      '/api/subscription/reactivate'
    );
    return response.data;
  },
};
