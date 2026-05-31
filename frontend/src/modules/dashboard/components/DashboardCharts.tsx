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
import type { DashboardSummary, ChartDataPoint } from '@/modules/dashboard/types'
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

// ─── 3. Donut: métodos de pago (por monto, 30 días) ──────────────────────────

type PayProps = { data: DashboardSummary | undefined; isLoading: boolean }

export function PaymentMethodChart({ data, isLoading }: Readonly<PayProps>) {
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
    <ChartCard subtitle="Distribución por monto (30 días)" title="Métodos de pago">
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

// ─── 4. Horizontal bar: estado de ventas (30 días) ────────────────────────────

type StatusProps = { data: DashboardSummary | undefined; isLoading: boolean }

export function SaleStatusChart({ data, isLoading }: Readonly<StatusProps>) {
  const breakdown = data?.saleStatusBreakdown
  const total = breakdown
    ? breakdown.completed + breakdown.cancelled + breakdown.pending + breakdown.failed + breakdown.other
    : 0

  const slices = breakdown && total > 0
    ? [
        { label: 'Completadas', value: breakdown.completed, color: P.emerald },
        { label: 'Pendientes',  value: breakdown.pending,   color: P.amber },
        { label: 'Canceladas',  value: breakdown.cancelled, color: P.red },
        { label: 'Fallidas',    value: breakdown.failed,    color: P.slate },
        ...(breakdown.other > 0
          ? [{ label: 'Otras', value: breakdown.other, color: P.muted }]
          : []),
      ].filter((s) => s.value > 0)
    : []

  const cfg = {
    labels: slices.map((s) => s.label),
    datasets: [{
      data: slices.map((s) => s.value),
      backgroundColor: slices.map((s) => s.color),
      borderRadius: 4,
      borderSkipped: false,
    }],
  }

  const opts: ChartOptions<'bar'> = {
    indexAxis: 'y' as const,
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        ...TIP,
        callbacks: {
          label: (ctx) => {
            const pct = total > 0 ? ((Number(ctx.parsed.x) / total) * 100).toFixed(1) : '0'
            return ` ${ctx.parsed.x} ventas (${pct}%)`
          },
        },
      },
    },
    scales: {
      x: {
        ...SCALE,
        ticks: { ...SCALE.ticks, stepSize: 1 },
      },
      y: { ...SCALE },
    },
  }

  return (
    <ChartCard subtitle="Distribución por estado (30 días)" title="Estado de ventas">
      {isLoading ? <Skeleton h={190} /> : slices.length === 0 ? <Empty h={190} /> : (
        <div style={{ height: 190 }}>
          <Bar data={cfg} options={opts} />
        </div>
      )}
    </ChartCard>
  )
}

// ─── 5. Avg ticket stat card ──────────────────────────────────────────────────

type AvgTicketProps = { data: DashboardSummary | undefined; isLoading: boolean }

export function AvgTicketCard({ data, isLoading }: Readonly<AvgTicketProps>) {
  const count = data?.salesToday.count ?? 0
  const total = data?.salesToday.totalAmount ?? 0
  const avg = count > 0 ? total / count : 0

  return (
    <div className="flex flex-col justify-between rounded-2xl bg-card p-5 shadow-sm ring-1 ring-border">
      <p className="text-[11.5px] font-semibold uppercase tracking-[0.08em] text-muted-foreground">
        Ticket promedio hoy
      </p>
      {isLoading ? (
        <div className="my-2 h-8 w-28 animate-pulse rounded-lg bg-muted" />
      ) : (
        <p className="mt-2 text-[1.75rem] font-bold leading-none tracking-tight text-foreground">
          {formatMoney(avg)}
        </p>
      )}
      <p className="mt-2 text-[12px] font-medium text-muted-foreground">
        {count} {count === 1 ? 'venta' : 'ventas'} hoy
      </p>
    </div>
  )
}
