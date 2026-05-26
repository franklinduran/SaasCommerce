import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { cashRegisterApi } from '@/modules/cash-register/services/cashRegisterApi'
import type {
  CloseCashRegisterRequest,
  OpenCashRegisterRequest,
  RegisterMovementRequest,
} from '@/modules/cash-register/types'

export const cashRegisterQueryKeys = {
  active: ['cash-register', 'active'] as const,
  dailySummary: (params?: object) => ['cash-register', 'daily-summary', params] as const,
}

export function useActiveCashRegister() {
  return useQuery({
    queryKey: cashRegisterQueryKeys.active,
    queryFn: cashRegisterApi.getActive,
    staleTime: 1000 * 30,
    refetchOnWindowFocus: true,
  })
}

export function useDailyCashRegisterSummary(params: { date?: string; branchId?: string }) {
  return useQuery({
    queryKey: cashRegisterQueryKeys.dailySummary(params),
    queryFn: () => cashRegisterApi.getDailySummary(params),
    staleTime: 1000 * 60,
    retry: false,
  })
}

export function useOpenCashRegister() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: OpenCashRegisterRequest) => cashRegisterApi.open(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: cashRegisterQueryKeys.active })
    },
  })
}

export function useRegisterCashMovement() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: RegisterMovementRequest }) =>
      cashRegisterApi.registerMovement(id, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: cashRegisterQueryKeys.active })
    },
  })
}

export function useCloseCashRegister() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: CloseCashRegisterRequest }) =>
      cashRegisterApi.close(id, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: cashRegisterQueryKeys.active })
      void queryClient.invalidateQueries({ queryKey: ['cash-register', 'daily-summary'] })
    },
  })
}
