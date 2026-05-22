/**
 * Subscription Module Types
 * Defines all TypeScript interfaces for the subscription management system
 */

export enum SubscriptionStatus {
  Trial = 'Trial',
  Active = 'Active',
  PastDue = 'PastDue',
  Suspended = 'Suspended',
  Expired = 'Expired',
  Cancelled = 'Cancelled'
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  monthlyPrice: number;
  maxBranches: number;
  maxUsers: number;
  maxProducts: number;
  maxSalesPerMonth: number;
  features: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface BusinessSubscription {
  id: string;
  businessId: string;
  plan: SubscriptionPlan;
  status: SubscriptionStatus;
  startedAt: string;
  trialEndsAt?: string;
  currentPeriodEnd?: string;
  cancelledAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface SubscriptionUsage {
  subscriptionId: string;
  planName: string;
  status: SubscriptionStatus;
  branches: ResourceUsage;
  users: ResourceUsage;
  products: ResourceUsage;
  sales: ResourceUsage;
  features: FeatureStatus[];
  trialEndsAt?: string;
  periodEndsAt?: string;
}

export interface ResourceUsage {
  current: number;
  maximum: number;
  isAtLimit: boolean;
}

export interface FeatureStatus {
  name: string;
  isEnabled: boolean;
}

export interface SubscriptionLimitCheckResult {
  isAllowed: boolean;
  code: string;
  message: string;
  currentUsage: number;
  maxAllowed: number;
}
