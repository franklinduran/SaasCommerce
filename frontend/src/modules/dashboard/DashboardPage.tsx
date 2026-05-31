import { AlertTriangle, ArrowRight, RefreshCw, Users } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useDashboardRealtimeInvalidation, useDashboardSummary } from '@/modules/dashboard/hooks/useDashboard'
import { DashboardKpiCards } from '@/modules/dashboard/components/DashboardKpiCards'
import {
  AvgTicketCard,
  DailySalesBarChart,
  PaymentMethodChart,
  SaleStatusChart,
  SalesVsPurchasesChart,
} from '@/modules/dashboard/components/DashboardCharts'
import { DashboardTables } from '@/modules/dashboard/components/DashboardTables'
import { buildChartData, formatDate, formatMoney } from '@/modules/dashboard/utils/dashboardFormat'
import type { DateRangeFilter } from '@/modules/dashboard/types'
import { cn } from '@/shared/utils/cn'

const DATE_FILTERS: { label: string; value: DateRangeFilter; days: number }[] = [
  { label: '7 días', value: '7d', days: 7 },
  { label: '14 días', value: '14d', days: 14 },
  { label: '30 días', value: '30d', days: 30 },
]

// ─── Section heading ──────────────────────────────────────────────────────────
function SectionLabel({ label }: Readonly<{ label: string }>) {
  return (
    <div className="flex items-center gap-3">
      <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
        {label}
      </p>
      <div className="h-px flex-1 bg-border" />
    </div>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export function DashboardPage() {
  const summary = useDashboardSummary()
  useDashboardRealtimeInvalidation()

  const [dateFilter, setDateFilter] = useState<DateRangeFilter>('7d')
  const selectedDays = DATE_FILTERS.find((f) => f.value === dateFilter)?.days ?? 7

  const data = summary.data
  const chartData = buildChartData(
    data?.dailySales ?? [],
    data?.dailyPurchases ?? [],
    selectedDays,
  )

  return (
    <div className="flex min-h-full flex-col gap-8 bg-background p-6">

      {/* ── Header ───────────────────────────────────────────────────────── */}
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
            {formatDate()}
          </p>
          <h1 className="mt-1.5 text-2xl font-bold tracking-tight text-foreground">
            Resumen del día
          </h1>
          <p className="mt-1 text-[13.5px] text-muted-foreground">
            Ventas, facturas, cobros e inventario en tiempo real.
          </p>
        </div>

        <div className="flex items-center gap-2">
          {/* Date range filter — controls all trend charts */}
          <div className="flex overflow-hidden rounded-lg border border-border bg-card shadow-sm">
            {DATE_FILTERS.map((f) => (
              <button
                className={cn(
                  'px-3 py-1.5 text-[12px] font-medium transition-colors focus-visible:outline-none',
                  dateFilter === f.value
                    ? 'bg-primary text-primary-foreground'
                    : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                )}
                key={f.value}
                onClick={() => setDateFilter(f.value)}
                type="button"
              >
                {f.label}
              </button>
            ))}
          </div>

          {/* Refresh */}
          <button
            aria-label="Actualizar"
            className={cn(
              'flex h-9 items-center gap-2 rounded-lg border border-border bg-card px-3 text-[13px] font-medium text-foreground shadow-sm transition-colors',
              'hover:bg-muted',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/25',
              summary.isLoading && 'cursor-not-allowed opacity-50',
            )}
            disabled={summary.isLoading}
            onClick={() => summary.refetch()}
            type="button"
          >
            <RefreshCw
              aria-hidden="true"
              className={cn('shrink-0 text-muted-foreground', summary.isLoading && 'animate-spin')}
              size={14}
              strokeWidth={2}
            />
            Actualizar
          </button>
        </div>
      </header>

      {/* ── Error ────────────────────────────────────────────────────────── */}
      {summary.isError && (
        <div className="flex items-center gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-[13px] font-medium text-red-700">
          <AlertTriangle aria-hidden="true" className="shrink-0" size={15} />
          Error al cargar el resumen. Intenta actualizar.
        </div>
      )}

      {/* ── Sección 1: Resumen de hoy ─────────────────────────────────────── */}
      <section aria-label="Resumen de hoy">
        <SectionLabel label="Resumen de hoy" />
        <div className="mt-4">
          <DashboardKpiCards data={data} isLoading={summary.isLoading} />
        </div>
      </section>

      {/* ── Sección 2: Tendencias ─────────────────────────────────────────── */}
      <section aria-label="Tendencias">
        <SectionLabel label={`Tendencias · últimos ${selectedDays} días`} />
        <div className="mt-4 grid gap-4 xl:grid-cols-3">
          <SalesVsPurchasesChart data={chartData} isLoading={summary.isLoading} />
          <DailySalesBarChart data={chartData} isLoading={summary.isLoading} />
        </div>
      </section>

      {/* ── Sección 3: Análisis ───────────────────────────────────────────── */}
      <section aria-label="Análisis">
        <SectionLabel label="Análisis · últimos 30 días" />
        <div className="mt-4 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          <PaymentMethodChart data={data} isLoading={summary.isLoading} />
          <SaleStatusChart data={data} isLoading={summary.isLoading} />
          <AvgTicketCard data={data} isLoading={summary.isLoading} />
        </div>
      </section>

      {/* ── Sección 4: Actividad reciente ─────────────────────────────────── */}
      <section aria-label="Actividad reciente">
        <SectionLabel label="Actividad reciente" />
        <div className="mt-4">
          <DashboardTables data={data} isLoading={summary.isLoading} />
        </div>
      </section>

      {/* ── Alerta: cuentas por cobrar ────────────────────────────────────── */}
      {(data?.receivables.customerCount ?? 0) > 0 && (
        <div className="flex flex-col gap-3 rounded-2xl border border-amber-200 bg-amber-50 p-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-amber-100">
              <Users aria-hidden="true" className="text-amber-700" size={15} strokeWidth={2} />
            </div>
            <div>
              <p className="text-[13.5px] font-semibold text-amber-900">
                {data!.receivables.customerCount} cliente{data!.receivables.customerCount !== 1 ? 's' : ''} con saldo pendiente
              </p>
              <p className="text-[12.5px] text-amber-700">
                Total por cobrar: {formatMoney(data!.receivables.totalPending)}
              </p>
            </div>
          </div>
          <Link
            className="flex h-8 shrink-0 items-center gap-1.5 rounded-lg border border-amber-300 bg-white px-3 text-[12.5px] font-medium text-amber-800 transition-colors hover:bg-amber-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400/40"
            to="/reports"
          >
            Ver cuentas por cobrar
            <ArrowRight aria-hidden="true" size={13} />
          </Link>
        </div>
      )}

    </div>
  )
}
