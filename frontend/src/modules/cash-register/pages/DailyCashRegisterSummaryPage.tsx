import {
  Banknote,
  CalendarDays,
  CheckCircle,
  Clock,
  Loader2,
  Receipt,
  TrendingUp,
  Wallet,
} from 'lucide-react'
import { useState } from 'react'
import type { ReactNode } from 'react'
import { useBranches } from '@/modules/branches/hooks/useBranches'
import {
  useCashRegisterRealtimeInvalidation,
  useDailyCashRegisterSummary,
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

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('es-DO', { hour: '2-digit', minute: '2-digit' })
}

function todayString() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

// ─── Stat card ────────────────────────────────────────────────────────────────

type StatCardProps = {
  iconClass: string
  icon: ReactNode
  label: string
  value: string
  valueClass?: string
}

function StatCard({ iconClass, icon, label, value, valueClass }: Readonly<StatCardProps>) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4">
      <div className="mb-2.5 flex items-center gap-1.5">
        <span className={cn('shrink-0', iconClass)}>{icon}</span>
        <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">{label}</p>
      </div>
      <p className={cn('text-[16px] font-bold tabular-nums', valueClass ?? 'text-gray-900')}>{value}</p>
    </div>
  )
}

// ─── Status badge ─────────────────────────────────────────────────────────────

const statusClass: Record<string, string> = {
  Open:      'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Closed:    'bg-stone-100 text-stone-600 ring-stone-200',
  Cancelled: 'bg-red-50 text-red-700 ring-red-200',
}
const statusLabel: Record<string, string> = {
  Open: 'Abierta', Closed: 'Cerrada', Cancelled: 'Cancelada',
}

// ─── Difference badge ─────────────────────────────────────────────────────────

const diffClass: Record<string, string> = {
  Balanced: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Surplus:  'bg-amber-50 text-amber-700 ring-amber-200',
  Shortage: 'bg-red-50 text-red-700 ring-red-200',
}
const diffLabel: Record<string, string> = {
  Balanced: 'Cuadrado', Surplus: 'Sobrante', Shortage: 'Faltante',
}

// ─── Register row ─────────────────────────────────────────────────────────────

function RegisterRow({ reg }: Readonly<{ reg: DailyCashRegisterSummaryItem }>) {
  const diff    = reg.differenceType
  const diffAmt = reg.difference

  return (
    <tr className="bg-white transition-colors hover:bg-gray-50/60">
      <td className="whitespace-nowrap px-5 py-3 text-[12.5px] font-medium text-gray-500">
        {fmtTime(reg.openedAt)}
        {reg.closedAt && (
          <span className="ml-1.5 text-[11px] text-gray-400">→ {fmtTime(reg.closedAt)}</span>
        )}
      </td>
      <td className="px-5 py-3">
        <span className={cn(
          'inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold ring-1',
          statusClass[reg.status] ?? statusClass.Closed,
        )}>
          {statusLabel[reg.status] ?? reg.status}
        </span>
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] tabular-nums text-gray-500">
        {fmt(reg.openingAmount)}
      </td>
      <td className="px-5 py-3 text-right text-[13px] font-semibold tabular-nums text-gray-900">
        {fmt(reg.cashSalesTotal)}
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] tabular-nums text-gray-600">
        {reg.expectedCashAmount !== null ? fmt(reg.expectedCashAmount) : '—'}
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] tabular-nums text-gray-600">
        {reg.countedAmount !== null ? fmt(reg.countedAmount) : '—'}
      </td>
      <td className="px-5 py-3 text-right">
        {diff ? (
          <div className="flex flex-col items-end gap-0.5">
            <span className={cn(
              'inline-flex rounded-full px-2 py-0.5 text-[11px] font-semibold ring-1',
              diffClass[diff] ?? '',
            )}>
              {diffLabel[diff] ?? diff}
            </span>
            {diffAmt !== null && diffAmt !== 0 && (
              <span className={cn(
                'text-[11px] font-medium tabular-nums',
                diffAmt > 0 ? 'text-amber-600' : 'text-red-600',
              )}>
                {diffAmt > 0 ? '+' : ''}{fmt(diffAmt)}
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

export function DailyCashRegisterSummaryPage({ embedded = false }: Readonly<{ embedded?: boolean }> = {}) {
  useCashRegisterRealtimeInvalidation()
  const [date, setDate]                 = useState(todayString())
  const [selectedBranchId, setSelectedBranchId] = useState('')

  const { data: branchesData } = useBranches({ isActive: true })
  const branchList = branchesData?.items ?? []

  const { data: summary, isLoading, isError } = useDailyCashRegisterSummary({
    date,
    branchId: selectedBranchId || undefined,
  })

  const totalSales = summary
    ? summary.totalCashSales + summary.totalCardSales + summary.totalTransferSales + summary.totalCreditSales
    : 0
  const netDiff = summary?.totalDifference ?? 0

  return (
    <div className={cn('flex flex-col gap-6', embedded ? '' : 'overflow-y-auto p-6 lg:p-8')}>

      {/* Header */}
      {!embedded && (
        <header>
          <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
            Arqueo
          </p>
          <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">
            Arqueo diario de cajas
          </h1>
          <p className="mt-1 text-[13.5px] text-muted-foreground">
            Resumen de todas las cajas abiertas y cerradas del día.
          </p>
        </header>
      )}

      {/* Filters */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="border-b border-gray-100 px-5 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">Filtros</p>
        </div>
        <div className="grid gap-4 px-5 py-4 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="summary-date">Fecha</Label>
            <Input
              id="summary-date"
              max={todayString()}
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="summary-branch">Sucursal</Label>
            {branchList.length > 0 ? (
              <Select
                value={selectedBranchId || 'all'}
                onValueChange={(val) => setSelectedBranchId(val === 'all' ? '' : val)}
              >
                <SelectTrigger id="summary-branch">
                  <SelectValue placeholder="Todas las sucursales" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todas las sucursales</SelectItem>
                  {branchList.map((b) => (
                    <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <p className="pt-2.5 text-sm text-muted-foreground">Cargando sucursales...</p>
            )}
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
          Error al cargar el arqueo. Intenta nuevamente.
        </div>
      )}

      {/* Summary */}
      {summary && !isLoading && (
        <>
          {/* KPI grid */}
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
            <StatCard
              icon={<Clock size={14} />}
              iconClass="text-sky-500"
              label="Cajas abiertas"
              value={String(summary.openRegisters)}
            />
            <StatCard
              icon={<CheckCircle size={14} />}
              iconClass="text-stone-400"
              label="Cajas cerradas"
              value={String(summary.closedRegisters)}
            />
            <StatCard
              icon={<Wallet size={14} />}
              iconClass="text-blue-500"
              label="Ef. esperado"
              value={fmt(summary.totalExpectedCash)}
            />
            <StatCard
              icon={<Banknote size={14} />}
              iconClass="text-emerald-600"
              label="Ef. contado"
              value={fmt(summary.totalCountedCash)}
            />
            <StatCard
              icon={<Receipt size={14} />}
              iconClass="text-violet-500"
              label="Total ventas"
              value={fmt(totalSales)}
            />
            <StatCard
              icon={<TrendingUp size={14} />}
              iconClass={netDiff === 0 ? 'text-gray-400' : netDiff > 0 ? 'text-amber-500' : 'text-red-500'}
              label="Diferencia neta"
              value={(netDiff > 0 ? '+' : '') + fmt(netDiff)}
              valueClass={netDiff === 0 ? 'text-gray-900' : netDiff > 0 ? 'text-amber-600' : 'text-red-600'}
            />
          </div>

          {/* Sales by payment method */}
          <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
            <div className="border-b border-gray-100 px-5 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">
                Ventas del día por método de pago
              </p>
            </div>
            <div className="grid sm:grid-cols-4">
              {[
                { label: 'Efectivo',      value: summary.totalCashSales,     cls: 'text-emerald-700' },
                { label: 'Tarjeta',       value: summary.totalCardSales,     cls: 'text-blue-600'    },
                { label: 'Transferencia', value: summary.totalTransferSales, cls: 'text-violet-600'  },
                { label: 'Crédito',       value: summary.totalCreditSales,   cls: 'text-amber-600'   },
              ].map(({ label, value, cls }, i) => (
                <div
                  key={label}
                  className={cn(
                    'px-5 py-4',
                    i < 3 ? 'border-b border-gray-100 sm:border-b-0 sm:border-r' : '',
                  )}
                >
                  <p className="text-[11px] font-semibold uppercase tracking-[0.1em] text-gray-400">{label}</p>
                  <p className={cn('mt-1.5 text-[15px] font-bold tabular-nums', cls)}>{fmt(value)}</p>
                </div>
              ))}
            </div>
          </div>

          {/* Register table or empty state */}
          {summary.registers.length === 0 ? (
            <div className="flex h-40 items-center justify-center rounded-xl border border-gray-200 bg-white">
              <div className="flex items-center gap-2 text-gray-400">
                <CalendarDays size={18} />
                <p className="text-[13.5px]">No hay cajas registradas para esta fecha.</p>
              </div>
            </div>
          ) : (
            <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
              <div className="border-b border-gray-100 px-5 py-3">
                <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">
                  Detalle por caja · {summary.registers.length} registro{summary.registers.length !== 1 ? 's' : ''}
                </p>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="border-b border-gray-100 bg-gray-50/70">
                    <tr>
                      {[
                        { label: 'Horario',     right: false },
                        { label: 'Estado',      right: false },
                        { label: 'Apertura',    right: true  },
                        { label: 'Ventas ef.',  right: true  },
                        { label: 'Esperado',    right: true  },
                        { label: 'Contado',     right: true  },
                        { label: 'Diferencia',  right: true  },
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
                    {summary.registers.map((reg) => (
                      <RegisterRow key={reg.cashRegisterId} reg={reg} />
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}

      {/* Empty state (no query yet) */}
      {!summary && !isLoading && !isError && (
        <div className="flex h-40 items-center justify-center rounded-xl border border-gray-200 bg-white">
          <div className="flex items-center gap-2 text-gray-400">
            <CalendarDays size={18} />
            <p className="text-[13.5px]">Selecciona una fecha para ver el arqueo.</p>
          </div>
        </div>
      )}
    </div>
  )
}
