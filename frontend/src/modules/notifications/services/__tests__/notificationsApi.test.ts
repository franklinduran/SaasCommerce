import { beforeEach, describe, expect, it, vi } from 'vitest'
import { notificationsApi } from '@/modules/notifications/services/notificationsApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'notification-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('notificationsApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { items: [], unreadCount: 3 } })
  })

  it('loads notifications with filters and default fallback values', async () => {
    await notificationsApi.getNotifications({ branchId: 'branch-1', page: 2, pageSize: 5, status: 'Unread', type: 'LowStock' })
    vi.mocked(httpClient).mockResolvedValueOnce({ data: undefined })
    const fallback = await notificationsApi.getNotifications()
    vi.mocked(httpClient).mockResolvedValueOnce({ data: undefined })
    const count = await notificationsApi.getUnreadCount()

    expect(httpClient).toHaveBeenNthCalledWith(
      1,
      '/api/notifications?branchId=branch-1&status=Unread&type=LowStock&page=2&pageSize=5',
      { accessToken: 'notification-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/notifications?page=1&pageSize=20', { accessToken: 'notification-token' })
    expect(fallback).toEqual({ items: [], page: 1, pageSize: 20, totalCount: 0, unreadCount: 0 })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/notifications/unread-count', { accessToken: 'notification-token' })
    expect(count).toEqual({ unreadCount: 0 })
  })

  it('marks notifications as read', async () => {
    await notificationsApi.markAsRead('notification-1')
    await notificationsApi.markAllAsRead()

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/notifications/notification-1/read', expect.objectContaining({ method: 'PUT' }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/notifications/read-all', expect.objectContaining({ method: 'PUT' }))
  })
})
