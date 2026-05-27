import { useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  createBetaFeedback,
  getBetaFeedback,
  updateBetaFeedbackStatus,
} from '@/modules/beta-feedback/services/betaFeedbackService'
import type {
  BetaFeedbackFilters,
  BetaFeedbackRealtimeNotification,
  UpdateBetaFeedbackStatusRequest,
} from '@/modules/beta-feedback/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const betaFeedbackKeys = {
  all: ['beta-feedback'] as const,
  list: (filters: BetaFeedbackFilters) => ['beta-feedback', 'list', filters] as const,
}

export function useBetaFeedback(filters: BetaFeedbackFilters) {
  return useQuery({
    queryKey: betaFeedbackKeys.list(filters),
    queryFn: () => getBetaFeedback(filters),
  })
}

export function useCreateBetaFeedback() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createBetaFeedback,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: betaFeedbackKeys.all })
    },
  })
}

export function useUpdateBetaFeedbackStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      feedbackId,
      request,
    }: {
      feedbackId: string
      request: UpdateBetaFeedbackStatusRequest
    }) => updateBetaFeedbackStatus(feedbackId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: betaFeedbackKeys.all })
    },
  })
}

export function useBetaFeedbackRealtimeInvalidation() {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: BetaFeedbackRealtimeNotification) => {
      if (payload.businessId === businessId) {
        void queryClient.invalidateQueries({ queryKey: betaFeedbackKeys.all })
      }
    }

    onRealtimeEvent('betaFeedback.created', handler)
    onRealtimeEvent('betaFeedback.statusChanged', handler)

    return () => {
      offRealtimeEvent('betaFeedback.created', handler)
      offRealtimeEvent('betaFeedback.statusChanged', handler)
    }
  }, [businessId, queryClient])
}
