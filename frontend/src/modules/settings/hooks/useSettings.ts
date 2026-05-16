import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  changeMyPassword,
  getCurrentBranch,
  getCurrentBusiness,
  getMe,
  updateCurrentBranch,
  updateCurrentBusiness,
  updateMyProfile,
} from '@/modules/settings/services/settingsService'

export const settingsKeys = {
  branch: ['settings', 'branch'] as const,
  business: ['settings', 'business'] as const,
  me: ['settings', 'me'] as const,
}

export function useMeQuery() {
  return useQuery({
    queryFn: getMe,
    queryKey: settingsKeys.me,
  })
}

export function useCurrentBusinessQuery() {
  return useQuery({
    queryFn: getCurrentBusiness,
    queryKey: settingsKeys.business,
  })
}

export function useCurrentBranchQuery() {
  return useQuery({
    queryFn: getCurrentBranch,
    queryKey: settingsKeys.branch,
  })
}

export function useUpdateProfileMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: updateMyProfile,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: settingsKeys.me })
    },
  })
}

export function useChangePasswordMutation() {
  return useMutation({ mutationFn: changeMyPassword })
}

export function useUpdateBusinessMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: updateCurrentBusiness,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: settingsKeys.business })
      void queryClient.invalidateQueries({ queryKey: settingsKeys.me })
    },
  })
}

export function useUpdateBranchMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: updateCurrentBranch,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: settingsKeys.branch })
      void queryClient.invalidateQueries({ queryKey: settingsKeys.me })
    },
  })
}
