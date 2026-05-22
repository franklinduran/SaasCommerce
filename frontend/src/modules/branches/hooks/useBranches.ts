import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  activateBranch,
  createBranch,
  deactivateBranch,
  getBranchById,
  getBranches,
  updateBranch,
} from '@/modules/branches/services/branchesApi'
import type { BranchFilters, CreateBranchRequest, UpdateBranchRequest } from '@/modules/branches/types'

export const branchKeys = {
  all: ['branches'] as const,
  detail: (branchId: string) => ['branches', 'detail', branchId] as const,
  list: (filters: BranchFilters) => ['branches', 'list', filters] as const,
}

export function useBranches(filters: BranchFilters = {}) {
  return useQuery({
    queryKey: branchKeys.list(filters),
    queryFn: () => getBranches(filters),
  })
}

export function useBranchById(branchId?: string) {
  return useQuery({
    enabled: Boolean(branchId),
    queryKey: branchId ? branchKeys.detail(branchId) : ['branches', 'detail'],
    queryFn: () => getBranchById(branchId!),
  })
}

export function useCreateBranchMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateBranchRequest) => createBranch(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: branchKeys.all })
    },
  })
}

export function useUpdateBranchMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ branchId, request }: { branchId: string; request: UpdateBranchRequest }) =>
      updateBranch(branchId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: branchKeys.all })
    },
  })
}

export function useActivateBranchMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (branchId: string) => activateBranch(branchId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: branchKeys.all })
    },
  })
}

export function useDeactivateBranchMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (branchId: string) => deactivateBranch(branchId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: branchKeys.all })
    },
  })
}
