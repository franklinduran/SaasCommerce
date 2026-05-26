import { ArrowDownLeft, ArrowUpRight, Banknote, CheckCircle, Clock, Loader2, Plus, X } from 'lucide-react'
import { useState } from 'react'
import type { SubmitEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  useCloseCashSession,
  useCurrentCashSession,
  useOpenCashSession,
  useRegisterCashMovement,
} from '@/modules/cash/hooks/useCash'
import type { CashClosingResult, CashSession } from '@/modules/cash/types'
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

export function CashPage() {
  const { data: currentSession, isLoading } = useCurrentCashSession()
  const navigate = useNavigate()

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="animate-spin text-stone-400" size={28} />
      </div>
    )
  }

  if (!currentSession) {
    return <OpenSessionPanel />
  }

  return <ActiveSessionPanel session={currentSession} onViewHistory={() => navigate('/cash/history')} />
}

// ─────────────────────────────────────────────────────────────────────────────
// Panel: Open session (no active session)
// ─────────────────────────────────────────────────────────────────────────────

function OpenSessionPanel() {
  const openSession = useOpenCashSession()
  const [openingBalance, setOpeningBalance] = useState('')
  const [notes, setNotes] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function handleOpen(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const balance = parseFloat(openingBalance)
    if (isNaN(balance) || balance < 0) {
      setError('El balance inicial debe ser 0 o mayor.')
      return
    }

    try {
      await openSession.mutateAsync({ openingBalance: balance, notes: notes.trim() || null })
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
            <h2 className="text-lg font-semibold text-stone-900">Abrir caja</h2>
            <p className="text-sm text-stone-500">No hay una caja abierta para esta sucursal.</p>
          </div>
        </div>

        <Card>
          <CardContent className="pt-6">
            <form className="space-y-4" onSubmit={(e) => void handleOpen(e)}>
              <div className="space-y-2">
                <Label htmlFor="opening-balance">Balance inicial (RD$)</Label>
                <Input
                  id="opening-balance"
                  min="0"
                  placeholder="0.00"
                  step="0.01"
                  type="number"
                  value={openingBalance}
                  onChange={(e) => setOpeningBalance(e.target.value)}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="notes">Notas (opcional)</Label>
                <Textarea
                  id="notes"
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

              <Button
                className="w-full"
                disabled={openSession.isPending}
                type="submit"
              >
                {openSession.isPending ? (
                  <>
                    <Loader2 className="animate-spin" size={16} />
                    Abriendo caja...
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
// Panel: Active session
// ─────────────────────────────────────────────────────────────────────────────

type ActiveSessionPanelProps = {
  session: CashSession
  onViewHistory: () => void
}

function ActiveSessionPanel({ session, onViewHistory }: Readonly<ActiveSessionPanelProps>) {
  const [showMovementForm, setShowMovementForm] = useState(false)
  const [showCloseForm, setShowCloseForm] = useState(false)
  const [closeResult, setCloseResult] = useState<CashClosingResult | null>(null)

  if (closeResult) {
    return <CloseResultPanel result={closeResult} onViewHistory={onViewHistory} />
  }

  return (
    <div className="space-y-6 p-6">
      {/* Session summary */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Balance inicial
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-xl font-semibold text-stone-900">
              {formatCurrency(session.openingBalance)}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Balance sistema
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-xl font-semibold text-stone-900">
              {formatCurrency(session.systemBalance)}
            </p>
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
              <p className="text-sm font-medium text-stone-700">{formatDate(session.openedAt)}</p>
            </div>
            <Badge className="mt-1" variant="outline">
              <span className="mr-1.5 inline-block h-2 w-2 rounded-full bg-green-500" />
              {'Abierta'}
            </Badge>
          </CardContent>
        </Card>
      </div>

      {/* Action buttons */}
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
        <Button onClick={onViewHistory} variant="ghost">
          Ver historial
        </Button>
      </div>

      {/* Forms */}
      {showMovementForm && (
        <MovementForm
          sessionId={session.id}
          onClose={() => setShowMovementForm(false)}
        />
      )}
      {showCloseForm && (
        <CloseSessionForm
          session={session}
          onClose={() => setShowCloseForm(false)}
          onSuccess={(result) => setCloseResult(result)}
        />
      )}

      {/* Movements list */}
      {session.movements.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-semibold">Movimientos de caja</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y divide-stone-100">
              {[...session.movements].reverse().map((movement) => (
                <div key={movement.id} className="flex items-center justify-between px-4 py-3">
                  <div className="flex items-center gap-3">
                    <span
                      className={cn(
                        'flex h-8 w-8 items-center justify-center rounded-full',
                        movement.type === 'CashIn'
                          ? 'bg-green-50 text-green-600'
                          : 'bg-red-50 text-red-600',
                      )}
                    >
                      {movement.type === 'CashIn' ? (
                        <ArrowDownLeft size={16} />
                      ) : (
                        <ArrowUpRight size={16} />
                      )}
                    </span>
                    <div>
                      <p className="text-sm font-medium text-stone-900">{movement.description}</p>
                      <p className="text-xs text-stone-500">{formatDate(movement.createdAt)}</p>
                    </div>
                  </div>
                  <p
                    className={cn(
                      'text-sm font-semibold tabular-nums',
                      movement.type === 'CashIn' ? 'text-green-700' : 'text-red-700',
                    )}
                  >
                    {movement.type === 'CashIn' ? '+' : '-'}
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
// Movement registration form
// ─────────────────────────────────────────────────────────────────────────────

type MovementFormProps = {
  sessionId: string
  onClose: () => void
}

function MovementForm({ sessionId, onClose }: Readonly<MovementFormProps>) {
  const registerMovement = useRegisterCashMovement()
  const [type, setType] = useState<'CashIn' | 'CashOut'>('CashIn')
  const [amount, setAmount] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const parsedAmount = parseFloat(amount)
    if (isNaN(parsedAmount) || parsedAmount <= 0) {
      setError('El monto debe ser mayor a 0.')
      return
    }
    if (!description.trim()) {
      setError('La descripcion es requerida.')
      return
    }
    try {
      await registerMovement.mutateAsync({ id: sessionId, request: { type, amount: parsedAmount, description: description.trim() } })
      onClose()
    } catch (err) {
      const message = err instanceof HttpClientError ? err.error?.message ?? 'Error al registrar movimiento.' : 'Error al registrar movimiento.'
      setError(message)
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm font-semibold">Nuevo movimiento</CardTitle>
      </CardHeader>
      <CardContent>
        <form className="space-y-4" onSubmit={(e) => void handleSubmit(e)}>
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
            <Label htmlFor="movement-amount">Monto (RD$)</Label>
            <Input
              id="movement-amount"
              min="0.01"
              placeholder="0.00"
              step="0.01"
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="movement-description">Descripcion</Label>
            <Input
              id="movement-description"
              placeholder="Ej. Cambio de turno"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          {error && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
              {error}
            </p>
          )}

          <div className="flex gap-2">
            <Button disabled={registerMovement.isPending} type="submit">
              {registerMovement.isPending ? <Loader2 className="animate-spin" size={16} /> : 'Registrar'}
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
// Close session form
// ─────────────────────────────────────────────────────────────────────────────

type CloseSessionFormProps = {
  session: CashSession
  onClose: () => void
  onSuccess: (result: CashClosingResult) => void
}

function CloseSessionForm({ session, onClose, onSuccess }: Readonly<CloseSessionFormProps>) {
  const closeSession = useCloseCashSession()
  const [closingBalance, setClosingBalance] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setError(null)
    const balance = parseFloat(closingBalance)
    if (isNaN(balance) || balance < 0) {
      setError('El balance de cierre debe ser 0 o mayor.')
      return
    }
    try {
      const result = await closeSession.mutateAsync({ id: session.id, request: { closingBalance: balance } })
      onSuccess(result)
    } catch (err) {
      const message = err instanceof HttpClientError ? err.error?.message ?? 'Error al cerrar la caja.' : 'Error al cerrar la caja.'
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
          Balance sistema: <strong>{formatCurrency(session.systemBalance)}</strong>. Ingresa el balance real del cajero.
        </p>
        <form className="space-y-4" onSubmit={(e) => void handleSubmit(e)}>
          <div className="space-y-2">
            <Label htmlFor="closing-balance">Balance contado (RD$)</Label>
            <Input
              className="bg-white"
              id="closing-balance"
              min="0"
              placeholder="0.00"
              step="0.01"
              type="number"
              value={closingBalance}
              onChange={(e) => setClosingBalance(e.target.value)}
            />
          </div>

          {error && (
            <p className="rounded-md bg-red-100 px-3 py-2 text-sm font-medium text-red-800">
              {error}
            </p>
          )}

          <div className="flex gap-2">
            <Button disabled={closeSession.isPending} type="submit" variant="destructive">
              {closeSession.isPending ? <Loader2 className="animate-spin" size={16} /> : 'Cerrar caja'}
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
  result: CashClosingResult
  onViewHistory: () => void
}

function CloseResultPanel({ result, onViewHistory }: Readonly<CloseResultPanelProps>) {
  const outcomeConfig = {
    Balanced: { label: 'Cuadrado', className: 'border-green-200 bg-green-50 text-green-900' },
    Surplus: { label: 'Sobrante', className: 'border-amber-200 bg-amber-50 text-amber-900' },
    Shortage: { label: 'Faltante', className: 'border-red-200 bg-red-50 text-red-900' },
  }[result.outcome]

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
                <span className="font-medium opacity-80">Balance inicial</span>
                <span className="font-semibold">{formatCurrency(result.openingBalance)}</span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Balance sistema</span>
                <span className="font-semibold">{formatCurrency(result.systemBalance)}</span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Balance contado</span>
                <span className="font-semibold">{formatCurrency(result.closingBalance)}</span>
              </div>
              <div className="border-t pt-3 flex justify-between">
                <span className="font-semibold">Diferencia</span>
                <span className="font-bold text-base">
                  {result.difference > 0 ? '+' : ''}{formatCurrency(result.difference)}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="font-medium opacity-80">Resultado</span>
                <span className="font-bold">{outcomeConfig.label}</span>
              </div>
            </div>
          </CardContent>
        </Card>

        <Button className="w-full" onClick={onViewHistory} variant="outline">
          Ver historial de cajas
        </Button>
      </div>
    </div>
  )
}
