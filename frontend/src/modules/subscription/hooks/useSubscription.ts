import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { subscriptionApi } from '@/modules/subscription/services/subscriptionApi'

export const subscriptionQueryKeys = {
  current: ['subscription', 'current'] as const,
  plans: ['subscription', 'plans'] as const,
  usage: ['subscription', 'usage'] as const,
}

export function useSubscription() {
  const queryClient = useQueryClient()

  const currentSubscription = useQuery({
    queryFn: subscriptionApi.getCurrentSubscription,
    queryKey: subscriptionQueryKeys.current,
    retry: false,
    staleTime: 1000 * 60 * 5,
  })

  const changePlanMutation = useMutation({
    mutationFn: (planId: string) => subscriptionApi.changePlan(planId),
    onSuccess: (subscription) => {
      queryClient.setQueryData(subscriptionQueryKeys.current, subscription)
      queryClient.invalidateQueries({ queryKey: subscriptionQueryKeys.usage })
    },
  })

  const cancelMutation = useMutation({
    mutationFn: subscriptionApi.cancelSubscription,
    onSuccess: (subscription) => {
      queryClient.setQueryData(subscriptionQueryKeys.current, subscription)
      queryClient.invalidateQueries({ queryKey: subscriptionQueryKeys.usage })
    },
  })

  const reactivateMutation = useMutation({
    mutationFn: subscriptionApi.reactivateSubscription,
    onSuccess: (subscription) => {
      queryClient.setQueryData(subscriptionQueryKeys.current, subscription)
      queryClient.invalidateQueries({ queryKey: subscriptionQueryKeys.usage })
    },
  })

  const startTrialMutation = useMutation({
    mutationFn: subscriptionApi.startTrial,
    onSuccess: (subscription) => {
      queryClient.setQueryData(subscriptionQueryKeys.current, subscription)
      queryClient.invalidateQueries({ queryKey: subscriptionQueryKeys.usage })
    },
  })

  return {
    cancelSubscription: cancelMutation.mutateAsync,
    changePlan: changePlanMutation.mutateAsync,
    error: currentSubscription.error,
    isCancelling: cancelMutation.isPending,
    isChangingPlan: changePlanMutation.isPending,
    isLoading: currentSubscription.isLoading,
    isReactivating: reactivateMutation.isPending,
    isStartingTrial: startTrialMutation.isPending,
    reactivateSubscription: reactivateMutation.mutateAsync,
    refetch: currentSubscription.refetch,
    startTrial: startTrialMutation.mutateAsync,
    subscription: currentSubscription.data,
  }
}

export function useSubscriptionPlans() {
  return useQuery({
    queryFn: subscriptionApi.getPlans,
    queryKey: subscriptionQueryKeys.plans,
    staleTime: 1000 * 60 * 30,
  })
}
