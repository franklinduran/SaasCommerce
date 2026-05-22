export const SubscriptionStatus = {
  Active: 'Active',
  Cancelled: 'Cancelled',
  Expired: 'Expired',
  PastDue: 'PastDue',
  Suspended: 'Suspended',
  Trial: 'Trial',
} as const

export type SubscriptionStatus = typeof SubscriptionStatus[keyof typeof SubscriptionStatus]

export type SubscriptionResourceKey = 'branches' | 'users' | 'products' | 'sales'

export interface SubscriptionPlan {
  id: string
  name: string
  code: string
  description: string
  monthlyPrice: number
  maxBranches: number
  maxUsers: number
  maxProducts: number
  maxSalesPerMonth: number
  features: string[]
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface BusinessSubscription {
  id: string
  businessId: string
  plan: SubscriptionPlan
  status: SubscriptionStatus
  startedAt: string
  trialEndsAt?: string | null
  currentPeriodStart?: string | null
  currentPeriodEnd?: string | null
  cancelledAt?: string | null
  suspendedAt?: string | null
  cancellationReason?: string | null
  createdAt: string
  updatedAt: string
}

export interface ResourceUsage {
  current: number
  maximum: number
  isAtLimit: boolean
}

export interface FeatureStatus {
  name: string
  isEnabled: boolean
}

export interface SubscriptionUsage {
  subscriptionId: string
  planName: string
  status: SubscriptionStatus
  branches: ResourceUsage
  users: ResourceUsage
  products: ResourceUsage
  sales: ResourceUsage
  features: FeatureStatus[]
  trialEndsAt?: string | null
  periodEndsAt?: string | null
}

export interface SubscriptionLimitCheckResult {
  isAllowed: boolean
  code: string
  message: string
  currentUsage: number
  maxAllowed: number
}

export type SubscriptionPlanResponse = SubscriptionPlan
export type BusinessSubscriptionResponse = BusinessSubscription
export type SubscriptionUsageResponse = SubscriptionUsage
