import { useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  createCustomer,
  deactivateCustomer,
  getCustomers,
  updateCustomer,
} from '@/modules/customers/services/customersApi'
import type {
  CustomerCreditRealtimeNotification,
  CustomerFilters,
  CustomerUpsertRequest,
} from '@/modules/customers/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const customerKeys = {
  all: ['customers'] as const,
  credit: (customerId: string) => ['customers', 'credit', customerId] as const,
  detail: (customerId: string) => ['customers', 'detail', customerId] as const,
  list: (filters: CustomerFilters) => ['customers', 'list', filters] as const,
  movements: (customerId: string) => ['customers', 'movements', customerId] as const,
}

export function useCustomers(filters: CustomerFilters) {
  return useQuery({
    queryKey: customerKeys.list(filters),
    queryFn: () => getCustomers(filters),
  })
}

export function useCreateCustomer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createCustomer,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
    },
  })
}

export function useUpdateCustomer(customerId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CustomerUpsertRequest) => updateCustomer(customerId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
      await queryClient.invalidateQueries({ queryKey: customerKeys.detail(customerId) })
    },
  })
}

export function useDeactivateCustomer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deactivateCustomer,
    onSuccess: async (customer) => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
      await queryClient.invalidateQueries({ queryKey: customerKeys.detail(customer.id) })
    },
  })
}

export function useCustomerCreditInvalidation(customerId?: string) {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: CustomerCreditRealtimeNotification) => {
      if (payload.businessId !== businessId) {
        return
      }

      void queryClient.invalidateQueries({ queryKey: customerKeys.all })

      if (customerId && payload.customerId === customerId) {
        void queryClient.invalidateQueries({ queryKey: customerKeys.credit(customerId) })
        void queryClient.invalidateQueries({ queryKey: customerKeys.movements(customerId) })
      }
    }

    onRealtimeEvent('customer.creditDebited', handler)
    onRealtimeEvent('customer.paymentRegistered', handler)
    onRealtimeEvent('customer.creditBlocked', handler)
    onRealtimeEvent('customer.creditUnblocked', handler)

    return () => {
      offRealtimeEvent('customer.creditDebited', handler)
      offRealtimeEvent('customer.paymentRegistered', handler)
      offRealtimeEvent('customer.creditBlocked', handler)
      offRealtimeEvent('customer.creditUnblocked', handler)
    }
  }, [businessId, customerId, queryClient])
}
