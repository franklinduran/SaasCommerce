import { CalendarDays, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useBranches } from '@/modules/branches/hooks/useBranches'
import { useDailyCashRegisterSummary } from '@/modules/cash-register/hooks/useCashRegister'
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

function differenceLabel(type: string | null): string {
  switch (type) {
    case 'Balanced':
      return 'Cuadrado'
    case 'Surplus':
      return 'Sobrante'
    case 'Shortage':
      return 'Faltante'
    default:
      return '—'
  }
}

function differenceBadgeClass(type: string | null): string {
  switch (type) {
    case 'Balanced':
      return 'border-green-200 bg-green-50 text-green-700'
    case 'Surplus':
      return 'border-amber-200 bg-amber-50 text-amber-700'
    case 'Shortage':
      return 'border-red-200 bg-red-50 text-red-700'
    default:
      return 'border-stone-200 bg-stone-50 text-stone-600'
  }
}

export function DailyCashRegisterSummaryPage() {
  const [date, setDate] = useState(todayString())
  const [selectedBranchId, setSelectedBranchId] = useState('')

  const { data: branchesData } = useBranches({ isActive: true })
  const branchList = branchesData?.items ?? []

  const { data: summary, isLoading, isError } = useDailyCashRegisterSummary({
    date,
    branchId: selectedBranchId || undefined,
  })

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Arqueo diario de cajas</h2>
          <p className="text-sm text-stone-500">Resumen de todas las cajas abiertas y cerradas del día.</p>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="grid gap-4 pt-6 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="summary-date">Fecha</Label>
            <input
              id="summary-date"
              type="date"
              value={date}
              max={todayString()}
              onChange={(e) => setDate(e.target.value)}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="summary-branch">Sucursal</Label>
            {branchList.length > 0 ? (
              <Select value={selectedBranchId} onValueChange={setSelectedBranchId}>
                <SelectTrigger id="summary-branch">
                  <SelectValue placeholder="Todas las sucursales" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todas las sucursales</SelectItem>
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
      {isLoading && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      )}

      {/* Error */}
      {isError && !isLoading && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Error al cargar el arqueo. Intenta nuevamente.
        </p>
      )}

      {/* Summary */}
      {summary && !isLoading && (
        <>
          {/* KPI cards */}
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
                  Cajas abiertas
                </CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-semibold text-stone-900">{summary.openRegisters}</p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
                  Cajas cerradas
                </CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-semibold text-stone-900">{summary.closedRegisters}</p>
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
                  {formatCurrency(summary.totalExpectedCash)}
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
                  Efectivo contado
                </CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-xl font-semibold text-stone-900">
                  {formatCurrency(summary.totalCountedCash)}
                </p>
              </CardContent>
            </Card>
          </div>

          {/* Sales by method */}
          <Card>
            <CardHeader>
              <CardTitle className="text-sm font-semibold">Ventas totales del día</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-2 sm:grid-cols-4">
              {[
                { label: 'Efectivo', value: summary.totalCashSales },
                { label: 'Tarjeta', value: summary.totalCardSales },
                { label: 'Transferencia', value: summary.totalTransferSales },
                { label: 'Crédito', value: summary.totalCreditSales },
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

          {/* Register list */}
          {summary.registers.length === 0 ? (
            <Card>
              <CardContent className="flex h-32 items-center justify-center">
                <div className="flex items-center gap-2 text-stone-400">
                  <CalendarDays size={20} />
                  <p className="text-sm">No hay cajas registradas para esta fecha.</p>
                </div>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardHeader>
                <CardTitle className="text-sm font-semibold">
                  Detalle por caja ({summary.registers.length})
                </CardTitle>
              </CardHeader>
              <CardContent className="p-0">
                <div className="divide-y divide-stone-100">
                  {summary.registers.map((reg) => (
                    <div key={reg.cashRegisterId} className="px-4 py-3">
                      <div className="flex items-start justify-between gap-4">
                        <div className="min-w-0">
                          <div className="flex items-center gap-2">
                            <Badge
                              variant="outline"
                              className={
                                reg.status === 'Open'
                                  ? 'border-green-200 bg-green-50 text-green-700'
                                  : 'border-stone-200 bg-stone-50 text-stone-600'
                              }
                            >
                              {reg.status === 'Open' ? 'Abierta' : 'Cerrada'}
                            </Badge>
                          </div>
                          <div className="mt-2 grid gap-x-6 gap-y-1 text-xs text-stone-500 sm:grid-cols-3">
                            <span>Inicial: {formatCurrency(reg.openingAmount)}</span>
                            <span>Ef. ventas: {formatCurrency(reg.cashSalesTotal)}</span>
                            {reg.expectedCashAmount !== null && (
                              <span>Esperado: {formatCurrency(reg.expectedCashAmount)}</span>
                            )}
                            {reg.countedAmount !== null && (
                              <span>Contado: {formatCurrency(reg.countedAmount)}</span>
                            )}
                          </div>
                        </div>
                        {reg.differenceType && (
                          <Badge
                            variant="outline"
                            className={differenceBadgeClass(reg.differenceType)}
                          >
                            {differenceLabel(reg.differenceType)}
                            {reg.difference !== null && reg.difference !== 0 && (
                              <span className="ml-1">
                                ({reg.difference > 0 ? '+' : ''}
                                {formatCurrency(reg.difference)})
                              </span>
                            )}
                          </Badge>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </>
      )}

      {/* Empty state (no data yet) */}
      {!summary && !isLoading && !isError && (
        <Card>
          <CardContent className="flex h-32 items-center justify-center">
            <div className="flex items-center gap-2 text-stone-400">
              <CalendarDays size={20} />
              <p className="text-sm">Selecciona una fecha para ver el arqueo.</p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

