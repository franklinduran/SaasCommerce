import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  changeMyPassword,
  getBillingSettings,
  getBusinessSettings,
  getCurrentBranch,
  getCurrentBusiness,
  getInventorySettings,
  getMe,
  getSalesSettings,
  updateBillingSettings,
  updateBusinessSettings,
  updateCurrentBranch,
  updateCurrentBusiness,
  updateInventorySettings,
  updateMyProfile,
  updateSalesSettings,
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
      queryClient.invalidateQueries({ queryKey: settingsKeys.me })
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
      queryClient.invalidateQueries({ queryKey: settingsKeys.business })
      queryClient.invalidateQueries({ queryKey: settingsKeys.me })
    },
  })
}

export function useUpdateBranchMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: updateCurrentBranch,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: settingsKeys.branch })
      queryClient.invalidateQueries({ queryKey: settingsKeys.me })
    },
  })
}

// ── Operational / SaaS settings ─────────────────────────────────────────────

export const opsSettingsKeys = {
  billing: ['ops-settings', 'billing'] as const,
  business: ['ops-settings', 'business'] as const,
  inventory: ['ops-settings', 'inventory'] as const,
  sales: ['ops-settings', 'sales'] as const,
}

export function useBusinessSettingsQuery() {
  return useQuery({
    queryFn: getBusinessSettings,
    queryKey: opsSettingsKeys.business,
  })
}

export function useUpdateBusinessSettingsMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateBusinessSettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opsSettingsKeys.business })
    },
  })
}

export function useSalesSettingsQuery() {
  return useQuery({
    queryFn: getSalesSettings,
    queryKey: opsSettingsKeys.sales,
  })
}

export function useUpdateSalesSettingsMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateSalesSettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opsSettingsKeys.sales })
    },
  })
}

export function useInventorySettingsQuery() {
  return useQuery({
    queryFn: getInventorySettings,
    queryKey: opsSettingsKeys.inventory,
  })
}

export function useUpdateInventorySettingsMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateInventorySettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opsSettingsKeys.inventory })
    },
  })
}

export function useBillingSettingsQuery() {
  return useQuery({
    queryFn: getBillingSettings,
    queryKey: opsSettingsKeys.billing,
  })
}

export function useUpdateBillingSettingsMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateBillingSettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: opsSettingsKeys.billing })
    },
  })
}
