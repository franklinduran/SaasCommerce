import { useQuery } from '@tanstack/react-query'
import { customerKeys } from '@/modules/customers/hooks/useCustomers'
import { getCustomer } from '@/modules/customers/services/customersApi'

export function useCustomerDetail(customerId?: string) {
  return useQuery({
    enabled: Boolean(customerId),
    queryKey: customerId ? customerKeys.detail(customerId) : ['customers', 'detail'],
    queryFn: () => getCustomer(customerId!),
  })
}
