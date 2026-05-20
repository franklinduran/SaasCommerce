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

// ── Operational / SaaS settings ─────────────────────────────────────────────

export type BusinessSettingsData = {
  businessId: string
  commercialName: string | null
  legalName: string | null
  rnc: string | null
  phone: string | null
  email: string | null
  address: string | null
  currency: string
  timezone: string
  logoUrl: string | null
  receiptFooterText: string | null
  updatedBy: string
  updatedAt: string
}

export type SalesSettingsData = {
  businessId: string
  allowNegativeStock: boolean
  allowDiscounts: boolean
  requireCustomerForCreditSale: boolean
  defaultPaymentMethod: string | null
  enableReceiptPrintAfterSale: boolean
  enableInvoiceAutoGeneration: boolean
  updatedBy: string
  updatedAt: string
}

export type InventorySettingsData = {
  businessId: string
  enableLowStockAlerts: boolean
  defaultLowStockThreshold: number
  requireReasonForInventoryAdjustment: boolean
  allowInventoryTransferBetweenBranches: boolean
  updatedBy: string
  updatedAt: string
}

export type BillingSettingsData = {
  businessId: string
  receiptHeaderText: string | null
  receiptFooterText: string | null
  showLogoOnReceipt: boolean
  showRncOnReceipt: boolean
  enableInvoiceAutoGeneration: boolean
  invoicePrefix: string
  invoiceSequenceStart: number
  updatedBy: string
  updatedAt: string
}

export type UpdateBusinessSettingsRequest = {
  commercialName: string | null
  legalName: string | null
  rnc: string | null
  phone: string | null
  email: string | null
  address: string | null
  currency: string
  timezone: string
  logoUrl: string | null
  receiptFooterText: string | null
}

export type UpdateSalesSettingsRequest = {
  allowNegativeStock: boolean
  allowDiscounts: boolean
  requireCustomerForCreditSale: boolean
  defaultPaymentMethod: string | null
  enableReceiptPrintAfterSale: boolean
  enableInvoiceAutoGeneration: boolean
}

export type UpdateInventorySettingsRequest = {
  enableLowStockAlerts: boolean
  defaultLowStockThreshold: number
  requireReasonForInventoryAdjustment: boolean
  allowInventoryTransferBetweenBranches: boolean
}

export type UpdateBillingSettingsRequest = {
  receiptHeaderText: string | null
  receiptFooterText: string | null
  showLogoOnReceipt: boolean
  showRncOnReceipt: boolean
  enableInvoiceAutoGeneration: boolean
  invoicePrefix: string
  invoiceSequenceStart: number
}
