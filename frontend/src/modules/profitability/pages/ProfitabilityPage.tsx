import { AlertTriangle, Loader2, TrendingDown, TrendingUp } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useProfitabilitySummary } from '../hooks/useProfitability'
import type { ProfitabilityFilters } from '../types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function formatPercent(value: number) {
  return `${value >= 0 ? '+' : ''}${value.toFixed(1)}%`
}

function defaultFilters(): ProfitabilityFilters {
  const now = new Date()
  const from = new Date(now.getFullYear(), now.getMonth(), 1)
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  return {
    from: from.toISOString().slice(0, 10) + 'T00:00:00.000Z',
    to: to.toISOString().slice(0, 10) + 'T23:59:59.999Z',
  }
}

export function ProfitabilityPage() {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<ProfitabilityFilters>(defaultFilters)
  const [fromInput, setFromInput] = useState(() => filters.from.slice(0, 10))
  const [toInput, setToInput] = useState(() => filters.to.slice(0, 10))

  const { data: summary, isLoading, isError } = useProfitabilitySummary(filters)

  function applyFilters() {
    if (fromInput && toInput) {
      setFilters({
        from: fromInput + 'T00:00:00.000Z',
        to: toInput + 'T23:59:59.999Z',
      })
    }
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Rentabilidad</h2>
          <p className="text-sm text-stone-500">Resumen de ganancias y márgenes del negocio.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => navigate('/profitability/products')}>
            Por producto
          </Button>
          <Button variant="outline" size="sm" onClick={() => navigate('/profitability/branches')}>
            Por sucursal
          </Button>
          <Button variant="outline" size="sm" onClick={() => navigate('/profitability/alerts')}>
            Alertas
          </Button>
        </div>
      </div>

      {/* Date range filter */}
      <div className="flex flex-wrap items-end gap-3">
        <div className="space-y-1">
          <Label htmlFor="from-date" className="text-xs font-medium text-stone-600">
            Desde
          </Label>
          <Input
            id="from-date"
            type="date"
            value={fromInput}
            onChange={(e) => setFromInput(e.target.value)}
            className="w-40"
          />
        </div>
        <div className="space-y-1">
          <Label htmlFor="to-date" className="text-xs font-medium text-stone-600">
            Hasta
          </Label>
          <Input
            id="to-date"
            type="date"
            value={toInput}
            onChange={(e) => setToInput(e.target.value)}
            className="w-40"
          />
        </div>
        <Button size="sm" onClick={applyFilters}>
          Aplicar
        </Button>
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
          Error al cargar los datos de rentabilidad. Intenta nuevamente.
        </p>
      )}

      {/* Summary cards */}
      {summary && !isLoading && (
        <>
          {/* Warning banner */}
          {summary.warningCount > 0 && (
            <div className="flex items-center gap-2 rounded-md border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
              <AlertTriangle size={16} className="shrink-0" />
              <span>
                {summary.warningCount} producto{summary.warningCount !== 1 ? 's' : ''} sin costo registrado.
                Los cálculos pueden ser inexactos.{' '}
                <button
                  type="button"
                  className="underline hover:text-amber-900"
                  onClick={() => navigate('/profitability/alerts')}
                >
                  Ver alertas
                </button>
              </span>
            </div>
          )}

          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            <SummaryCard
              title="Ventas totales"
              value={formatCurrency(summary.totalSales)}
              subtitle={`${summary.salesCount} transaccion${summary.salesCount !== 1 ? 'es' : ''}`}
            />
            <SummaryCard
              title="Costo de ventas"
              value={formatCurrency(summary.totalCost)}
              subtitle="Costo de productos vendidos"
              valueClassName="text-red-700"
            />
            <SummaryCard
              title="Ganancia bruta"
              value={formatCurrency(summary.grossProfit)}
              subtitle={formatPercent(summary.grossMarginPercent) + ' margen bruto'}
              valueClassName={summary.grossProfit >= 0 ? 'text-green-700' : 'text-red-700'}
              icon={summary.grossProfit >= 0 ? TrendingUp : TrendingDown}
              iconClassName={summary.grossProfit >= 0 ? 'text-green-500' : 'text-red-500'}
            />
            <SummaryCard
              title="Ganancia neta estimada"
              value={formatCurrency(summary.estimatedNetProfit)}
              subtitle={formatPercent(summary.netMarginPercent) + ' margen neto'}
              valueClassName={summary.estimatedNetProfit >= 0 ? 'text-green-700' : 'text-red-700'}
              icon={summary.estimatedNetProfit >= 0 ? TrendingUp : TrendingDown}
              iconClassName={summary.estimatedNetProfit >= 0 ? 'text-green-500' : 'text-red-500'}
            />
          </div>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-stone-600">
                  Gastos operativos
                </CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-bold text-stone-900">
                  {formatCurrency(summary.operatingExpenses)}
                </p>
                <p className="text-xs text-stone-500">Gastos pagados en el período</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-stone-600">
                  Margen bruto vs neto
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-6">
                  <div>
                    <p className="text-2xl font-bold text-stone-900">
                      {summary.grossMarginPercent.toFixed(1)}%
                    </p>
                    <p className="text-xs text-stone-500">Margen bruto</p>
                  </div>
                  <div className="h-10 w-px bg-stone-200" />
                  <div>
                    <p className={`text-2xl font-bold ${summary.netMarginPercent >= 0 ? 'text-green-700' : 'text-red-700'}`}>
                      {summary.netMarginPercent.toFixed(1)}%
                    </p>
                    <p className="text-xs text-stone-500">Margen neto</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </>
      )}
    </div>
  )
}

type SummaryCardProps = {
  title: string
  value: string
  subtitle: string
  valueClassName?: string
  icon?: React.ComponentType<{ size?: number; className?: string }>
  iconClassName?: string
}

function SummaryCard({
  title,
  value,
  subtitle,
  valueClassName = 'text-stone-900',
  icon: Icon,
  iconClassName,
}: Readonly<SummaryCardProps>) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium text-stone-600">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex items-center gap-2">
          <p className={`text-xl font-bold ${valueClassName}`}>{value}</p>
          {Icon && <Icon size={18} className={iconClassName} />}
        </div>
        <p className="text-xs text-stone-500">{subtitle}</p>
      </CardContent>
    </Card>
  )
}
