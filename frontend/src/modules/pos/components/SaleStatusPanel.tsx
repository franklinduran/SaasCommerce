import { CheckCircle2, CircleDashed, Loader2, XCircle } from 'lucide-react'
import type { SaleStatus } from '@/modules/pos/types/posTypes'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { cn } from '@/shared/utils/cn'

type PanelStatus = SaleStatus | 'Idle' | 'Submitting'

type SaleStatusPanelProps = {
  errorMessage: string | null
  reason?: string | null
  saleId: string | null
  status: PanelStatus
  total?: number | null
}

export function SaleStatusPanel({
  errorMessage,
  reason,
  saleId,
  status,
  total,
}: Readonly<SaleStatusPanelProps>) {
  const details = statusDetails[status]
  const StatusIcon = details.icon

  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <span className={cn('flex h-10 w-10 items-center justify-center rounded-md', details.iconClassName)}>
          <StatusIcon
            aria-hidden="true"
            className={status === 'Submitting' || status === 'Processing' ? 'animate-spin' : undefined}
            size={18}
          />
        </span>
        <div>
          <h2 className="text-base font-semibold text-stone-950">Estado de venta</h2>
          <p className="text-sm font-medium text-stone-600">{details.caption}</p>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        <div className={cn('rounded-md px-4 py-3 ring-1', details.boxClassName)}>
          <p className="text-sm font-semibold">{details.title}</p>
          {saleId && (
            <p className="mt-1 break-all font-mono text-xs font-semibold opacity-80">
              SaleId {saleId}
            </p>
          )}
          {typeof total === 'number' && (
            <p className="mt-2 text-sm font-semibold">Total backend {formatMoney(total)}</p>
          )}
          {reason && <p className="mt-2 text-sm font-medium">{reason}</p>}
        </div>
        {errorMessage && (
          <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
            {errorMessage}
          </p>
        )}
      </CardContent>
    </Card>
  )
}

const statusDetails: Record<
  PanelStatus,
  {
    boxClassName: string
    caption: string
    icon: typeof CircleDashed
    iconClassName: string
    title: string
  }
> = {
  Idle: {
    boxClassName: 'bg-stone-50 text-stone-700 ring-stone-200',
    caption: 'Sin venta activa',
    icon: CircleDashed,
    iconClassName: 'bg-stone-100 text-stone-700',
    title: 'Lista para procesar',
  },
  Submitting: {
    boxClassName: 'bg-sky-50 text-sky-800 ring-sky-200',
    caption: 'Enviando al API',
    icon: Loader2,
    iconClassName: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200',
    title: 'Procesando venta',
  },
  Received: {
    boxClassName: 'bg-sky-50 text-sky-800 ring-sky-200',
    caption: 'Recibida por backend',
    icon: Loader2,
    iconClassName: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200',
    title: 'Venta recibida',
  },
  Processing: {
    boxClassName: 'bg-sky-50 text-sky-800 ring-sky-200',
    caption: 'Saga en curso',
    icon: Loader2,
    iconClassName: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200',
    title: 'Procesando venta',
  },
  Completed: {
    boxClassName: 'bg-emerald-50 text-emerald-800 ring-emerald-200',
    caption: 'Finalizada',
    icon: CheckCircle2,
    iconClassName: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200',
    title: 'Venta completada',
  },
  Failed: {
    boxClassName: 'bg-red-50 text-red-800 ring-red-200',
    caption: 'Requiere revision',
    icon: XCircle,
    iconClassName: 'bg-red-50 text-red-700 ring-1 ring-red-200',
    title: 'Venta fallida',
  },
  Cancelled: {
    boxClassName: 'bg-amber-50 text-amber-800 ring-amber-200',
    caption: 'Cancelada',
    icon: XCircle,
    iconClassName: 'bg-amber-50 text-amber-700 ring-1 ring-amber-200',
    title: 'Venta cancelada',
  },
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
