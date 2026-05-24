import { useAuthStore } from '@/modules/auth/authStore'
import type {
  NotificationListResponse,
  NotificationStatus,
  NotificationType,
  UnreadCountResponse,
} from '@/modules/notifications/types'
import { httpClient } from '@/shared/services/httpClient'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export interface GetNotificationsParams {
  branchId?: string
  status?: NotificationStatus
  type?: NotificationType
  page?: number
  pageSize?: number
}

export const notificationsApi = {
  async getNotifications(params: GetNotificationsParams = {}): Promise<NotificationListResponse> {
    const query = new URLSearchParams()
    if (params.branchId) query.set('branchId', params.branchId)
    if (params.status) query.set('status', params.status)
    if (params.type) query.set('type', params.type)
    query.set('page', String(params.page ?? 1))
    query.set('pageSize', String(params.pageSize ?? 20))

    const response = await httpClient<NotificationListResponse>(
      `/api/notifications?${query.toString()}`,
      { accessToken: getAccessToken() },
    )

    return (
      response.data ?? {
        items: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
        unreadCount: 0,
      }
    )
  },

  async getUnreadCount(): Promise<UnreadCountResponse> {
    const response = await httpClient<UnreadCountResponse>('/api/notifications/unread-count', {
      accessToken: getAccessToken(),
    })

    return response.data ?? { unreadCount: 0 }
  },

  async markAsRead(notificationId: string): Promise<void> {
    await httpClient<void>(`/api/notifications/${notificationId}/read`, {
      accessToken: getAccessToken(),
      method: 'PUT',
    })
  },

  async markAllAsRead(): Promise<void> {
    await httpClient<void>('/api/notifications/read-all', {
      accessToken: getAccessToken(),
      method: 'PUT',
    })
  },
}
