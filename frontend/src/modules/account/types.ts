import type { AuthUser } from '@/modules/auth/types'

export type RegisterBusinessRequest = {
  businessName: string
  ownerFullName: string
  email: string
  password: string
  identificationType: string | null
  identificationNumber: string | null
  phones: RegisterBusinessPhoneRequest[] | null
  branchName: string
  planId: string
}

export type RegisterBusinessPhoneRequest = {
  number: string
  label: string | null
  isPrimary: boolean
}

export type RegisterBusinessResponse = {
  businessId: string
  branchId: string
  userId: string
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: AuthUser
}
