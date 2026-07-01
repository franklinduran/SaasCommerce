import { useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  ArrowDownLeft,
  ArrowUpRight,
  Banknote,
  CheckCircle,
  ChevronDown,
  Clock,
  Loader2,
  Receipt,
  TrendingUp,
  Wallet,
  X,
} from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import {
  useCashSessionRealtimeInvalidation,
  useCloseCashSession,
  useCurrentCashSession,
  useOpenCashSession,
  useRegisterCashMovement,
} from '@/modules/cash/hooks/useCash'
import type { CashClosingResult, CashSession } from '@/modules/cash/types'
import { getSales } from '@/modules/sales/services/salesApi'
import type { PaymentMethodFilter, SaleStatus } from '@/modules/sales/types/salesTypes'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Textarea } from '@/shared/components/ui/textarea'
import { HttpClientError } from '@/shared/services/httpClient'
import { cn } from '@/shared/utils/cn'

// ─── Formatters ───────────────────────────────────────────────────────────────

function fmt(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function fmtDate(iso: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(iso))
}

function fmtTime(iso: string) {
  return new Intl.DateTimeFormat('es-DO', { hour: '2-digit', minute: '2-digit' }).format(new Date(iso))
}

function elapsed(openedAt: string) {
  const ms = Date.now() - new Date(openedAt).getTime()
  const h  = Math.floor(ms / 3_600_000)
  const m  = Math.floor((ms % 3_600_000) / 60_000)
  return h > 0 ? `${h}h ${m}min` : `${m}min`
}

// ─── Shared helpers ──────────────────────────────────────────────────────────

function FormError({ message }: Readonly<{ message: string }>) {
  return (
    <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
      {message}
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Root
// ─────────────────────────────────────────────────────────────────────────────

export function CashPage() {
  const { data: currentSession, isLoading } = useCurrentCashSession()
  useCashSessionRealtimeInvalidation()
  const navigate = useNavigate()

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="animate-spin text-gray-400" size={28} />
      </div>
    )
  }

  return currentSession
    ? <ActiveSessionPanel session={currentSession} onViewHistory={() => navigate('/cash/history')} />
    : <OpenSessionPanel />
}

// ─────────────────────────────────────────────────────────────────────────────
// Abrir sesión
// ─────────────────────────────────────────────────────────────────────────────

function OpenSessionPanel() {
  const openSession = useOpenCashSession()
  const [openingBalance, setOpeningBalance] = useState('')
  const [notes, setNotes]                   = useState('')
  const [error, setError]                   = useState<string | null>(null)

  async function handleOpen(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const balance = parseFloat(openingBalance)
    if (isNaN(balance) || balance < 0) { setError('El balance inicial debe ser 0 o mayor.'); return }
    try {
      await openSession.mutateAsync({ openingBalance: balance, notes: notes.trim() || null })
    } catch (err) {
      setError(err instanceof HttpClientError ? (err.error?.message ?? 'No se pudo abrir la caja.') : 'No se pudo abrir la caja.')
    }
  }

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">
      <header>
        <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
          Caja
        </p>
        <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">
          Apertura de caja
        </h1>
        <p className="mt-1 text-[13.5px] text-muted-foreground">
          No hay un turno activo para esta sucursal.
        </p>
      </header>

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="border-b border-gray-100 px-5 py-3.5">
          <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">
            Iniciar turno
          </p>
        </div>
        <div className="max-w-sm px-5 py-6">
          <form className="space-y-4" onSubmit={(e) => void handleOpen(e)}>
            <div className="space-y-1.5">
              <Label htmlFor="ob">Balance de apertura (RD$)</Label>
              <Input id="ob" min="0" placeholder="0.00" step="0.01" type="number" value={openingBalance} onChange={(e) => setOpeningBalance(e.target.value)} />
              <p className="text-[11px] text-muted-foreground">
                Monto en efectivo disponible para dar cambio al iniciar el turno.
              </p>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="notes">Notas (opcional)</Label>
              <Textarea id="notes" placeholder="Observaciones de apertura..." rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
            </div>
            {error && <FormError message={error} />}
            <Button className="w-full" disabled={openSession.isPending} type="submit">
              {openSession.isPending ? <><Loader2 className="animate-spin" size={15} />Abriendo...</> : 'Abrir caja'}
            </Button>
          </form>
        </div>
      </div>
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Historial entry types
// ─────────────────────────────────────────────────────────────────────────────

type HistorialEntry = {
  id: string
  description: string
  amount: number
  kind: 'opening' | 'CashIn' | 'CashOut'
  createdAt: string
  runningBalance: number
  isSale: boolean
}

type SalesGroupEntry = {
  id: string
  kind: 'sales-group'
  entries: HistorialEntry[]
  totalAmount: number
  count: number
  runningBalance: number
}

type HistorialItem = HistorialEntry | SalesGroupEntry

// ─────────────────────────────────────────────────────────────────────────────
// Sesión activa
// ─────────────────────────────────────────────────────────────────────────────

type ActiveSessionPanelProps = {
  session: CashSession
  onViewHistory: () => void
}

type ActiveForm = 'ingreso' | 'salida' | 'cierre' | null

function ActiveSessionPanel({ session, onViewHistory }: Readonly<ActiveSessionPanelProps>) {
  const [activeForm, setActiveForm] = useState<ActiveForm>(null)
  const [closeResult, setCloseResult] = useState<CashClosingResult | null>(null)

  const dateFrom = session.openedAt.slice(0, 10)
  const dateTo   = session.closedAt?.slice(0, 10) ?? new Date().toISOString().slice(0, 10)

  const { data: salesData } = useQuery({
    queryKey: ['cash', 'session-sales', session.id, dateFrom, dateTo],
    queryFn: () =>
      getSales({
        page: 1, pageSize: 50,
        paymentMethod: 'Cash' as PaymentMethodFilter,
        dateFrom, dateTo,
        status: 'Completed' as '' | SaleStatus,
        query: '',
      }),
    staleTime: 60_000,
    refetchInterval: 60_000,
  })

  const cashSales = salesData?.items ?? []

  const { historialItems, currentBalance, metrics } = useMemo(() => {
    const linkedCodes = new Set(
      session.movements
        .filter((m) => m.description.startsWith('Venta #'))
        .map((m) => m.description.slice(7)),
    )
    const unlinkedSales = cashSales.filter((s) => !linkedCodes.has(s.code))

    type RawEntry = { id: string; description: string; amount: number; type: 'CashIn' | 'CashOut'; createdAt: string; isSale: boolean }

    const combined: RawEntry[] = [
      ...session.movements.map((m): RawEntry => ({
        id: m.id, description: m.description, amount: m.amount,
        type: m.type, createdAt: m.createdAt, isSale: m.description.startsWith('Venta #'),
      })),
      ...unlinkedSales.map((s): RawEntry => ({
        id: `sale-${s.id}`, description: `Venta #${s.code}`, amount: s.total,
        type: 'CashIn', createdAt: s.createdAt, isSale: true,
      })),
    ].sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())

    let balance = session.openingBalance
    let salesTotal = 0, salesCount = 0, manualInTotal = 0, outTotal = 0

    const openingItem: HistorialEntry = {
      id: '__opening__', description: 'Apertura de caja',
      amount: session.openingBalance, kind: 'opening',
      createdAt: session.openedAt, runningBalance: balance, isSale: false,
    }

    const saleEntries: HistorialEntry[] = []
    const nonSaleEntries: HistorialEntry[] = []

    for (const raw of combined) {
      balance = raw.type === 'CashIn' ? balance + raw.amount : balance - raw.amount
      const entry: HistorialEntry = {
        id: raw.id, description: raw.description, amount: raw.amount,
        kind: raw.type, createdAt: raw.createdAt, runningBalance: balance, isSale: raw.isSale,
      }
      if (raw.isSale) {
        saleEntries.push(entry)
        salesTotal += raw.amount; salesCount++
      } else {
        nonSaleEntries.push(entry)
        if (raw.type === 'CashOut') outTotal += raw.amount
        else manualInTotal += raw.amount
      }
    }

    const historialItems: HistorialItem[] = [openingItem, ...nonSaleEntries]
    if (saleEntries.length > 0) {
      historialItems.push({
        id: '__sales-group__',
        kind: 'sales-group',
        entries: saleEntries,
        totalAmount: salesTotal,
        count: saleEntries.length,
        runningBalance: saleEntries[saleEntries.length - 1].runningBalance,
      })
    }

    return {
      historialItems,
      currentBalance: balance,
      metrics: { salesTotal, salesCount, manualInTotal, outTotal, netChange: balance - session.openingBalance },
    }
  }, [session, cashSales])

  function toggleForm(form: ActiveForm) {
    setActiveForm((prev) => (prev === form ? null : form))
  }

  if (closeResult) {
    return <CloseResultPanel result={closeResult} onViewHistory={onViewHistory} />
  }

  const movementsCount = historialItems.reduce((sum, item) =>
    item.kind === 'sales-group' ? sum + item.count : item.kind !== 'opening' ? sum + 1 : sum, 0)

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">

      {/* ── Header ──────────────────────────────────────────────────── */}
      <div className="flex items-start justify-between gap-6">
        <header>
          <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
            Caja
          </p>
          <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">
            Turno activo
          </h1>
          <div className="mt-1.5 flex flex-wrap items-center gap-2 text-[13.5px] text-muted-foreground">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-2 py-0.5 text-[11.5px] font-semibold text-emerald-700 ring-1 ring-emerald-200">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              Abierta
            </span>
            <span className="flex items-center gap-1.5">
              <Clock size={12} />
              {elapsed(session.openedAt)} activo · desde {fmtDate(session.openedAt)}
            </span>
          </div>
        </header>

        <div className="flex shrink-0 flex-wrap items-center gap-2 pt-1">
          <Button
            className={cn(
              'h-9 gap-1.5 ring-1',
              activeForm === 'ingreso'
                ? 'bg-emerald-600 text-white ring-emerald-700 hover:bg-emerald-700'
                : 'bg-emerald-50 text-emerald-700 ring-emerald-200 hover:bg-emerald-100',
            )}
            size="sm"
            variant="ghost"
            onClick={() => toggleForm('ingreso')}
          >
            <ArrowDownLeft size={14} />
            Ingreso
          </Button>
          <Button
            className={cn(
              'h-9 gap-1.5 ring-1',
              activeForm === 'salida'
                ? 'bg-red-600 text-white ring-red-700 hover:bg-red-700'
                : 'bg-red-50 text-red-700 ring-red-200 hover:bg-red-100',
            )}
            size="sm"
            variant="ghost"
            onClick={() => toggleForm('salida')}
          >
            <ArrowUpRight size={14} />
            Salida
          </Button>
          <Button size="sm" variant="ghost" onClick={onViewHistory}>
            Historial
          </Button>
          <Button size="sm" variant="outline" onClick={() => toggleForm('cierre')}>
            <X size={13} />
            Cerrar caja
          </Button>
        </div>
      </div>

      {/* ── Métricas ─────────────────────────────────────────────────── */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
        <StatCard
          icon={<Wallet size={14} />} iconClass="text-gray-400"
          label="Balance actual" value={fmt(currentBalance)}
        />
        <StatCard
          icon={<Banknote size={14} />} iconClass="text-gray-400"
          label="Apertura" value={fmt(session.openingBalance)}
        />
        <StatCard
          icon={<Receipt size={14} />} iconClass="text-emerald-600"
          label="Ventas efectivo" value={fmt(metrics.salesTotal)}
          sub={metrics.salesCount > 0 ? `${metrics.salesCount} venta${metrics.salesCount !== 1 ? 's' : ''}` : 'Sin ventas aún'}
        />
        <StatCard
          icon={<ArrowDownLeft size={14} />} iconClass="text-sky-500"
          label="Ingresos manuales" value={fmt(metrics.manualInTotal)}
        />
        <StatCard
          icon={<ArrowUpRight size={14} />} iconClass="text-red-500"
          label="Salidas / gastos" value={fmt(metrics.outTotal)}
        />
        <StatCard
          icon={<TrendingUp size={14} />}
          iconClass={metrics.netChange >= 0 ? 'text-emerald-600' : 'text-red-500'}
          label="Neto del turno"
          value={(metrics.netChange >= 0 ? '+' : '') + fmt(metrics.netChange)}
          valueClass={metrics.netChange > 0 ? 'text-emerald-700' : metrics.netChange < 0 ? 'text-red-600' : undefined}
        />
      </div>

      {/* ── Formularios ──────────────────────────────────────────────── */}
      {activeForm === 'ingreso' && (
        <MovementForm label="Registrar ingreso de efectivo" type="CashIn" sessionId={session.id} onClose={() => setActiveForm(null)} />
      )}
      {activeForm === 'salida' && (
        <MovementForm label="Registrar salida / gasto" type="CashOut" sessionId={session.id} onClose={() => setActiveForm(null)} />
      )}
      {activeForm === 'cierre' && (
        <CloseSessionForm currentBalance={currentBalance} session={session} onClose={() => setActiveForm(null)} onSuccess={(r) => setCloseResult(r)} />
      )}

      {/* ── Historial ────────────────────────────────────────────────── */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">

        {/* Info bar */}
        <div className="flex flex-wrap items-center gap-x-5 gap-y-1.5 border-b border-gray-100 bg-gray-50/50 px-5 py-3">
          <span className="flex items-center gap-1.5 text-[12.5px] text-gray-500">
            <Banknote size={13} className="shrink-0 text-gray-400" />
            Apertura <span className="font-semibold text-gray-700">{fmt(session.openingBalance)}</span>
          </span>
          <span className="flex items-center gap-1.5 text-[12.5px] text-gray-500">
            <Clock size={13} className="shrink-0 text-gray-400" />
            {fmtDate(session.openedAt)}
          </span>
          {session.notes && (
            <span className="text-[12px] italic text-gray-400">"{session.notes}"</span>
          )}
          <span className="ml-auto text-[12px] text-gray-400">
            {movementsCount} movimiento{movementsCount !== 1 ? 's' : ''}
          </span>
        </div>

        {/* Tabla */}
        <div className="overflow-x-auto">
          <table className="w-full min-w-[580px] text-left">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/70">
                {(['Hora', 'Tipo', 'Descripción', 'Monto', 'Balance'] as const).map((col, i) => (
                  <th
                    key={col}
                    className={cn(
                      'px-5 py-2.5 text-[11px] font-semibold uppercase tracking-[0.1em] text-gray-400',
                      i >= 3 && 'text-right',
                    )}
                  >
                    {col}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {historialItems.map((item) =>
                item.kind === 'sales-group'
                  ? <SalesGroupRow key={item.id} group={item} />
                  : <HistorialRow key={item.id} entry={item} />,
              )}
            </tbody>
          </table>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between border-t-2 border-gray-200 bg-gray-50/70 px-5 py-3.5">
          <span className="text-[13px] font-semibold text-gray-600">Total neto en caja</span>
          <span className="text-[14px] font-bold tabular-nums text-gray-900">{fmt(currentBalance)}</span>
        </div>
      </div>
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Stat card
// ─────────────────────────────────────────────────────────────────────────────

type StatCardProps = {
  icon: ReactNode
  iconClass: string
  label: string
  sub?: string
  value: string
  valueClass?: string
}

function StatCard({ icon, iconClass, label, sub, value, valueClass }: Readonly<StatCardProps>) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4">
      <div className="mb-2.5 flex items-center gap-1.5">
        <span className={cn('shrink-0', iconClass)}>{icon}</span>
        <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">{label}</p>
      </div>
      <p className={cn('text-[16px] font-bold tabular-nums text-gray-900', valueClass)}>{value}</p>
      {sub && <p className="mt-0.5 text-[12px] text-gray-400">{sub}</p>}
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Kind badge (like SaleStatusBadge)
// ─────────────────────────────────────────────────────────────────────────────

const kindBadgeClass: Record<HistorialEntry['kind'], string> = {
  opening: 'bg-stone-100 text-stone-600 ring-stone-200',
  CashIn:  'bg-emerald-50 text-emerald-700 ring-emerald-200',
  CashOut: 'bg-red-50 text-red-700 ring-red-200',
}

const kindBadgeLabel: Record<HistorialEntry['kind'], string> = {
  opening: 'Apertura',
  CashIn:  'Ingreso',
  CashOut: 'Salida',
}

function KindBadge({ kind }: Readonly<{ kind: HistorialEntry['kind'] }>) {
  return (
    <span className={cn('inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold ring-1', kindBadgeClass[kind])}>
      {kindBadgeLabel[kind]}
    </span>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Fila del historial
// ─────────────────────────────────────────────────────────────────────────────

function HistorialRow({ entry }: Readonly<{ entry: HistorialEntry }>) {
  const isOut   = entry.kind === 'CashOut'
  const isOpen  = entry.kind === 'opening'
  const amountStr   = isOpen ? fmt(entry.amount) : isOut ? `−${fmt(entry.amount)}` : `+${fmt(entry.amount)}`
  const amountClass = isOpen ? 'text-gray-600' : isOut ? 'text-red-600' : 'text-emerald-700'

  return (
    <tr className="bg-white transition-colors hover:bg-gray-50/60">
      <td className="whitespace-nowrap px-5 py-3 text-[12.5px] font-medium text-gray-500">
        {fmtTime(entry.createdAt)}
      </td>
      <td className="px-5 py-3">
        <KindBadge kind={entry.kind} />
      </td>
      <td className="px-5 py-3">
        <p className="text-[13px] font-medium text-gray-900">{entry.description}</p>
        {entry.isSale && (
          <p className="text-[11px] text-gray-400">Venta en efectivo</p>
        )}
      </td>
      <td className={cn('px-5 py-3 text-right text-[13px] font-semibold tabular-nums', amountClass)}>
        {amountStr}
      </td>
      <td className="px-5 py-3 text-right text-[12.5px] font-medium tabular-nums text-gray-500">
        {fmt(entry.runningBalance)}
      </td>
    </tr>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Grupo de ventas (colapsable)
// ─────────────────────────────────────────────────────────────────────────────

function SalesGroupRow({ group }: Readonly<{ group: SalesGroupEntry }>) {
  const [expanded, setExpanded] = useState(false)
  const first = group.entries[0]
  const last  = group.entries[group.entries.length - 1]
  const timeRange = first.createdAt === last.createdAt
    ? fmtTime(first.createdAt)
    : `${fmtTime(first.createdAt)} – ${fmtTime(last.createdAt)}`

  return (
    <>
      <tr
        className="cursor-pointer bg-white transition-colors hover:bg-emerald-50/40"
        onClick={() => setExpanded((v) => !v)}
      >
        <td className="whitespace-nowrap px-5 py-3 text-[12.5px] font-medium text-gray-500">
          {timeRange}
        </td>
        <td className="px-5 py-3">
          <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-2.5 py-1 text-[11px] font-semibold text-emerald-700 ring-1 ring-emerald-200">
            <Receipt size={10} />
            Ventas ({group.count})
          </span>
        </td>
        <td className="px-5 py-3">
          <p className="text-[13px] font-medium text-gray-900">
            {group.count} venta{group.count !== 1 ? 's' : ''} en efectivo
          </p>
          <p className="text-[11px] text-gray-400">Haz clic para {expanded ? 'colapsar' : 'ver detalle'}</p>
        </td>
        <td className="px-5 py-3 text-right text-[13px] font-semibold tabular-nums text-emerald-700">
          +{fmt(group.totalAmount)}
        </td>
        <td className="px-5 py-3 text-right">
          <div className="flex items-center justify-end gap-2">
            <span className="text-[12.5px] font-medium tabular-nums text-gray-500">
              {fmt(group.runningBalance)}
            </span>
            <ChevronDown
              className={cn('shrink-0 text-gray-400 transition-transform duration-200', expanded && 'rotate-180')}
              size={14}
            />
          </div>
        </td>
      </tr>
      {expanded && group.entries.map((entry) => (
        <HistorialSubRow key={entry.id} entry={entry} />
      ))}
    </>
  )
}

function HistorialSubRow({ entry }: Readonly<{ entry: HistorialEntry }>) {
  return (
    <tr className="bg-emerald-50/25 transition-colors hover:bg-emerald-50/50">
      <td className="whitespace-nowrap py-2.5 pl-10 pr-5 text-[12px] text-gray-400">
        {fmtTime(entry.createdAt)}
      </td>
      <td className="px-5 py-2.5" />
      <td className="px-5 py-2.5">
        <p className="text-[12.5px] text-gray-700">{entry.description}</p>
      </td>
      <td className="px-5 py-2.5 text-right text-[12.5px] font-medium tabular-nums text-emerald-700">
        +{fmt(entry.amount)}
      </td>
      <td className="px-5 py-2.5 text-right text-[12px] tabular-nums text-gray-400">
        {fmt(entry.runningBalance)}
      </td>
    </tr>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Formulario de movimiento
// ─────────────────────────────────────────────────────────────────────────────

type MovementFormProps = {
  label: string
  sessionId: string
  type: 'CashIn' | 'CashOut'
  onClose: () => void
}

function MovementForm({ label, sessionId, type, onClose }: Readonly<MovementFormProps>) {
  const registerMovement = useRegisterCashMovement()
  const [amount, setAmount]           = useState('')
  const [description, setDescription] = useState('')
  const [error, setError]             = useState<string | null>(null)

  const isCashIn = type === 'CashIn'

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const amt = parseFloat(amount)
    if (isNaN(amt) || amt <= 0) { setError('El monto debe ser mayor a 0.'); return }
    if (!description.trim()) { setError('La descripción es requerida.'); return }
    try {
      await registerMovement.mutateAsync({ id: sessionId, request: { type, amount: amt, description: description.trim() } })
      onClose()
    } catch (err) {
      setError(err instanceof HttpClientError ? (err.error?.message ?? 'No se pudo registrar.') : 'No se pudo registrar.')
    }
  }

  return (
    <div className={cn('overflow-hidden rounded-xl border bg-white', isCashIn ? 'border-emerald-100' : 'border-red-100')}>
      <div className={cn('border-b px-5 py-3.5', isCashIn ? 'border-emerald-100 bg-emerald-50/40' : 'border-red-100 bg-red-50/40')}>
        <p className={cn('text-[11px] font-semibold uppercase tracking-[0.12em]', isCashIn ? 'text-emerald-700' : 'text-red-700')}>
          {label}
        </p>
      </div>
      <form className="space-y-4 px-5 py-5" onSubmit={(e) => void handleSubmit(e)}>
        <div className="grid gap-4 sm:grid-cols-[9rem_1fr]">
          <div className="space-y-1.5">
            <Label htmlFor="mv-amount">Monto (RD$)</Label>
            <Input
              autoFocus
              id="mv-amount"
              min="0.01"
              placeholder="0.00"
              step="0.01"
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="mv-desc">Descripción</Label>
            <Input
              id="mv-desc"
              placeholder={isCashIn ? 'Ej. Aporte de monedas para dar cambio' : 'Ej. Compra de suministros de limpieza'}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
        </div>
        {error && <FormError message={error} />}
        <div className="flex justify-end gap-2">
          <Button size="sm" type="button" variant="ghost" onClick={onClose}>Cancelar</Button>
          <Button
            className={isCashIn ? 'bg-emerald-600 hover:bg-emerald-700' : ''}
            disabled={registerMovement.isPending}
            size="sm"
            type="submit"
            variant={isCashIn ? 'default' : 'destructive'}
          >
            {registerMovement.isPending ? <><Loader2 className="animate-spin" size={14} />Registrando...</> : 'Registrar'}
          </Button>
        </div>
      </form>
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Formulario de cierre
// ─────────────────────────────────────────────────────────────────────────────

type CloseSessionFormProps = {
  currentBalance: number
  session: CashSession
  onClose: () => void
  onSuccess: (result: CashClosingResult) => void
}

function CloseSessionForm({ currentBalance, session, onClose, onSuccess }: Readonly<CloseSessionFormProps>) {
  const closeSession = useCloseCashSession()
  const [closingBalance, setClosingBalance] = useState('')
  const [error, setError]                   = useState<string | null>(null)

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const balance = parseFloat(closingBalance)
    if (isNaN(balance) || balance < 0) { setError('El balance de cierre debe ser 0 o mayor.'); return }
    try {
      const result = await closeSession.mutateAsync({ id: session.id, request: { closingBalance: balance } })
      onSuccess(result)
    } catch (err) {
      setError(err instanceof HttpClientError ? (err.error?.message ?? 'Error al cerrar la caja.') : 'Error al cerrar la caja.')
    }
  }

  return (
    <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
      <div className="border-b border-gray-100 px-5 py-3.5">
        <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">Cierre de caja</p>
      </div>
      <div className="px-5 py-5">
        <form className="space-y-4" onSubmit={(e) => void handleSubmit(e)}>
          <div className="rounded-lg bg-gray-50 px-4 py-3.5 ring-1 ring-gray-100">
            <div className="flex items-center justify-between">
              <span className="text-[12.5px] text-gray-500">Balance esperado en caja</span>
              <span className="text-[13.5px] font-bold tabular-nums text-gray-900">{fmt(currentBalance)}</span>
            </div>
            <p className="mt-1 text-[11px] text-gray-400">
              Suma de apertura + ingresos − salidas según el sistema.
            </p>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="cb">Efectivo contado (RD$)</Label>
            <Input
              autoFocus
              id="cb"
              min="0"
              placeholder="0.00"
              step="0.01"
              type="number"
              value={closingBalance}
              onChange={(e) => setClosingBalance(e.target.value)}
            />
            <p className="text-[11px] text-muted-foreground">
              Cuenta el efectivo físico disponible en la caja antes de cerrar.
            </p>
          </div>
          {error && <FormError message={error} />}
          <div className="flex justify-end gap-2">
            <Button size="sm" type="button" variant="ghost" onClick={onClose}>Cancelar</Button>
            <Button disabled={closeSession.isPending} size="sm" type="submit" variant="destructive">
              {closeSession.isPending ? <><Loader2 className="animate-spin" size={14} />Cerrando...</> : 'Cerrar caja'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Resultado de cierre
// ─────────────────────────────────────────────────────────────────────────────

type CloseResultPanelProps = {
  result: CashClosingResult
  onViewHistory: () => void
}

function CloseResultPanel({ result, onViewHistory }: Readonly<CloseResultPanelProps>) {
  const outcomeLabel = { Balanced: 'Cuadrado', Surplus: 'Sobrante', Shortage: 'Faltante' }[result.outcome]
  const outcomeClass = { Balanced: 'text-emerald-700', Surplus: 'text-amber-600', Shortage: 'text-red-700' }[result.outcome]
  const outcomeBadge = { Balanced: 'bg-emerald-50 ring-emerald-200', Surplus: 'bg-amber-50 ring-amber-200', Shortage: 'bg-red-50 ring-red-200' }[result.outcome]

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">
      <header>
        <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
          Caja
        </p>
        <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">
          Caja cerrada
        </h1>
        <p className="mt-1 text-[13.5px] text-muted-foreground">
          El turno fue cerrado correctamente.
        </p>
      </header>

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="flex items-center gap-3 border-b border-gray-100 px-5 py-3.5">
          <CheckCircle className="shrink-0 text-emerald-600" size={15} />
          <p className="text-[13px] font-semibold text-gray-900">Resumen del cierre</p>
          <span className={cn('ml-auto inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold ring-1', outcomeBadge, outcomeClass)}>
            {outcomeLabel}
          </span>
        </div>
        <div className="divide-y divide-gray-100">
          <ResultRow label="Balance de apertura" value={fmt(result.openingBalance)} />
          <ResultRow label="Balance sistema"     value={fmt(result.systemBalance)} />
          <ResultRow label="Efectivo contado"    value={fmt(result.closingBalance)} />
          <div className="flex items-center justify-between px-5 py-3.5">
            <span className="text-[13px] font-semibold text-gray-700">Diferencia</span>
            <span className={cn(
              'text-[13px] font-bold tabular-nums',
              result.difference === 0 ? 'text-gray-900'
                : result.difference > 0 ? 'text-emerald-700' : 'text-red-600',
            )}>
              {result.difference > 0 ? '+' : ''}{fmt(result.difference)}
            </span>
          </div>
        </div>
      </div>

      <Button variant="outline" onClick={onViewHistory}>
        Ver historial de turnos
      </Button>
    </div>
  )
}

function ResultRow({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="flex items-center justify-between px-5 py-3.5">
      <span className="text-[13px] text-gray-500">{label}</span>
      <span className="text-[13px] font-semibold tabular-nums text-gray-900">{value}</span>
    </div>
  )
}
