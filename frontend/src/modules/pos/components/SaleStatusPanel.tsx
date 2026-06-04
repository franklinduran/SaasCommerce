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
  const isSpinning = status === 'Submitting' || status === 'Processing' || status === 'Received'

  return (
    <div>
      {/* Section header */}
      <div className="flex items-center gap-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          Estado de venta
        </p>
        <div className="h-px flex-1 bg-border" />
      </div>

      {/* Status row */}
      <div className="mt-3 flex items-center gap-2.5">
        <StatusIcon
          aria-hidden="true"
          className={cn(details.iconColor, isSpinning && 'animate-spin')}
          size={15}
          strokeWidth={2}
        />
        <div className="min-w-0 flex-1">
          <p className={cn('text-[13px] font-semibold', details.textColor)}>
            {details.title}
          </p>
          <p className="text-[11.5px] text-muted-foreground">{details.caption}</p>
        </div>
        {typeof total === 'number' && (
          <span className="shrink-0 text-[13px] font-bold tabular-nums text-foreground">
            {formatMoney(total)}
          </span>
        )}
      </div>

      {saleId && (
        <p className="mt-1.5 break-all font-mono text-[10.5px] text-muted-foreground/60">
          {saleId}
        </p>
      )}

      {reason && (
        <p className="mt-1.5 text-[12.5px] text-muted-foreground">{reason}</p>
      )}

      {errorMessage && (
        <p className="mt-3 rounded-lg bg-red-50 px-3 py-2 text-[12.5px] font-semibold text-red-700 ring-1 ring-red-200">
          {errorMessage}
        </p>
      )}
    </div>
  )
}

const statusDetails: Record<
  PanelStatus,
  {
    caption: string
    icon: typeof CircleDashed
    iconColor: string
    textColor: string
    title: string
  }
> = {
  Idle: {
    caption: 'Sin venta activa',
    icon: CircleDashed,
    iconColor: 'text-muted-foreground/50',
    textColor: 'text-muted-foreground',
    title: 'Lista para procesar',
  },
  Submitting: {
    caption: 'Enviando al servidor',
    icon: Loader2,
    iconColor: 'text-sky-600',
    textColor: 'text-sky-700',
    title: 'Procesando venta',
  },
  Received: {
    caption: 'Recibida por servidor',
    icon: Loader2,
    iconColor: 'text-sky-600',
    textColor: 'text-sky-700',
    title: 'Venta recibida',
  },
  Processing: {
    caption: 'Procesando',
    icon: Loader2,
    iconColor: 'text-sky-600',
    textColor: 'text-sky-700',
    title: 'Procesando venta',
  },
  Completed: {
    caption: 'Finalizada con éxito',
    icon: CheckCircle2,
    iconColor: 'text-emerald-600',
    textColor: 'text-emerald-700',
    title: 'Venta completada',
  },
  Failed: {
    caption: 'Requiere revisión',
    icon: XCircle,
    iconColor: 'text-red-500',
    textColor: 'text-red-700',
    title: 'Venta fallida',
  },
  Cancelled: {
    caption: 'Cancelada',
    icon: XCircle,
    iconColor: 'text-amber-500',
    textColor: 'text-amber-700',
    title: 'Venta cancelada',
  },
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
