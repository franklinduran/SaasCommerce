import { CircleDollarSign, Loader2 } from 'lucide-react'
import type { PropsWithChildren } from 'react'
import { useSubscription } from '@/modules/subscription/hooks/useSubscription'
import { PlanComparisonCard } from '@/modules/subscription/components/PlanComparisonCard'
import { SubscriptionStatus } from '@/modules/subscription/types'
import { Button } from '@/shared/components/ui/button'
import { useAppStore } from '@/shared/hooks/useAppStore'

const ACTIVE_STATUSES: string[] = [
  SubscriptionStatus.Trial,
  SubscriptionStatus.Active,
  SubscriptionStatus.PastDue,
]

/**
 * Blocks access to the full app until the business has an active subscription.
 * Shows a plan-selection screen for first-time users and expired/cancelled accounts.
 */
export function SubscriptionGate({ children }: Readonly<PropsWithChildren>) {
  const { subscription, isLoading, startTrial, isStartingTrial } = useSubscription()
  const businessName = useAppStore((state) => state.businessName)

  // While checking subscription status, show a minimal loading screen
  if (isLoading) {
    return (
      <div className="flex h-dvh flex-col items-center justify-center gap-4 bg-stone-50">
        <Loader2 className="animate-spin text-stone-400" size={32} />
        <p className="text-sm font-medium text-stone-500">Verificando suscripción...</p>
      </div>
    )
  }

  // If subscription is active (Trial / Active / PastDue), render the app normally
  if (subscription && ACTIVE_STATUSES.includes(subscription.status)) {
    return <>{children}</>
  }

  // No subscription, Expired, Suspended, or Cancelled → show plan-selection wall
  const isBlockedStatus =
    subscription?.status === SubscriptionStatus.Suspended ||
    subscription?.status === SubscriptionStatus.Expired ||
    subscription?.status === SubscriptionStatus.Cancelled

  return (
    <div className="flex min-h-dvh flex-col bg-stone-50">
      {/* Minimal top bar */}
      <header className="flex h-14 shrink-0 items-center justify-between border-b border-stone-200 bg-white px-6">
        <div className="flex items-center gap-2.5">
          <span className="flex h-8 w-8 items-center justify-center rounded-md bg-stone-900 text-white">
            <CircleDollarSign aria-hidden="true" size={16} />
          </span>
          <span className="text-sm font-semibold text-stone-900">ComercioFlow</span>
        </div>
        <span className="text-sm font-medium text-stone-500">{businessName}</span>
      </header>

      <main className="flex flex-1 flex-col items-center gap-8 px-4 py-12">
        {/* Headline */}
        <div className="max-w-xl text-center">
          {isBlockedStatus ? (
            <>
              <h1 className="text-2xl font-bold text-stone-900">
                Tu suscripción no está activa
              </h1>
              <p className="mt-2 text-sm font-medium text-stone-500">
                {subscription?.status === SubscriptionStatus.Suspended &&
                  'Tu cuenta está suspendida. Elige un plan para reactivarla.'}
                {subscription?.status === SubscriptionStatus.Expired &&
                  'Tu período de prueba venció. Elige un plan para continuar.'}
                {subscription?.status === SubscriptionStatus.Cancelled &&
                  'Cancelaste tu suscripción. Elige un plan para reactivarla.'}
              </p>
            </>
          ) : (
            <>
              <h1 className="text-2xl font-bold text-stone-900">
                Elige tu plan para comenzar
              </h1>
              <p className="mt-2 text-sm font-medium text-stone-500">
                Empieza con 14 días gratuitos en el plan Básico, sin necesidad de tarjeta.
              </p>
            </>
          )}
        </div>

        {/* Start trial CTA — only shown when no subscription yet */}
        {!subscription && (
          <Button
            className="min-w-48"
            disabled={isStartingTrial}
            onClick={() => { startTrial() }}
            type="button"
          >
            {isStartingTrial ? (
              <>
                <Loader2 className="animate-spin" size={16} />
                Iniciando prueba...
              </>
            ) : (
              'Empezar prueba gratuita de 14 días'
            )}
          </Button>
        )}

        {/* Plan comparison table */}
        <div className="w-full max-w-5xl">
          <PlanComparisonCard
            currentPlanId={subscription?.plan.id}
            title="Planes disponibles"
          />
        </div>

        {/* Contact support link */}
        <p className="text-xs font-medium text-stone-400">
          ¿Tienes preguntas?{' '}
          <a
            className="underline hover:text-stone-600"
            href="mailto:soporte@comercioflowrd.com"
          >
            Contacta a soporte
          </a>
        </p>
      </main>
    </div>
  )
}
