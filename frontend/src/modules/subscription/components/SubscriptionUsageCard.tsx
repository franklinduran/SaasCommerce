import type { LucideIcon } from 'lucide-react'
import { Building2, Package, ReceiptText, Users } from 'lucide-react'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { useSubscriptionUsage } from '@/modules/subscription/hooks/useSubscriptionUsage'
import type {
  ResourceUsage,
  SubscriptionResourceKey,
  SubscriptionUsage,
} from '@/modules/subscription/types'
import { cn } from '@/shared/utils/cn'

type SubscriptionUsageCardProps = {
  error?: Error | null
  isLoading?: boolean
  usage?: SubscriptionUsage
}

const resources: Array<{
  description: string
  icon: LucideIcon
  key: SubscriptionResourceKey
  label: string
}> = [
  { description: 'Locales operativos', icon: Building2, key: 'branches', label: 'Sucursales' },
  { description: 'Usuarios con acceso', icon: Users, key: 'users', label: 'Usuarios' },
  { description: 'Catalogo activo', icon: Package, key: 'products', label: 'Productos' },
  { description: 'Ventas del periodo', icon: ReceiptText, key: 'sales', label: 'Ventas mensuales' },
]

export function SubscriptionUsageCard({
  error,
  isLoading,
  usage: providedUsage,
}: Readonly<SubscriptionUsageCardProps>) {
  const query = useSubscriptionUsage({ enabled: providedUsage === undefined })
  const usage = providedUsage ?? query.usage
  const loading = isLoading ?? query.isLoading
  const loadError = error ?? query.error

  return (
    <Card className="rounded-md">
      <CardHeader className="border-b border-stone-200">
        <div className="flex flex-col gap-1 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h3 className="text-base font-semibold text-stone-950">Uso contra limites</h3>
            <p className="text-sm font-medium text-stone-600">Consumo actual del negocio en el periodo vigente.</p>
          </div>
          {usage && (
            <span className="rounded-md bg-stone-100 px-2 py-1 text-xs font-semibold text-stone-700">
              {usage.planName}
            </span>
          )}
        </div>
      </CardHeader>
      <CardContent className="pt-4 sm:pt-5">
        {loading && <UsageSkeleton />}
        {!loading && loadError && (
          <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
            No se pudo cargar el uso de la suscripcion.
          </p>
        )}
        {!loading && !loadError && !usage && (
          <p className="rounded-md bg-stone-50 px-3 py-2 text-sm font-medium text-stone-600 ring-1 ring-stone-200">
            No hay datos de uso disponibles.
          </p>
        )}
        {!loading && usage && (
          <div className="grid gap-3">
            {resources.map((resource) => (
              <UsageRow
                description={resource.description}
                icon={resource.icon}
                key={resource.key}
                label={resource.label}
                usage={usage[resource.key]}
              />
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

function UsageRow({
  description,
  icon: Icon,
  label,
  usage,
}: Readonly<{
  description: string
  icon: LucideIcon
  label: string
  usage: ResourceUsage
}>) {
  const percentage = getPercentage(usage)
  let state: 'limit' | 'warning' | 'ok'
  if (usage.isAtLimit) {
    state = 'limit'
  } else if (percentage >= 80) {
    state = 'warning'
  } else {
    state = 'ok'
  }

  return (
    <div className="rounded-md border border-stone-200 bg-white p-3">
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-start gap-3">
          <span className={cn(
            'flex h-9 w-9 shrink-0 items-center justify-center rounded-md',
            state === 'limit' && 'bg-red-50 text-red-700',
            state === 'warning' && 'bg-amber-50 text-amber-700',
            state === 'ok' && 'bg-stone-100 text-stone-700',
          )}>
            <Icon aria-hidden="true" size={17} />
          </span>
          <div className="min-w-0">
            <p className="font-semibold text-stone-950">{label}</p>
            <p className="text-sm font-medium text-stone-500">{description}</p>
          </div>
        </div>
        <div className="shrink-0 text-right">
          <p className="text-sm font-semibold tabular-nums text-stone-950">
            {usage.current.toLocaleString('es-DO')} / {formatLimit(usage.maximum)}
          </p>
          <p className={cn(
            'text-xs font-semibold',
            state === 'limit' && 'text-red-700',
            state === 'warning' && 'text-amber-700',
            state === 'ok' && 'text-stone-500',
          )}>
            {usage.isAtLimit ? 'Limite alcanzado' : `${Math.round(percentage)}% usado`}
          </p>
        </div>
      </div>
      <div className="mt-3 h-2 overflow-hidden rounded-full bg-stone-100">
        <div
          aria-hidden="true"
          className={cn(
            'h-full rounded-full transition-all',
            state === 'limit' && 'bg-red-600',
            state === 'warning' && 'bg-amber-500',
            state === 'ok' && 'bg-stone-900',
          )}
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  )
}

function UsageSkeleton() {
  return (
    <div className="grid gap-3">
      {[1, 2, 3, 4].map((item) => (
        <div className="h-20 animate-pulse rounded-md bg-stone-100" key={item} />
      ))}
    </div>
  )
}

function getPercentage(usage: ResourceUsage): number {
  if (usage.maximum <= 0) return 0

  return Math.min((usage.current / usage.maximum) * 100, 100)
}

function formatLimit(value: number): string {
  if (value === 999 || value >= 999999) return 'Ilimitado'

  return value.toLocaleString('es-DO')
}
