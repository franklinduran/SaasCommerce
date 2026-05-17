import { useMutation } from '@tanstack/react-query'
import { createSale } from '@/modules/pos/services/salesApi'

export function useCreateSaleMutation() {
  return useMutation({
    mutationFn: createSale,
  })
}
