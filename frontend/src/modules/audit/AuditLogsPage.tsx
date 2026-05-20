import { useState } from 'react'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'
import { AuditLogActionBadge } from './components/AuditLogActionBadge'
import { AuditLogDetailDrawer } from './components/AuditLogDetailDrawer'
import { AuditLogFiltersBar } from './components/AuditLogFilters'
import { useAuditLogs } from './useAuditLogs'
import type { AuditLogFilters } from './auditLogTypes'

const defaultFilters: AuditLogFilters = {
  dateFrom: '',
  dateTo: '',
  userId: '',
  action: '',
  entityName: '',
  page: 1,
  pageSize: 25,
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('es-DO', { dateStyle: 'short', timeStyle: 'short' })
}

const SKELETON_ROWS = ['r1', 'r2', 'r3', 'r4', 'r5']
const SKELETON_COLS = ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']

export default function AuditLogsPage() {
  const [filters, setFilters] = useState(defaultFilters)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const { data, isLoading, isError } = useAuditLogs(filters)

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div>
        <p className="text-sm font-semibold uppercase text-stone-500">Sistema</p>
        <h2 className="mt-1 text-2xl font-semibold text-stone-950">Auditoría</h2>
        <p className="mt-2 text-sm font-medium text-stone-600">
          {data?.totalItems ?? 0} registros
        </p>
      </div>

      <Card>
        <CardHeader>
          <AuditLogFiltersBar filters={filters} onChange={setFilters} />
        </CardHeader>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="border-b border-stone-200 bg-stone-50">
              <tr>
                <th className="px-4 py-3 text-left font-medium text-stone-600">Fecha</th>
                <th className="px-4 py-3 text-left font-medium text-stone-600">Usuario</th>
                <th className="px-4 py-3 text-left font-medium text-stone-600">Acción</th>
                <th className="px-4 py-3 text-left font-medium text-stone-600">Entidad</th>
                <th className="hidden px-4 py-3 text-left font-medium text-stone-600 lg:table-cell">Descripción</th>
                <th className="hidden px-4 py-3 text-left font-medium text-stone-600 xl:table-cell">IP</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-100">
              {isLoading && SKELETON_ROWS.map((k) => (
                <tr key={k}>
                  {SKELETON_COLS.map((c) => (
                    <td key={c} className="px-4 py-3">
                      <div className="h-4 animate-pulse rounded bg-stone-100" />
                    </td>
                  ))}
                </tr>
              ))}

              {!isLoading && isError && (
                <tr>
                  <td className="px-4 py-8 text-center text-red-600" colSpan={6}>
                    Error al cargar los registros de auditoría.
                  </td>
                </tr>
              )}

              {!isLoading && !isError && data?.items.length === 0 && (
                <tr>
                  <td className="px-4 py-12 text-center text-stone-500" colSpan={6}>
                    No hay registros de auditoría con los filtros aplicados.
                  </td>
                </tr>
              )}

              {!isLoading && !isError && data?.items.map((log) => (
                <tr
                  className="cursor-pointer transition-colors hover:bg-stone-50"
                  key={log.auditLogId}
                  onClick={() => setSelectedId(log.auditLogId)}
                >
                  <td className="whitespace-nowrap px-4 py-3 text-stone-600">
                    {formatDate(log.createdAt)}
                  </td>
                  <td className="px-4 py-3 text-stone-900">
                    {log.userFullName ?? log.userId ?? '—'}
                  </td>
                  <td className="px-4 py-3">
                    <AuditLogActionBadge action={log.action} />
                  </td>
                  <td className="px-4 py-3 text-stone-600">{log.entityName}</td>
                  <td className="hidden max-w-xs truncate px-4 py-3 text-stone-600 lg:table-cell">
                    {log.description ?? '—'}
                  </td>
                  <td className="hidden px-4 py-3 font-mono text-xs text-stone-500 xl:table-cell">
                    {log.ipAddress ?? '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-stone-500">
            Página {data.page} de {data.totalPages}
          </p>
          <div className="flex gap-2">
            <Button
              disabled={!data.hasPreviousPage}
              onClick={() => setFilters((f) => ({ ...f, page: f.page - 1 }))}
              type="button"
              variant="secondary"
            >
              Anterior
            </Button>
            <Button
              disabled={!data.hasNextPage}
              onClick={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
              type="button"
              variant="secondary"
            >
              Siguiente
            </Button>
          </div>
        </div>
      )}

      <AuditLogDetailDrawer auditLogId={selectedId} onClose={() => setSelectedId(null)} />
    </section>
  )
}
