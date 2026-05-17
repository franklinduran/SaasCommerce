import type { SaleStatus } from '@/modules/sales/types/salesTypes'
import { cn } from '@/shared/utils/cn'

const statusLabels: Record<SaleStatus, string> = {
  Cancelled: 'Cancelada',
  Completed: 'Completada',
  Failed: 'Fallida',
  Processing: 'Procesando',
  Received: 'Recibida',
}

const statusClassNames: Record<SaleStatus, string> = {
  Cancelled: 'bg-stone-100 text-stone-700 ring-stone-200',
  Completed: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Failed: 'bg-red-50 text-red-700 ring-red-200',
  Processing: 'bg-sky-50 text-sky-700 ring-sky-200',
  Received: 'bg-amber-50 text-amber-700 ring-amber-200',
}

export function SaleStatusBadge({ status }: Readonly<{ status: SaleStatus }>) {
  return (
    <span
      className={cn(
        'inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ring-1',
        statusClassNames[status],
      )}
    >
      {statusLabels[status]}
    </span>
  )
}
