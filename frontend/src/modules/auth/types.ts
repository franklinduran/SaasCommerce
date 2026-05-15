export type AuthUser = {
  id: string
  businessId: string
  branchId: string | null
  fullName: string
  email: string
  roles: string[]
}

export type LoginRequest = {
  email: string
  password: string
}

export type LoginResponse = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: AuthUser
}

export type RefreshTokenRequest = {
  refreshToken: string
}

export type AuthSession = LoginResponse
