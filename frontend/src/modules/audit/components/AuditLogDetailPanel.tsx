import {
  Calendar,
  ClipboardList,
  Globe,
  Hash,
  Link as LinkIcon,
  Monitor,
  RefreshCw,
  User,
  X,
} from 'lucide-react'
import { useAuditLogDetail } from '../useAuditLogs'
import { AuditLogActionBadge } from './AuditLogActionBadge'
import { Button } from '@/shared/components/ui/button'

type AuditLogDetailPanelProps = {
  auditLogId: string
  onClose: () => void
}

function formatDate(iso: string) {
  try {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      month: 'long',
      second: '2-digit',
      year: 'numeric',
    }).format(new Date(iso))
  } catch {
    return iso
  }
}

function prettyJson(value: string | null): string | null {
  if (!value) return null
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

export function AuditLogDetailPanel({ auditLogId, onClose }: Readonly<AuditLogDetailPanelProps>) {
  const { data, isError, isLoading, refetch } = useAuditLogDetail(auditLogId)

  if (isLoading) {
    return <DetailSkeleton />
  }

  if (isError || !data) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
        <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          Error al cargar el detalle.
        </div>
        <Button onClick={() => void refetch()} size="sm" type="button" variant="secondary">
          <RefreshCw size={14} />
          Reintentar
        </Button>
      </div>
    )
  }

  const metadata = prettyJson(data.metadataJson)

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="sticky top-0 z-10 border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-center gap-3">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white shadow-sm">
              <ClipboardList size={20} />
            </div>
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <AuditLogActionBadge action={data.action} />
                <span className="text-xs font-medium text-stone-500">
                  {data.entityName}
                </span>
              </div>
              <h3 className="mt-1 truncate text-base font-semibold text-stone-950">
                {data.description ?? `${data.entityName} · ${data.action}`}
              </h3>
              <p className="mt-0.5 text-xs font-medium text-stone-500">{formatDate(data.createdAt)}</p>
            </div>
          </div>
          <Button
            aria-label="Cerrar panel"
            className="lg:hidden"
            onClick={onClose}
            size="icon"
            type="button"
            variant="ghost"
          >
            <X size={16} />
          </Button>
        </div>
      </div>

      <div className="min-h-0 flex-1 space-y-4 p-4 sm:p-6 lg:p-8">
        <Section title="Quien y donde">
          <dl className="grid gap-4 sm:grid-cols-2">
            <Detail
              icon={<User size={13} />}
              label="Usuario"
              value={data.userFullName ?? 'Sistema'}
              hint={data.userId ?? undefined}
            />
            <Detail
              icon={<Globe size={13} />}
              label="Direccion IP"
              value={data.ipAddress ?? '—'}
              mono
            />
            <Detail
              className="sm:col-span-2"
              icon={<Monitor size={13} />}
              label="User Agent"
              value={data.userAgent ?? '—'}
              mono
            />
          </dl>
        </Section>

        <Section title="Que ocurrio">
          <dl className="grid gap-4 sm:grid-cols-2">
            <Detail icon={<Hash size={13} />} label="ID del registro" mono value={data.auditLogId} />
            <Detail
              icon={<LinkIcon size={13} />}
              label="ID de correlacion"
              mono
              value={data.correlationId ?? '—'}
            />
            <Detail icon={<ClipboardList size={13} />} label="Entidad" value={data.entityName} />
            <Detail icon={<Hash size={13} />} label="ID de entidad" mono value={data.entityId ?? '—'} />
            <Detail
              className="sm:col-span-2"
              icon={<Calendar size={13} />}
              label="Fecha y hora"
              value={formatDate(data.createdAt)}
            />
          </dl>
        </Section>

        {metadata && (
          <Section description="Datos adicionales en formato JSON." title="Metadata">
            <pre className="overflow-x-auto whitespace-pre-wrap break-all rounded-md bg-stone-950 p-3 font-mono text-xs leading-relaxed text-stone-100">
              {metadata}
            </pre>
          </Section>
        )}
      </div>
    </div>
  )
}

function Section({
  children,
  description,
  title,
}: Readonly<{
  children: React.ReactNode
  description?: string
  title: string
}>) {
  return (
    <section className="rounded-md bg-white p-4 ring-1 ring-stone-200 sm:p-5">
      <header className="mb-4">
        <h4 className="text-sm font-semibold text-stone-950">{title}</h4>
        {description && (
          <p className="mt-0.5 text-xs font-medium text-stone-500">{description}</p>
        )}
      </header>
      {children}
    </section>
  )
}

function Detail({
  className,
  hint,
  icon,
  label,
  mono,
  value,
}: Readonly<{
  className?: string
  hint?: string
  icon: React.ReactNode
  label: string
  mono?: boolean
  value: string
}>) {
  return (
    <div className={className}>
      <dt className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
        {icon}
        {label}
      </dt>
      <dd className={`mt-1 break-words text-sm font-medium text-stone-900 ${mono ? 'font-mono' : ''}`}>
        {value}
      </dd>
      {hint && <dd className="mt-0.5 font-mono text-[11px] text-stone-500">{hint}</dd>}
    </div>
  )
}

function DetailSkeleton() {
  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-center gap-3">
          <div className="h-12 w-12 shrink-0 rounded-md bg-stone-100" />
          <div className="min-w-0 flex-1 space-y-2">
            <div className="h-4 w-1/2 rounded bg-stone-100" />
            <div className="h-3 w-1/3 rounded bg-stone-100" />
          </div>
        </div>
      </div>
      <div className="space-y-4 p-4 sm:p-6 lg:p-8">
        <div className="h-32 rounded-md bg-white ring-1 ring-stone-200" />
        <div className="h-40 rounded-md bg-white ring-1 ring-stone-200" />
      </div>
    </div>
  )
}
