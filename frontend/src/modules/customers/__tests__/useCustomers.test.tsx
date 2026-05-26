import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook } from '@testing-library/react'
import { createElement } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { customerKeys, useCustomerCreditInvalidation } from '@/modules/customers/hooks/useCustomers'
import type { CustomerCreditRealtimeNotification } from '@/modules/customers/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

const realtimeHandlers = vi.hoisted(() => new Map<string, (payload: CustomerCreditRealtimeNotification) => void>())

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: (selector: (state: { session: { user: { businessId: string } } | null }) => unknown) =>
    selector({ session: { user: { businessId: 'business-1' } } }),
}))

vi.mock('@/shared/services/signalrClient', () => ({
  offRealtimeEvent: vi.fn((event: string) => realtimeHandlers.delete(event)),
  onRealtimeEvent: vi.fn((event: string, handler: (payload: CustomerCreditRealtimeNotification) => void) => {
    realtimeHandlers.set(event, handler)
  }),
}))

describe('useCustomerCreditInvalidation', () => {
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

  it('subscribes to customer credit events and invalidates matching customer caches', () => {
    const { unmount } = renderHook(() => useCustomerCreditInvalidation('customer-1'), { wrapper })

    realtimeHandlers.get('customer.paymentRegistered')?.({
      businessId: 'business-1',
      customerId: 'customer-1',
    } as CustomerCreditRealtimeNotification)
    realtimeHandlers.get('customer.creditDebited')?.({
      businessId: 'other-business',
      customerId: 'customer-1',
    } as CustomerCreditRealtimeNotification)

    expect(onRealtimeEvent).toHaveBeenCalledTimes(4)
    expect(queryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: customerKeys.all })
    expect(queryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: customerKeys.credit('customer-1') })
    expect(queryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: customerKeys.movements('customer-1') })
    expect(queryClient.invalidateQueries).toHaveBeenCalledTimes(3)

    unmount()

    expect(offRealtimeEvent).toHaveBeenCalledTimes(4)
  })

  it('only invalidates the list when the payload belongs to another selected customer', () => {
    renderHook(() => useCustomerCreditInvalidation('customer-1'), { wrapper })

    realtimeHandlers.get('customer.creditBlocked')?.({
      businessId: 'business-1',
      customerId: 'customer-2',
    } as CustomerCreditRealtimeNotification)

    expect(queryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: customerKeys.all })
    expect(queryClient.invalidateQueries).toHaveBeenCalledTimes(1)
  })
})
