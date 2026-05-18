import type { InvoiceStatus } from '@/modules/invoices/types'
import { cn } from '@/shared/utils/cn'

const statusLabels: Record<InvoiceStatus, string> = {
  Cancelled: 'Cancelado',
  Draft: 'Borrador',
  Issued: 'Emitido',
}

export function InvoiceStatusBadge({ status }: Readonly<{ status: InvoiceStatus }>) {
  return (
    <span
      className={cn(
        'inline-flex h-7 items-center rounded-md border px-2.5 text-xs font-semibold',
        status === 'Issued' && 'border-emerald-200 bg-emerald-50 text-emerald-700',
        status === 'Cancelled' && 'border-stone-200 bg-stone-100 text-stone-700',
        status === 'Draft' && 'border-amber-200 bg-amber-50 text-amber-700',
      )}
    >
      {statusLabels[status]}
    </span>
  )
}
