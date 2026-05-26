import { ArrowLeft, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useBranchProfitability } from '../hooks/useProfitability'
import type { ProfitabilityFilters } from '../types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function defaultFilters(): Omit<ProfitabilityFilters, 'branchId'> {
  const now = new Date()
  const from = new Date(now.getFullYear(), now.getMonth(), 1)
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  return {
    from: from.toISOString().slice(0, 10) + 'T00:00:00.000Z',
    to: to.toISOString().slice(0, 10) + 'T23:59:59.999Z',
  }
}

function branchSummaryText(count: number) {
  if (count === 0) return 'Comparativa de ganancias por sucursal.'

  const suffix = count === 1 ? '' : 'es'
  return `${count} sucursal${suffix} con ventas en el período`
}

export function BranchProfitabilityPage() {
  const navigate = useNavigate()
  const [filters] = useState(defaultFilters)

  const { data: branches = [], isLoading, isError } = useBranchProfitability(filters)

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/profitability')}>
          <ArrowLeft size={18} />
        </Button>
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Rentabilidad por sucursal</h2>
          <p className="text-sm text-stone-500">{branchSummaryText(branches.length)}</p>
        </div>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      )}

      {/* Error */}
      {isError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Error al cargar rentabilidad de sucursales. Intenta nuevamente.
        </p>
      )}

      {/* Empty state */}
      {!isLoading && !isError && branches.length === 0 && (
        <Card>
          <CardContent className="flex h-40 flex-col items-center justify-center gap-2">
            <p className="text-sm text-stone-500">No hay ventas completadas en el período seleccionado.</p>
          </CardContent>
        </Card>
      )}

      {/* Branch cards */}
      {branches.length > 0 && (
        <div className="space-y-4">
          {branches.map((branch, index) => (
            <Card key={branch.branchId}>
              <CardContent className="p-4">
                <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                  <div className="flex items-center gap-3">
                    <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-stone-100 text-xs font-bold text-stone-600">
                      {index + 1}
                    </div>
                    <div>
                      <p className="font-semibold text-stone-900">{branch.branchName}</p>
                      <p className="text-xs text-stone-500">
                        {branch.salesCount} venta{branch.salesCount !== 1 ? 's' : ''}
                      </p>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-x-8 gap-y-2 sm:flex sm:gap-8">
                    <Metric label="Ventas" value={formatCurrency(branch.totalSales)} />
                    <Metric
                      label="Ganancia bruta"
                      value={formatCurrency(branch.grossProfit)}
                      valueClassName={branch.grossProfit >= 0 ? 'text-green-700' : 'text-red-600'}
                    />
                    <Metric
                      label="Gastos"
                      value={formatCurrency(branch.operatingExpenses)}
                      valueClassName="text-stone-600"
                    />
                    <Metric
                      label="Ganancia neta"
                      value={formatCurrency(branch.estimatedNetProfit)}
                      valueClassName={branch.estimatedNetProfit >= 0 ? 'text-green-700 font-bold' : 'text-red-600 font-bold'}
                      subtitle={`${branch.netMarginPercent >= 0 ? '+' : ''}${branch.netMarginPercent.toFixed(1)}% margen`}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

type MetricProps = {
  label: string
  value: string
  valueClassName?: string
  subtitle?: string
}

function Metric({ label, value, valueClassName = 'text-stone-900', subtitle }: Readonly<MetricProps>) {
  return (
    <div className="min-w-0">
      <p className="text-xs text-stone-500">{label}</p>
      <p className={`text-sm font-semibold ${valueClassName}`}>{value}</p>
      {subtitle && <p className="text-xs text-stone-400">{subtitle}</p>}
    </div>
  )
}
