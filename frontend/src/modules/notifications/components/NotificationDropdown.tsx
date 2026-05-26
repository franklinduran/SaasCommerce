import { Loader2 } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
} from '@/modules/notifications/hooks/useNotifications'
import { NotificationEmptyState, NotificationItem } from './NotificationItem'

interface NotificationDropdownProps {
  onClose?: () => void
}

export function NotificationDropdown({ onClose }: Readonly<NotificationDropdownProps>) {
  const navigate = useNavigate()
  const { data, isLoading } = useNotifications({ page: 1, pageSize: 10 })
  const markRead = useMarkNotificationRead()
  const markAll = useMarkAllNotificationsRead()

  const items = data?.items ?? []
  const unreadCount = data?.unreadCount ?? 0

  function handleMarkRead(id: string) {
    markRead.mutate(id)
  }

  function handleMarkAll() {
    markAll.mutate()
  }

  function handleViewAll() {
    onClose?.()
    navigate('/notifications')
  }

  return (
    <div className="flex w-80 flex-col rounded-xl border border-stone-200 bg-white shadow-lg">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-stone-100 px-4 py-3">
        <div>
          <p className="text-sm font-semibold text-stone-900">Notificaciones</p>
          {unreadCount > 0 && (
            <p className="text-xs text-stone-500">{unreadCount} sin leer</p>
          )}
        </div>
        {unreadCount > 0 && (
          <button
            className="text-xs font-medium text-blue-600 hover:text-blue-700"
            disabled={markAll.isPending}
            onClick={handleMarkAll}
          >
            Marcar todas
          </button>
        )}
      </div>

      {/* List */}
      <div className="max-h-80 overflow-y-auto divide-y divide-stone-100">
        {isLoading && (
          <div className="flex h-24 items-center justify-center">
            <Loader2 className="animate-spin text-stone-400" size={18} />
          </div>
        )}

        {!isLoading && items.length === 0 && <NotificationEmptyState />}

        {!isLoading &&
          items.map((n) => (
            <NotificationItem key={n.id} notification={n} onRead={handleMarkRead} />
          ))}
      </div>

      {/* Footer */}
      <div className="border-t border-stone-100 px-4 py-2.5">
        <button
          className="w-full text-center text-xs font-medium text-stone-600 hover:text-stone-900"
          onClick={handleViewAll}
        >
          Ver todas las notificaciones
        </button>
      </div>
    </div>
  )
}
