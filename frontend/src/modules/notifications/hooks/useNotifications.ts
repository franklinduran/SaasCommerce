import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import type { GetNotificationsParams } from '@/modules/notifications/services/notificationsApi'
import { notificationsApi } from '@/modules/notifications/services/notificationsApi'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const notificationQueryKeys = {
  all: ['notifications'] as const,
  list: (params: GetNotificationsParams) => ['notifications', 'list', params] as const,
  unreadCount: ['notifications', 'unread-count'] as const,
}

export function useNotifications(params: GetNotificationsParams = {}) {
  return useQuery({
    queryKey: notificationQueryKeys.list(params),
    queryFn: () => notificationsApi.getNotifications(params),
    staleTime: 1000 * 30,
  })
}

export function useUnreadNotificationCount() {
  return useQuery({
    queryKey: notificationQueryKeys.unreadCount,
    queryFn: notificationsApi.getUnreadCount,
    staleTime: 1000 * 60,
  })
}

// Invalidates the unread count when any backend event that creates a notification fires.
export function useNotificationsRealtimeInvalidation() {
  const queryClient = useQueryClient()

  useEffect(() => {
    const invalidate = () => {
      void queryClient.invalidateQueries({ queryKey: notificationQueryKeys.unreadCount })
    }

    const events = [
      'inventory.lowStockDetected',
      'sale.failed',
      'cashRegister.differenceDetected',
      'betaFeedback.created',
      'betaFeedback.statusChanged',
    ]

    events.forEach((e) => onRealtimeEvent(e, invalidate))

    return () => {
      events.forEach((e) => offRealtimeEvent(e, invalidate))
    }
  }, [queryClient])
}

export function useMarkNotificationRead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationQueryKeys.all })
    },
  })
}

export function useMarkAllNotificationsRead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: notificationsApi.markAllAsRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationQueryKeys.all })
    },
  })
}
