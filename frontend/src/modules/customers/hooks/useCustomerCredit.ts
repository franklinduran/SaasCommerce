import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { customerKeys } from '@/modules/customers/hooks/useCustomers'
import {
  blockCustomerCredit,
  getCustomerCredit,
  getCustomerCreditMovements,
  unblockCustomerCredit,
} from '@/modules/customers/services/customersApi'

export function useCustomerCredit(customerId?: string) {
  return useQuery({
    enabled: Boolean(customerId),
    queryKey: customerId ? customerKeys.credit(customerId) : ['customers', 'credit'],
    queryFn: () => getCustomerCredit(customerId!),
  })
}

export function useCustomerCreditMovements(customerId?: string) {
  return useQuery({
    enabled: Boolean(customerId),
    queryKey: customerId ? customerKeys.movements(customerId) : ['customers', 'movements'],
    queryFn: () => getCustomerCreditMovements(customerId!),
  })
}

export function useBlockCustomerCredit(customerId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => blockCustomerCredit(customerId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
      await queryClient.invalidateQueries({ queryKey: customerKeys.credit(customerId) })
      await queryClient.invalidateQueries({ queryKey: customerKeys.detail(customerId) })
    },
  })
}

export function useUnblockCustomerCredit(customerId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => unblockCustomerCredit(customerId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
      await queryClient.invalidateQueries({ queryKey: customerKeys.credit(customerId) })
      await queryClient.invalidateQueries({ queryKey: customerKeys.detail(customerId) })
    },
  })
}
