import { Check, Loader2, Minus } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { useSubscriptionPlans } from '@/modules/subscription/hooks/useSubscription'
import type { SubscriptionPlanResponse } from '@/modules/subscription/types'
import { cn } from '@/shared/utils/cn'

type PlanComparisonCardProps = {
  currentPlanId?: string
  isLoading?: boolean
  onSelectPlan?: (planId: string) => void | Promise<void>
  plans?: SubscriptionPlanResponse[]
  title?: string
}

const baseRows: Array<{
  getValue: (plan: SubscriptionPlanResponse) => string
  label: string
}> = [
  { getValue: (plan) => formatMoney(plan.monthlyPrice), label: 'Precio mensual' },
  { getValue: (plan) => formatLimit(plan.maxBranches), label: 'Sucursales' },
  { getValue: (plan) => formatLimit(plan.maxUsers), label: 'Usuarios' },
  { getValue: (plan) => formatLimit(plan.maxProducts), label: 'Productos' },
  { getValue: (plan) => formatLimit(plan.maxSalesPerMonth), label: 'Ventas mensuales' },
]

export function PlanComparisonCard({
  currentPlanId,
  isLoading = false,
  onSelectPlan,
  plans: providedPlans,
  title = 'Comparativa de planes',
}: Readonly<PlanComparisonCardProps>) {
  const plansQuery = useSubscriptionPlans()
  const plans = providedPlans ?? plansQuery.data ?? []
  const loadingPlans = providedPlans === undefined && plansQuery.isLoading
  const features = getFeatureList(plans)
  const showActions = Boolean(onSelectPlan)

  return (
    <Card className="overflow-hidden rounded-md">
      <CardHeader className="border-b border-stone-200">
        <div className="flex flex-col gap-1 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h3 className="text-base font-semibold text-stone-950">{title}</h3>
            <p className="text-sm font-medium text-stone-600">
              Limites comerciales y funciones habilitadas por plan.
            </p>
          </div>
          {loadingPlans && <Loader2 className="animate-spin text-stone-500" size={18} />}
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {plansQuery.isError && providedPlans === undefined && (
          <div className="p-4 text-sm font-semibold text-red-700">
            No se pudieron cargar los planes.
          </div>
        )}
        {!loadingPlans && plans.length === 0 && (
          <div className="p-4 text-sm font-medium text-stone-600">
            No hay planes activos para mostrar.
          </div>
        )}
        {plans.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[880px] text-sm">
              <thead className="bg-stone-50">
                <tr className="border-b border-stone-200">
                  <th className="w-56 px-4 py-3 text-left text-xs font-semibold uppercase text-stone-500">
                    Plan
                  </th>
                  {plans.map((plan) => (
                    <th className="px-4 py-3 text-left align-top" key={plan.id}>
                      <div className="flex min-w-40 flex-col gap-1">
                        <div className="flex items-center gap-2">
                          <span className="text-base font-semibold text-stone-950">{plan.name}</span>
                          {plan.id === currentPlanId && (
                            <span className="rounded-md bg-stone-900 px-2 py-0.5 text-xs font-semibold text-white">
                              Actual
                            </span>
                          )}
                        </div>
                        <span className="text-xs font-medium uppercase text-stone-400">{plan.code}</span>
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {baseRows.map((row) => (
                  <tr className="border-b border-stone-100" key={row.label}>
                    <td className="bg-stone-50/60 px-4 py-3 font-semibold text-stone-700">{row.label}</td>
                    {plans.map((plan) => (
                      <td className="px-4 py-3 font-medium text-stone-950" key={plan.id}>
                        {row.getValue(plan)}
                      </td>
                    ))}
                  </tr>
                ))}

                {features.map((feature) => (
                  <tr className="border-b border-stone-100" key={feature}>
                    <td className="bg-stone-50/60 px-4 py-3 font-semibold text-stone-700">
                      {formatFeatureName(feature)}
                    </td>
                    {plans.map((plan) => {
                      const hasFeature = plan.features.includes(feature)
                      return (
                        <td className="px-4 py-3" key={plan.id}>
                          <span className={cn(
                            'inline-flex h-7 w-7 items-center justify-center rounded-md',
                            hasFeature ? 'bg-emerald-50 text-emerald-700' : 'bg-stone-100 text-stone-400',
                          )}>
                            {hasFeature ? <Check aria-label="Incluido" size={15} /> : <Minus aria-label="No incluido" size={15} />}
                          </span>
                        </td>
                      )
                    })}
                  </tr>
                ))}

                {showActions && (
                  <tr>
                    <td className="bg-stone-50/60 px-4 py-3" />
                    {plans.map((plan) => (
                      <td className="px-4 py-3" key={plan.id}>
                        <Button
                          className="w-full"
                          disabled={isLoading || currentPlanId === plan.id}
                          onClick={() => onSelectPlan?.(plan.id)}
                          size="sm"
                          type="button"
                          variant={currentPlanId === plan.id ? 'secondary' : 'default'}
                        >
                          {currentPlanId === plan.id ? 'Plan actual' : 'Cambiar'}
                        </Button>
                      </td>
                    ))}
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

function getFeatureList(plans: SubscriptionPlanResponse[]): string[] {
  return Array.from(new Set(plans.flatMap((plan) => plan.features))).sort((a, b) =>
    a.localeCompare(b, 'es-DO'),
  )
}

function formatMoney(value: number): string {
  return new Intl.NumberFormat('es-DO', {
    currency: 'DOP',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency',
  }).format(value)
}

function formatLimit(value: number): string {
  if (value === 999 || value >= 999999) return 'Ilimitado'

  return value.toLocaleString('es-DO')
}

function formatFeatureName(value: string): string {
  const labels: Record<string, string> = {
    AdvancedReports: 'Reportes avanzados',
    AuditLogs: 'Auditoria',
    Branches: 'Sucursales',
    InventoryTransfers: 'Transferencias',
    Invoices: 'Recibos',
    Payments: 'Abonos',
    Products: 'Productos',
    Purchases: 'Compras',
    Reports: 'Reportes',
    Sales: 'Ventas',
    Users: 'Usuarios',
  }

  return labels[value] ?? value.replace(/([a-z])([A-Z])/g, '$1 $2')
}
