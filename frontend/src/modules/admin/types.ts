// ── Pilot Metrics ─────────────────────────────────────────────────────────────

export type RecentPilotBusiness = {
  businessName: string
  subscriptionStatus: string
  planName: string | null
  createdAt: string
  trialEndsAt: string | null
}

export type PilotMetricsSummary = {
  totalBusinesses: number
  activeTrials: number
  activeSubscriptions: number
  suspendedSubscriptions: number
  cancelledSubscriptions: number
  newBusinessesLast30Days: number
  trialsExpiringIn7Days: number
  recentBusinesses: RecentPilotBusiness[]
}

// ── Pilot Business ─────────────────────────────────────────────────────────────

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
