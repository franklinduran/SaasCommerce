import { useAuthStore } from '@/modules/auth/authStore'
import type {
  BillingSettingsData,
  BusinessSettingsData,
  ChangePasswordRequest,
  CurrentBranch,
  CurrentBusiness,
  CurrentUser,
  InventorySettingsData,
  SalesSettingsData,
  UpdateBillingSettingsRequest,
  UpdateBranchRequest,
  UpdateBusinessRequest,
  UpdateBusinessSettingsRequest,
  UpdateInventorySettingsRequest,
  UpdateProfileRequest,
  UpdateSalesSettingsRequest,
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

// ── Operational / SaaS settings ─────────────────────────────────────────────

export async function getBusinessSettings(): Promise<BusinessSettingsData> {
  const response = await httpClient<BusinessSettingsData>('/api/settings/business', {
    accessToken: getAccessToken(),
  })
  return response.data!
}

export async function updateBusinessSettings(
  request: UpdateBusinessSettingsRequest,
): Promise<BusinessSettingsData> {
  const response = await httpClient<BusinessSettingsData>('/api/settings/business', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })
  return response.data!
}

export async function getSalesSettings(): Promise<SalesSettingsData> {
  const response = await httpClient<SalesSettingsData>('/api/settings/sales', {
    accessToken: getAccessToken(),
  })
  return response.data!
}

export async function updateSalesSettings(
  request: UpdateSalesSettingsRequest,
): Promise<SalesSettingsData> {
  const response = await httpClient<SalesSettingsData>('/api/settings/sales', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })
  return response.data!
}

export async function getInventorySettings(): Promise<InventorySettingsData> {
  const response = await httpClient<InventorySettingsData>('/api/settings/inventory', {
    accessToken: getAccessToken(),
  })
  return response.data!
}

export async function updateInventorySettings(
  request: UpdateInventorySettingsRequest,
): Promise<InventorySettingsData> {
  const response = await httpClient<InventorySettingsData>('/api/settings/inventory', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })
  return response.data!
}

export async function getBillingSettings(): Promise<BillingSettingsData> {
  const response = await httpClient<BillingSettingsData>('/api/settings/billing', {
    accessToken: getAccessToken(),
  })
  return response.data!
}

export async function updateBillingSettings(
  request: UpdateBillingSettingsRequest,
): Promise<BillingSettingsData> {
  const response = await httpClient<BillingSettingsData>('/api/settings/billing', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })
  return response.data!
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
