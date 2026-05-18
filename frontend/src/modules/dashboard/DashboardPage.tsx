import {
  AlertTriangle,
  ArrowRight,
  BadgeDollarSign,
  CircleDollarSign,
  Loader2,
  Package,
  ReceiptText,
  RefreshCw,
  Users,
} from 'lucide-react'
import { Link } from 'react-router-dom'
import { useDashboardRealtimeInvalidation, useDashboardSummary } from '@/modules/dashboard/hooks/useDashboard'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

export function DashboardPage() {
  const summary = useDashboardSummary()
  useDashboardRealtimeInvalidation()

  const data = summary.data

  return (
    <section className="min-h-full bg-surface-subtle p-4 lg:p-6">
      <div className="flex w-full flex-col gap-5">

        {/* Header */}
        <div className="flex flex-col gap-3 pb-2 lg:flex-row lg:items-center lg:justify-between">
          <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Inicio / Resumen</p>
          <Button
            disabled={summary.isLoading}
            onClick={() => summary.refetch()}
            size="sm"
            type="button"
            variant="outline"
          >
            {summary.isLoading ? (
              <Loader2 className="animate-spin" size={15} />
            ) : (
              <RefreshCw size={15} />
            )}
            Actualizar
          </Button>
        </div>

        <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-normal text-foreground">
              Tu tienda de un vistazo
            </h1>
            <p className="mt-1 text-sm text-stone-600">
              Resumen operativo de hoy — ventas, facturas, cobros e inventario.
            </p>
          </div>
          <div className="flex items-center gap-2 text-sm">
            <Link
              className="flex items-center gap-1.5 rounded-md bg-stone-100 px-3 py-2 font-semibold text-stone-700 ring-1 ring-stone-200 transition-colors hover:bg-stone-200"
              to="/reports"
            >
              Ver reportes
              <ArrowRight size={14} />
            </Link>
          </div>
        </header>

        {/* Error state */}
        {summary.isError && (
          <div className="flex items-center gap-3 rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700 ring-1 ring-red-200">
            <AlertTriangle size={16} />
            Error al cargar el resumen. Intenta actualizar.
          </div>
        )}

        {/* Metric cards */}
        <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <MetricCard
            icon={CircleDollarSign}
            iconBox="bg-emerald-50 text-emerald-700"
            isLoading={summary.isLoading}
            label="Ventas hoy"
            sub={`${data?.salesToday.count ?? 0} transacciones`}
            value={formatMoney(data?.salesToday.totalAmount ?? 0)}
          />
          <MetricCard
            icon={ReceiptText}
            iconBox="bg-stone-900 text-white"
            isLoading={summary.isLoading}
            label="Facturas hoy"
            sub={`${data?.invoicesToday.count ?? 0} documentos`}
            value={formatMoney(data?.invoicesToday.totalAmount ?? 0)}
          />
          <MetricCard
            icon={BadgeDollarSign}
            iconBox="bg-amber-50 text-amber-700"
            isLoading={summary.isLoading}
            label="Cuentas por cobrar"
            sub={`${data?.receivables.customerCount ?? 0} clientes`}
            value={formatMoney(data?.receivables.totalPending ?? 0)}
          />
          <MetricCard
            icon={Package}
            iconBox="bg-red-50 text-red-700"
            isLoading={summary.isLoading}
            label="Bajo stock"
            sub="productos"
            value={String(data?.lowStock.productCount ?? 0)}
          />
        </section>

        {/* Recent tables */}
        <section className="grid gap-4 xl:grid-cols-2">
          {/* Recent Sales */}
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <h2 className="text-base font-semibold text-foreground">Ventas recientes</h2>
              <Link
                className="flex items-center gap-1 text-sm font-semibold text-stone-600 hover:text-stone-900"
                to="/reports"
              >
                Ver todas
                <ArrowRight size={14} />
              </Link>
            </CardHeader>
            <CardContent className="p-0">
              {summary.isLoading ? (
                <LoadingRows />
              ) : !data?.recentSales.length ? (
                <EmptyTable message="Sin ventas hoy" />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[520px] text-sm">
                    <thead>
                      <tr className="border-b border-stone-100 bg-stone-50 text-xs font-semibold text-stone-500">
                        <th className="px-4 py-2.5 text-left">Nro. Venta</th>
                        <th className="px-4 py-2.5 text-left">Método</th>
                        <th className="px-4 py-2.5 text-left">Estado</th>
                        <th className="px-4 py-2.5 text-right">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.recentSales.map((sale) => (
                        <tr
                          className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60"
                          key={sale.saleId}
                        >
                          <td className="px-4 py-3 font-medium text-stone-900">{sale.saleNumber}</td>
                          <td className="px-4 py-3 text-stone-600">{sale.paymentMethod}</td>
                          <td className="px-4 py-3">
                            <SaleStatusBadge status={sale.status} />
                          </td>
                          <td className="px-4 py-3 text-right font-semibold text-stone-900">
                            {formatMoney(sale.total)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Recent Invoices */}
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <h2 className="text-base font-semibold text-foreground">Facturas recientes</h2>
              <Link
                className="flex items-center gap-1 text-sm font-semibold text-stone-600 hover:text-stone-900"
                to="/invoices"
              >
                Ver todas
                <ArrowRight size={14} />
              </Link>
            </CardHeader>
            <CardContent className="p-0">
              {summary.isLoading ? (
                <LoadingRows />
              ) : !data?.recentInvoices.length ? (
                <EmptyTable message="Sin facturas hoy" />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[520px] text-sm">
                    <thead>
                      <tr className="border-b border-stone-100 bg-stone-50 text-xs font-semibold text-stone-500">
                        <th className="px-4 py-2.5 text-left">Nro. Factura</th>
                        <th className="px-4 py-2.5 text-left">Estado</th>
                        <th className="px-4 py-2.5 text-right">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.recentInvoices.map((invoice) => (
                        <tr
                          className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60"
                          key={invoice.invoiceId}
                        >
                          <td className="px-4 py-3 font-medium text-stone-900">
                            <Link
                              className="hover:underline"
                              to={`/invoices/${invoice.invoiceId}`}
                            >
                              {invoice.invoiceNumber}
                            </Link>
                          </td>
                          <td className="px-4 py-3">
                            <InvoiceStatusBadge status={invoice.status} />
                          </td>
                          <td className="px-4 py-3 text-right font-semibold text-stone-900">
                            {formatMoney(invoice.total)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>
        </section>

        {/* Recent Purchases */}
        {(data?.recentPurchases.length ?? 0) > 0 && (
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <h2 className="text-base font-semibold text-foreground">Compras recientes</h2>
              <Link
                className="flex items-center gap-1 text-sm font-semibold text-stone-600 hover:text-stone-900"
                to="/purchases"
              >
                Ver todas
                <ArrowRight size={14} />
              </Link>
            </CardHeader>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full min-w-[520px] text-sm">
                  <thead>
                    <tr className="border-b border-stone-100 bg-stone-50 text-xs font-semibold text-stone-500">
                      <th className="px-4 py-2.5 text-left">Nro. Compra</th>
                      <th className="px-4 py-2.5 text-left">Proveedor</th>
                      <th className="px-4 py-2.5 text-left">Estado</th>
                      <th className="px-4 py-2.5 text-right">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data!.recentPurchases.map((purchase) => (
                      <tr
                        className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60"
                        key={purchase.purchaseId}
                      >
                        <td className="px-4 py-3 font-medium text-stone-900">{purchase.purchaseNumber}</td>
                        <td className="px-4 py-3 text-stone-600">{purchase.supplierName ?? '—'}</td>
                        <td className="px-4 py-3">
                          <span className="rounded-md bg-stone-100 px-2 py-1 text-xs font-semibold text-stone-700">
                            {purchase.status}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-right font-semibold text-stone-900">
                          {formatMoney(purchase.total)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Accounts receivable alert */}
        {(data?.receivables.customerCount ?? 0) > 0 && (
          <div className="flex flex-col gap-3 rounded-lg bg-amber-50 p-4 ring-1 ring-amber-200 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
              <Users className="shrink-0 text-amber-600" size={20} />
              <div>
                <p className="font-semibold text-amber-900">
                  {data!.receivables.customerCount} cliente{data!.receivables.customerCount !== 1 ? 's' : ''} con saldo pendiente
                </p>
                <p className="text-sm text-amber-700">
                  Total por cobrar: {formatMoney(data!.receivables.totalPending)}
                </p>
              </div>
            </div>
            <Link to="/reports">
              <Button size="sm" variant="outline">
                Ver cuentas por cobrar
              </Button>
            </Link>
          </div>
        )}
      </div>
    </section>
  )
}

// ── Sub-components ─────────────────────────────────────────────────────────

type MetricCardProps = {
  label: string
  value: string
  sub: string
  icon: React.ElementType
  iconBox: string
  isLoading: boolean
}

function MetricCard({ label, value, sub, icon: Icon, iconBox, isLoading }: MetricCardProps) {
  return (
    <Card className="bg-white shadow-none">
      <CardHeader>
        <div className="flex items-center justify-between gap-3">
          <p className="text-sm font-semibold text-stone-700">{label}</p>
          <span className={`flex h-8 w-8 items-center justify-center rounded-md ${iconBox}`}>
            <Icon aria-hidden="true" size={16} />
          </span>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="h-9 w-24 animate-pulse rounded bg-stone-100" />
        ) : (
          <p className="text-3xl font-semibold text-foreground">{value}</p>
        )}
        <p className="mt-2 text-sm font-medium text-stone-500">{sub}</p>
      </CardContent>
    </Card>
  )
}

function LoadingRows() {
  return (
    <div className="space-y-2 p-4">
      {[1, 2, 3].map((i) => (
        <div className="h-9 animate-pulse rounded bg-stone-100" key={i} />
      ))}
    </div>
  )
}

function EmptyTable({ message }: { message: string }) {
  return (
    <div className="flex items-center justify-center py-10 text-sm font-medium text-stone-400">
      {message}
    </div>
  )
}

function SaleStatusBadge({ status }: { status: string }) {
  const map: Record<string, string> = {
    Completed: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200',
    Cancelled: 'bg-red-50 text-red-700 ring-1 ring-red-200',
    Pending: 'bg-amber-50 text-amber-700 ring-1 ring-amber-200',
  }

  return (
    <span className={`rounded-md px-2 py-1 text-xs font-semibold ${map[status] ?? 'bg-stone-100 text-stone-700'}`}>
      {status}
    </span>
  )
}

function InvoiceStatusBadge({ status }: { status: string }) {
  const map: Record<string, string> = {
    Issued: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200',
    Cancelled: 'bg-red-50 text-red-700 ring-1 ring-red-200',
    Draft: 'bg-stone-100 text-stone-600',
  }

  return (
    <span className={`rounded-md px-2 py-1 text-xs font-semibold ${map[status] ?? 'bg-stone-100 text-stone-700'}`}>
      {status}
    </span>
  )
}

function formatMoney(amount: number): string {
  return new Intl.NumberFormat('es-DO', {
    currency: 'DOP',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency',
  }).format(amount)
}
