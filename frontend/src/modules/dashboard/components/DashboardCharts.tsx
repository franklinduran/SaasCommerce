import {
  ArcElement,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Filler,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Tooltip,
  type ChartOptions,
} from 'chart.js'
import { Bar, Doughnut, Line } from 'react-chartjs-2'
import type { DashboardSummary, DashboardDailySalesPoint, ChartDataPoint, DashboardTopProduct } from '@/modules/dashboard/types'
import { formatMoney, formatMoneyShort } from '@/modules/dashboard/utils/dashboardFormat'
import { cn } from '@/shared/utils/cn'

ChartJS.register(ArcElement, BarElement, CategoryScale, Filler, Legend, LinearScale, LineElement, PointElement, Tooltip)

// ─── Palette ──────────────────────────────────────────────────────────────────
// Matches the project's charcoal + soft gray aesthetic (no bright colors)
const P = {
  ink:    '#1F2937', // gray-800   — primary dataset
  slate:  '#6B7280', // gray-500   — secondary dataset
  muted:  '#D1D5DB', // gray-300   — bars / tertiary
  line:   '#F3F4F6', // gray-100   — grid lines
  label:  '#9CA3AF', // gray-400   — axis labels
  white:  '#FFFFFF',
  border: '#E5E7EB', // gray-200
  // Status — semantic only
  emerald: '#059669',
  amber:   '#D97706',
  red:     '#DC2626',
  sky:     '#0284C7',
  donut:  ['#1F2937', '#4B5563', '#9CA3AF', '#D1D5DB', '#E5E7EB'],
}

// ─── Shared tooltip config ────────────────────────────────────────────────────
const TIP = {
  backgroundColor: P.white,
  borderColor: P.border,
  borderWidth: 1,
  bodyColor: '#374151',
  bodyFont: { size: 12, weight: 500 as const },
  titleColor: P.label,
  titleFont: { size: 11, weight: 600 as const },
  padding: 10,
  cornerRadius: 8,
  boxPadding: 4,
  displayColors: false,
}

// ─── Shared scale config ──────────────────────────────────────────────────────
const SCALE = {
  grid: { color: P.line, drawBorder: false },
  ticks: { color: P.label, font: { size: 11 } },
  border: { display: false },
}

// ─── Card wrapper ─────────────────────────────────────────────────────────────
function ChartCard({ children, className, subtitle, title }: Readonly<{
  children: React.ReactNode
  className?: string
  subtitle?: string
  title: string
}>) {
  return (
    <div className={cn('flex flex-col gap-3 rounded-2xl bg-card p-5 shadow-sm ring-1 ring-border', className)}>
      <div>
        <p className="text-[13px] font-semibold text-foreground">{title}</p>
        {subtitle && <p className="mt-0.5 text-[11px] text-muted-foreground">{subtitle}</p>}
      </div>
      {children}
    </div>
  )
}

function Skeleton({ h }: Readonly<{ h: number }>) {
  return <div className="w-full animate-pulse rounded-lg bg-muted" style={{ height: h }} />
}

function Empty({ h = 180 }: Readonly<{ h?: number }>) {
  return (
    <div className="flex items-center justify-center text-[12px] text-muted-foreground" style={{ height: h }}>
      Sin datos aún
    </div>
  )
}

// ─── 1. Line: ventas vs compras ───────────────────────────────────────────────

type TrendProps = { data: ChartDataPoint[]; isLoading: boolean }

export function SalesVsPurchasesChart({ data, isLoading }: Readonly<TrendProps>) {
  const hasData = data.some((d) => d.ventas > 0 || d.gastos > 0)

  const cfg = {
    labels: data.map((d) => d.label),
    datasets: [
      {
        label: 'Ventas',
        data: data.map((d) => d.ventas),
        borderColor: P.ink,
        backgroundColor: `${P.ink}12`,
        borderWidth: 1.5,
        fill: true,
        tension: 0.4,
        pointRadius: 0,
        pointHoverRadius: 4,
        pointHoverBackgroundColor: P.ink,
        pointHoverBorderColor: P.white,
        pointHoverBorderWidth: 2,
      },
      {
        label: 'Compras',
        data: data.map((d) => d.gastos),
        borderColor: P.slate,
        backgroundColor: 'transparent',
        borderWidth: 1.5,
        fill: false,
        tension: 0.4,
        pointRadius: 0,
        pointHoverRadius: 4,
        pointHoverBackgroundColor: P.slate,
        pointHoverBorderColor: P.white,
        pointHoverBorderWidth: 2,
        borderDash: [4, 3],
      },
    ],
  }

  const opts: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    interaction: { mode: 'index', intersect: false },
    plugins: {
      legend: {
        position: 'top',
        align: 'end',
        labels: {
          boxWidth: 12,
          boxHeight: 2,
          borderRadius: 0,
          useBorderRadius: false,
          color: P.slate,
          font: { size: 11, weight: 500 },
          padding: 12,
        },
      },
      tooltip: {
        ...TIP,
        callbacks: {
          label: (ctx) => ` ${ctx.dataset.label}  ${formatMoneyShort(Number(ctx.parsed.y))}`,
        },
      },
    },
    scales: {
      x: { ...SCALE },
      y: {
        ...SCALE,
        ticks: { ...SCALE.ticks, callback: (v) => formatMoneyShort(Number(v)) },
      },
    },
  }

  return (
    <ChartCard className="xl:col-span-2" subtitle="Ingresos por ventas vs gasto en compras" title="Ventas vs Compras">
      {isLoading ? <Skeleton h={200} /> : !hasData ? <Empty h={200} /> : (
        <div style={{ height: 200 }}>
          <Line data={cfg} options={opts} />
        </div>
      )}
    </ChartCard>
  )
}

// ─── 2. Bar: ventas diarias ───────────────────────────────────────────────────

export function DailySalesBarChart({ data, isLoading }: Readonly<TrendProps>) {
  const hasData = data.some((d) => d.ventas > 0)

  const cfg = {
    labels: data.map((d) => d.label),
    datasets: [{
      label: 'Ventas',
      data: data.map((d) => d.ventas),
      backgroundColor: data.map((_, i) =>
        i === data.length - 1 ? P.ink : P.muted
      ),
      borderRadius: 4,
      borderSkipped: false,
    }],
  }

  const opts: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        ...TIP,
        callbacks: { label: (ctx) => ` ${formatMoneyShort(Number(ctx.parsed.y))}` },
      },
    },
    scales: {
      x: { ...SCALE },
      y: {
        ...SCALE,
        ticks: { ...SCALE.ticks, callback: (v) => formatMoneyShort(Number(v)) },
      },
    },
  }

  return (
    <ChartCard subtitle="Monto diario (barras)" title="Ventas por día">
      {isLoading ? <Skeleton h={190} /> : !hasData ? <Empty h={190} /> : (
        <div style={{ height: 190 }}>
          <Bar data={cfg} options={opts} />
        </div>
      )}
    </ChartCard>
  )
}

// ─── 3. Donut: métodos de pago ────────────────────────────────────────────────

type PayProps = { data: DashboardSummary | undefined; days: number; isLoading: boolean }

export function PaymentMethodChart({ data, days, isLoading }: Readonly<PayProps>) {
  const slices = data?.paymentMethodTotals ?? []

  const cfg = {
    labels: slices.map((s) => s.method),
    datasets: [{
      data: slices.map((s) => s.total),
      backgroundColor: P.donut.slice(0, slices.length),
      borderWidth: 0,
      hoverOffset: 3,
    }],
  }

  const opts = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: {
      legend: { display: false },
      tooltip: {
        ...TIP,
        callbacks: {
          label: (ctx: { label: string; parsed: number }) =>
            ` ${ctx.label}: ${formatMoneyShort(ctx.parsed)}`,
        },
      },
    },
  }

  return (
    <ChartCard subtitle={`Distribución por monto · ${days} días`} title="Métodos de pago">
      {isLoading ? <Skeleton h={190} /> : slices.length === 0 ? <Empty h={190} /> : (
        <div className="flex items-center gap-5">
          <div className="shrink-0" style={{ height: 150, width: 150 }}>
            <Doughnut data={cfg} options={opts} />
          </div>
          <ul className="flex min-w-0 flex-1 flex-col gap-2.5">
            {slices.map((s, i) => (
              <li className="flex items-center justify-between gap-2 text-[12px]" key={s.method}>
                <span className="flex min-w-0 items-center gap-2">
                  <span className="h-1.5 w-1.5 shrink-0 rounded-full" style={{ background: P.donut[i] }} />
                  <span className="truncate text-muted-foreground">{s.method}</span>
                </span>
                <span className="font-semibold text-foreground">{formatMoneyShort(s.total)}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </ChartCard>
  )
}

// ─── 4. Salud de ventas: tasa de cierre + desglose de excepciones ─────────────

type StatusProps = { data: DashboardSummary | undefined; days: number; isLoading: boolean }

export function SaleStatusChart({ data, days, isLoading }: Readonly<StatusProps>) {
  const breakdown = data?.saleStatusBreakdown
  const total = breakdown
    ? breakdown.completed + breakdown.cancelled + breakdown.pending + breakdown.failed + breakdown.other
    : 0
  const rate = total > 0 ? (breakdown!.completed / total) * 100 : 0

  const exceptions = breakdown && total > 0
    ? [
        { label: 'Canceladas',   value: breakdown.cancelled, color: P.red    },
        { label: 'Fallidas',     value: breakdown.failed,    color: P.slate  },
        { label: 'En proceso',   value: breakdown.other,     color: P.amber  },
      ].filter((s) => s.value > 0)
    : []

  return (
    <ChartCard subtitle={`Últimos ${days} días`} title="Salud de ventas">
      {isLoading ? (
        <Skeleton h={190} />
      ) : total === 0 ? (
        <Empty h={190} />
      ) : (
        <div className="flex flex-col gap-4">
          {/* Tasa de cierre */}
          <div className="flex items-end gap-3">
            <p
              className="text-[2.1rem] font-bold leading-none tracking-tight"
              style={{ color: rate >= 80 ? P.emerald : rate >= 60 ? P.amber : P.red }}
            >
              {rate.toFixed(1)}%
            </p>
            <div className="mb-0.5">
              <p className="text-[11.5px] font-semibold text-foreground">tasa de cierre</p>
              <p className="text-[11px] text-muted-foreground">
                {breakdown!.completed.toLocaleString()} de {total.toLocaleString()} ventas
              </p>
            </div>
          </div>

          {/* Barra de progreso */}
          <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
            <div
              className="h-full rounded-full transition-all"
              style={{
                width: `${rate}%`,
                background: rate >= 80 ? P.emerald : rate >= 60 ? P.amber : P.red,
              }}
            />
          </div>

          {/* Desglose de excepciones */}
          {exceptions.length > 0 ? (
            <ul className="flex flex-col gap-2">
              {exceptions.map((s) => {
                const pct = ((s.value / total) * 100).toFixed(1)
                return (
                  <li key={s.label} className="flex items-center justify-between gap-2 text-[12px]">
                    <span className="flex items-center gap-2 text-muted-foreground">
                      <span className="h-1.5 w-1.5 shrink-0 rounded-full" style={{ background: s.color }} />
                      {s.label}
                    </span>
                    <span className="flex items-center gap-2">
                      <span className="font-semibold text-foreground">{s.value}</span>
                      <span
                        className="rounded px-1.5 py-0.5 text-[10.5px] font-semibold"
                        style={{ background: `${s.color}18`, color: s.color }}
                      >
                        {pct}%
                      </span>
                    </span>
                  </li>
                )
              })}
            </ul>
          ) : (
            <p className="text-[12px] font-medium text-emerald-600">
              Sin excepciones en el período ✓
            </p>
          )}
        </div>
      )}
    </ChartCard>
  )
}

// ─── 5. Line: ticket promedio por día ─────────────────────────────────────────

type AvgTicketTrendProps = {
  dailySales: DashboardDailySalesPoint[]
  isLoading: boolean
  days: number
}

export function AvgTicketTrendChart({ dailySales, isLoading, days }: Readonly<AvgTicketTrendProps>) {
  // Slice to the requested window and compute per-day avg ticket
  const points = (() => {
    const cutoff = new Date()
    cutoff.setDate(cutoff.getDate() - days)
    return dailySales
      .filter((d) => new Date(d.date) >= cutoff)
      .map((d) => ({
        label: new Intl.DateTimeFormat('es-DO', days <= 7 ? { weekday: 'short' } : { day: '2-digit', month: '2-digit' }).format(new Date(d.date + 'T12:00:00')),
        avg: d.saleCount > 0 ? d.totalSales / d.saleCount : 0,
      }))
  })()

  const hasData = points.some((p) => p.avg > 0)

  const cfg = {
    labels: points.map((p) => p.label),
    datasets: [{
      label: 'Ticket promedio',
      data: points.map((p) => p.avg),
      borderColor: P.ink,
      backgroundColor: `${P.ink}10`,
      borderWidth: 1.5,
      fill: true,
      tension: 0.4,
      pointRadius: 0,
      pointHoverRadius: 4,
      pointHoverBackgroundColor: P.ink,
      pointHoverBorderColor: P.white,
      pointHoverBorderWidth: 2,
    }],
  }

  const opts: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    interaction: { mode: 'index', intersect: false },
    plugins: {
      legend: { display: false },
      tooltip: {
        ...TIP,
        callbacks: { label: (ctx) => ` ${formatMoneyShort(Number(ctx.parsed.y))} / venta` },
      },
    },
    scales: {
      x: { ...SCALE },
      y: {
        ...SCALE,
        ticks: { ...SCALE.ticks, callback: (v) => formatMoneyShort(Number(v)) },
      },
    },
  }

  // Compute overall trend: last point vs first point
  const first = points.find((p) => p.avg > 0)?.avg ?? 0
  const last  = [...points].reverse().find((p) => p.avg > 0)?.avg ?? 0
  const trend = first > 0 ? ((last - first) / first) * 100 : 0

  return (
    <ChartCard subtitle={`Tendencia últimos ${days} días`} title="Ticket promedio">
      {isLoading ? <Skeleton h={190} /> : !hasData ? <Empty h={190} /> : (
        <div className="flex flex-col gap-3">
          {/* Delta badge */}
          <div className="flex items-center gap-2">
            <span className="text-[1.25rem] font-bold leading-none tracking-tight text-foreground">
              {formatMoney(last)}
            </span>
            {trend !== 0 && (
              <span
                className="rounded px-1.5 py-0.5 text-[10.5px] font-semibold"
                style={{
                  background: trend > 0 ? `${P.emerald}18` : `${P.red}18`,
                  color: trend > 0 ? P.emerald : P.red,
                }}
              >
                {trend > 0 ? '▲' : '▼'} {Math.abs(trend).toFixed(1)}%
              </span>
            )}
          </div>
          <div style={{ height: 140 }}>
            <Line data={cfg} options={opts} />
          </div>
        </div>
      )}
    </ChartCard>
  )
}

// ─── 6. Top 5 productos por revenue ──────────────────────────────────────────

type TopProductsProps = { products: DashboardTopProduct[]; days: number; isLoading: boolean }

export function TopProductsChart({ products, days, isLoading }: Readonly<TopProductsProps>) {
  const maxRevenue = products[0]?.totalRevenue ?? 1

  return (
    <ChartCard subtitle={`Por ingresos · últimos ${days} días`} title="Top productos">
      {isLoading ? (
        <Skeleton h={190} />
      ) : products.length === 0 ? (
        <Empty h={190} />
      ) : (
        <ul className="flex flex-col gap-3">
          {products.map((p, i) => {
            const pct = (p.totalRevenue / maxRevenue) * 100
            return (
              <li key={p.name} className="flex flex-col gap-1">
                <div className="flex items-center justify-between gap-2 text-[12px]">
                  <span className="flex min-w-0 items-center gap-1.5 text-muted-foreground">
                    <span
                      className="shrink-0 text-[10px] font-bold tabular-nums"
                      style={{ color: P.label }}
                    >
                      {i + 1}
                    </span>
                    <span className="truncate">{p.name}</span>
                  </span>
                  <span className="shrink-0 font-semibold text-foreground">
                    {formatMoneyShort(p.totalRevenue)}
                  </span>
                </div>
                <div className="h-1 w-full overflow-hidden rounded-full bg-muted">
                  <div
                    className="h-full rounded-full"
                    style={{ width: `${pct}%`, background: ['#1F2937','#374151','#4B5563','#6B7280','#9CA3AF'][i] }}
                  />
                </div>
              </li>
            )
          })}
        </ul>
      )}
    </ChartCard>
  )
}

