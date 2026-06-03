import { CheckCircle2, CircleDashed, Loader2, XCircle } from 'lucide-react'
import type { SaleStatus } from '@/modules/pos/types/posTypes'
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
    <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
      <div className="flex items-center gap-3 border-b border-border px-5 py-4">
        <span className={cn('flex h-8 w-8 shrink-0 items-center justify-center rounded-lg', details.iconBg)}>
          <StatusIcon
            aria-hidden="true"
            className={cn(details.iconColor, (status === 'Submitting' || status === 'Processing') && 'animate-spin')}
            size={15}
            strokeWidth={2}
          />
        </span>
        <div>
          <h2 className="text-[13.5px] font-semibold text-foreground">Estado de venta</h2>
          <p className="text-[12px] text-muted-foreground">{details.caption}</p>
        </div>
      </div>

      <div className="space-y-3 p-4">
        <div className={cn('rounded-xl px-4 py-3 ring-1', details.boxClassName)}>
          <p className="text-[13px] font-semibold">{details.title}</p>
          {saleId && (
            <p className="mt-1 break-all font-mono text-[10.5px] font-medium opacity-70">
              {saleId}
            </p>
          )}
          {typeof total === 'number' && (
            <p className="mt-2 text-[13px] font-semibold">Total: {formatMoney(total)}</p>
          )}
          {reason && <p className="mt-2 text-[12.5px]">{reason}</p>}
        </div>

        {errorMessage && (
          <p className="rounded-xl bg-red-50 px-3 py-2 text-[12.5px] font-semibold text-red-700 ring-1 ring-red-200">
            {errorMessage}
          </p>
        )}
      </div>
    </div>
  )
}

const statusDetails: Record<
  PanelStatus,
  {
    boxClassName: string
    caption: string
    icon: typeof CircleDashed
    iconBg: string
    iconColor: string
    title: string
  }
> = {
  Idle: {
    boxClassName: 'bg-muted text-muted-foreground ring-border',
    caption: 'Sin venta activa',
    icon: CircleDashed,
    iconBg: 'bg-muted',
    iconColor: 'text-muted-foreground',
    title: 'Lista para procesar',
  },
  Submitting: {
    boxClassName: 'bg-sky-50 text-sky-800 ring-sky-200',
    caption: 'Enviando al servidor',
    icon: Loader2,
    iconBg: 'bg-sky-50',
    iconColor: 'text-sky-700',
    title: 'Procesando venta',
  },
  Received: {
    boxClassName: 'bg-sky-50 text-sky-800 ring-sky-200',
    caption: 'Recibida por servidor',
    icon: Loader2,
    iconBg: 'bg-sky-50',
    iconColor: 'text-sky-700',
    title: 'Venta recibida',
  },
  Processing: {
    boxClassName: 'bg-sky-50 text-sky-800 ring-sky-200',
    caption: 'Procesando',
    icon: Loader2,
    iconBg: 'bg-sky-50',
    iconColor: 'text-sky-700',
    title: 'Procesando venta',
  },
  Completed: {
    boxClassName: 'bg-emerald-50 text-emerald-800 ring-emerald-200',
    caption: 'Finalizada',
    icon: CheckCircle2,
    iconBg: 'bg-emerald-50',
    iconColor: 'text-emerald-700',
    title: 'Venta completada',
  },
  Failed: {
    boxClassName: 'bg-red-50 text-red-800 ring-red-200',
    caption: 'Requiere revision',
    icon: XCircle,
    iconBg: 'bg-red-50',
    iconColor: 'text-red-700',
    title: 'Venta fallida',
  },
  Cancelled: {
    boxClassName: 'bg-amber-50 text-amber-800 ring-amber-200',
    caption: 'Cancelada',
    icon: XCircle,
    iconBg: 'bg-amber-50',
    iconColor: 'text-amber-700',
    title: 'Venta cancelada',
  },
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
