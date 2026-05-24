import { AlertTriangle, CheckCircle, Info, Loader2 } from 'lucide-react'
import { useState } from 'react'
import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
} from '@/modules/notifications/hooks/useNotifications'
import type { NotificationItem, NotificationSeverity, NotificationType } from '@/modules/notifications/types'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'
import { cn } from '@/shared/utils/cn'

const PAGE_SIZE = 20

function formatDateTime(dateString: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(dateString))
}

function typeLabel(type: NotificationType): string {
  const labels: Record<NotificationType, string> = {
    LowStock: 'Stock bajo',
    CashDifference: 'Diferencia de caja',
    SaleFailed: 'Venta fallida',
    InvoiceFailed: 'Recibo fallido',
    HighExpense: 'Gasto elevado',
    NegativeProfit: 'Ganancia negativa',
    ProductMissingCost: 'Costo faltante',
    DailyClosingPending: 'Cierre pendiente',
    CustomerDebtOverdue: 'Deuda vencida',
  }
  return labels[type] ?? type
}

function SeverityBadge({ severity }: { severity: NotificationSeverity }) {
  if (severity === 'Critical')
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-red-50 px-2 py-0.5 text-xs font-medium text-red-700 ring-1 ring-red-200">
        <AlertTriangle size={10} />
        Crítico
      </span>
    )
  if (severity === 'Warning')
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-700 ring-1 ring-amber-200">
        <AlertTriangle size={10} />
        Advertencia
      </span>
    )
  return (
    <span className="inline-flex items-center gap-1 rounded-full bg-blue-50 px-2 py-0.5 text-xs font-medium text-blue-700 ring-1 ring-blue-200">
      <Info size={10} />
      Info
    </span>
  )
}

function NotificationRow({
  notification,
  onRead,
}: {
  notification: NotificationItem
  onRead: (id: string) => void
}) {
  const isUnread = notification.status === 'Unread'

  return (
    <Card className={cn('transition-colors', isUnread && 'border-blue-200 bg-blue-50/30')}>
      <CardContent className="flex items-start gap-4 p-4">
        <div className="flex-1 min-w-0">
          <div className="flex flex-wrap items-center gap-2 mb-1">
            <span
              className={cn('text-sm text-stone-900', isUnread && 'font-semibold')}
            >
              {notification.title}
            </span>
            <SeverityBadge severity={notification.severity} />
            <Badge variant="secondary" className="text-[10px]">
              {typeLabel(notification.type)}
            </Badge>
            {isUnread && (
              <span className="h-2 w-2 rounded-full bg-blue-500" aria-label="Sin leer" />
            )}
          </div>
          <p className="text-sm text-stone-600">{notification.message}</p>
          <p className="mt-1 text-xs text-stone-400">{formatDateTime(notification.createdAt)}</p>
        </div>

        {isUnread && (
          <Button
            size="sm"
            variant="ghost"
            className="shrink-0 text-stone-500"
            onClick={() => onRead(notification.id)}
          >
            <CheckCircle size={14} />
            <span className="sr-only">Marcar como leída</span>
          </Button>
        )}
      </CardContent>
    </Card>
  )
}

export function NotificationListPage() {
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useNotifications({ page, pageSize: PAGE_SIZE })
  const markRead = useMarkNotificationRead()
  const markAll = useMarkAllNotificationsRead()

  const items = data?.items ?? []
  const total = data?.totalCount ?? 0
  const unreadCount = data?.unreadCount ?? 0
  const totalPages = Math.ceil(total / PAGE_SIZE)

  function handleMarkRead(id: string) {
    markRead.mutate(id)
  }

  function handleMarkAll() {
    markAll.mutate()
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Notificaciones</h2>
          <p className="text-sm text-stone-500">
            {unreadCount > 0
              ? `${unreadCount} sin leer`
              : 'Todo al día'}
          </p>
        </div>
        {unreadCount > 0 && (
          <Button
            variant="outline"
            size="sm"
            disabled={markAll.isPending}
            onClick={handleMarkAll}
          >
            {markAll.isPending ? (
              <Loader2 className="animate-spin" size={14} />
            ) : (
              <CheckCircle size={14} />
            )}
            Marcar todas como leídas
          </Button>
        )}
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      )}

      {/* Error */}
      {isError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Error al cargar las notificaciones. Intenta nuevamente.
        </p>
      )}

      {/* Empty */}
      {!isLoading && !isError && items.length === 0 && (
        <Card>
          <CardContent className="flex h-40 flex-col items-center justify-center gap-3">
            <CheckCircle className="text-stone-300" size={32} />
            <div className="text-center">
              <p className="text-sm font-medium text-stone-600">Todo al día</p>
              <p className="text-xs text-stone-400">No hay notificaciones en este momento.</p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* List */}
      {!isLoading && !isError && items.length > 0 && (
        <div className="space-y-3">
          {items.map((n) => (
            <NotificationRow key={n.id} notification={n} onRead={handleMarkRead} />
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between border-t border-stone-200 pt-4">
          <p className="text-sm text-stone-500">
            {total} notificaciones · Página {page} de {totalPages}
          </p>
          <div className="flex gap-2">
            <Button
              disabled={page <= 1}
              size="sm"
              variant="outline"
              onClick={() => setPage((p) => p - 1)}
            >
              Anterior
            </Button>
            <Button
              disabled={page >= totalPages}
              size="sm"
              variant="outline"
              onClick={() => setPage((p) => p + 1)}
            >
              Siguiente
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
