export type CreatePilotBusinessRequest = {
  businessName: string
  identificationType: string
  identificationNumber: string
  phone: string
  branchName: string
  adminFullName: string
  adminEmail: string
  adminPassword: string
}

export type CreatePilotBusinessResponse = {
  businessId: string
  branchId: string
  adminUserId: string
  businessName: string
  branchName: string
  adminEmail: string
  trialEndsAt: string | null
}
