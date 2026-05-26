import type { LucideIcon } from 'lucide-react'
import {
  CalendarClock,
  CreditCard,
  Headphones,
  Loader2,
  RefreshCw,
  RotateCcw,
  ShieldCheck,
  TrendingUp,
  XCircle,
} from 'lucide-react'
import { useRef, useState } from 'react'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { PlanComparisonCard } from '@/modules/subscription/components/PlanComparisonCard'
import { SubscriptionAlertBanner } from '@/modules/subscription/components/SubscriptionAlertBanner'
import { SubscriptionStatusBadge } from '@/modules/subscription/components/SubscriptionStatusBadge'
import { SubscriptionUsageCard } from '@/modules/subscription/components/SubscriptionUsageCard'
import { UpgradeBanner } from '@/modules/subscription/components/UpgradeBanner'
import { useSubscription, useSubscriptionPlans } from '@/modules/subscription/hooks/useSubscription'
import { useSubscriptionUsage } from '@/modules/subscription/hooks/useSubscriptionUsage'
import type {
  BusinessSubscription,
  FeatureStatus,
  SubscriptionPlan,
  SubscriptionUsage,
} from '@/modules/subscription/types'
import { SubscriptionStatus } from '@/modules/subscription/types'
import { cn } from '@/shared/utils/cn'

export function SubscriptionPage() {
  const plansRef = useRef<HTMLDivElement | null>(null)
  const [feedback, setFeedback] = useState<string | null>(null)
  const {
    cancelSubscription,
    changePlan,
    error,
    isCancelling,
    isChangingPlan,
    isLoading,
    isReactivating,
    isStartingTrial,
    reactivateSubscription,
    refetch,
    startTrial,
    subscription,
  } = useSubscription()
  const usageQuery = useSubscriptionUsage({ enabled: Boolean(subscription) })
  const plansQuery = useSubscriptionPlans()

  function scrollToPlans() {
    plansRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }

  function contactSupport() {
    window.location.href = 'mailto:soporte@comercioflowrd.com?subject=Soporte%20suscripcion'
  }

  async function handleStartTrial() {
    setFeedback(null)
    await startTrial()
    setFeedback('Trial iniciado correctamente.')
  }

  async function handleChangePlan(planId: string) {
    setFeedback(null)
    await changePlan(planId)
    setFeedback('Plan actualizado correctamente.')
  }

  async function handleCancel() {
    if (!window.confirm('Confirmas que deseas cancelar la suscripcion?')) return

    setFeedback(null)
    await cancelSubscription()
    setFeedback('Suscripcion cancelada correctamente.')
  }

  async function handleReactivate() {
    setFeedback(null)
    await reactivateSubscription()
    setFeedback('Suscripcion reactivada correctamente.')
  }

  if (isLoading) {
    return (
      <section className="space-y-5 p-4 sm:p-6 lg:p-8">
        <PageHeader isRefreshing onRefresh={() => refetch()} />
        <SubscriptionSkeleton />
      </section>
    )
  }

  if (!subscription) {
    return (
      <section className="space-y-5 p-4 sm:p-6 lg:p-8">
        <PageHeader isRefreshing={false} onRefresh={() => refetch()} />
        <EmptySubscriptionState
          error={error instanceof Error ? error : null}
          isStartingTrial={isStartingTrial}
          onStartTrial={handleStartTrial}
        />
        <PlanComparisonCard plans={plansQuery.data} title="Planes disponibles" />
      </section>
    )
  }

  const usage = usageQuery.usage
  const isBlocked = isBlockedStatus(subscription.status)

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <PageHeader isRefreshing={isLoading} onRefresh={() => refetch()} />

      <SubscriptionAlertBanner
        onChoosePlan={scrollToPlans}
        onContactSupport={contactSupport}
        onReactivateClick={handleReactivate}
        subscription={subscription}
        usage={usage}
      />

      <UpgradeBanner onUpgradeClick={scrollToPlans} usage={usage} />

      {feedback && (
        <div className="flex items-center gap-2 rounded-md bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-700 ring-1 ring-emerald-200">
          <ShieldCheck aria-hidden="true" size={16} />
          {feedback}
        </div>
      )}

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          icon={CreditCard}
          label="Plan actual"
          sub={subscription.plan.code}
          value={subscription.plan.name}
        />
        <MetricCard
          icon={CalendarClock}
          label={subscription.status === SubscriptionStatus.Trial ? 'Fin de trial' : 'Fin de periodo'}
          sub={subscription.status}
          value={formatDate(subscription.trialEndsAt ?? subscription.currentPeriodEnd)}
        />
        <MetricCard
          icon={TrendingUp}
          label="Uso critico"
          sub="Recursos en limite"
          value={String(countReachedLimits(usage))}
        />
        <MetricCard
          danger={isBlocked}
          icon={isBlocked ? XCircle : ShieldCheck}
          label="Acceso comercial"
          sub={isBlocked ? 'Operaciones bloqueadas' : 'Operaciones habilitadas'}
          value={isBlocked ? 'Bloqueado' : 'Operativo'}
        />
      </section>

      <section className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_420px]">
        <CurrentPlanPanel subscription={subscription} usageFeatures={usage?.features ?? []} />
        <SubscriptionUsageCard
          error={usageQuery.error instanceof Error ? usageQuery.error : null}
          isLoading={usageQuery.isLoading}
          usage={usage}
        />
      </section>

      <div ref={plansRef}>
        <PlanComparisonCard
          currentPlanId={subscription.plan.id}
          isLoading={isChangingPlan}
          onSelectPlan={handleChangePlan}
          plans={plansQuery.data}
        />
      </div>

      <ActionsPanel
        isCancelling={isCancelling}
        isReactivating={isReactivating}
        onCancel={handleCancel}
        onContactSupport={contactSupport}
        onReactivate={handleReactivate}
        status={subscription.status}
      />
    </section>
  )
}

function PageHeader({
  isRefreshing,
  onRefresh,
}: Readonly<{
  isRefreshing: boolean
  onRefresh: () => void
}>) {
  return (
    <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
      <div>
        <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
          <CreditCard size={13} />
          Comercial
        </p>
        <h2 className="mt-1 text-2xl font-semibold text-stone-950">Suscripcion</h2>
        <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
          Estado comercial, limites del plan y capacidad operativa del negocio.
        </p>
      </div>
      <Button disabled={isRefreshing} onClick={onRefresh} size="sm" type="button" variant="secondary">
        {isRefreshing ? <Loader2 className="animate-spin" size={14} /> : <RefreshCw size={14} />}
        Refrescar
      </Button>
    </div>
  )
}

function EmptySubscriptionState({
  error,
  isStartingTrial,
  onStartTrial,
}: Readonly<{
  error: Error | null
  isStartingTrial: boolean
  onStartTrial: () => void
}>) {
  return (
    <Card className="rounded-md">
      <CardHeader className="border-b border-stone-200">
        <div className="flex items-start gap-3">
          <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
            <CreditCard aria-hidden="true" size={18} />
          </span>
          <div>
            <h3 className="text-base font-semibold text-stone-950">Sin suscripcion activa</h3>
            <p className="mt-1 text-sm font-medium text-stone-600">
              Inicia un trial para habilitar las operaciones del tenant.
            </p>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-4 pt-4 sm:pt-5">
        {error && (
          <p className="rounded-md bg-amber-50 px-3 py-2 text-sm font-semibold text-amber-800 ring-1 ring-amber-200">
            {toErrorMessage(error)}
          </p>
        )}
        <Button disabled={isStartingTrial} onClick={onStartTrial} type="button">
          {isStartingTrial ? <Loader2 className="animate-spin" size={16} /> : <RotateCcw size={16} />}
          Iniciar trial de 14 dias
        </Button>
      </CardContent>
    </Card>
  )
}

function CurrentPlanPanel({
  subscription,
  usageFeatures,
}: Readonly<{
  subscription: BusinessSubscription
  usageFeatures: FeatureStatus[]
}>) {
  const plan = subscription.plan
  const features = mergeFeatures(plan, usageFeatures)

  return (
    <Card className="rounded-md">
      <CardHeader className="border-b border-stone-200">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <div className="flex flex-wrap items-center gap-2">
              <h3 className="text-base font-semibold text-stone-950">{plan.name}</h3>
              <SubscriptionStatusBadge status={subscription.status} />
            </div>
            <p className="mt-1 text-sm font-medium text-stone-600">{plan.description}</p>
          </div>
          <div className="text-left sm:text-right">
            <p className="text-2xl font-semibold tabular-nums text-stone-950">{formatMoney(plan.monthlyPrice)}</p>
            <p className="text-xs font-semibold uppercase text-stone-400">mensual</p>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-5 pt-4 sm:pt-5">
        <dl className="grid gap-3 sm:grid-cols-2">
          <Definition label="Inicio" value={formatDate(subscription.startedAt)} />
          <Definition label="Periodo actual" value={formatDate(subscription.currentPeriodStart)} />
          <Definition label="Fin del periodo" value={formatDate(subscription.currentPeriodEnd)} />
          <Definition label="Fin de trial" value={formatDate(subscription.trialEndsAt)} />
        </dl>

        <div className="border-t border-stone-200 pt-4">
          <h4 className="text-sm font-semibold text-stone-950">Funciones del plan</h4>
          <div className="mt-3 grid gap-2 sm:grid-cols-2">
            {features.map((feature) => (
              <div
                className={cn(
                  'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium ring-1',
                  feature.enabled
                    ? 'bg-emerald-50 text-emerald-800 ring-emerald-200'
                    : 'bg-stone-50 text-stone-500 ring-stone-200',
                )}
                key={feature.name}
              >
                <span className={cn(
                  'h-2 w-2 shrink-0 rounded-full',
                  feature.enabled ? 'bg-emerald-600' : 'bg-stone-300',
                )} />
                {formatFeatureName(feature.name)}
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

function Definition({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="rounded-md bg-stone-50 px-3 py-2 ring-1 ring-stone-200">
      <dt className="text-xs font-semibold uppercase text-stone-400">{label}</dt>
      <dd className="mt-1 text-sm font-semibold text-stone-900">{value}</dd>
    </div>
  )
}

function MetricCard({
  danger,
  icon: Icon,
  label,
  sub,
  value,
}: Readonly<{
  danger?: boolean
  icon: LucideIcon
  label: string
  sub: string
  value: string
}>) {
  return (
    <Card className="rounded-md bg-white shadow-none">
      <CardHeader>
        <div className="flex items-center justify-between gap-3">
          <p className="text-sm font-semibold text-stone-700">{label}</p>
          <span className={cn(
            'flex h-8 w-8 items-center justify-center rounded-md',
            danger ? 'bg-red-50 text-red-700' : 'bg-stone-100 text-stone-700',
          )}>
            <Icon aria-hidden="true" size={16} />
          </span>
        </div>
      </CardHeader>
      <CardContent>
        <p className="truncate text-2xl font-semibold text-stone-950">{value}</p>
        <p className="mt-1 text-sm font-medium text-stone-500">{sub}</p>
      </CardContent>
    </Card>
  )
}

function ActionsPanel({
  isCancelling,
  isReactivating,
  onCancel,
  onContactSupport,
  onReactivate,
  status,
}: Readonly<{
  isCancelling: boolean
  isReactivating: boolean
  onCancel: () => void
  onContactSupport: () => void
  onReactivate: () => void
  status: SubscriptionStatus
}>) {
  return (
    <Card className="rounded-md">
      <CardHeader className="border-b border-stone-200">
        <h3 className="text-base font-semibold text-stone-950">Acciones comerciales</h3>
        <p className="text-sm font-medium text-stone-600">
          Gestiona cambios de estado sin afectar los datos del tenant.
        </p>
      </CardHeader>
      <CardContent className="flex flex-col gap-3 pt-4 sm:flex-row sm:items-center sm:justify-between sm:pt-5">
        <div className="flex flex-wrap gap-2">
          {status === SubscriptionStatus.Cancelled ? (
            <Button disabled={isReactivating} onClick={onReactivate} type="button">
              {isReactivating ? <Loader2 className="animate-spin" size={16} /> : <RotateCcw size={16} />}
              Reactivar
            </Button>
          ) : (
            <Button disabled={isCancelling} onClick={onCancel} type="button" variant="destructive">
              {isCancelling ? <Loader2 className="animate-spin" size={16} /> : <XCircle size={16} />}
              Cancelar suscripcion
            </Button>
          )}
          <Button onClick={onContactSupport} type="button" variant="secondary">
            <Headphones size={16} />
            Contactar soporte
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

function SubscriptionSkeleton() {
  return (
    <div className="space-y-5">
      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {[1, 2, 3, 4].map((item) => (
          <div className="h-32 animate-pulse rounded-md bg-stone-100" key={item} />
        ))}
      </section>
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_420px]">
        <div className="h-96 animate-pulse rounded-md bg-stone-100" />
        <div className="h-96 animate-pulse rounded-md bg-stone-100" />
      </div>
    </div>
  )
}

function isBlockedStatus(status: SubscriptionStatus): boolean {
  return status === SubscriptionStatus.Cancelled ||
    status === SubscriptionStatus.Expired ||
    status === SubscriptionStatus.PastDue ||
    status === SubscriptionStatus.Suspended
}

function countReachedLimits(usage: SubscriptionUsage | undefined): number {
  if (!usage) return 0

  return [usage.branches, usage.users, usage.products, usage.sales].filter((resource) => resource.isAtLimit).length
}

function mergeFeatures(plan: SubscriptionPlan, usageFeatures: FeatureStatus[]) {
  const names = new Set([...plan.features, ...usageFeatures.map((feature) => feature.name)])

  return Array.from(names)
    .sort((a, b) => a.localeCompare(b, 'es-DO'))
    .map((name) => ({
      enabled: usageFeatures.find((feature) => feature.name === name)?.isEnabled ?? plan.features.includes(name),
      name,
    }))
}

function formatDate(value?: string | null): string {
  if (!value) return 'No definido'

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'No definido'

  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(date)
}

function formatMoney(value: number): string {
  return new Intl.NumberFormat('es-DO', {
    currency: 'DOP',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency',
  }).format(value)
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

function toErrorMessage(error: Error): string {
  return error.message || 'No se pudo cargar la suscripcion.'
}
