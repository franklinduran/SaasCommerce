import { Badge } from '@/shared/components/ui/badge'
import { cn } from '@/shared/utils/cn'
import { SubscriptionStatus } from '@/modules/subscription/types'

type SubscriptionStatusBadgeProps = {
  className?: string
  status: SubscriptionStatus
}

const statusConfig: Record<SubscriptionStatus, {
  className: string
  label: string
  variant: 'default' | 'destructive' | 'outline' | 'secondary' | 'success' | 'warning'
}> = {
  Active: {
    className: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200',
    label: 'Activo',
    variant: 'success',
  },
  Cancelled: {
    className: 'bg-stone-100 text-stone-700 ring-1 ring-stone-200',
    label: 'Cancelado',
    variant: 'secondary',
  },
  Expired: {
    className: 'bg-red-50 text-red-700 ring-1 ring-red-200',
    label: 'Vencido',
    variant: 'destructive',
  },
  PastDue: {
    className: 'bg-amber-50 text-amber-700 ring-1 ring-amber-200',
    label: 'Pago pendiente',
    variant: 'warning',
  },
  Suspended: {
    className: 'bg-red-50 text-red-700 ring-1 ring-red-200',
    label: 'Suspendido',
    variant: 'destructive',
  },
  Trial: {
    className: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200',
    label: 'Trial',
    variant: 'outline',
  },
}

export function SubscriptionStatusBadge({ className, status }: Readonly<SubscriptionStatusBadgeProps>) {
  const config = statusConfig[status] ?? statusConfig.Expired

  return (
    <Badge className={cn('rounded-md border-0', config.className, className)} variant={config.variant}>
      {config.label}
    </Badge>
  )
}
