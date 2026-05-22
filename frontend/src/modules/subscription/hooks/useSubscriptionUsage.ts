import { useQuery } from '@tanstack/react-query'
import { subscriptionApi } from '@/modules/subscription/services/subscriptionApi'
import type { SubscriptionResourceKey } from '@/modules/subscription/types'
import { subscriptionQueryKeys } from '@/modules/subscription/hooks/useSubscription'

type UseSubscriptionUsageOptions = {
  enabled?: boolean
}

export function useSubscriptionUsage(options: UseSubscriptionUsageOptions = {}) {
  const query = useQuery({
    enabled: options.enabled ?? true,
    queryFn: subscriptionApi.getSubscriptionUsage,
    queryKey: subscriptionQueryKeys.usage,
    retry: false,
    staleTime: 1000 * 60 * 2,
  })

  function getUsagePercentage(resource: SubscriptionResourceKey): number {
    const usage = query.data
    if (!usage) return 0

    const item = usage[resource]
    if (item.maximum <= 0) return 0

    return Math.min((item.current / item.maximum) * 100, 100)
  }

  function isNearLimit(resource: SubscriptionResourceKey): boolean {
    return getUsagePercentage(resource) >= 80
  }

  function isAtLimit(resource: SubscriptionResourceKey): boolean {
    return query.data?.[resource].isAtLimit ?? false
  }

  function isFeatureEnabled(featureName: string): boolean {
    return query.data?.features.some((feature) => feature.name === featureName && feature.isEnabled) ?? false
  }

  return {
    error: query.error,
    getUsagePercentage,
    isAtLimit,
    isFeatureEnabled,
    isLoading: query.isLoading,
    isNearLimit,
    refetch: query.refetch,
    usage: query.data,
  }
}
