import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { GetNotificationsParams } from '@/modules/notifications/services/notificationsApi'
import { notificationsApi } from '@/modules/notifications/services/notificationsApi'

export const notificationQueryKeys = {
  all: ['notifications'] as const,
  list: (params: GetNotificationsParams) => ['notifications', 'list', params] as const,
  unreadCount: ['notifications', 'unread-count'] as const,
}

export function useNotifications(params: GetNotificationsParams = {}) {
  return useQuery({
    queryKey: notificationQueryKeys.list(params),
    queryFn: () => notificationsApi.getNotifications(params),
    staleTime: 1000 * 30, // 30s
  })
}

export function useUnreadNotificationCount() {
  return useQuery({
    queryKey: notificationQueryKeys.unreadCount,
    queryFn: notificationsApi.getUnreadCount,
    staleTime: 1000 * 60, // 1 min
    refetchInterval: 1000 * 60, // poll every minute
  })
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
