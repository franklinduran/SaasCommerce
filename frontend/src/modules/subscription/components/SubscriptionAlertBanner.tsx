import { AlertTriangle, Clock, CreditCard, ShieldAlert, TrendingUp } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { useSubscription } from '@/modules/subscription/hooks/useSubscription'
import { useSubscriptionUsage } from '@/modules/subscription/hooks/useSubscriptionUsage'
import type { BusinessSubscription, SubscriptionUsage } from '@/modules/subscription/types'
import { SubscriptionStatus } from '@/modules/subscription/types'
import { cn } from '@/shared/utils/cn'

type SubscriptionAlertBannerProps = {
  className?: string
  onChoosePlan?: () => void
  onContactSupport?: () => void
  onReactivateClick?: () => void
  subscription?: BusinessSubscription
  usage?: SubscriptionUsage
}

export function SubscriptionAlertBanner({
  className,
  onChoosePlan,
  onContactSupport,
  onReactivateClick,
  subscription: providedSubscription,
  usage: providedUsage,
}: Readonly<SubscriptionAlertBannerProps>) {
  const subscriptionQuery = useSubscription()
  const subscription = providedSubscription ?? subscriptionQuery.subscription
  const usageQuery = useSubscriptionUsage({
    enabled: providedUsage === undefined && Boolean(subscription),
  })
  const usage = providedUsage ?? usageQuery.usage

  if (!subscription) return null

  const alert = getAlert(subscription, usage)
  if (!alert) return null

  const Icon = alert.icon

  return (
    <div
      className={cn(
        'flex flex-col gap-3 rounded-md border px-4 py-3 text-sm sm:flex-row sm:items-center sm:justify-between',
        alert.className,
        className,
      )}
      role="status"
    >
      <div className="flex min-w-0 items-start gap-3">
        <Icon aria-hidden="true" className="mt-0.5 shrink-0" size={18} />
        <div className="min-w-0">
          <p className="font-semibold">{alert.title}</p>
          <p className="mt-0.5 font-medium opacity-90">{alert.message}</p>
        </div>
      </div>
      <div className="flex shrink-0 flex-wrap gap-2">
        {alert.action === 'choose-plan' && onChoosePlan && (
          <Button onClick={onChoosePlan} size="sm" type="button" variant="secondary">
            Ver planes
          </Button>
        )}
        {alert.action === 'reactivate' && onReactivateClick && (
          <Button onClick={onReactivateClick} size="sm" type="button" variant="secondary">
            Reactivar
          </Button>
        )}
        {alert.action === 'support' && onContactSupport && (
          <Button onClick={onContactSupport} size="sm" type="button" variant="secondary">
            Soporte
          </Button>
        )}
      </div>
    </div>
  )
}

function getAlert(subscription: BusinessSubscription, usage?: SubscriptionUsage) {
  const limitReached = getReachedLimit(usage)
  if (limitReached) {
    return {
      action: 'choose-plan' as const,
      className: 'border-amber-200 bg-amber-50 text-amber-900',
      icon: TrendingUp,
      message: `Alcanzaste el limite de ${limitReached}. Cambia de plan para continuar sin interrupciones.`,
      title: 'Limite alcanzado',
    }
  }

  if (subscription.status === SubscriptionStatus.Suspended) {
    return {
      action: 'support' as const,
      className: 'border-red-200 bg-red-50 text-red-900',
      icon: ShieldAlert,
      message: 'Puedes consultar tus datos, pero las operaciones principales estan bloqueadas.',
      title: 'Cuenta suspendida',
    }
  }

  if (subscription.status === SubscriptionStatus.Expired) {
    return {
      action: 'choose-plan' as const,
      className: 'border-red-200 bg-red-50 text-red-900',
      icon: AlertTriangle,
      message: 'Puedes ver informacion en modo lectura. Renueva o cambia el plan para operar.',
      title: 'Suscripcion vencida',
    }
  }

  if (subscription.status === SubscriptionStatus.Cancelled) {
    return {
      action: 'reactivate' as const,
      className: 'border-stone-300 bg-stone-100 text-stone-900',
      icon: CreditCard,
      message: 'La suscripcion esta cancelada. Reactivala para recuperar el acceso comercial.',
      title: 'Suscripcion cancelada',
    }
  }

  if (subscription.status === SubscriptionStatus.PastDue) {
    return {
      action: 'choose-plan' as const,
      className: 'border-amber-200 bg-amber-50 text-amber-900',
      icon: CreditCard,
      message: 'Hay un pago pendiente. Regulariza la suscripcion para evitar suspension.',
      title: 'Pago pendiente',
    }
  }

  const trialDays = daysUntil(subscription.trialEndsAt)
  if (subscription.status === SubscriptionStatus.Trial && trialDays !== null && trialDays <= 7) {
    return {
      action: 'choose-plan' as const,
      className: 'border-sky-200 bg-sky-50 text-sky-900',
      icon: Clock,
      message: `Tu periodo de prueba vence en ${Math.max(trialDays, 0)} dia${trialDays === 1 ? '' : 's'}.`,
      title: 'Trial por vencer',
    }
  }

  const periodDays = daysUntil(subscription.currentPeriodEnd)
  if (subscription.status === SubscriptionStatus.Active && periodDays !== null && periodDays <= 7) {
    return {
      action: 'choose-plan' as const,
      className: 'border-sky-200 bg-sky-50 text-sky-900',
      icon: Clock,
      message: `El periodo actual vence en ${Math.max(periodDays, 0)} dia${periodDays === 1 ? '' : 's'}.`,
      title: 'Renovacion cercana',
    }
  }

  return null
}

function getReachedLimit(usage?: SubscriptionUsage): string | null {
  if (!usage) return null

  if (usage.products.isAtLimit) return 'productos'
  if (usage.sales.isAtLimit) return 'ventas mensuales'
  if (usage.users.isAtLimit) return 'usuarios'
  if (usage.branches.isAtLimit) return 'sucursales'

  return null
}

function daysUntil(value?: string | null): number | null {
  if (!value) return null

  const target = new Date(value).getTime()
  if (Number.isNaN(target)) return null

  return Math.ceil((target - Date.now()) / 86_400_000)
}
