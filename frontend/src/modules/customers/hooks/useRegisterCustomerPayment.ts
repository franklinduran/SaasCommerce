import { useMutation, useQueryClient } from '@tanstack/react-query'
import { customerKeys } from '@/modules/customers/hooks/useCustomers'
import { registerCustomerPayment } from '@/modules/customers/services/customersApi'

export function useRegisterCustomerPayment(customerId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: registerCustomerPayment,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: customerKeys.all })
      await queryClient.invalidateQueries({ queryKey: customerKeys.credit(customerId) })
      await queryClient.invalidateQueries({ queryKey: customerKeys.movements(customerId) })
      await queryClient.invalidateQueries({ queryKey: customerKeys.detail(customerId) })
    },
  })
}
