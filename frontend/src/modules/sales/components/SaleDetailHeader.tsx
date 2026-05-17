import { AlertTriangle, CalendarClock, CreditCard, MapPin, UserRound } from 'lucide-react'
import { SaleStatusBadge } from '@/modules/sales/components/SaleStatusBadge'
import type { SaleDetail } from '@/modules/sales/types/salesTypes'
import { formatCurrency, formatDateTime } from '@/modules/sales/utils/formatSales'

export function SaleDetailHeader({ sale }: Readonly<{ sale: SaleDetail }>) {
  const failureMessage = sale.status === 'Failed' ? sale.failureReason : null
  const cancellationMessage = sale.status === 'Cancelled' ? sale.cancellationReason : null

  return (
    <div className="rounded-md bg-white p-5 shadow-sm ring-1 ring-stone-200">
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-start">
        <div>
          <p className="font-mono text-xs font-semibold uppercase text-stone-500">{sale.code}</p>
          <div className="mt-2 flex flex-wrap items-center gap-3">
            <h2 className="text-2xl font-semibold text-stone-950">Detalle de venta</h2>
            <SaleStatusBadge status={sale.status} />
          </div>
        </div>
        <div className="text-left lg:text-right">
          <p className="text-sm font-semibold text-stone-500">Total</p>
          <p className="text-2xl font-semibold text-stone-950">{formatCurrency(sale.total)}</p>
        </div>
      </div>

      <div className="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        <SaleFact icon={CalendarClock} label="Fecha" value={formatDateTime(sale.createdAt)} />
        <SaleFact icon={UserRound} label="Cliente" value={sale.customerName ?? 'Consumidor final'} />
        <SaleFact icon={MapPin} label="Sucursal" value={sale.branchName ?? 'Sucursal no disponible'} />
        <SaleFact icon={CreditCard} label="Metodo" value={sale.paymentMethod} />
      </div>

      {(failureMessage || cancellationMessage) && (
        <div className="mt-5 flex gap-3 rounded-md bg-red-50 p-4 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          <AlertTriangle aria-hidden="true" className="mt-0.5 shrink-0" size={18} />
          <p>{failureMessage ?? cancellationMessage}</p>
        </div>
      )}
    </div>
  )
}

type SaleFactProps = {
  icon: typeof CalendarClock
  label: string
  value: string
}

function SaleFact({ icon: Icon, label, value }: Readonly<SaleFactProps>) {
  return (
    <div className="rounded-md bg-stone-50 p-3 ring-1 ring-stone-200">
      <div className="flex items-center gap-2 text-xs font-semibold uppercase text-stone-500">
        <Icon aria-hidden="true" size={15} />
        {label}
      </div>
      <p className="mt-2 text-sm font-semibold text-stone-900">{value}</p>
    </div>
  )
}
