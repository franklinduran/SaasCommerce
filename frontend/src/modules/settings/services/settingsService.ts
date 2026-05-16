import { useAuthStore } from '@/modules/auth/authStore'
import type {
  ChangePasswordRequest,
  CurrentBranch,
  CurrentBusiness,
  CurrentUser,
  UpdateBranchRequest,
  UpdateBusinessRequest,
  UpdateProfileRequest,
} from '@/modules/settings/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getMe(): Promise<CurrentUser> {
  const response = await httpClient<CurrentUser>('/api/me', {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function updateMyProfile(request: UpdateProfileRequest): Promise<CurrentUser> {
  const response = await httpClient<CurrentUser>('/api/me/profile', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })

  return response.data!
}

export async function changeMyPassword(request: ChangePasswordRequest): Promise<string> {
  const response = await httpClient<string>('/api/me/password', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })

  return response.data!
}

export async function getCurrentBusiness(): Promise<CurrentBusiness> {
  const response = await httpClient<CurrentBusiness>('/api/business/current', {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function updateCurrentBusiness(
  request: UpdateBusinessRequest,
): Promise<CurrentBusiness> {
  const response = await httpClient<CurrentBusiness>('/api/business/current', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })

  return response.data!
}

export async function getCurrentBranch(): Promise<CurrentBranch> {
  const response = await httpClient<CurrentBranch>('/api/branches/current', {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function updateCurrentBranch(
  request: UpdateBranchRequest,
): Promise<CurrentBranch> {
  const response = await httpClient<CurrentBranch>('/api/branches/current', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })

  return response.data!
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
