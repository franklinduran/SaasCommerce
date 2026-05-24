import { AlertTriangle, CalendarDays, CheckCircle2, Loader2 } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'
import { useBranches } from '@/modules/branches/hooks/useBranches'
import {
  useCreateDailyClosing,
  useDailyClosingPreview,
} from '@/modules/daily-closing/hooks/useDailyClosing'
import type { DailyClosingAlertType } from '@/modules/daily-closing/types'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { Textarea } from '@/shared/components/ui/textarea'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function todayString() {
  const d = new Date()
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
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

export function DailyClosingPage() {
  const navigate = useNavigate()
  const session = useAuthStore((state) => state.session)
  const defaultBranchId = session?.user.branchId ?? ''

  const [date, setDate] = useState(todayString())
  const [branchId, setBranchId] = useState(defaultBranchId)
  const [notes, setNotes] = useState('')

  const { data: branchesData } = useBranches({ isActive: true })
  const branchList = branchesData?.items ?? []

  useEffect(() => {
    if (!branchId && branchList.length > 0) {
      setBranchId(branchList[0].id)
    }
  }, [branchId, branchList])

  const canPreview = Boolean(date) && Boolean(branchId)

  const {
    data: preview,
    isLoading: isPreviewLoading,
    isError: isPreviewError,
    isFetching: isPreviewFetching,
  } = useDailyClosingPreview(date, branchId)

  const {
    mutate: createClosing,
    isPending: isCreating,
    error: createError,
  } = useCreateDailyClosing()

  function handleCreate() {
    if (!date || !branchId) return
    createClosing(
      { date, branchId, notes: notes.trim() || undefined },
      { onSuccess: (closing) => navigate(`/daily-closing/${closing.id}`) },
    )
  }

  const isLoadingPreview = isPreviewLoading || isPreviewFetching

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Cierre del día</h2>
          <p className="text-sm text-stone-500">Consolida y cierra las operaciones del día.</p>
        </div>
        <Button variant="outline" onClick={() => navigate('/daily-closing/history')}>
          Historial
        </Button>
      </div>

      {/* Controls */}
      <Card>
        <CardContent className="grid gap-4 pt-6 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="closing-date">Fecha</Label>
            <input
              id="closing-date"
              type="date"
              value={date}
              max={todayString()}
              onChange={(e) => setDate(e.target.value)}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="closing-branch">Sucursal</Label>
            {branchList.length > 0 ? (
              <Select value={branchId} onValueChange={setBranchId}>
                <SelectTrigger id="closing-branch">
                  <SelectValue placeholder="Selecciona sucursal" />
                </SelectTrigger>
                <SelectContent>
                  {branchList.map((b) => (
                    <SelectItem key={b.id} value={b.id}>
                      {b.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <p className="text-sm text-stone-500">Cargando sucursales...</p>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Loading */}
      {isLoadingPreview && canPreview && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      )}

      {/* Error */}
      {isPreviewError && canPreview && !isLoadingPreview && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Error al cargar la vista previa. Verifica la fecha y sucursal.
        </p>
      )}

      {/* Empty state before selecting */}
      {!canPreview && (
        <Card>
          <CardContent className="flex h-32 items-center justify-center">
            <div className="flex items-center gap-2 text-stone-400">
              <CalendarDays size={20} />
              <p className="text-sm">Selecciona fecha y sucursal para ver la vista previa.</p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Preview data */}
      {preview && !isLoadingPreview && (
        <>
          {/* Summary cards */}
          <div className="grid gap-4 sm:grid-cols-3">
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
                  Total ventas
                </CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-xl font-semibold text-stone-900">
                  {formatCurrency(preview.totalSales)}
                </p>
                <p className="mt-1 text-xs text-stone-500">{preview.salesCount} transacciones</p>
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
                  className={`text-xl font-semibold ${preview.estimatedNetProfit >= 0 ? 'text-green-700' : 'text-red-700'}`}
                >
                  {formatCurrency(preview.estimatedNetProfit)}
                </p>
                <p className="mt-1 text-xs text-stone-500">
                  {preview.netMarginPercent.toFixed(1)}% margen neto
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
                  {formatCurrency(preview.cashExpected)}
                </p>
              </CardContent>
            </Card>
          </div>

          {/* Sales by payment method */}
          <Card>
            <CardHeader>
              <CardTitle className="text-sm font-semibold">Ventas por método de pago</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-2 sm:grid-cols-4">
              {[
                { label: 'Efectivo', value: preview.cashSales },
                { label: 'Transferencia', value: preview.transferSales },
                { label: 'Tarjeta', value: preview.cardSales },
                { label: 'Crédito', value: preview.creditSales },
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
            <CardContent className="grid gap-2 sm:grid-cols-3">
              <div>
                <p className="text-xs font-medium text-stone-500">Gastos operativos</p>
                <p className="mt-0.5 text-sm font-semibold text-stone-900">
                  {formatCurrency(preview.totalExpenses)}
                </p>
              </div>
              <div>
                <p className="text-xs font-medium text-stone-500">Costo de ventas</p>
                <p className="mt-0.5 text-sm font-semibold text-stone-900">
                  {formatCurrency(preview.totalCost)}
                </p>
              </div>
              <div>
                <p className="text-xs font-medium text-stone-500">Ganancia bruta</p>
                <p
                  className={`mt-0.5 text-sm font-semibold ${preview.grossProfit >= 0 ? 'text-green-700' : 'text-red-700'}`}
                >
                  {formatCurrency(preview.grossProfit)}
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Credits */}
          {(preview.newCreditsCount > 0 || preview.creditPaymentsReceived > 0) && (
            <Card>
              <CardHeader>
                <CardTitle className="text-sm font-semibold">Créditos (Fiados)</CardTitle>
              </CardHeader>
              <CardContent className="grid gap-2 sm:grid-cols-3">
                <div>
                  <p className="text-xs font-medium text-stone-500">Nuevos fiados</p>
                  <p className="mt-0.5 text-sm font-semibold text-stone-900">
                    {preview.newCreditsCount} · {formatCurrency(preview.newCreditsAmount)}
                  </p>
                </div>
                <div>
                  <p className="text-xs font-medium text-stone-500">Pagos recibidos</p>
                  <p className="mt-0.5 text-sm font-semibold text-green-700">
                    {formatCurrency(preview.creditPaymentsReceived)}
                  </p>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Alerts */}
          {preview.alerts.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-sm font-semibold text-amber-700">
                  <AlertTriangle size={16} />
                  Alertas ({preview.alerts.length})
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {preview.alerts.map((alert) => (
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

          {/* Create form */}
          <Card>
            <CardHeader>
              <CardTitle className="text-sm font-semibold">Crear cierre del día</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="closing-notes">Notas (opcional)</Label>
                <Textarea
                  id="closing-notes"
                  placeholder="Observaciones del día..."
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  rows={3}
                />
              </div>

              {createError && (
                <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
                  Error al crear el cierre. Intenta nuevamente.
                </p>
              )}

              <Button onClick={handleCreate} disabled={isCreating || !canPreview}>
                {isCreating ? (
                  <Loader2 className="mr-2 animate-spin" size={16} />
                ) : (
                  <CheckCircle2 className="mr-2" size={16} />
                )}
                {isCreating ? 'Creando cierre...' : 'Crear cierre del día'}
              </Button>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  )
}
