import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'
import type { CreateUserRequest, ResetPasswordResponse, UpdateUserRequest, UserDetail, UserListResponse } from '../types'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const usersApi = {
  async getUsers(): Promise<UserListResponse> {
    const response = await httpClient<UserListResponse>('/api/users', {
      accessToken: getAccessToken(),
    })
    return response.data!
  },

  async getUserById(userId: string): Promise<UserDetail> {
    const response = await httpClient<UserDetail>(`/api/users/${userId}`, {
      accessToken: getAccessToken(),
    })
    return response.data!
  },

  async createUser(data: CreateUserRequest): Promise<{ userId: string }> {
    const response = await httpClient<{ userId: string }>('/api/users', {
      accessToken: getAccessToken(),
      body: JSON.stringify(data),
      method: 'POST',
    })
    return response.data!
  },

  async updateUser(userId: string, data: UpdateUserRequest): Promise<void> {
    await httpClient<void>(`/api/users/${userId}`, {
      accessToken: getAccessToken(),
      body: JSON.stringify(data),
      method: 'PUT',
    })
  },

  async updateUserRole(userId: string, role: string): Promise<void> {
    await httpClient<void>(`/api/users/${userId}/role`, {
      accessToken: getAccessToken(),
      body: JSON.stringify({ role }),
      method: 'PUT',
    })
  },

  async disableUser(userId: string): Promise<void> {
    await httpClient<void>(`/api/users/${userId}/disable`, {
      accessToken: getAccessToken(),
      method: 'PUT',
    })
  },

  async activateUser(userId: string): Promise<void> {
    await httpClient<void>(`/api/users/${userId}/activate`, {
      accessToken: getAccessToken(),
      method: 'POST',
    })
  },

  async resetPassword(userId: string): Promise<ResetPasswordResponse> {
    const response = await httpClient<ResetPasswordResponse>(
      `/api/users/${userId}/reset-password`,
      {
        accessToken: getAccessToken(),
        method: 'POST',
      },
    )
    return response.data!
  },
}
