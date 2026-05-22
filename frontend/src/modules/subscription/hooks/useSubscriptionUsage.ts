import { useQuery } from '@tanstack/react-query';
import { subscriptionApi } from '../services/subscriptionApi';

/**
 * Hook to fetch and manage subscription usage information
 */
export const useSubscriptionUsage = () => {
  const {
    data: usage,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['subscription', 'usage'],
    queryFn: subscriptionApi.getSubscriptionUsage,
    staleTime: 1000 * 60 * 2, // 2 minutes
    retry: 3,
  });

  /**
   * Calculate percentage of usage for a resource
   */
  const getUsagePercentage = (resource: 'branches' | 'users' | 'products' | 'sales'): number => {
    if (!usage) return 0;
    const res = usage[resource];
    if (res.maximum === 0) return 0;
    return (res.current / res.maximum) * 100;
  };

  /**
   * Check if a resource is at or near limit
   */
  const isNearLimit = (resource: 'branches' | 'users' | 'products' | 'sales'): boolean => {
    if (!usage) return false;
    const percentage = getUsagePercentage(resource);
    return percentage >= 80; // 80% or more
  };

  /**
   * Check if a resource is at its limit
   */
  const isAtLimit = (resource: 'branches' | 'users' | 'products' | 'sales'): boolean => {
    if (!usage) return false;
    return usage[resource].isAtLimit;
  };

  /**
   * Check if a feature is enabled
   */
  const isFeatureEnabled = (featureName: string): boolean => {
    if (!usage) return false;
    const feature = usage.features.find(f => f.name === featureName);
    return feature?.isEnabled ?? false;
  };

  return {
    usage,
    isLoading,
    error,
    refetch,
    getUsagePercentage,
    isNearLimit,
    isAtLimit,
    isFeatureEnabled,
  };
};
