import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { cashRegisterApi } from '@/modules/cash-register/services/cashRegisterApi'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'
import type {
  CloseCashRegisterRequest,
  OpenCashRegisterRequest,
  RegisterMovementRequest,
} from '@/modules/cash-register/types'

export const cashRegisterQueryKeys = {
  active: ['cash-register', 'active'] as const,
  dailySummary: (params?: object) => ['cash-register', 'daily-summary', params] as const,
  history: (params?: object) => ['cash-register', 'history', params] as const,
}

export function useActiveCashRegister() {
  return useQuery({
    queryKey: cashRegisterQueryKeys.active,
    queryFn: cashRegisterApi.getActive,
    staleTime: 1000 * 30,
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

export function useCashRegisterHistory(params: {
  dateFrom?: string
  dateTo?: string
  status?: string
  page?: number
  pageSize?: number
}) {
  return useQuery({
    queryKey: cashRegisterQueryKeys.history(params),
    queryFn: () => cashRegisterApi.getHistory(params),
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
      void queryClient.invalidateQueries({ queryKey: ['cash-register', 'history'] })
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
      void queryClient.invalidateQueries({ queryKey: ['cash-register', 'history'] })
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
      void queryClient.invalidateQueries({ queryKey: ['cash-register', 'history'] })
    },
  })
}

export function useCashRegisterRealtimeInvalidation() {
  const queryClient = useQueryClient()

  useEffect(() => {
    const invalidate = () => {
      void queryClient.invalidateQueries({ queryKey: cashRegisterQueryKeys.active })
      void queryClient.invalidateQueries({ queryKey: ['cash-register', 'history'] })
      void queryClient.invalidateQueries({ queryKey: ['cash-register', 'daily-summary'] })
    }

    const events = [
      'cashRegister.opened',
      'cashRegister.movementRegistered',
      'cashRegister.closed',
      'cashRegister.differenceDetected',
    ]

    events.forEach((eventName) => onRealtimeEvent(eventName, invalidate))

    return () => {
      events.forEach((eventName) => offRealtimeEvent(eventName, invalidate))
    }
  }, [queryClient])
}
