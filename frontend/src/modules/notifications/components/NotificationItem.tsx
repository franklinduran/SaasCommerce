import { AlertTriangle, CheckCircle, Info } from 'lucide-react'
import type { NotificationItem as NotificationItemType } from '@/modules/notifications/types'
import { cn } from '@/shared/utils/cn'

interface NotificationItemProps {
  notification: NotificationItemType
  onRead?: (id: string) => void
}

function formatRelativeTime(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMin = Math.floor(diffMs / 60_000)
  const diffHr = Math.floor(diffMin / 60)
  const diffDay = Math.floor(diffHr / 24)

  if (diffMin < 1) return 'Ahora mismo'
  if (diffMin < 60) return `Hace ${diffMin} min`
  if (diffHr < 24) return `Hace ${diffHr}h`
  if (diffDay < 7) return `Hace ${diffDay}d`
  return new Intl.DateTimeFormat('es-DO', { day: '2-digit', month: 'short' }).format(date)
}

function SeverityIcon({ severity }: { severity: NotificationItemType['severity'] }) {
  switch (severity) {
    case 'Critical':
      return <AlertTriangle className="shrink-0 text-red-500" size={15} />
    case 'Warning':
      return <AlertTriangle className="shrink-0 text-amber-500" size={15} />
    default:
      return <Info className="shrink-0 text-blue-500" size={15} />
  }
}

export function NotificationItem({ notification, onRead }: NotificationItemProps) {
  const isUnread = notification.status === 'Unread'

  return (
    <div
      className={cn(
        'flex items-start gap-3 px-4 py-3 transition-colors hover:bg-stone-50',
        isUnread && 'bg-blue-50/40',
      )}
    >
      <div className="mt-0.5">
        <SeverityIcon severity={notification.severity} />
      </div>

      <div className="min-w-0 flex-1">
        <p
          className={cn(
            'text-sm text-stone-800 leading-snug',
            isUnread && 'font-semibold',
          )}
        >
          {notification.title}
        </p>
        <p className="mt-0.5 text-xs text-stone-500 leading-snug">{notification.message}</p>
        <p className="mt-1 text-[10px] text-stone-400">
          {formatRelativeTime(notification.createdAt)}
        </p>
      </div>

      {isUnread && onRead && (
        <button
          aria-label="Marcar como leída"
          className="shrink-0 rounded p-1 text-stone-400 hover:bg-stone-100 hover:text-stone-600"
          onClick={() => onRead(notification.id)}
        >
          <CheckCircle size={14} />
        </button>
      )}
    </div>
  )
}

export function NotificationEmptyState() {
  return (
    <div className="flex flex-col items-center justify-center gap-2 px-4 py-8 text-center">
      <CheckCircle className="text-stone-300" size={32} />
      <p className="text-sm font-medium text-stone-500">Todo al día</p>
      <p className="text-xs text-stone-400">No tienes notificaciones pendientes.</p>
    </div>
  )
}
