import { BadgeDollarSign, CircleDollarSign, Package, ReceiptText, TrendingUp } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import type { DashboardSummary } from '@/modules/dashboard/types'
import { formatMoney } from '@/modules/dashboard/utils/dashboardFormat'

type Props = {
  data: DashboardSummary | undefined
  isLoading: boolean
}

export function DashboardKpiCards({ data, isLoading }: Readonly<Props>) {
  const salesCount = data?.salesToday.count ?? 0
  const salesTotalAmount = data?.salesToday.totalAmount ?? 0
  const avgTicket = salesCount > 0 ? salesTotalAmount / salesCount : 0

  return (
    <section aria-label="Métricas del día" className="grid gap-3 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-5">
      <KpiCard
        icon={CircleDollarSign}
        isLoading={isLoading}
        label="Ventas hoy"
        sub={`${salesCount} transacciones`}
        value={formatMoney(salesTotalAmount)}
      />
      <KpiCard
        icon={TrendingUp}
        isLoading={isLoading}
        label="Ticket promedio"
        sub="por venta hoy"
        value={formatMoney(avgTicket)}
      />
      <KpiCard
        icon={ReceiptText}
        isLoading={isLoading}
        label="Facturas hoy"
        sub={`${data?.invoicesToday.count ?? 0} documentos`}
        value={formatMoney(data?.invoicesToday.totalAmount ?? 0)}
      />
      <KpiCard
        icon={BadgeDollarSign}
        isLoading={isLoading}
        label="Por cobrar"
        sub={`${data?.receivables.customerCount ?? 0} clientes`}
        value={formatMoney(data?.receivables.totalPending ?? 0)}
      />
      <KpiCard
        icon={Package}
        isLoading={isLoading}
        label="Bajo stock"
        sub="productos"
        value={String(data?.lowStock.productCount ?? 0)}
        alert={(data?.lowStock.productCount ?? 0) > 0}
      />
    </section>
  )
}

type KpiCardProps = {
  alert?: boolean
  icon: LucideIcon
  isLoading: boolean
  label: string
  sub: string
  value: string
}

function KpiCard({ alert, icon: Icon, isLoading, label, sub, value }: Readonly<KpiCardProps>) {
  return (
    <div className="flex flex-col gap-3 rounded-2xl bg-card px-5 py-5 shadow-sm ring-1 ring-border">
      <div className="flex items-center justify-between gap-2">
        <p className="text-[11.5px] font-semibold uppercase tracking-[0.08em] text-muted-foreground">{label}</p>
        <span className={`flex h-7 w-7 shrink-0 items-center justify-center rounded-md ${alert ? 'bg-amber-50' : 'bg-muted'}`}>
          <Icon
            aria-hidden="true"
            className={alert ? 'text-amber-600' : 'text-muted-foreground'}
            size={14}
            strokeWidth={1.85}
          />
        </span>
      </div>
      {isLoading ? (
        <div className="h-8 w-28 animate-pulse rounded-lg bg-muted" />
      ) : (
        <p className={`text-[1.75rem] font-bold leading-none tracking-tight ${alert ? 'text-amber-700' : 'text-foreground'}`}>
          {value}
        </p>
      )}
      <p className="text-[12px] font-medium text-muted-foreground">{sub}</p>
    </div>
  )
}
