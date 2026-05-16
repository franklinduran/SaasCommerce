export type BusinessPhone = {
  number: string
  label: string | null
  isPrimary: boolean
}

export type CurrentBusiness = {
  businessId: string
  name: string
  identificationType: string | null
  identificationNumber: string | null
  phones: BusinessPhone[]
}

export type CurrentBranch = {
  branchId: string
  businessId: string
  name: string
  address: string | null
  phone: string | null
}

export type CurrentUserBusiness = {
  businessId: string
  name: string
  identificationType: string | null
  identificationNumber: string | null
}

export type CurrentUserBranch = {
  branchId: string
  name: string
}

export type CurrentUser = {
  userId: string
  fullName: string
  email: string
  phone: string | null
  roles: string[]
  business: CurrentUserBusiness
  branch: CurrentUserBranch | null
}

export type UpdateProfileRequest = {
  fullName: string
  phone: string | null
}

export type ChangePasswordRequest = {
  currentPassword: string
  newPassword: string
}

export type UpdateBusinessRequest = {
  businessName: string
  identificationType: string
  identificationNumber: string
  phones: BusinessPhone[]
}

export type UpdateBranchRequest = {
  name: string
  address: string | null
  phone: string | null
}
