import { Building2, Plus, RefreshCw, Search } from 'lucide-react'
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
import { SupplierDetailPanel } from '@/modules/suppliers/components/SupplierDetailPanel'
import { SupplierFormDialog } from '@/modules/suppliers/components/SupplierFormDialog'
import { SupplierListItem } from '@/modules/suppliers/components/SupplierListItem'
import { useSuppliers } from '@/modules/suppliers/hooks/useSuppliers'
import type { Supplier, SupplierFilters } from '@/modules/suppliers/types'

const defaultFilters: SupplierFilters = {
  isActive: '',
  page: 1,
  pageSize: 50,
  query: '',
  sortBy: 'name',
  sortDirection: 'asc',
}

export function SuppliersPage() {
  const [filters, setFilters] = useState<SupplierFilters>(defaultFilters)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)

  const suppliers = useSuppliers(filters)
  const items = suppliers.data?.items ?? []

  const stats = useMemo(() => {
    const active = items.filter((s) => s.isActive).length
    const withRnc = items.filter((s) => Boolean(s.rnc)).length
    return { active, inactive: items.length - active, total: items.length, withRnc }
  }, [items])

  function updateFilters(values: Partial<SupplierFilters>) {
    setFilters((current) => ({ ...current, ...values, page: 1 }))
  }

  const hasFilters = Boolean(filters.query || filters.isActive)

  return (
    <section className="flex min-h-full flex-col">
      <div className="shrink-0 border-b border-stone-200 bg-white px-4 py-5 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
              <Building2 size={13} />
              Compras
            </p>
            <h2 className="mt-1 text-2xl font-semibold text-stone-950">Proveedores</h2>
            <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
              Administra los proveedores con los que tu negocio realiza compras.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button
              disabled={suppliers.isFetching}
              onClick={() => void suppliers.refetch()}
              size="sm"
              type="button"
              variant="secondary"
            >
              <RefreshCw className={suppliers.isFetching ? 'animate-spin' : undefined} size={14} />
              Refrescar
            </Button>
            <Button onClick={() => setCreateOpen(true)} size="sm" type="button">
              <Plus size={14} />
              Crear proveedor
            </Button>
          </div>
        </div>

        <div className="mt-5 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
          <StatChip label="Total" tone="stone" value={String(stats.total)} />
          <StatChip label="Activos" tone="emerald" value={String(stats.active)} />
          <StatChip label="Inactivos" tone="amber" value={String(stats.inactive)} />
          <StatChip label="Con RNC" tone="stone" value={String(stats.withRnc)} />
        </div>
      </div>

      <div className="grid flex-1 lg:grid-cols-[400px_minmax(0,1fr)]">
        <div className="flex min-h-0 flex-col bg-white lg:border-r lg:border-stone-200">
          <div className="shrink-0 space-y-2 border-b border-stone-200 p-3">
            <div className="relative">
              <Search
                aria-hidden="true"
                className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400"
                size={14}
              />
              <Input
                className="h-9 pl-9 text-sm"
                onChange={(e) => updateFilters({ query: e.target.value })}
                placeholder="Buscar por nombre, RNC o telefono"
                value={filters.query}
              />
            </div>
            <Select
              value={filters.isActive || '_'}
              onValueChange={(v) => updateFilters({ isActive: v === '_' ? '' : v })}
            >
              <SelectTrigger aria-label="Filtrar por estado" className="h-9">
                <SelectValue placeholder="Estado" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="_">Todos</SelectItem>
                <SelectItem value="true">Activos</SelectItem>
                <SelectItem value="false">Inactivos</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex-1">
            {suppliers.isLoading ? (
              <ListSkeleton />
            ) : suppliers.isError ? (
              <ListError onRetry={() => void suppliers.refetch()} />
            ) : items.length === 0 ? (
              <ListEmpty hasFilters={hasFilters} onCreate={() => setCreateOpen(true)} />
            ) : (
              <ul className="divide-y divide-stone-100">
                {items.map((s) => (
                  <SupplierListItem
                    key={s.id}
                    onClick={() => setSelectedId(s.id)}
                    selected={selectedId === s.id}
                    supplier={s}
                  />
                ))}
              </ul>
            )}
          </div>

          <div className="shrink-0 border-t border-stone-200 px-3 py-2 text-xs font-medium text-stone-500">
            Mostrando {items.length} de {suppliers.data?.totalItems ?? 0}
          </div>
        </div>

        <div className="bg-stone-50">
          {selectedId ? (
            <SupplierDetailPanel
              key={selectedId}
              onClose={() => setSelectedId(null)}
              supplier={items.find((s) => s.id === selectedId) ?? null}
            />
          ) : (
            <DetailEmptyState onCreate={() => setCreateOpen(true)} />
          )}
        </div>
      </div>

      <SupplierFormDialog
        onOpenChange={setCreateOpen}
        onSaved={(saved?: Supplier) => {
          setCreateOpen(false)
          if (saved?.id) setSelectedId(saved.id)
        }}
        open={createOpen}
        supplier={null}
      />
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
        <li className="flex items-center gap-3 px-3 py-3" key={k}>
          <div className="h-10 w-10 shrink-0 rounded-md bg-stone-100" />
          <div className="min-w-0 flex-1 space-y-1.5">
            <div className="h-3.5 w-3/4 rounded bg-stone-100" />
            <div className="h-3 w-1/2 rounded bg-stone-100" />
          </div>
        </li>
      ))}
    </ul>
  )
}

function ListError({ onRetry }: Readonly<{ onRetry: () => void }>) {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
        No se pudieron cargar los proveedores.
      </div>
      <Button onClick={onRetry} size="sm" type="button" variant="secondary">
        <RefreshCw size={14} />
        Reintentar
      </Button>
    </div>
  )
}

function ListEmpty({ hasFilters, onCreate }: Readonly<{ hasFilters: boolean; onCreate: () => void }>) {
  if (hasFilters) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-2 p-8 text-center">
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-stone-100">
          <Search className="text-stone-500" size={18} />
        </div>
        <p className="text-sm font-semibold text-stone-900">Sin resultados</p>
        <p className="max-w-xs text-xs font-medium text-stone-500">
          Ajusta la busqueda o los filtros para encontrar proveedores.
        </p>
      </div>
    )
  }

  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-stone-100">
        <Building2 className="text-stone-500" size={20} />
      </div>
      <p className="text-sm font-semibold text-stone-900">Aun no hay proveedores</p>
      <p className="max-w-xs text-xs font-medium text-stone-500">
        Crea el primer proveedor para registrar compras y costos.
      </p>
      <Button onClick={onCreate} size="sm" type="button">
        <Plus size={14} />
        Crear proveedor
      </Button>
    </div>
  )
}

function DetailEmptyState({ onCreate }: Readonly<{ onCreate: () => void }>) {
  return (
    <div className="flex h-full min-h-[420px] flex-col items-center justify-center gap-4 p-8 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-white ring-1 ring-stone-200">
        <Building2 className="text-stone-400" size={28} />
      </div>
      <div>
        <p className="text-base font-semibold text-stone-900">Selecciona un proveedor</p>
        <p className="mt-1 max-w-sm text-sm font-medium text-stone-500">
          Elige un proveedor para ver sus datos, contacto e historial de compras.
        </p>
      </div>
      <Button onClick={onCreate} type="button" variant="secondary">
        <Plus size={15} />
        Crear proveedor
      </Button>
    </div>
  )
}
