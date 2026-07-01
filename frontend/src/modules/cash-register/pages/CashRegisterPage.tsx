import {
  ArrowDownLeft,
  ArrowUpRight,
  BarChart3,
  CheckCircle,
  Clock,
  Loader2,
  Receipt,
  TrendingUp,
  Wallet,
  X,
} from 'lucide-react'
import { useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  useActiveCashRegister,
  useCloseCashRegister,
  useCashRegisterRealtimeInvalidation,
  useOpenCashRegister,
  useRegisterCashMovement,
} from '@/modules/cash-register/hooks/useCashRegister'
import type { CashRegisterDetail, CloseCashRegisterResponse } from '@/modules/cash-register/types'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Textarea } from '@/shared/components/ui/textarea'
import { HttpClientError } from '@/shared/services/httpClient'
import { cn } from '@/shared/utils/cn'

// ─── Helpers ──────────────────────────────────────────────────────────────────

const fmt = (n: number) =>
  new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(n)

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('es-DO', { hour: '2-digit', minute: '2-digit' })
}

function fmtDateTime(iso: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(iso))
}

function elapsed(from: string) {
  const ms = Date.now() - new Date(from).getTime()
  const h  = Math.floor(ms / 3_600_000)
  const m  = Math.floor((ms % 3_600_000) / 60_000)
  return h > 0 ? `${h}h ${m}min` : `${m}min`
}

// ─── Shared helpers ───────────────────────────────────────────────────────────

function FormError({ message }: Readonly<{ message: string }>) {
  return (
    <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
      {message}
    </div>
  )
}

// ─── Stat card ────────────────────────────────────────────────────────────────

type StatCardProps = {
  icon: ReactNode
  iconClass: string
  label: string
  value: string
  sub?: string
  valueClass?: string
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

// ─── Root ─────────────────────────────────────────────────────────────────────

export function CashRegisterPage() {
  useCashRegisterRealtimeInvalidation()
  const { data: activeRegister, isLoading } = useActiveCashRegister()
  const navigate = useNavigate()

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="animate-spin text-gray-400" size={28} />
      </div>
    )
  }

  if (!activeRegister) {
    return <OpenRegisterPanel />
  }

  return (
    <ActiveRegisterPanel
      register={activeRegister}
      onViewSummary={() => navigate('/cash-register/daily-summary')}
    />
  )
}

// ─── Open register panel ──────────────────────────────────────────────────────

function OpenRegisterPanel() {
  const openRegister = useOpenCashRegister()
  const session      = useAuthStore((s) => s.session)
  const [openingAmount, setOpeningAmount] = useState('')
  const [notes, setNotes]                 = useState('')
  const [error, setError]                 = useState<string | null>(null)

  async function handleOpen(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const amount   = parseFloat(openingAmount)
    const branchId = session?.user.branchId
    if (isNaN(amount) || amount < 0) { setError('El balance inicial debe ser 0 o mayor.'); return }
    if (!branchId) { setError('No se pudo determinar la sucursal. Vuelve a iniciar sesión.'); return }
    try {
      await openRegister.mutateAsync({ branchId, openingAmount: amount, notes: notes.trim() || null })
    } catch (err) {
      setError(err instanceof HttpClientError ? (err.error?.message ?? 'No se pudo abrir la caja.') : 'No se pudo abrir la caja.')
    }
  }

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">
      <header>
        <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">Caja</p>
        <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">Apertura de caja</h1>
        <p className="mt-1 text-[13.5px] text-muted-foreground">
          No hay un turno activo para esta sucursal.
        </p>
      </header>

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="border-b border-gray-100 px-5 py-3.5">
          <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-gray-400">Iniciar turno</p>
        </div>
        <div className="max-w-sm px-5 py-6">
          <form className="space-y-4" onSubmit={(e) => void handleOpen(e)}>
            <div className="space-y-1.5">
              <Label htmlFor="ob">Balance de apertura (RD$)</Label>
              <Input
                autoFocus
                id="ob"
                min="0"
                placeholder="0.00"
                step="0.01"
                type="number"
                value={openingAmount}
                onChange={(e) => setOpeningAmount(e.target.value)}
              />
              <p className="text-[11px] text-muted-foreground">
                Monto en efectivo disponible para dar cambio al iniciar el turno.
              </p>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="notes">Notas (opcional)</Label>
              <Textarea
                id="notes"
                placeholder="Observaciones de apertura..."
                rows={2}
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
              />
            </div>
            {error && <FormError message={error} />}
            <Button className="w-full" disabled={openRegister.isPending} type="submit">
              {openRegister.isPending
                ? <><Loader2 className="animate-spin" size={15} />Abriendo...</>
                : 'Abrir caja'}
            </Button>
          </form>
        </div>
      </div>
    </div>
  )
}

// ─── Active register panel ────────────────────────────────────────────────────

type ActiveForm = 'ingreso' | 'salida' | 'cierre' | null

type ActiveRegisterPanelProps = {
  register: CashRegisterDetail
  onViewSummary: () => void
}

function ActiveRegisterPanel({ register, onViewSummary }: Readonly<ActiveRegisterPanelProps>) {
  const [activeForm, setActiveForm]   = useState<ActiveForm>(null)
  const [closeResult, setCloseResult] = useState<CloseCashRegisterResponse | null>(null)

  const { expectedCash, totalSales } = useMemo(() => ({
    expectedCash:
      register.openingAmount +
      register.cashSalesTotal -
      register.cashReturnsTotal +
      register.manualCashIn -
      register.manualCashOut,
    totalSales:
      register.cashSalesTotal +
      register.cardSalesTotal +
      register.transferSalesTotal +
      register.creditSalesTotal,
  }), [register])

  function toggleForm(form: ActiveForm) {
    setActiveForm((prev) => (prev === form ? null : form))
  }

  if (closeResult) {
    return <CloseResultPanel result={closeResult} onViewSummary={onViewSummary} />
  }

  const movementsCount = register.movements.length

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">

      {/* Header */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">Caja</p>
          <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">Turno activo</h1>
          <div className="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-2.5 py-1 text-[11.5px] font-semibold text-emerald-700 ring-1 ring-emerald-200">
              <span className="inline-block h-1.5 w-1.5 animate-pulse rounded-full bg-emerald-500" />
              Abierta
            </span>
            <span className="flex items-center gap-1 text-[12px] text-gray-400">
              <Clock size={12} />
              {elapsed(register.openedAt)} activo · desde {fmtDateTime(register.openedAt)}
            </span>
          </div>
        </div>
        <div className="flex shrink-0 flex-wrap items-center gap-2 pt-1">
          <Button
            className="bg-emerald-600 text-white hover:bg-emerald-700"
            size="sm"
            variant="default"
            onClick={() => toggleForm('ingreso')}
          >
            <ArrowDownLeft size={14} />
            Ingreso
          </Button>
          <Button
            className="border-red-200 text-red-600 hover:bg-red-50"
            size="sm"
            variant="outline"
            onClick={() => toggleForm('salida')}
          >
            <ArrowUpRight size={14} />
            Salida
          </Button>
          <Button size="sm" variant="ghost" onClick={onViewSummary}>
            <BarChart3 size={14} />
            Arqueo
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => toggleForm('cierre')}
          >
            <X size={13} />
            Cerrar caja
          </Button>
        </div>
      </div>

      {/* Metrics */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
        <StatCard icon={<Wallet size={14} />}      iconClass="text-gray-400"   label="Balance apertura" value={fmt(register.openingAmount)} />
        <StatCard icon={<Wallet size={14} />}      iconClass="text-blue-500"   label="Ef. esperado"     value={fmt(expectedCash)} />
        <StatCard icon={<Receipt size={14} />}     iconClass="text-emerald-600" label="Ventas efectivo" value={fmt(register.cashSalesTotal)} />
        <StatCard icon={<TrendingUp size={14} />}  iconClass="text-violet-500" label="Total ventas"     value={fmt(totalSales)} />
        <StatCard icon={<ArrowDownLeft size={14} />} iconClass="text-sky-500"  label="Entradas"         value={fmt(register.manualCashIn)} />
        <StatCard icon={<ArrowUpRight size={14} />}  iconClass="text-red-500"  label="Salidas"          value={fmt(register.manualCashOut)} />
      </div>

      {/* Inline forms */}
      {activeForm === 'ingreso' && (
        <MovementForm
          isCashIn
          label="Registrar ingreso de efectivo"
          registerId={register.id}
          onClose={() => setActiveForm(null)}
        />
      )}
      {activeForm === 'salida' && (
        <MovementForm
          isCashIn={false}
          label="Registrar salida de efectivo"
          registerId={register.id}
          onClose={() => setActiveForm(null)}
        />
      )}
      {activeForm === 'cierre' && (
        <CloseRegisterForm
          expectedCash={expectedCash}
          register={register}
          onClose={() => setActiveForm(null)}
          onSuccess={(result) => setCloseResult(result)}
        />
      )}

      {/* Main content card */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        {/* Info bar */}
        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 border-b border-gray-100 px-5 py-3 text-[12px] text-gray-500">
          <span className="flex items-center gap-1.5">
            <Wallet size={12} className="text-gray-400" />
            Apertura {fmt(register.openingAmount)}
          </span>
          <span className="flex items-center gap-1.5">
            <Clock size={12} className="text-gray-400" />
            {fmtDateTime(register.openedAt)}
          </span>
          <span className="ml-auto text-[11.5px] font-medium text-gray-400">
            {movementsCount} movimiento{movementsCount !== 1 ? 's' : ''} manual{movementsCount !== 1 ? 'es' : ''}
          </span>
        </div>

        {/* Ventas por método */}
        <div className="grid border-b border-gray-100 sm:grid-cols-4">
          {[
            { label: 'Efectivo',      value: register.cashSalesTotal,     cls: 'text-emerald-700' },
            { label: 'Tarjeta',       value: register.cardSalesTotal,     cls: 'text-blue-600'    },
            { label: 'Transferencia', value: register.transferSalesTotal, cls: 'text-violet-600'  },
            { label: 'Crédito',       value: register.creditSalesTotal,   cls: 'text-amber-600'   },
          ].map(({ label, value, cls }, i) => (
            <div
              key={label}
              className={cn('px-5 py-3.5', i < 3 ? 'border-b border-gray-100 sm:border-b-0 sm:border-r' : '')}
            >
              <p className="text-[10.5px] font-semibold uppercase tracking-[0.1em] text-gray-400">{label}</p>
              <p className={cn('mt-1 text-[14px] font-bold tabular-nums', cls)}>{fmt(value)}</p>
            </div>
          ))}
        </div>

        {/* Movements table */}
        {register.movements.length === 0 ? (
          <div className="flex h-28 items-center justify-center text-[13px] text-gray-400">
            Sin movimientos manuales en este turno.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="border-b border-gray-100 bg-gray-50/70">
                <tr>
                  {[
                    { label: 'Hora',    right: false },
                    { label: 'Tipo',    right: false },
                    { label: 'Motivo',  right: false },
                    { label: 'Monto',   right: true  },
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
                {[...register.movements].reverse().map((mov) => {
                  const isCashIn = mov.movementType === 'CashIn'
                  return (
                    <tr key={mov.id} className="bg-white transition-colors hover:bg-gray-50/60">
                      <td className="whitespace-nowrap px-5 py-3 text-[12.5px] font-medium text-gray-500">
                        {fmtTime(mov.createdAt)}
                      </td>
                      <td className="px-5 py-3">
                        <span className={cn(
                          'inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold ring-1',
                          isCashIn
                            ? 'bg-emerald-50 text-emerald-700 ring-emerald-200'
                            : 'bg-red-50 text-red-700 ring-red-200',
                        )}>
                          {isCashIn ? 'Ingreso' : 'Salida'}
                        </span>
                      </td>
                      <td className="px-5 py-3 text-[13px] font-medium text-gray-900">
                        {mov.reason}
                      </td>
                      <td className={cn(
                        'px-5 py-3 text-right text-[13px] font-semibold tabular-nums',
                        isCashIn ? 'text-emerald-700' : 'text-red-600',
                      )}>
                        {isCashIn ? '+' : '−'}{fmt(mov.amount)}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}

        {/* Footer */}
        <div className="flex items-center justify-between border-t border-gray-100 px-5 py-3.5">
          <span className="text-[12.5px] font-medium text-gray-500">Efectivo esperado en caja</span>
          <span className="text-[14px] font-bold tabular-nums text-gray-900">{fmt(expectedCash)}</span>
        </div>
      </div>
    </div>
  )
}

// ─── Movement form ────────────────────────────────────────────────────────────

type MovementFormProps = {
  isCashIn: boolean
  label: string
  registerId: string
  onClose: () => void
}

function MovementForm({ isCashIn, label, registerId, onClose }: Readonly<MovementFormProps>) {
  const registerMovement = useRegisterCashMovement()
  const [amount, setAmount]   = useState('')
  const [reason, setReason]   = useState('')
  const [error, setError]     = useState<string | null>(null)

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const amt = parseFloat(amount)
    if (isNaN(amt) || amt <= 0) { setError('El monto debe ser mayor a 0.'); return }
    if (!reason.trim())         { setError('El motivo es requerido.'); return }
    try {
      await registerMovement.mutateAsync({
        id: registerId,
        request: { type: isCashIn ? 'CashIn' : 'CashOut', amount: amt, reason: reason.trim() },
      })
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
            <Label htmlFor="mv-reason">Motivo</Label>
            <Input
              id="mv-reason"
              placeholder={isCashIn ? 'Ej. Aporte de monedas para dar cambio' : 'Ej. Compra de suministros de limpieza'}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
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
            {registerMovement.isPending
              ? <><Loader2 className="animate-spin" size={14} />Registrando...</>
              : 'Registrar'}
          </Button>
        </div>
      </form>
    </div>
  )
}

// ─── Close register form ──────────────────────────────────────────────────────

type CloseRegisterFormProps = {
  expectedCash: number
  register: CashRegisterDetail
  onClose: () => void
  onSuccess: (result: CloseCashRegisterResponse) => void
}

function CloseRegisterForm({ expectedCash, onClose, onSuccess, register }: Readonly<CloseRegisterFormProps>) {
  const closeRegister = useCloseCashRegister()
  const [countedAmount, setCountedAmount] = useState('')
  const [closeNotes, setCloseNotes]       = useState('')
  const [error, setError]                 = useState<string | null>(null)

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const amount = parseFloat(countedAmount)
    if (isNaN(amount) || amount < 0) { setError('El monto contado debe ser 0 o mayor.'); return }
    try {
      const result = await closeRegister.mutateAsync({
        id: register.id,
        request: { countedAmount: amount, closeNotes: closeNotes.trim() || null },
      })
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
              <span className="text-[13.5px] font-bold tabular-nums text-gray-900">{fmt(expectedCash)}</span>
            </div>
            <p className="mt-1 text-[11px] text-gray-400">
              Apertura + ventas efectivo − devoluciones + entradas − salidas.
            </p>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="counted">Efectivo contado (RD$)</Label>
            <Input
              autoFocus
              id="counted"
              min="0"
              placeholder="0.00"
              step="0.01"
              type="number"
              value={countedAmount}
              onChange={(e) => setCountedAmount(e.target.value)}
            />
            <p className="text-[11px] text-muted-foreground">
              Cuenta el efectivo físico disponible en la caja antes de cerrar.
            </p>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="close-notes">Notas de cierre (opcional)</Label>
            <Textarea
              id="close-notes"
              placeholder="Observaciones del cierre..."
              rows={2}
              value={closeNotes}
              onChange={(e) => setCloseNotes(e.target.value)}
            />
          </div>
          {error && <FormError message={error} />}
          <div className="flex justify-end gap-2">
            <Button size="sm" type="button" variant="ghost" onClick={onClose}>Cancelar</Button>
            <Button disabled={closeRegister.isPending} size="sm" type="submit" variant="destructive">
              {closeRegister.isPending
                ? <><Loader2 className="animate-spin" size={14} />Cerrando...</>
                : 'Cerrar caja'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  )
}

// ─── Close result panel ───────────────────────────────────────────────────────

type CloseResultPanelProps = {
  result: CloseCashRegisterResponse
  onViewSummary: () => void
}

function CloseResultPanel({ result, onViewSummary }: Readonly<CloseResultPanelProps>) {
  const outcomeLabel = { Balanced: 'Cuadrado', Surplus: 'Sobrante', Shortage: 'Faltante' }[result.differenceType]
  const outcomeClass = { Balanced: 'text-emerald-700', Surplus: 'text-amber-600', Shortage: 'text-red-700' }[result.differenceType]
  const outcomeBadge = {
    Balanced: 'bg-emerald-50 ring-emerald-200',
    Surplus:  'bg-amber-50 ring-amber-200',
    Shortage: 'bg-red-50 ring-red-200',
  }[result.differenceType]

  const rows = [
    { label: 'Monto inicial',      value: fmt(result.openingAmount),      cls: 'text-gray-900' },
    { label: 'Ventas efectivo',    value: `+${fmt(result.cashSales)}`,     cls: 'text-emerald-700' },
    { label: 'Devoluciones',       value: `−${fmt(result.cashReturns)}`,   cls: 'text-red-600' },
    { label: 'Entradas manuales',  value: `+${fmt(result.manualCashIn)}`,  cls: 'text-emerald-700' },
    { label: 'Salidas manuales',   value: `−${fmt(result.manualCashOut)}`, cls: 'text-red-600' },
  ]

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">
      <header>
        <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">Caja</p>
        <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">Caja cerrada</h1>
        <p className="mt-1 text-[13.5px] text-muted-foreground">El turno fue cerrado correctamente.</p>
      </header>

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="flex items-center gap-3 border-b border-gray-100 px-5 py-3.5">
          <CheckCircle className="shrink-0 text-emerald-600" size={15} />
          <p className="text-[13px] font-semibold text-gray-900">Resumen del cierre</p>
          <span className={cn(
            'ml-auto inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold ring-1',
            outcomeBadge, outcomeClass,
          )}>
            {outcomeLabel}
          </span>
        </div>

        <div className="divide-y divide-gray-100">
          {rows.map(({ label, value, cls }) => (
            <div key={label} className="flex items-center justify-between px-5 py-3">
              <span className="text-[13px] text-gray-500">{label}</span>
              <span className={cn('text-[13px] font-semibold tabular-nums', cls)}>{value}</span>
            </div>
          ))}
          <div className="flex items-center justify-between bg-gray-50/60 px-5 py-3">
            <span className="text-[13px] font-semibold text-gray-700">Efectivo esperado</span>
            <span className="text-[13.5px] font-bold tabular-nums text-gray-900">{fmt(result.expectedCashAmount)}</span>
          </div>
          <div className="flex items-center justify-between bg-gray-50/60 px-5 py-3">
            <span className="text-[13px] font-semibold text-gray-700">Efectivo contado</span>
            <span className="text-[13.5px] font-bold tabular-nums text-gray-900">{fmt(result.countedAmount)}</span>
          </div>
          <div className="flex items-center justify-between px-5 py-4">
            <span className="text-[14px] font-bold text-gray-900">Diferencia</span>
            <span className={cn('text-[15px] font-bold tabular-nums', outcomeClass)}>
              {result.difference > 0 ? '+' : ''}{fmt(result.difference)}
            </span>
          </div>
        </div>

        <div className="border-t border-gray-100 px-5 py-4">
          <Button className="w-full" variant="outline" onClick={onViewSummary}>
            <BarChart3 size={15} />
            Ver arqueo del día
          </Button>
        </div>
      </div>
    </div>
  )
}
