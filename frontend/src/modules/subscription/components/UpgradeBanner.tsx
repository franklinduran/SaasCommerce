import { TrendingUp } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { useSubscriptionUsage } from '@/modules/subscription/hooks/useSubscriptionUsage'
import type { SubscriptionResourceKey, SubscriptionUsage } from '@/modules/subscription/types'

type UpgradeBannerProps = {
  onUpgradeClick?: () => void
  usage?: SubscriptionUsage
}

const labels: Record<SubscriptionResourceKey, string> = {
  branches: 'sucursales',
  products: 'productos',
  sales: 'ventas mensuales',
  users: 'usuarios',
}

export function UpgradeBanner({ onUpgradeClick, usage: providedUsage }: Readonly<UpgradeBannerProps>) {
  const query = useSubscriptionUsage({ enabled: providedUsage === undefined })
  const usage = providedUsage ?? query.usage

  if (!usage) return null

  const nearLimits = (Object.keys(labels) as SubscriptionResourceKey[]).filter((key) => {
    const resource = usage[key]
    if (resource.maximum <= 0 || resource.isAtLimit) return false

    return (resource.current / resource.maximum) * 100 >= 80
  })

  if (nearLimits.length === 0) return null

  return (
    <div className="flex flex-col gap-3 rounded-md border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex items-start gap-3">
        <TrendingUp aria-hidden="true" className="mt-0.5 shrink-0" size={18} />
        <div>
          <p className="font-semibold">Uso cercano al limite</p>
          <p className="mt-0.5 font-medium">
            Revisa {formatList(nearLimits.map((key) => labels[key]))} antes de que se bloqueen operaciones.
          </p>
        </div>
      </div>
      {onUpgradeClick && (
        <Button onClick={onUpgradeClick} size="sm" type="button" variant="secondary">
          Ver planes
        </Button>
      )}
    </div>
  )
}

function formatList(values: string[]): string {
  if (values.length <= 1) return values[0] ?? ''
  if (values.length === 2) return `${values[0]} y ${values[1]}`

  return `${values.slice(0, -1).join(', ')} y ${values[values.length - 1]}`
}
