import type { StockStatus } from '@/modules/inventory/types'
import { cn } from '@/shared/utils/cn'

const labels: Record<StockStatus, string> = {
  Available: 'Disponible',
  LowStock: 'Stock bajo',
  OutOfStock: 'Agotado',
}

export function StockStatusBadge({ status }: Readonly<{ status: StockStatus }>) {
  return (
    <span
      className={cn(
        'inline-flex rounded-md px-2.5 py-1 text-xs font-semibold ring-1',
        status === 'Available' && 'bg-emerald-50 text-emerald-700 ring-emerald-200',
        status === 'LowStock' && 'bg-amber-50 text-amber-700 ring-amber-200',
        status === 'OutOfStock' && 'bg-red-50 text-red-700 ring-red-200',
      )}
    >
      {labels[status]}
    </span>
  )
}
