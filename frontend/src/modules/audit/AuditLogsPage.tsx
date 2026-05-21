import { ChevronLeft, ChevronRight, ClipboardList, RefreshCw, Search } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { AuditLogDetailPanel } from './components/AuditLogDetailPanel'
import { AuditLogListItem } from './components/AuditLogListItem'
import { useAuditLogs } from './useAuditLogs'
import type { AuditLogFilters } from './auditLogTypes'

const defaultFilters: AuditLogFilters = {
  action: '',
  dateFrom: '',
  dateTo: '',
  entityName: '',
  page: 1,
  pageSize: 50,
  userId: '',
}

const ACTION_OPTIONS = [
  { label: 'Todas las acciones', value: '' },
  { label: 'Login exitoso', value: 'auth.login_succeeded' },
  { label: 'Login fallido', value: 'auth.login_failed' },
  { label: 'Venta creada', value: 'sale.created' },
  { label: 'Venta completada', value: 'sale.completed' },
  { label: 'Venta cancelada', value: 'sale.cancelled' },
  { label: 'Pago registrado', value: 'payment.registered' },
  { label: 'Factura generada', value: 'invoice.generated' },
  { label: 'Inventario ajustado', value: 'inventory.adjusted' },
  { label: 'Compra recibida', value: 'purchase.received' },
  { label: 'Usuario creado', value: 'user.created' },
  { label: 'Usuario actualizado', value: 'user.updated' },
  { label: 'Rol cambiado', value: 'user.role_changed' },
]

const ENTITY_OPTIONS = [
  { label: 'Todas las entidades', value: '' },
  { label: 'Autenticacion', value: 'Auth' },
  { label: 'Usuarios', value: 'User' },
  { label: 'Ventas', value: 'Sale' },
  { label: 'Pagos', value: 'Payment' },
  { label: 'Facturas', value: 'Invoice' },
  { label: 'Inventario', value: 'Inventory' },
  { label: 'Compras', value: 'Purchase' },
  { label: 'Clientes', value: 'Customer' },
  { label: 'Proveedores', value: 'Supplier' },
]

export default function AuditLogsPage() {
  const [filters, setFilters] = useState(defaultFilters)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const { data, isError, isFetching, isLoading, refetch } = useAuditLogs(filters)
  const items = data?.items ?? []

  const stats = useMemo(() => {
    const failedLogins = items.filter((i) => i.action === 'auth.login_failed').length
    const totalLogins = items.filter((i) => i.action.startsWith('auth.')).length
    const salesEvents = items.filter((i) => i.action.startsWith('sale.')).length
    return { failedLogins, salesEvents, total: data?.totalItems ?? 0, totalLogins }
  }, [items, data?.totalItems])

  function updateFilters(values: Partial<AuditLogFilters>) {
    setFilters((current) => ({ ...current, ...values, page: 1 }))
  }

  const hasFilters = Boolean(
    filters.action || filters.entityName || filters.dateFrom || filters.dateTo || filters.userId,
  )

  return (
    <section className="flex min-h-full flex-col">
      <div className="shrink-0 border-b border-stone-200 bg-white px-4 py-5 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
              <ClipboardList size={13} />
              Sistema
            </p>
            <h2 className="mt-1 text-2xl font-semibold text-stone-950">Auditoria</h2>
            <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
              Historial completo de eventos y cambios realizados en el sistema.
            </p>
          </div>
          <Button
            disabled={isFetching}
            onClick={() => void refetch()}
            size="sm"
            type="button"
            variant="secondary"
          >
            <RefreshCw className={isFetching ? 'animate-spin' : undefined} size={14} />
            Refrescar
          </Button>
        </div>

        <div className="mt-5 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
          <StatChip label="Total registros" tone="stone" value={String(stats.total)} />
          <StatChip label="Eventos visibles" tone="stone" value={String(items.length)} />
          <StatChip label="Logins fallidos" tone="amber" value={String(stats.failedLogins)} />
          <StatChip label="Eventos de ventas" tone="emerald" value={String(stats.salesEvents)} />
        </div>
      </div>

      <div className="grid flex-1 lg:grid-cols-[400px_minmax(0,1fr)]">
        <div className="flex min-h-0 flex-col bg-white lg:border-r lg:border-stone-200">
          <div className="shrink-0 space-y-2 border-b border-stone-200 p-3">
            <Select
              value={filters.action || '_'}
              onValueChange={(v) => updateFilters({ action: v === '_' ? '' : v })}
            >
              <SelectTrigger aria-label="Filtrar accion" className="h-9">
                <SelectValue placeholder="Accion" />
              </SelectTrigger>
              <SelectContent>
                {ACTION_OPTIONS.map((opt) => (
                  <SelectItem key={opt.value || '_'} value={opt.value || '_'}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={filters.entityName || '_'}
              onValueChange={(v) => updateFilters({ entityName: v === '_' ? '' : v })}
            >
              <SelectTrigger aria-label="Filtrar entidad" className="h-9">
                <SelectValue placeholder="Entidad" />
              </SelectTrigger>
              <SelectContent>
                {ENTITY_OPTIONS.map((opt) => (
                  <SelectItem key={opt.value || '_'} value={opt.value || '_'}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <div className="grid grid-cols-2 gap-2">
              <Input
                aria-label="Fecha desde"
                className="h-9 text-xs"
                onChange={(e) => updateFilters({ dateFrom: e.target.value })}
                type="date"
                value={filters.dateFrom}
              />
              <Input
                aria-label="Fecha hasta"
                className="h-9 text-xs"
                onChange={(e) => updateFilters({ dateTo: e.target.value })}
                type="date"
                value={filters.dateTo}
              />
            </div>
            {hasFilters && (
              <Button
                className="w-full"
                onClick={() => setFilters(defaultFilters)}
                size="sm"
                type="button"
                variant="ghost"
              >
                Limpiar filtros
              </Button>
            )}
          </div>

          <div className="flex-1">
            {isLoading ? (
              <ListSkeleton />
            ) : isError ? (
              <ListError onRetry={() => void refetch()} />
            ) : items.length === 0 ? (
              <ListEmpty hasFilters={hasFilters} />
            ) : (
              <ul className="divide-y divide-stone-100">
                {items.map((log) => (
                  <AuditLogListItem
                    key={log.auditLogId}
                    log={log}
                    onClick={() => setSelectedId(log.auditLogId)}
                    selected={selectedId === log.auditLogId}
                  />
                ))}
              </ul>
            )}
          </div>

          {data && data.totalPages > 1 && (
            <div className="shrink-0 border-t border-stone-200 px-3 py-2">
              <div className="flex items-center justify-between gap-2">
                <p className="text-xs font-medium text-stone-500">
                  Pagina {data.page} de {data.totalPages}
                </p>
                <div className="flex gap-1">
                  <Button
                    aria-label="Pagina anterior"
                    disabled={!data.hasPreviousPage}
                    onClick={() => setFilters((f) => ({ ...f, page: f.page - 1 }))}
                    size="icon"
                    type="button"
                    variant="secondary"
                  >
                    <ChevronLeft size={14} />
                  </Button>
                  <Button
                    aria-label="Pagina siguiente"
                    disabled={!data.hasNextPage}
                    onClick={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
                    size="icon"
                    type="button"
                    variant="secondary"
                  >
                    <ChevronRight size={14} />
                  </Button>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="bg-stone-50">
          {selectedId ? (
            <AuditLogDetailPanel
              auditLogId={selectedId}
              key={selectedId}
              onClose={() => setSelectedId(null)}
            />
          ) : (
            <DetailEmptyState />
          )}
        </div>
      </div>
    </section>
  )
}

// ── Stat chip ────────────────────────────────────────────────────────────────

type ChipTone = 'stone' | 'emerald' | 'amber'

const toneClasses: Record<ChipTone, string> = {
  amber: 'bg-amber-50 ring-amber-200 text-amber-800',
  emerald: 'bg-emerald-50 ring-emerald-200 text-emerald-800',
  stone: 'bg-stone-50 ring-stone-200 text-stone-700',
}

function StatChip({ label, tone, value }: Readonly<{ label: string; tone: ChipTone; value: string }>) {
  return (
    <div className={`flex items-center justify-between rounded-md px-3 py-2.5 ring-1 ${toneClasses[tone]}`}>
      <span className="text-xs font-semibold uppercase tracking-wide">{label}</span>
      <span className="text-lg font-semibold tabular-nums">{value}</span>
    </div>
  )
}

// ── List states ──────────────────────────────────────────────────────────────

const skeletonKeys = ['a', 'b', 'c', 'd', 'e', 'f']

function ListSkeleton() {
  return (
    <ul className="divide-y divide-stone-100">
      {skeletonKeys.map((k) => (
        <li className="space-y-1.5 px-3 py-3" key={k}>
          <div className="h-3 w-1/3 rounded bg-stone-100" />
          <div className="h-3.5 w-2/3 rounded bg-stone-100" />
          <div className="h-3 w-1/2 rounded bg-stone-100" />
        </li>
      ))}
    </ul>
  )
}

function ListError({ onRetry }: Readonly<{ onRetry: () => void }>) {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
        Error al cargar los registros de auditoria.
      </div>
      <Button onClick={onRetry} size="sm" type="button" variant="secondary">
        <RefreshCw size={14} />
        Reintentar
      </Button>
    </div>
  )
}

function ListEmpty({ hasFilters }: Readonly<{ hasFilters: boolean }>) {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-2 p-8 text-center">
      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-stone-100">
        {hasFilters ? <Search className="text-stone-500" size={18} /> : <ClipboardList className="text-stone-500" size={18} />}
      </div>
      <p className="text-sm font-semibold text-stone-900">
        {hasFilters ? 'Sin resultados' : 'Sin registros'}
      </p>
      <p className="max-w-xs text-xs font-medium text-stone-500">
        {hasFilters
          ? 'Ajusta los filtros para ver mas eventos.'
          : 'Los eventos del sistema apareceran aqui automaticamente.'}
      </p>
    </div>
  )
}

function DetailEmptyState() {
  return (
    <div className="flex h-full min-h-[420px] flex-col items-center justify-center gap-4 p-8 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-white ring-1 ring-stone-200">
        <ClipboardList className="text-stone-400" size={28} />
      </div>
      <div>
        <p className="text-base font-semibold text-stone-900">Selecciona un evento</p>
        <p className="mt-1 max-w-sm text-sm font-medium text-stone-500">
          Elige un registro de la lista para ver sus detalles completos, metadata y datos de contexto.
        </p>
      </div>
    </div>
  )
}
