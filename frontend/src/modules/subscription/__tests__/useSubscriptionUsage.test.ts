import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook } from '@testing-library/react'
import { createElement } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { subscriptionQueryKeys } from '@/modules/subscription/hooks/useSubscription'
import { useSubscriptionUsage } from '@/modules/subscription/hooks/useSubscriptionUsage'
import type { SubscriptionUsageResponse } from '@/modules/subscription/types'

const createUsageSeed = (overrides: Partial<SubscriptionUsageResponse> = {}): SubscriptionUsageResponse => ({
  branches: { current: 1, isAtLimit: false, maximum: 3 },
  features: [
    { isEnabled: true, name: 'Sales' },
    { isEnabled: false, name: 'AuditLogs' },
  ],
  periodEndsAt: '2026-06-01T12:00:00Z',
  planName: 'Pro',
  products: { current: 1700, isAtLimit: false, maximum: 2000 },
  sales: { current: 450, isAtLimit: false, maximum: 10000 },
  status: 'Active',
  subscriptionId: 'sub-id',
  trialEndsAt: null,
  users: { current: 8, isAtLimit: true, maximum: 10 },
  ...overrides,
})

describe('useSubscriptionUsage helpers', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false, staleTime: Infinity } },
    })
    // Pre-load usage data so the query doesn't fire
    queryClient.setQueryData(subscriptionQueryKeys.usage, createUsageSeed())
  })

  function wrapper({ children }: { children: React.ReactNode }) {
    return createElement(QueryClientProvider, { client: queryClient }, children)
  }

  it('getUsagePercentage returns 0 when no usage data', () => {
    // fresh client with no cached data + disabled query → data is undefined
    const emptyClient = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: Infinity } } })
    function emptyWrapper({ children }: { children: React.ReactNode }) {
      return createElement(QueryClientProvider, { client: emptyClient }, children)
    }
    const { result } = renderHook(() => useSubscriptionUsage({ enabled: false }), { wrapper: emptyWrapper })
    expect(result.current.getUsagePercentage('products')).toBe(0)
  })

  it('getUsagePercentage returns 0 when maximum is 0', () => {
    queryClient.setQueryData(subscriptionQueryKeys.usage, createUsageSeed({
      branches: { current: 0, isAtLimit: false, maximum: 0 },
    }))
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.getUsagePercentage('branches')).toBe(0)
  })

  it('getUsagePercentage calculates percentage correctly', () => {
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    // products: 1700/2000 = 85%
    expect(result.current.getUsagePercentage('products')).toBe(85)
  })

  it('getUsagePercentage caps at 100 when over limit', () => {
    queryClient.setQueryData(subscriptionQueryKeys.usage, createUsageSeed({
      branches: { current: 5, isAtLimit: false, maximum: 3 },
    }))
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.getUsagePercentage('branches')).toBe(100)
  })

  it('isNearLimit returns true when usage >= 80%', () => {
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.isNearLimit('products')).toBe(true) // 85%
  })

  it('isNearLimit returns false when usage < 80%', () => {
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.isNearLimit('sales')).toBe(false) // 4.5%
  })

  it('isAtLimit returns true when resource isAtLimit', () => {
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.isAtLimit('users')).toBe(true)
  })

  it('isAtLimit returns false when not at limit', () => {
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.isAtLimit('products')).toBe(false)
  })

  it('isAtLimit returns false when no usage data', () => {
    // fresh client with no cached data + disabled query → data is undefined
    const emptyClient = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: Infinity } } })
    function emptyWrapper({ children }: { children: React.ReactNode }) {
      return createElement(QueryClientProvider, { client: emptyClient }, children)
    }
    const { result } = renderHook(() => useSubscriptionUsage({ enabled: false }), { wrapper: emptyWrapper })
    expect(result.current.isAtLimit('products')).toBe(false)
  })

  it('isFeatureEnabled returns true for enabled feature', () => {
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.isFeatureEnabled('Sales')).toBe(true)
  })

  it('isFeatureEnabled returns false for disabled feature', () => {
    const { result } = renderHook(() => useSubscriptionUsage(), { wrapper })
    expect(result.current.isFeatureEnabled('AuditLogs')).toBe(false)
  })

  it('isFeatureEnabled returns false when no usage data', () => {
    // fresh client with no cached data + disabled query → data is undefined
    const emptyClient = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: Infinity } } })
    function emptyWrapper({ children }: { children: React.ReactNode }) {
      return createElement(QueryClientProvider, { client: emptyClient }, children)
    }
    const { result } = renderHook(() => useSubscriptionUsage({ enabled: false }), { wrapper: emptyWrapper })
    expect(result.current.isFeatureEnabled('Sales')).toBe(false)
  })
})
