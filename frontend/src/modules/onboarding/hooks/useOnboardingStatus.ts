import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { onboardingApi } from '@/modules/onboarding/services/onboardingApi'

export const onboardingQueryKeys = {
  status: ['onboarding', 'status'] as const,
}

export function useOnboardingStatus() {
  return useQuery({
    queryFn: onboardingApi.getStatus,
    queryKey: onboardingQueryKeys.status,
    staleTime: 1000 * 60 * 2,
  })
}

export function useCompleteOnboardingStep() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (step: string) => onboardingApi.completeStep(step),
    onSuccess: (status) => {
      queryClient.setQueryData(onboardingQueryKeys.status, status)
    },
  })
}
