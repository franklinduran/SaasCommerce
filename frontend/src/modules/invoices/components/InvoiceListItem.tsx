import { ChevronRight, Receipt } from 'lucide-react'
import { InvoiceStatusBadge } from '@/modules/invoices/components/InvoiceStatusBadge'
import type { Invoice } from '@/modules/invoices/types'
import { formatInvoiceMoney } from '@/modules/invoices/utils/formatInvoiceMoney'
import { cn } from '@/shared/utils/cn'

type InvoiceListItemProps = {
  invoice: Invoice
  selected: boolean
  onClick: () => void
}

function formatDate(value: string) {
  try {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      month: 'short',
    }).format(new Date(value))
  } catch {
    return value
  }
}

export function InvoiceListItem({ invoice, selected, onClick }: Readonly<InvoiceListItemProps>) {
  return (
    <li>
      <button
        aria-current={selected ? 'true' : undefined}
        className={cn(
          'group flex w-full items-start gap-3 px-3 py-3 text-left transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-stone-900/25',
          selected
            ? 'bg-stone-900 text-white shadow-sm hover:bg-stone-900 active:bg-stone-950'
            : 'text-stone-900 hover:bg-stone-50 active:bg-stone-100',
        )}
        onClick={onClick}
        type="button"
      >
        <div
          className={cn(
            'flex h-10 w-10 shrink-0 items-center justify-center rounded-md ring-1',
            selected
              ? 'bg-white text-stone-900 ring-white/30'
              : 'bg-white text-stone-700 ring-stone-200 group-hover:ring-stone-300',
          )}
        >
          <Receipt size={16} />
        </div>

        <div className="min-w-0 flex-1">
          <div className="flex items-center justify-between gap-2">
            <p className={cn('truncate font-mono text-sm font-semibold', selected ? 'text-white' : 'text-stone-900')}>
              {invoice.invoiceNumber}
            </p>
            <ChevronRight
              aria-hidden="true"
              className={cn(
                'shrink-0 transition-transform',
                selected ? 'text-white/80' : 'text-stone-300 group-hover:translate-x-0.5 group-hover:text-stone-500',
              )}
              size={14}
            />
          </div>
          <p className={cn('truncate text-xs font-medium', selected ? 'text-stone-200' : 'text-stone-500')}>
            Venta {invoice.saleId.slice(0, 8).toUpperCase()} · {formatDate(invoice.createdAt)}
          </p>
          <div className="mt-1.5 flex items-center justify-between gap-2">
            <InvoiceStatusBadge status={invoice.status} />
            <span className={cn('text-xs font-semibold tabular-nums', selected ? 'text-white' : 'text-stone-900')}>
              {formatInvoiceMoney(invoice.total)}
            </span>
          </div>
        </div>
      </button>
    </li>
  )
}
