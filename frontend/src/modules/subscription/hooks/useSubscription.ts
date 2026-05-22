import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { subscriptionApi } from '../services/subscriptionApi';

/**
 * Hook to manage business subscription state
 */
export const useSubscription = () => {
  const queryClient = useQueryClient();

  // Fetch current subscription
  const {
    data: subscription,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['subscription', 'current'],
    queryFn: subscriptionApi.getCurrentSubscription,
    staleTime: 1000 * 60 * 5, // 5 minutes
    retry: 3,
  });

  // Mutation for changing plan
  const changeplanMutation = useMutation({
    mutationFn: (planId: string) => subscriptionApi.changePlan(planId),
    onSuccess: (newSubscription) => {
      queryClient.setQueryData(['subscription', 'current'], newSubscription);
      queryClient.invalidateQueries({ queryKey: ['subscription', 'usage'] });
    },
  });

  // Mutation for cancelling subscription
  const cancelMutation = useMutation({
    mutationFn: () => subscriptionApi.cancelSubscription(),
    onSuccess: (cancelledSubscription) => {
      queryClient.setQueryData(['subscription', 'current'], cancelledSubscription);
      queryClient.invalidateQueries({ queryKey: ['subscription', 'usage'] });
    },
  });

  // Mutation for reactivating subscription
  const reactivateMutation = useMutation({
    mutationFn: () => subscriptionApi.reactivateSubscription(),
    onSuccess: (reactivatedSubscription) => {
      queryClient.setQueryData(['subscription', 'current'], reactivatedSubscription);
      queryClient.invalidateQueries({ queryKey: ['subscription', 'usage'] });
    },
  });

  // Mutation for starting trial
  const startTrialMutation = useMutation({
    mutationFn: () => subscriptionApi.startTrial(),
    onSuccess: (trialSubscription) => {
      queryClient.setQueryData(['subscription', 'current'], trialSubscription);
      queryClient.invalidateQueries({ queryKey: ['subscription', 'usage'] });
    },
  });

  return {
    subscription,
    isLoading,
    error,
    refetch,
    changePlan: changeplanMutation.mutateAsync,
    isChangingPlan: changeplanMutation.isPending,
    cancelSubscription: cancelMutation.mutateAsync,
    isCancelling: cancelMutation.isPending,
    reactivateSubscription: reactivateMutation.mutateAsync,
    isReactivating: reactivateMutation.isPending,
    startTrial: startTrialMutation.mutateAsync,
    isStartingTrial: startTrialMutation.isPending,
  };
};
