import { ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { DashboardSummary } from '@/modules/dashboard/types'
import { formatMoney } from '@/modules/dashboard/utils/dashboardFormat'
import { cn } from '@/shared/utils/cn'

type Props = {
  data: DashboardSummary | undefined
  isLoading: boolean
}

export function DashboardTables({ data, isLoading }: Readonly<Props>) {
  return (
    <section aria-label="Actividad reciente" className="grid gap-4 xl:grid-cols-2">
      {/* Ventas recientes */}
      <DataCard href="/reports" hrefLabel="Ver todas" title="Ventas recientes">
        {isLoading ? (
          <SkeletonRows />
        ) : !data?.recentSales.length ? (
          <EmptyState message="Sin ventas hoy" />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[440px] text-[13px]">
              <thead>
                <TableHead cols={['Nro. Venta', 'Método', 'Estado', 'Total']} lastRight />
              </thead>
              <tbody>
                {data.recentSales.map((sale) => (
                  <tr className="border-b border-border last:border-0 transition-colors hover:bg-muted" key={sale.saleId}>
                    <td className="px-4 py-3 font-medium text-foreground">{sale.code}</td>
                    <td className="px-4 py-3 text-muted-foreground">{sale.paymentMethod}</td>
                    <td className="px-4 py-3"><StatusBadge status={sale.status} type="sale" /></td>
                    <td className="px-4 py-3 text-right font-semibold text-foreground">{formatMoney(sale.total)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </DataCard>

      {/* Facturas recientes */}
      <DataCard href="/invoices" hrefLabel="Ver todas" title="Facturas recientes">
        {isLoading ? (
          <SkeletonRows />
        ) : !data?.recentInvoices.length ? (
          <EmptyState message="Sin facturas hoy" />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[380px] text-[13px]">
              <thead>
                <TableHead cols={['Nro. Factura', 'Estado', 'Total']} lastRight />
              </thead>
              <tbody>
                {data.recentInvoices.map((invoice) => (
                  <tr className="border-b border-border last:border-0 transition-colors hover:bg-muted" key={invoice.invoiceId}>
                    <td className="px-4 py-3">
                      <Link
                        className="font-medium text-foreground underline-offset-4 hover:underline focus-visible:outline-none"
                        to={`/invoices/${invoice.invoiceId}`}
                      >
                        {invoice.invoiceNumber}
                      </Link>
                    </td>
                    <td className="px-4 py-3"><StatusBadge status={invoice.status} type="invoice" /></td>
                    <td className="px-4 py-3 text-right font-semibold text-foreground">{formatMoney(invoice.total)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </DataCard>

      {/* Compras recientes — full width */}
      {(data?.recentPurchases.length ?? 0) > 0 && (
        <DataCard className="xl:col-span-2" href="/purchases" hrefLabel="Ver todas" title="Compras recientes">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[480px] text-[13px]">
              <thead>
                <TableHead cols={['Proveedor', 'Estado', 'Total']} lastRight />
              </thead>
              <tbody>
                {data!.recentPurchases.map((purchase) => (
                  <tr className="border-b border-border last:border-0 transition-colors hover:bg-muted" key={purchase.purchaseId}>
                    <td className="px-4 py-3 text-muted-foreground">{purchase.supplierName ?? '—'}</td>
                    <td className="px-4 py-3">
                      <span className="rounded-md bg-muted px-2 py-0.5 text-[11px] font-semibold text-muted-foreground">
                        {purchase.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right font-semibold text-foreground">{formatMoney(purchase.total)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </DataCard>
      )}
    </section>
  )
}

// ─── Shared sub-components ────────────────────────────────────────────────────

type DataCardProps = {
  children: React.ReactNode
  className?: string
  href: string
  hrefLabel: string
  title: string
}

function DataCard({ children, className, href, hrefLabel, title }: Readonly<DataCardProps>) {
  return (
    <div className={cn('overflow-hidden rounded-2xl bg-card shadow-sm ring-1 ring-border', className)}>
      <div className="flex items-center justify-between border-b border-border px-5 py-3.5">
        <h2 className="text-[13.5px] font-semibold text-foreground">{title}</h2>
        <Link
          className="flex items-center gap-1 text-[12px] font-medium text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:underline"
          to={href}
        >
          {hrefLabel}
          <ArrowRight aria-hidden="true" size={12} />
        </Link>
      </div>
      {children}
    </div>
  )
}

function TableHead({ cols, lastRight }: Readonly<{ cols: string[]; lastRight?: boolean }>) {
  return (
    <tr className="border-b border-border">
      {cols.map((col, i) => (
        <th
          className={cn(
            'px-4 py-2.5 text-[10.5px] font-semibold uppercase tracking-[0.08em] text-muted-foreground',
            lastRight && i === cols.length - 1 ? 'text-right' : 'text-left',
          )}
          key={col}
        >
          {col}
        </th>
      ))}
    </tr>
  )
}

type StatusBadgeProps = {
  status: string
  type: 'sale' | 'invoice'
}

function StatusBadge({ status, type }: Readonly<StatusBadgeProps>) {
  const saleStyles: Record<string, string> = {
    Completed: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200/60',
    Cancelled: 'bg-red-50 text-red-700 ring-1 ring-red-200/60',
    Pending: 'bg-amber-50 text-amber-700 ring-1 ring-amber-200/60',
  }
  const invoiceStyles: Record<string, string> = {
    Issued: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200/60',
    Cancelled: 'bg-red-50 text-red-700 ring-1 ring-red-200/60',
    Draft: 'bg-gray-200/70 text-gray-600',
  }
  const styles = type === 'sale' ? saleStyles : invoiceStyles
  return (
    <span className={cn('rounded-md px-2 py-0.5 text-[11px] font-semibold', styles[status] ?? 'bg-gray-100 text-gray-600')}>
      {status}
    </span>
  )
}

function SkeletonRows() {
  return (
    <div className="space-y-2 px-5 py-4">
      {[1, 2, 3].map((i) => (
        <div className="h-8 animate-pulse rounded-lg bg-muted" key={i} />
      ))}
    </div>
  )
}

function EmptyState({ message }: Readonly<{ message: string }>) {
  return (
    <div className="flex items-center justify-center py-10 text-[13px] font-medium text-muted-foreground">
      {message}
    </div>
  )
}
