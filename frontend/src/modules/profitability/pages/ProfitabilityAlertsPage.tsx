import { AlertCircle, AlertTriangle, ArrowLeft, Loader2, TrendingDown } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useProfitabilityAlerts } from '../hooks/useProfitability'
import type { ProfitabilityAlert, ProfitabilityFilters } from '../types'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
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

type AlertConfig = {
  label: string
  variant: 'default' | 'secondary' | 'destructive' | 'outline'
  icon: React.ComponentType<{ size?: number; className?: string }>
  iconClass: string
}

function alertConfig(alertType: ProfitabilityAlert['alertType']): AlertConfig {
  switch (alertType) {
    case 'NegativeMargin':
      return {
        label: 'Margen negativo',
        variant: 'destructive',
        icon: TrendingDown,
        iconClass: 'text-red-500',
      }
    case 'MissingCost':
      return {
        label: 'Sin costo',
        variant: 'outline',
        icon: AlertCircle,
        iconClass: 'text-amber-500',
      }
    case 'HighExpenses':
      return {
        label: 'Gastos altos',
        variant: 'secondary',
        icon: AlertTriangle,
        iconClass: 'text-orange-500',
      }
    case 'HighVolumeLowMargin':
      return {
        label: 'Margen bajo',
        variant: 'secondary',
        icon: AlertTriangle,
        iconClass: 'text-yellow-500',
      }
  }
}

function alertsSummaryText(count: number) {
  if (count === 0) return 'Problemas detectados que afectan la rentabilidad.'

  const suffix = count === 1 ? '' : 's'
  return `${count} alerta${suffix} detectada${suffix} en el período.`
}

function alertKey(alert: ProfitabilityAlert) {
  return [
    alert.alertType,
    alert.productId ?? 'no-product',
    alert.branchId ?? 'no-branch',
    alert.message,
  ].join(':')
}

export function ProfitabilityAlertsPage() {
  const navigate = useNavigate()
  const [filters] = useState<ProfitabilityFilters>(defaultFilters)

  const { data: alerts = [], isLoading, isError } = useProfitabilityAlerts(filters)

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/profitability')}>
          <ArrowLeft size={18} />
        </Button>
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Alertas de rentabilidad</h2>
          <p className="text-sm text-stone-500">{alertsSummaryText(alerts.length)}</p>
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
          Error al cargar las alertas. Intenta nuevamente.
        </p>
      )}

      {/* No alerts */}
      {!isLoading && !isError && alerts.length === 0 && (
        <Card>
          <CardContent className="flex h-40 flex-col items-center justify-center gap-2">
            <p className="text-2xl">✅</p>
            <p className="text-sm font-medium text-stone-700">Sin alertas en este período.</p>
            <p className="text-xs text-stone-500">Todos los productos tienen costo y márgenes positivos.</p>
          </CardContent>
        </Card>
      )}

      {/* Alert list */}
      {alerts.length > 0 && (
        <div className="space-y-3">
          {alerts.map((alert) => {
            const config = alertConfig(alert.alertType)
            const Icon = config.icon
            return (
              <Card key={alertKey(alert)}>
                <CardContent className="flex items-start gap-4 p-4">
                  <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-stone-100">
                    <Icon size={18} className={config.iconClass} />
                  </div>
                  <div className="min-w-0 flex-1 space-y-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge variant={config.variant}>{config.label}</Badge>
                      {alert.productName && (
                        <span className="text-sm font-medium text-stone-900">{alert.productName}</span>
                      )}
                      {alert.branchName && (
                        <span className="text-sm font-medium text-stone-900">{alert.branchName}</span>
                      )}
                    </div>
                    <p className="text-sm text-stone-600">{alert.message}</p>
                    {alert.estimatedImpact != null && (
                      <p className="text-xs font-medium text-stone-500">
                        Impacto estimado:{' '}
                        <span className={alert.estimatedImpact < 0 ? 'text-red-600' : 'text-stone-700'}>
                          {formatCurrency(Math.abs(alert.estimatedImpact))}
                        </span>
                      </p>
                    )}
                  </div>
                </CardContent>
              </Card>
            )
          })}
        </div>
      )}
    </div>
  )
}
