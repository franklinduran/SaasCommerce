import { X } from 'lucide-react'
import { useAuditLogDetail } from '../useAuditLogs'
import { AuditLogActionBadge } from './AuditLogActionBadge'

interface AuditLogDetailDrawerProps {
  auditLogId: string | null
  onClose: () => void
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('es-DO', {
    dateStyle: 'medium',
    timeStyle: 'medium',
  })
}

function Row({ label, value }: Readonly<{ label: string; value?: string | null }>) {
  if (!value) return null
  return (
    <div className="grid grid-cols-[120px_1fr] gap-2 py-2 border-b border-stone-100 last:border-0">
      <dt className="text-xs font-semibold uppercase text-stone-500">{label}</dt>
      <dd className="text-sm text-stone-900 break-all">{value}</dd>
    </div>
  )
}

export function AuditLogDetailDrawer({ auditLogId, onClose }: Readonly<AuditLogDetailDrawerProps>) {
  const { data, isLoading, isError } = useAuditLogDetail(auditLogId)

  if (!auditLogId) return null

  return (
    <dialog
      aria-labelledby="audit-detail-title"
      className="fixed inset-0 z-50 m-0 flex max-h-screen max-w-full justify-end border-0 bg-transparent p-0"
      open
    >
      <div className="fixed inset-0 bg-black/20" onClick={onClose} aria-hidden="true" />

      <div className="relative z-10 flex h-full w-full max-w-lg flex-col bg-white shadow-xl">
        <header className="flex items-center justify-between border-b border-stone-200 px-6 py-4">
          <h2 className="text-base font-semibold text-stone-900" id="audit-detail-title">Detalle de auditoría</h2>
          <button
            aria-label="Cerrar detalle"
            className="rounded-md p-1 text-stone-500 hover:bg-stone-100"
            onClick={onClose}
            type="button"
          >
            <X size={18} />
          </button>
        </header>

        <div className="flex-1 overflow-y-auto px-6 py-4">
          {isLoading && (
            <div className="space-y-3">
              {['l1', 'l2', 'l3', 'l4'].map((k) => (
                <div key={k} className="h-8 rounded bg-stone-100 animate-pulse" />
              ))}
            </div>
          )}

          {isError && (
            <p className="text-sm text-red-600">Error al cargar el detalle de auditoría.</p>
          )}

          {data && (
            <dl className="space-y-0">
              <div className="py-2 border-b border-stone-100">
                <dt className="text-xs font-semibold uppercase text-stone-500 mb-1">Acción</dt>
                <dd><AuditLogActionBadge action={data.action} /></dd>
              </div>
              <Row label="ID" value={data.auditLogId} />
              <Row label="Entidad" value={data.entityName} />
              <Row label="ID Entidad" value={data.entityId ?? undefined} />
              <Row label="Usuario" value={data.userFullName ?? data.userId ?? undefined} />
              <Row label="Descripción" value={data.description ?? undefined} />
              <Row label="IP" value={data.ipAddress ?? undefined} />
              <Row label="User Agent" value={data.userAgent ?? undefined} />
              <Row label="Correlación" value={data.correlationId ?? undefined} />
              <Row label="Fecha" value={formatDate(data.createdAt)} />
              {data.metadataJson && (
                <div className="py-2">
                  <dt className="text-xs font-semibold uppercase text-stone-500 mb-1">Metadata</dt>
                  <dd>
                    <pre className="rounded bg-stone-50 p-3 text-xs text-stone-700 overflow-x-auto whitespace-pre-wrap break-all ring-1 ring-stone-200">
                      {data.metadataJson}
                    </pre>
                  </dd>
                </div>
              )}
            </dl>
          )}
        </div>
      </div>
    </dialog>
  )
}
