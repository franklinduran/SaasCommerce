import {
  ArrowDownLeft,
  ArrowUpRight,
  CalendarDays,
  CheckCircle,
  Clock,
  Loader2,
  Receipt,
  TrendingDown,
  TrendingUp,
  Wallet,
} from 'lucide-react'
import { useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import {
  useCashRegisterHistory,
  useCashRegisterRealtimeInvalidation,
} from '@/modules/cash-register/hooks/useCashRegister'
import type { DailyCashRegisterSummaryItem } from '@/modules/cash-register/types'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { cn } from '@/shared/utils/cn'

// ─── Helpers ──────────────────────────────────────────────────────────────────

const fmt = (n: number) =>
  new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(n)

function fmtDateTime(iso: string | null) {
  if (!iso) return '—'
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(iso))
}

function todayString() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

// ─── Stat card ────────────────────────────────────────────────────────────────

type StatCardProps = {
  icon: ReactNode
  iconClass: string
  label: string
  value: string
  valueClass?: string
  sub?: string
}

function StatCard({ icon, iconClass, label, sub, value, valueClass }: Readonly<StatCardProps>) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4">
      <div className="mb-2.5 flex items-center gap-1.5">
        <span className={cn('shrink-0', iconClass)}>{icon}</span>
        <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">{label}</p>
      </div>
      <p className={cn('text-[16px] font-bold tabular-nums', valueClass ?? 'text-gray-900')}>{value}</p>
      {sub && <p className="mt-0.5 text-[12px] text-gray-400">{sub}</p>}
    </div>
  )
}

// ─── Badge lookups ────────────────────────────────────────────────────────────

const statusClass: Record<string, string> = {
  Open:      'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Closed:    'bg-stone-100 text-stone-600 ring-stone-200',
  Cancelled: 'bg-red-50 text-red-700 ring-red-200',
}
const statusLabel: Record<string, string> = {
  Open: 'Abierta', Closed: 'Cerrada', Cancelled: 'Cancelada',
}

const diffClass: Record<string, string> = {
  Balanced: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Surplus:  'bg-amber-50  text-amber-700  ring-amber-200',
  Shortage: 'bg-red-50    text-red-700    ring-red-200',
}
const diffLabel: Record<string, string> = {
  Balanced: 'Cuadrado', Surplus: 'Sobrante', Shortage: 'Faltante',
}

// ─── Table row ────────────────────────────────────────────────────────────────

function HistoryRow({ item }: Readonly<{ item: DailyCashRegisterSummaryItem }>) {
  return (
    <tr className="bg-white transition-colors hover:bg-gray-50/60">
      <td className="whitespace-nowrap px-5 py-3 text-[12.5px] font-medium text-gray-700">
        {fmtDateTime(item.openedAt)}
      </td>
      <td className="whitespace-nowrap px-5 py-3 text-[12.5px] text-gray-500">
        {fmtDateTime(item.closedAt)}
      </td>
      <td className="px-5 py-3">
        <span className={cn(
          'inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold ring-1',
          statusClass[item.status] ?? statusClass.Closed,
        )}>
          {statusLabel[item.status] ?? item.status}
        </span>
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] tabular-nums text-gray-500">
        {fmt(item.openingAmount)}
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] font-semibold tabular-nums text-emerald-700">
        {fmt(item.cashSalesTotal)}
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] tabular-nums text-gray-600">
        {item.expectedCashAmount !== null ? fmt(item.expectedCashAmount) : '—'}
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] tabular-nums text-gray-600">
        {item.countedAmount !== null ? fmt(item.countedAmount) : '—'}
      </td>
      <td className="px-5 py-3 text-right">
        {item.differenceType ? (
          <div className="flex flex-col items-end gap-0.5">
            <span className={cn(
              'inline-flex rounded-full px-2 py-0.5 text-[11px] font-semibold ring-1',
              diffClass[item.differenceType] ?? '',
            )}>
              {diffLabel[item.differenceType] ?? item.differenceType}
            </span>
            {item.difference !== null && item.difference !== 0 && (
              <span className={cn(
                'text-[11px] font-medium tabular-nums',
                item.difference > 0 ? 'text-amber-600' : 'text-red-600',
              )}>
                {item.difference > 0 ? '+' : ''}{fmt(item.difference)}
              </span>
            )}
          </div>
        ) : (
          <span className="text-[12px] text-gray-400">—</span>
        )}
      </td>
    </tr>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function CashRegisterHistoryPage() {
  useCashRegisterRealtimeInvalidation()

  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo]     = useState('')
  const [status, setStatus]     = useState('')

  const { data, isLoading, isError } = useCashRegisterHistory({
    dateFrom: dateFrom || undefined,
    dateTo:   dateTo   || undefined,
    status:   status   || undefined,
    page:     1,
    pageSize: 100,
  })

  const items = data?.items ?? []

  const metrics = useMemo(() => {
    const closed    = items.filter((i) => i.status === 'Closed')
    const open      = items.filter((i) => i.status === 'Open')
    const balanced  = closed.filter((i) => i.differenceType === 'Balanced').length
    const totalCashSales = items.reduce((s, i) => s + i.cashSalesTotal, 0)
    const totalDiff      = closed.reduce((s, i) => s + (i.difference ?? 0), 0)
    return { closed: closed.length, open: open.length, balanced, totalCashSales, totalDiff }
  }, [items])

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">

      {/* Header */}
      <header>
        <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
          Caja
        </p>
        <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">
          Historial de cajas
        </h1>
        <p className="mt-1 text-[13.5px] text-muted-foreground">
          Consulta y filtra todos los turnos por rango de fecha y estado.
        </p>
      </header>

      {/* Filters */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="border-b border-gray-100 px-5 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">Filtros</p>
        </div>
        <div className="grid gap-4 px-5 py-4 sm:grid-cols-3">
          <div className="space-y-1.5">
            <Label htmlFor="hist-from">Desde</Label>
            <Input
              id="hist-from"
              max={todayString()}
              type="date"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="hist-to">Hasta</Label>
            <Input
              id="hist-to"
              max={todayString()}
              type="date"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="hist-status">Estado</Label>
            <Select
              value={status || 'all'}
              onValueChange={(v) => setStatus(v === 'all' ? '' : v)}
            >
              <SelectTrigger id="hist-status">
                <SelectValue placeholder="Todos los estados" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos los estados</SelectItem>
                <SelectItem value="Open">Abierta</SelectItem>
                <SelectItem value="Closed">Cerrada</SelectItem>
                <SelectItem value="Cancelled">Cancelada</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-gray-400" size={24} />
        </div>
      )}

      {/* Error */}
      {isError && !isLoading && (
        <div className="rounded-md bg-red-50 px-4 py-3 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          No se pudo cargar el historial. Intenta nuevamente.
        </div>
      )}

      {/* Results */}
      {!isLoading && !isError && items.length > 0 && (
        <>
          {/* Metrics */}
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
            <StatCard
              icon={<Receipt size={14} />}
              iconClass="text-gray-400"
              label="Total turnos"
              value={String(items.length)}
            />
            <StatCard
              icon={<CheckCircle size={14} />}
              iconClass="text-stone-400"
              label="Cerradas"
              value={String(metrics.closed)}
            />
            <StatCard
              icon={<Clock size={14} />}
              iconClass="text-emerald-500"
              label="Abiertas"
              value={String(metrics.open)}
            />
            <StatCard
              icon={<Wallet size={14} />}
              iconClass="text-emerald-600"
              label="Ventas efectivo"
              value={fmt(metrics.totalCashSales)}
            />
            <StatCard
              icon={metrics.totalDiff >= 0 ? <TrendingUp size={14} /> : <TrendingDown size={14} />}
              iconClass={metrics.totalDiff === 0 ? 'text-gray-400' : metrics.totalDiff > 0 ? 'text-amber-500' : 'text-red-500'}
              label="Diferencia neta"
              value={(metrics.totalDiff > 0 ? '+' : '') + fmt(metrics.totalDiff)}
              valueClass={metrics.totalDiff === 0 ? 'text-gray-900' : metrics.totalDiff > 0 ? 'text-amber-600' : 'text-red-600'}
            />
            <StatCard
              icon={<ArrowDownLeft size={14} />}
              iconClass="text-sky-500"
              label="Cuadradas"
              sub={metrics.closed > 0 ? `${Math.round((metrics.balanced / metrics.closed) * 100)}% del total` : undefined}
              value={String(metrics.balanced)}
            />
          </div>

          {/* Table */}
          <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
            <div className="border-b border-gray-100 px-5 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">
                Registros · {items.length} turno{items.length !== 1 ? 's' : ''}
              </p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="border-b border-gray-100 bg-gray-50/70">
                  <tr>
                    {[
                      { label: 'Apertura',    right: false },
                      { label: 'Cierre',      right: false },
                      { label: 'Estado',      right: false },
                      { label: 'Inicial',     right: true  },
                      { label: 'Ventas ef.',  right: true  },
                      { label: 'Esperado',    right: true  },
                      { label: 'Contado',     right: true  },
                      { label: 'Resultado',   right: true  },
                    ].map(({ label, right }) => (
                      <th
                        key={label}
                        className={cn(
                          'px-5 py-2.5 text-[11px] font-semibold uppercase tracking-[0.1em] text-gray-400',
                          right ? 'text-right' : 'text-left',
                        )}
                      >
                        {label}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {items.map((item) => (
                    <HistoryRow key={item.cashRegisterId} item={item} />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}

      {/* Empty state */}
      {!isLoading && !isError && items.length === 0 && (
        <div className="flex h-40 items-center justify-center rounded-xl border border-gray-200 bg-white">
          <div className="flex items-center gap-2 text-gray-400">
            <CalendarDays size={18} />
            <p className="text-[13.5px]">
              {dateFrom || dateTo || status
                ? 'No hay cajas para los filtros seleccionados.'
                : 'Selecciona un rango de fechas para ver el historial.'}
            </p>
          </div>
        </div>
      )}
    </div>
  )
}
