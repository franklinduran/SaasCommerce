import { useMutation } from '@tanstack/react-query'
import { adminApi } from '@/modules/admin/services/adminApi'
import type { CreatePilotBusinessRequest } from '@/modules/admin/types'

export function useCreatePilotBusiness() {
  return useMutation({
    mutationFn: (request: CreatePilotBusinessRequest) =>
      adminApi.createPilotBusiness(request),
  })
}
