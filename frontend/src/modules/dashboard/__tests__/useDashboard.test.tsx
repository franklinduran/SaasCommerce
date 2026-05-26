import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook } from '@testing-library/react'
import { createElement } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  dashboardKeys,
  useDashboardRealtimeInvalidation,
  useDashboardSummary,
} from '@/modules/dashboard/hooks/useDashboard'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

const realtimeHandlers = vi.hoisted(() => new Map<string, (payload: { businessId?: string }) => void>())

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: (selector: (state: { session: { user: { businessId: string } } | null }) => unknown) =>
    selector({ session: { user: { businessId: 'business-1' } } }),
}))

vi.mock('@/modules/dashboard/services/dashboardApi', () => ({
  getDashboardSummary: vi.fn().mockResolvedValue({ salesToday: 10 }),
}))

vi.mock('@/shared/services/signalrClient', () => ({
  offRealtimeEvent: vi.fn((event: string) => realtimeHandlers.delete(event)),
  onRealtimeEvent: vi.fn((event: string, handler: (payload: { businessId?: string }) => void) => {
    realtimeHandlers.set(event, handler)
  }),
}))

describe('useDashboard hooks', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    vi.clearAllMocks()
    realtimeHandlers.clear()
    queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    vi.spyOn(queryClient, 'invalidateQueries')
  })

  function wrapper({ children }: { children: React.ReactNode }) {
    return createElement(QueryClientProvider, { client: queryClient }, children)
  }

  it('uses the dashboard summary query options', () => {
    const { result } = renderHook(() => useDashboardSummary(), { wrapper })

    expect(result.current.refetch).toEqual(expect.any(Function))
    expect(queryClient.getQueryCache().find({ queryKey: dashboardKeys.summary() })?.options.staleTime).toBe(30_000)
    expect(queryClient.getQueryCache().find({ queryKey: dashboardKeys.summary() })?.options.refetchInterval).toBe(300_000)
  })

  it('subscribes to realtime dashboard events and invalidates matching payloads', () => {
    const { unmount } = renderHook(() => useDashboardRealtimeInvalidation(), { wrapper })

    realtimeHandlers.get('sale.statusChanged')?.({ businessId: 'business-1' })
    realtimeHandlers.get('invoice.generated')?.({})
    realtimeHandlers.get('payment.registered')?.({ businessId: 'other-business' })

    expect(onRealtimeEvent).toHaveBeenCalledTimes(6)
    expect(queryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: dashboardKeys.summary() })
    expect(queryClient.invalidateQueries).toHaveBeenCalledTimes(2)

    unmount()

    expect(offRealtimeEvent).toHaveBeenCalledTimes(6)
  })
})
