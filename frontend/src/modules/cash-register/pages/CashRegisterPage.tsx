import {
  ArrowDownLeft,
  ArrowUpRight,
  Banknote,
  CheckCircle,
  Clock,
  Loader2,
  Plus,
  X,
} from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  useActiveCashRegister,
  useCloseCashRegister,
  useCashRegisterRealtimeInvalidation,
  useOpenCashRegister,
  useRegisterCashMovement,
} from '@/modules/cash-register/hooks/useCashRegister'
import type {
  CashRegisterDetail,
  CloseCashRegisterResponse,
} from '@/modules/cash-register/types'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Textarea } from '@/shared/components/ui/textarea'
import { HttpClientError } from '@/shared/services/httpClient'
import { cn } from '@/shared/utils/cn'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function formatDate(dateString: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(dateString))
}

export function CashRegisterPage() {
  useCashRegisterRealtimeInvalidation()
  const { data: activeRegister, isLoading } = useActiveCashRegister()
  const navigate = useNavigate()

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="animate-spin text-stone-400" size={28} />
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

// ─────────────────────────────────────────────────────────────────────────────
// Panel: Open register (no active register)
// ─────────────────────────────────────────────────────────────────────────────

function OpenRegisterPanel() {
  const openRegister = useOpenCashRegister()
  const session = useAuthStore((state) => state.session)
  const [openingAmount, setOpeningAmount] = useState('')
  const [notes, setNotes] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function handleOpen() {
    setError(null)
    const amount = parseFloat(openingAmount)
    if (isNaN(amount) || amount < 0) {
      setError('El monto inicial debe ser 0 o mayor.')
      return
    }
    const branchId = session?.user.branchId
    if (!branchId) {
      setError('No se pudo determinar la sucursal. Vuelve a iniciar sesión.')
      return
    }
    try {
      await openRegister.mutateAsync({
        branchId,
        openingAmount: amount,
        notes: notes.trim() || null,
      })
    } catch (err) {
      const message =
        err instanceof HttpClientError
          ? err.error?.message ?? 'No se pudo abrir la caja.'
          : 'No se pudo abrir la caja.'
      setError(message)
    }
  }

  return (
    <div className="p-6">
      <div className="mx-auto max-w-md">
        <div className="mb-6 flex items-center gap-3">
          <span className="flex h-10 w-10 items-center justify-center rounded-full bg-stone-100">
            <Banknote className="text-stone-600" size={20} />
          </span>
          <div>
            <h2 className="text-lg font-semibold text-stone-900">Abrir caja avanzada</h2>
            <p className="text-sm text-stone-500">No tienes una caja abierta.</p>
          </div>
        </div>

        <Card>
          <CardContent className="pt-6">
            <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); void handleOpen() }}>
              <div className="space-y-2">
                <Label htmlFor="opening-amount">Monto inicial (RD$)</Label>
                <Input
                  id="opening-amount"
                  min="0"
                  placeholder="0.00"
                  step="0.01"
                  type="number"
                  value={openingAmount}
                  onChange={(e) => setOpeningAmount(e.target.value)}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="open-notes">Notas (opcional)</Label>
                <Textarea
                  id="open-notes"
                  placeholder="Observaciones de apertura..."
                  rows={2}
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                />
              </div>

              {error && (
                <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
                  {error}
                </p>
              )}

              <Button className="w-full" disabled={openRegister.isPending} type="submit">
                {openRegister.isPending ? (
                  <>
                    <Loader2 className="animate-spin" size={16} />
                    Abriendo...
                  </>
                ) : (
                  'Abrir caja'
                )}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Panel: Active register
// ─────────────────────────────────────────────────────────────────────────────

type ActiveRegisterPanelProps = {
  register: CashRegisterDetail
  onViewSummary: () => void
}

function ActiveRegisterPanel({ register, onViewSummary }: Readonly<ActiveRegisterPanelProps>) {
  const [showMovementForm, setShowMovementForm] = useState(false)
  const [showCloseForm, setShowCloseForm] = useState(false)
  const [closeResult, setCloseResult] = useState<CloseCashRegisterResponse | null>(null)

  if (closeResult) {
    return <CloseResultPanel result={closeResult} onViewSummary={onViewSummary} />
  }

  const expectedCash =
    register.openingAmount +
    register.cashSalesTotal -
    register.cashReturnsTotal +
    register.manualCashIn -
    register.manualCashOut

  return (
    <div className="space-y-6 p-6">
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Monto inicial
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-xl font-semibold text-stone-900">
              {formatCurrency(register.openingAmount)}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Efectivo esperado
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-xl font-semibold text-stone-900">{formatCurrency(expectedCash)}</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Apertura
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-1.5">
              <Clock className="text-stone-400" size={14} />
              <p className="text-sm font-medium text-stone-700">{formatDate(register.openedAt)}</p>
            </div>
            <Badge className="mt-1" variant="outline">
              <span className="mr-1.5 inline-block h-2 w-2 rounded-full bg-green-500" />{' '}Abierta
            </Badge>
          </CardContent>
        </Card>
      </div>

      {/* Sales breakdown */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-semibold">Ventas del turno</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-2 sm:grid-cols-4">
          {[
            { label: 'Efectivo', value: register.cashSalesTotal },
            { label: 'Tarjeta', value: register.cardSalesTotal },
            { label: 'Transferencia', value: register.transferSalesTotal },
            { label: 'Crédito', value: register.creditSalesTotal },
          ].map(({ label, value }) => (
            <div key={label} className="rounded-md bg-stone-50 px-3 py-2">
              <p className="text-xs font-medium text-stone-500">{label}</p>
              <p className="mt-0.5 text-sm font-semibold text-stone-900">
                {formatCurrency(value)}
              </p>
            </div>
          ))}
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex flex-wrap gap-2">
        <Button
          onClick={() => {
            setShowMovementForm(true)
            setShowCloseForm(false)
          }}
          variant="outline"
        >
          <Plus size={16} />
          Registrar movimiento
        </Button>
        <Button
          onClick={() => {
            setShowCloseForm(true)
            setShowMovementForm(false)
          }}
          variant="destructive"
        >
          <X size={16} />
          Cerrar caja
        </Button>
        <Button onClick={onViewSummary} variant="ghost">
          Arqueo diario
        </Button>
      </div>

      {showMovementForm && (
        <MovementForm registerId={register.id} onClose={() => setShowMovementForm(false)} />
      )}
      {showCloseForm && (
        <CloseRegisterForm
          register={register}
          onClose={() => setShowCloseForm(false)}
          onSuccess={(result) => setCloseResult(result)}
        />
      )}

      {register.movements.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-semibold">Movimientos manuales</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y divide-stone-100">
              {[...register.movements].reverse().map((movement) => (
                <div key={movement.id} className="flex items-center justify-between px-4 py-3">
                  <div className="flex items-center gap-3">
                    <span
                      className={cn(
                        'flex h-8 w-8 items-center justify-center rounded-full',
                        movement.movementType === 'CashIn'
                          ? 'bg-green-50 text-green-600'
                          : 'bg-red-50 text-red-600',
                      )}
                    >
                      {movement.movementType === 'CashIn' ? (
                        <ArrowDownLeft size={16} />
                      ) : (
                        <ArrowUpRight size={16} />
                      )}
                    </span>
                    <div>
                      <p className="text-sm font-medium text-stone-900">{movement.reason}</p>
                      <p className="text-xs text-stone-500">{formatDate(movement.createdAt)}</p>
                    </div>
                  </div>
                  <p
                    className={cn(
                      'text-sm font-semibold tabular-nums',
                      movement.movementType === 'CashIn' ? 'text-green-700' : 'text-red-700',
                    )}
                  >
                    {movement.movementType === 'CashIn' ? '+' : '-'}
                    {formatCurrency(movement.amount)}
                  </p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Movement form
// ─────────────────────────────────────────────────────────────────────────────

function MovementForm({
  registerId,
  onClose,
}: Readonly<{ registerId: string; onClose: () => void }>) {
  const registerMovement = useRegisterCashMovement()
  const [type, setType] = useState<'CashIn' | 'CashOut'>('CashIn')
  const [amount, setAmount] = useState('')
  const [reason, setReason] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit() {
    setError(null)
    const parsedAmount = parseFloat(amount)
    if (isNaN(parsedAmount) || parsedAmount <= 0) {
      setError('El monto debe ser mayor a 0.')
      return
    }
    if (!reason.trim()) {
      setError('El motivo es requerido.')
      return
    }
    try {
      await registerMovement.mutateAsync({
        id: registerId,
        request: { type, amount: parsedAmount, reason: reason.trim() },
      })
      onClose()
    } catch (err) {
      const message =
        err instanceof HttpClientError
          ? err.error?.message ?? 'Error al registrar movimiento.'
          : 'Error al registrar movimiento.'
      setError(message)
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm font-semibold">Nuevo movimiento</CardTitle>
      </CardHeader>
      <CardContent>
        <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); void handleSubmit() }}>
          <div className="grid grid-cols-2 gap-2">
            <button
              className={cn(
                'rounded-md border px-3 py-2 text-sm font-medium transition-colors',
                type === 'CashIn'
                  ? 'border-green-200 bg-green-50 text-green-700'
                  : 'border-stone-200 bg-white text-stone-600 hover:bg-stone-50',
              )}
              type="button"
              onClick={() => setType('CashIn')}
            >
              <ArrowDownLeft className="mr-1.5 inline" size={14} />
              Entrada
            </button>
            <button
              className={cn(
                'rounded-md border px-3 py-2 text-sm font-medium transition-colors',
                type === 'CashOut'
                  ? 'border-red-200 bg-red-50 text-red-700'
                  : 'border-stone-200 bg-white text-stone-600 hover:bg-stone-50',
              )}
              type="button"
              onClick={() => setType('CashOut')}
            >
              <ArrowUpRight className="mr-1.5 inline" size={14} />
              Salida
            </button>
          </div>

          <div className="space-y-2">
            <Label htmlFor="move-amount">Monto (RD$)</Label>
            <Input
              id="move-amount"
              min="0.01"
              placeholder="0.00"
              step="0.01"
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="move-reason">Motivo</Label>
            <Input
              id="move-reason"
              placeholder="Ej. Cambio de turno"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>

          {error && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
              {error}
            </p>
          )}

          <div className="flex gap-2">
            <Button disabled={registerMovement.isPending} type="submit">
              {registerMovement.isPending ? (
                <Loader2 className="animate-spin" size={16} />
              ) : (
                'Registrar'
              )}
            </Button>
            <Button type="button" variant="ghost" onClick={onClose}>
              Cancelar
            </Button>
          </div>
        </form>

      </CardContent>
    </Card>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Close register form
// ─────────────────────────────────────────────────────────────────────────────

type CloseRegisterFormProps = {
  register: CashRegisterDetail
  onClose: () => void
  onSuccess: (result: CloseCashRegisterResponse) => void
}

function CloseRegisterForm({ register, onClose, onSuccess }: Readonly<CloseRegisterFormProps>) {
  const closeRegister = useCloseCashRegister()
  const [countedAmount, setCountedAmount] = useState('')
  const [closeNotes, setCloseNotes] = useState('')
  const [error, setError] = useState<string | null>(null)

  const expectedCash =
    register.openingAmount +
    register.cashSalesTotal -
    register.cashReturnsTotal +
    register.manualCashIn -
    register.manualCashOut

  async function handleSubmit() {
    setError(null)
    const amount = parseFloat(countedAmount)
    if (isNaN(amount) || amount < 0) {
      setError('El monto contado debe ser 0 o mayor.')
      return
    }
    try {
      const result = await closeRegister.mutateAsync({
        id: register.id,
        request: { countedAmount: amount, closeNotes: closeNotes.trim() || null },
      })
      onSuccess(result)
    } catch (err) {
      const message =
        err instanceof HttpClientError
          ? err.error?.message ?? 'Error al cerrar la caja.'
          : 'Error al cerrar la caja.'
      setError(message)
    }
  }

  return (
    <Card className="border-red-200 bg-red-50">
      <CardHeader>
        <CardTitle className="text-sm font-semibold text-red-900">Cerrar caja</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="mb-4 text-sm text-red-700">
          Efectivo esperado:{' '}
          <strong>{formatCurrency(expectedCash)}</strong>. Ingresa el monto contado en físico.
        </p>
        <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); void handleSubmit() }}>
          <div className="space-y-2">
            <Label htmlFor="counted-amount">Monto contado (RD$)</Label>
            <Input
              className="bg-white"
              id="counted-amount"
              min="0"
              placeholder="0.00"
              step="0.01"
              type="number"
              value={countedAmount}
              onChange={(e) => setCountedAmount(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="close-notes">Notas de cierre (opcional)</Label>
            <Textarea
              className="bg-white"
              id="close-notes"
              placeholder="Observaciones del cierre..."
              rows={2}
              value={closeNotes}
              onChange={(e) => setCloseNotes(e.target.value)}
            />
          </div>

          {error && (
            <p className="rounded-md bg-red-100 px-3 py-2 text-sm font-medium text-red-800">
              {error}
            </p>
          )}

          <div className="flex gap-2">
            <Button disabled={closeRegister.isPending} type="submit" variant="destructive">
              {closeRegister.isPending ? (
                <Loader2 className="animate-spin" size={16} />
              ) : (
                'Confirmar cierre'
              )}
            </Button>
            <Button type="button" variant="ghost" onClick={onClose}>
              Cancelar
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

// ─────────────────────────────────────────────────────────────────────────────
// Close result panel
// ─────────────────────────────────────────────────────────────────────────────

type CloseResultPanelProps = {
  result: CloseCashRegisterResponse
  onViewSummary: () => void
}

function CloseResultPanel({ result, onViewSummary }: Readonly<CloseResultPanelProps>) {
  const outcomeConfig = {
    Balanced: { label: 'Cuadrado', className: 'border-green-200 bg-green-50 text-green-900' },
    Surplus: { label: 'Sobrante', className: 'border-amber-200 bg-amber-50 text-amber-900' },
    Shortage: { label: 'Faltante', className: 'border-red-200 bg-red-50 text-red-900' },
  }[result.differenceType]

  return (
    <div className="p-6">
      <div className="mx-auto max-w-md space-y-4">
        <div className="flex items-center gap-3">
          <CheckCircle className="text-green-600" size={24} />
          <h2 className="text-lg font-semibold text-stone-900">Caja cerrada</h2>
        </div>

        <Card className={cn('border', outcomeConfig.className)}>
          <CardContent className="pt-6">
            <div className="space-y-3 text-sm">
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Monto inicial</span>
                <span className="font-semibold">{formatCurrency(result.openingAmount)}</span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Ventas efectivo</span>
                <span className="font-semibold">{formatCurrency(result.cashSales)}</span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Devoluciones</span>
                <span className="font-semibold text-red-700">
                  -{formatCurrency(result.cashReturns)}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Entradas manuales</span>
                <span className="font-semibold text-green-700">
                  +{formatCurrency(result.manualCashIn)}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Salidas manuales</span>
                <span className="font-semibold text-red-700">
                  -{formatCurrency(result.manualCashOut)}
                </span>
              </div>
              <div className="flex justify-between border-t pt-3">
                <span className="font-medium opacity-80">Efectivo esperado</span>
                <span className="font-semibold">{formatCurrency(result.expectedCashAmount)}</span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Efectivo contado</span>
                <span className="font-semibold">{formatCurrency(result.countedAmount)}</span>
              </div>
              <div className="flex justify-between border-t pt-3">
                <span className="font-semibold">Diferencia</span>
                <span className="text-base font-bold">
                  {result.difference > 0 ? '+' : ''}
                  {formatCurrency(result.difference)}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Resultado</span>
                <span className="font-bold">{outcomeConfig.label}</span>
              </div>
            </div>
          </CardContent>
        </Card>

        <Button className="w-full" onClick={onViewSummary} variant="outline">
          Ver arqueo diario
        </Button>
      </div>
    </div>
  )
}
