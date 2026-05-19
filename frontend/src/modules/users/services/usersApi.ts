import { httpClient } from '@/shared/services/httpClient'
import type { CreateUserRequest, ResetPasswordResponse, UpdateUserRequest, UserDetail, UserListResponse } from '../types'

export const usersApi = {
  async getUsers(): Promise<UserListResponse> {
    const response = await httpClient.get('/api/users')
    return response.data
  },

  async getUserById(userId: string): Promise<UserDetail> {
    const response = await httpClient.get(`/api/users/${userId}`)
    return response.data
  },

  async createUser(data: CreateUserRequest): Promise<{ userId: string }> {
    const response = await httpClient.post('/api/users', data)
    return response.data
  },

  async updateUser(userId: string, data: UpdateUserRequest): Promise<void> {
    await httpClient.put(`/api/users/${userId}`, data)
  },

  async updateUserRole(userId: string, role: string): Promise<void> {
    await httpClient.put(`/api/users/${userId}/role`, { role })
  },

  async disableUser(userId: string): Promise<void> {
    await httpClient.put(`/api/users/${userId}/disable`)
  },

  async activateUser(userId: string): Promise<void> {
    await httpClient.post(`/api/users/${userId}/activate`)
  },

  async resetPassword(userId: string): Promise<ResetPasswordResponse> {
    const response = await httpClient.post(`/api/users/${userId}/reset-password`)
    return response.data
  },
}
