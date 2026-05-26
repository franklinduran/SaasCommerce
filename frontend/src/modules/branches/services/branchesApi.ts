import { useAuthStore } from '@/modules/auth/authStore'
import type {
  Branch,
  BranchFilters,
  BranchListResponse,
  CreateBranchRequest,
  UpdateBranchRequest,
} from '@/modules/branches/types'
import { httpClient } from '@/shared/services/httpClient'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

function toQueryString(filters: Record<string, boolean | number | string | null | undefined>): string {
  const params = new URLSearchParams()

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== '' && value !== false && value !== null && value !== undefined) {
      params.set(key, String(value))
    }
  })

  return params.toString()
}

export async function getBranches(filters: BranchFilters): Promise<BranchListResponse> {
  const qs = toQueryString(filters as Record<string, boolean | number | string | null | undefined>)
  const querySuffix = qs ? `?${qs}` : ''
  const response = await httpClient<BranchListResponse>(`/api/branches${querySuffix}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getBranchById(branchId: string): Promise<Branch> {
  const response = await httpClient<Branch>(`/api/branches/${branchId}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function createBranch(request: CreateBranchRequest): Promise<Branch> {
  const response = await httpClient<Branch>('/api/branches', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'POST',
  })

  return response.data!
}

export async function updateBranch(branchId: string, request: UpdateBranchRequest): Promise<void> {
  await httpClient(`/api/branches/${branchId}`, {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })
}

export async function activateBranch(branchId: string): Promise<void> {
  await httpClient(`/api/branches/${branchId}/activate`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })
}

export async function deactivateBranch(branchId: string): Promise<void> {
  await httpClient(`/api/branches/${branchId}/deactivate`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })
}
