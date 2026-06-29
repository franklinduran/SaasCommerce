import { AlertTriangle, CheckCircle2, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  useCloseDailyClosing,
  useDailyClosingDetail,
} from '@/modules/daily-closing/hooks/useDailyClosing'
import type { DailyClosingAlertType } from '@/modules/daily-closing/types'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Label } from '@/shared/components/ui/label'
import { Textarea } from '@/shared/components/ui/textarea'
import { cn } from '@/shared/utils/cn'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function formatDate(dateString: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(`${dateString}T00:00:00`))
}

function formatDateTime(dateString: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(dateString))
}

function alertTypeLabel(type: DailyClosingAlertType): string {
  switch (type) {
    case 'OpenCashSession':
      return 'Caja abierta'
    case 'MissingProductCost':
      return 'Costo faltante'
    case 'NegativeMargin':
      return 'Margen negativo'
    case 'HighExpenseRatio':
      return 'Gastos altos'
    case 'CreditSalesHigh':
      return 'Créditos altos'
    default:
      return type
  }
}

function cashDifferenceClassName(difference: number) {
  if (difference === 0) return 'text-green-700'
  return difference > 0 ? 'text-amber-700' : 'text-red-700'
}

export function DailyClosingDetailPage() {
  const { closingId } = useParams<{ closingId: string }>()
  const navigate = useNavigate()

  const { data: closing, isLoading, isError } = useDailyClosingDetail(closingId ?? '')
  const { mutate: closeDay, isPending: isClosing, error: closeError } = useCloseDailyClosing()

  const [cashCounted, setCashCounted] = useState('')
  const [closeNotes, setCloseNotes] = useState('')
  const [validationError, setValidationError] = useState<string | null>(null)

  function handleClose() {
    const amount = parseFloat(cashCounted)
    if (isNaN(amount) || amount < 0) {
      setValidationError('El efectivo contado debe ser 0 o mayor.')
      return
    }
    setValidationError(null)
    if (!closingId) return
    const normalizedNotes = closeNotes.trim()
    closeDay(
      {
        closingId,
        params: { cashCounted: amount, notes: normalizedNotes.length > 0 ? normalizedNotes : undefined },
      },
      {
        onSuccess: (updated) => {
          navigate(`/daily-closing/${updated.id}`, { replace: true })
        },
      },
    )
  }

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="animate-spin text-stone-400" size={28} />
      </div>
    )
  }

  if (isError || !closing) {
    return (
      <div className="p-6">
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          No se encontró el cierre.
        </p>
        <Button className="mt-4" variant="outline" onClick={() => navigate('/daily-closing/history')}>
          Volver al historial
        </Button>
      </div>
    )
  }

  const cashDiff = closing.cashDifference

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">
            Cierre del {formatDate(closing.closingDate)}
          </h2>
          <p className="text-sm text-stone-500">{closing.branchName}</p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant={closing.status === 'Closed' ? 'default' : 'secondary'}>
            {closing.status === 'Closed' ? 'Cerrado' : 'Borrador'}
          </Badge>
          <Button variant="outline" onClick={() => navigate('/daily-closing/history')}>
            Volver
          </Button>
        </div>
      </div>

      {/* Summary */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Total ventas
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-xl font-semibold text-stone-900">
              {formatCurrency(closing.totalSales)}
            </p>
            <p className="mt-1 text-xs text-stone-500">{closing.salesCount} transacciones</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Ganancia neta est.
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p
              className={`text-xl font-semibold ${closing.estimatedNetProfit >= 0 ? 'text-green-700' : 'text-red-700'}`}
            >
              {formatCurrency(closing.estimatedNetProfit)}
            </p>
            <p className="mt-1 text-xs text-stone-500">
              {closing.netMarginPercent.toFixed(1)}% margen neto
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
            <p className="text-xl font-semibold text-stone-900">
              {formatCurrency(closing.cashExpected)}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Cash reconciliation (only when closed) */}
      {closing.status === 'Closed' && closing.cashCounted !== null && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-semibold">Cuadre de efectivo</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-3 sm:grid-cols-3">
            <div>
              <p className="text-xs font-medium text-stone-500">Esperado</p>
              <p className="mt-0.5 text-sm font-semibold text-stone-900">
                {formatCurrency(closing.cashExpected)}
              </p>
            </div>
            <div>
              <p className="text-xs font-medium text-stone-500">Contado</p>
              <p className="mt-0.5 text-sm font-semibold text-stone-900">
                {formatCurrency(closing.cashCounted)}
              </p>
            </div>
            {cashDiff !== null && (
              <div>
                <p className="text-xs font-medium text-stone-500">Diferencia</p>
                <p
                  className={cn(
                    'mt-0.5 text-sm font-semibold tabular-nums',
                    cashDifferenceClassName(cashDiff),
                  )}
                >
                  {cashDiff > 0 ? '+' : ''}
                  {formatCurrency(cashDiff)}
                </p>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Sales breakdown */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-semibold">Ventas por método de pago</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-2 sm:grid-cols-4">
          {[
            { label: 'Efectivo', value: closing.cashSales },
            { label: 'Transferencia', value: closing.transferSales },
            { label: 'Tarjeta', value: closing.cardSales },
            { label: 'Crédito', value: closing.creditSales },
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

      {/* Profitability */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-semibold">Rentabilidad</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-3 sm:grid-cols-3">
          <div>
            <p className="text-xs font-medium text-stone-500">Gastos operativos</p>
            <p className="mt-0.5 text-sm font-semibold text-stone-900">
              {formatCurrency(closing.totalExpenses)}
            </p>
          </div>
          <div>
            <p className="text-xs font-medium text-stone-500">Costo de ventas</p>
            <p className="mt-0.5 text-sm font-semibold text-stone-900">
              {formatCurrency(closing.totalCost)}
            </p>
          </div>
          <div>
            <p className="text-xs font-medium text-stone-500">Ganancia bruta</p>
            <p
              className={`mt-0.5 text-sm font-semibold ${closing.grossProfit >= 0 ? 'text-green-700' : 'text-red-700'}`}
            >
              {formatCurrency(closing.grossProfit)}
            </p>
          </div>
          <div>
            <p className="text-xs font-medium text-stone-500">Margen bruto</p>
            <p className="mt-0.5 text-sm font-semibold text-stone-900">
              {closing.grossMarginPercent.toFixed(1)}%
            </p>
          </div>
          <div>
            <p className="text-xs font-medium text-stone-500">Margen neto</p>
            <p
              className={`mt-0.5 text-sm font-semibold ${closing.netMarginPercent >= 0 ? 'text-green-700' : 'text-red-700'}`}
            >
              {closing.netMarginPercent.toFixed(1)}%
            </p>
          </div>
        </CardContent>
      </Card>

      {/* Credits */}
      {(closing.newCreditsCount > 0 || closing.creditPaymentsReceived > 0) && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-semibold">Créditos</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-2 sm:grid-cols-3">
            <div>
              <p className="text-xs font-medium text-stone-500">Nuevos créditos</p>
              <p className="mt-0.5 text-sm font-semibold text-stone-900">
                {closing.newCreditsCount} · {formatCurrency(closing.newCreditsAmount)}
              </p>
            </div>
            <div>
              <p className="text-xs font-medium text-stone-500">Pagos recibidos</p>
              <p className="mt-0.5 text-sm font-semibold text-green-700">
                {formatCurrency(closing.creditPaymentsReceived)}
              </p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Alerts */}
      {closing.alerts.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-sm font-semibold text-amber-700">
              <AlertTriangle size={16} />
              Alertas ({closing.alerts.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {closing.alerts.map((alert) => (
              <div
                key={alert.id}
                className="flex items-start gap-3 rounded-md bg-amber-50 px-3 py-2"
              >
                <AlertTriangle className="mt-0.5 shrink-0 text-amber-500" size={14} />
                <div className="min-w-0">
                  <div className="flex items-center gap-2">
                    <Badge
                      variant="outline"
                      className="border-amber-200 bg-amber-50 text-amber-700"
                    >
                      {alertTypeLabel(alert.alertType)}
                    </Badge>
                  </div>
                  <p className="mt-0.5 text-xs text-amber-700">{alert.message}</p>
                  {alert.estimatedImpact !== null && (
                    <p className="mt-0.5 text-xs font-medium text-amber-800">
                      Impacto est.: {formatCurrency(alert.estimatedImpact)}
                    </p>
                  )}
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      )}

      {/* Metadata */}
      <Card>
        <CardContent className="grid gap-3 pt-6 sm:grid-cols-2">
          <div>
            <p className="text-xs font-medium text-stone-500">Creado</p>
            <p className="mt-0.5 text-sm font-medium text-stone-900">
              {formatDateTime(closing.createdAt)}
            </p>
          </div>
          {closing.closedAt && (
            <div>
              <p className="text-xs font-medium text-stone-500">Cerrado</p>
              <p className="mt-0.5 text-sm font-medium text-stone-900">
                {formatDateTime(closing.closedAt)}
              </p>
            </div>
          )}
          {closing.notes && (
            <div className="sm:col-span-2">
              <p className="text-xs font-medium text-stone-500">Notas</p>
              <p className="mt-0.5 text-sm text-stone-700">{closing.notes}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Close form — only for Draft status */}
      {closing.status === 'Draft' && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-semibold">Cerrar el día</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="cash-counted">Efectivo contado (RD$)</Label>
              <input
                id="cash-counted"
                type="number"
                min="0"
                step="0.01"
                placeholder="0.00"
                value={cashCounted}
                onChange={(e) => {
                  setCashCounted(e.target.value)
                  setValidationError(null)
                }}
                className="flex h-9 w-full max-w-xs rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              />
              {validationError && (
                <p className="text-sm font-medium text-red-600">{validationError}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="close-notes">Notas del cierre (opcional)</Label>
              <Textarea
                id="close-notes"
                placeholder="Observaciones del cierre..."
                value={closeNotes}
                onChange={(e) => setCloseNotes(e.target.value)}
                rows={2}
              />
            </div>

            {closeError && (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
                Error al cerrar el día. Verifica que no haya cajas abiertas e intenta nuevamente.
              </p>
            )}

            <Button onClick={handleClose} disabled={isClosing}>
              {isClosing ? (
                <Loader2 className="mr-2 animate-spin" size={16} />
              ) : (
                <CheckCircle2 className="mr-2" size={16} />
              )}
              {isClosing ? 'Cerrando...' : 'Cerrar día'}
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
