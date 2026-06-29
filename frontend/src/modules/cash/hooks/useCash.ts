import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { cashApi } from '@/modules/cash/services/cashApi'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'
import type {
  CloseCashSessionRequest,
  OpenCashSessionRequest,
  RegisterCashMovementRequest,
} from '@/modules/cash/types'

export const cashQueryKeys = {
  currentSession: ['cash', 'current'] as const,
  session: (id: string) => ['cash', 'sessions', id] as const,
  sessions: (params?: object) => ['cash', 'sessions', params] as const,
}

export function useCurrentCashSession() {
  return useQuery({
    queryFn: cashApi.getCurrentSession,
    queryKey: cashQueryKeys.currentSession,
    staleTime: 1000 * 30,
  })
}

export function useCashSessionRealtimeInvalidation() {
  const queryClient = useQueryClient()

  useEffect(() => {
    const invalidate = () => {
      void queryClient.invalidateQueries({ queryKey: cashQueryKeys.currentSession })
    }

    const events = ['cash.session.opened', 'cash.session.closed', 'cash.movement.registered']
    events.forEach((e) => onRealtimeEvent(e, invalidate))

    return () => {
      events.forEach((e) => offRealtimeEvent(e, invalidate))
    }
  }, [queryClient])
}

export function useCashSession(id: string) {
  return useQuery({
    queryFn: () => cashApi.getSessionById(id),
    queryKey: cashQueryKeys.session(id),
    enabled: Boolean(id),
    staleTime: 1000 * 60,
  })
}

export function useCashSessions(params?: {
  branchId?: string
  status?: string
  dateFrom?: string
  dateTo?: string
  page?: number
  pageSize?: number
}) {
  return useQuery({
    queryFn: () => cashApi.listSessions(params),
    queryKey: cashQueryKeys.sessions(params),
    staleTime: 1000 * 60,
  })
}

export function useOpenCashSession() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: OpenCashSessionRequest) => cashApi.openSession(request),
    onSuccess: (session) => {
      queryClient.setQueryData(cashQueryKeys.currentSession, session)
      void queryClient.invalidateQueries({ queryKey: cashQueryKeys.sessions() })
    },
  })
}

export function useCloseCashSession() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: CloseCashSessionRequest }) =>
      cashApi.closeSession(id, request),
    onSuccess: (_, variables) => {
      queryClient.setQueryData(cashQueryKeys.currentSession, null)
      void queryClient.invalidateQueries({ queryKey: cashQueryKeys.session(variables.id) })
      void queryClient.invalidateQueries({ queryKey: cashQueryKeys.sessions() })
    },
  })
}

export function useRegisterCashMovement() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: RegisterCashMovementRequest }) =>
      cashApi.registerMovement(id, request),
    onSuccess: (_, variables) => {
      queryClient.setQueryData(cashQueryKeys.currentSession, null) // force re-fetch
      void queryClient.invalidateQueries({ queryKey: cashQueryKeys.session(variables.id) })
      void queryClient.invalidateQueries({ queryKey: cashQueryKeys.currentSession })
    },
  })
}
