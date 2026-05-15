import { httpClient } from '@/shared/services/httpClient'
import type {
  AuthUser,
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
} from '@/modules/auth/types'

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await httpClient<LoginResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(request),
  })

  return response.data!
}

export async function refreshToken(
  request: RefreshTokenRequest,
): Promise<LoginResponse> {
  const response = await httpClient<LoginResponse>('/api/auth/refresh', {
    method: 'POST',
    body: JSON.stringify(request),
  })

  return response.data!
}

export async function getCurrentUser(accessToken: string): Promise<AuthUser> {
  const response = await httpClient<AuthUser>('/api/me', {
    accessToken,
  })

  return response.data!
}
