import { Loader2, RefreshCw } from 'lucide-react'
import { usePilotMetrics } from '@/modules/admin/hooks/usePilotMetrics'
import type { RecentPilotBusiness } from '@/modules/admin/types'
import { Button } from '@/shared/components/ui/button'

// ── Helpers ───────────────────────────────────────────────────────────────────

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(iso))
}

function statusLabel(status: string): { label: string; className: string } {
  const map: Record<string, { label: string; className: string }> = {
    Trial: { label: 'Trial', className: 'bg-sky-50 text-sky-700 ring-sky-200' },
    Active: { label: 'Activo', className: 'bg-emerald-50 text-emerald-700 ring-emerald-200' },
    PastDue: { label: 'Vencido', className: 'bg-amber-50 text-amber-700 ring-amber-200' },
    Suspended: { label: 'Suspendido', className: 'bg-red-50 text-red-700 ring-red-200' },
    Cancelled: { label: 'Cancelado', className: 'bg-stone-100 text-stone-600 ring-stone-200' },
    Expired: { label: 'Expirado', className: 'bg-orange-50 text-orange-700 ring-orange-200' },
  }
  return map[status] ?? { label: status, className: 'bg-stone-100 text-stone-600 ring-stone-200' }
}

// ── Metric card ───────────────────────────────────────────────────────────────

function MetricCard({
  label,
  value,
  sub,
  tone = 'stone',
}: Readonly<{
  label: string
  value: number | string
  sub?: string
  tone?: 'stone' | 'sky' | 'emerald' | 'amber' | 'red'
}>) {
  const tones = {
    stone: 'bg-white ring-stone-200',
    sky: 'bg-sky-50 ring-sky-200',
    emerald: 'bg-emerald-50 ring-emerald-200',
    amber: 'bg-amber-50 ring-amber-200',
    red: 'bg-red-50 ring-red-200',
  }
  const valueTones = {
    stone: 'text-stone-900',
    sky: 'text-sky-800',
    emerald: 'text-emerald-800',
    amber: 'text-amber-800',
    red: 'text-red-800',
  }

  return (
    <div className={`rounded-lg p-5 ring-1 ${tones[tone]}`}>
      <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">{label}</p>
      <p className={`mt-2 text-3xl font-bold tabular-nums ${valueTones[tone]}`}>{value}</p>
      {sub && <p className="mt-1 text-xs font-medium text-stone-500">{sub}</p>}
    </div>
  )
}

// ── Recent business row ───────────────────────────────────────────────────────

function BusinessRow({ business }: Readonly<{ business: RecentPilotBusiness }>) {
  const { label, className } = statusLabel(business.subscriptionStatus)

  return (
    <tr className="bg-white hover:bg-stone-50">
      <td className="px-5 py-3 font-semibold text-stone-900">{business.businessName}</td>
      <td className="px-5 py-3">
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ${className}`}>
          {label}
        </span>
      </td>
      <td className="px-5 py-3 text-sm text-stone-600">{business.planName ?? '—'}</td>
      <td className="px-5 py-3 text-sm text-stone-600">{formatDate(business.createdAt)}</td>
      <td className="px-5 py-3 text-sm text-stone-600">
        {business.trialEndsAt ? formatDate(business.trialEndsAt) : '—'}
      </td>
    </tr>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export function PilotMetricsPage() {
  const { data, isLoading, isError, refetch, isFetching } = usePilotMetrics()

  return (
    <section className="space-y-6 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">Administracion</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Metricas del piloto</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Resumen de negocios piloto, estados de suscripcion y actividad reciente.
          </p>
        </div>
        <Button
          disabled={isFetching}
          onClick={() => void refetch()}
          variant="secondary"
        >
          <RefreshCw className={isFetching ? 'animate-spin' : undefined} size={15} />
          Actualizar
        </Button>
      </div>

      {isLoading && (
        <div className="flex h-48 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={28} />
        </div>
      )}

      {isError && (
        <div className="rounded-md bg-red-50 px-4 py-3 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          Error al cargar las metricas. Verifica que tienes acceso de administrador.
        </div>
      )}

      {data && (
        <>
          {/* KPI grid */}
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <MetricCard
              label="Total negocios"
              value={data.totalBusinesses}
              sub="Todos los registrados"
              tone="stone"
            />
            <MetricCard
              label="En trial"
              value={data.activeTrials}
              sub={data.trialsExpiringIn7Days > 0 ? `${data.trialsExpiringIn7Days} expiran en 7 dias` : 'Ninguno expira pronto'}
              tone="sky"
            />
            <MetricCard
              label="Activos"
              value={data.activeSubscriptions}
              sub="Suscripcion activa y pagada"
              tone="emerald"
            />
            <MetricCard
              label="Nuevos (30 dias)"
              value={data.newBusinessesLast30Days}
              sub="Registros recientes"
              tone="stone"
            />
          </div>

          {/* Secondary stats */}
          {(data.suspendedSubscriptions > 0 || data.cancelledSubscriptions > 0) && (
            <div className="grid gap-4 sm:grid-cols-2">
              {data.suspendedSubscriptions > 0 && (
                <MetricCard
                  label="Suspendidos"
                  value={data.suspendedSubscriptions}
                  sub="Requieren atencion"
                  tone="red"
                />
              )}
              {data.cancelledSubscriptions > 0 && (
                <MetricCard
                  label="Cancelados"
                  value={data.cancelledSubscriptions}
                  sub="Ya no activos"
                  tone="amber"
                />
              )}
            </div>
          )}

          {/* Alerts */}
          {data.trialsExpiringIn7Days > 0 && (
            <div className="rounded-md bg-amber-50 px-4 py-3 text-sm font-semibold text-amber-800 ring-1 ring-amber-200">
              ⚠️ {data.trialsExpiringIn7Days} {data.trialsExpiringIn7Days === 1 ? 'negocio expira' : 'negocios expiran'} en los proximos 7 dias. Considera contactarlos para conversion.
            </div>
          )}

          {/* Recent businesses table */}
          <div className="overflow-hidden rounded-md bg-white ring-1 ring-stone-200">
            <div className="border-b border-stone-200 px-5 py-4">
              <h3 className="text-base font-semibold text-stone-950">Negocios recientes</h3>
              <p className="text-sm font-medium text-stone-500">Ultimos 20 registros ordenados por fecha de creacion</p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[640px] text-left text-sm">
                <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
                  <tr>
                    <th className="px-5 py-3">Negocio</th>
                    <th className="px-5 py-3">Estado</th>
                    <th className="px-5 py-3">Plan</th>
                    <th className="px-5 py-3">Registrado</th>
                    <th className="px-5 py-3">Trial hasta</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-stone-100">
                  {data.recentBusinesses.length === 0 && (
                    <tr>
                      <td className="px-5 py-10 text-center text-stone-500" colSpan={5}>
                        No hay negocios registrados aun.
                      </td>
                    </tr>
                  )}
                  {data.recentBusinesses.map((business, index) => (
                    <BusinessRow business={business} key={`${business.businessName}-${index}`} />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </section>
  )
}
