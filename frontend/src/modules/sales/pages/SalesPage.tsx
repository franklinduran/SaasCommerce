import { ChevronLeft, ChevronRight, RefreshCw, ShoppingCart } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { SalesFilters } from '@/modules/sales/components/SalesFilters'
import { SalesTable } from '@/modules/sales/components/SalesTable'
import { useSales, useSaleStatusInvalidation } from '@/modules/sales/hooks/useSales'
import type { SalesFilters as SalesFiltersState } from '@/modules/sales/types/salesTypes'
import { CsvExportButton } from '@/shared/components/CsvExportButton'
import { Button } from '@/shared/components/ui/button'
import { useHasPermission } from '@/shared/hooks/usePermissions'
import { Permission } from '@/shared/types/permissions'
import { cn } from '@/shared/utils/cn'

const initialFilters: SalesFiltersState = {
  dateFrom: '',
  dateTo: '',
  page: 1,
  pageSize: 10,
  paymentMethod: '',
  query: '',
  status: '',
}

const pageSizes = [10, 25, 50]

export function SalesPage() {
  const [filters, setFilters] = useState<SalesFiltersState>(initialFilters)
  const canExportSales = useHasPermission(Permission.SalesExport)
  const sales = useSales(filters)
  const items = useMemo(() => sales.data?.items ?? [], [sales.data?.items])
  const totalItems = sales.data?.totalItems ?? 0
  const totalPages = sales.data?.totalPages ?? 0
  const canGoPrevious = sales.data?.hasPreviousPage ?? filters.page > 1
  const canGoNext = sales.data?.hasNextPage ?? (totalPages > 0 && filters.page < totalPages)
  const hasActiveFilters =
    Boolean(filters.dateFrom) ||
    Boolean(filters.dateTo) ||
    Boolean(filters.paymentMethod) ||
    Boolean(filters.query.trim()) ||
    Boolean(filters.status)
  const visibleSaleIds = useMemo(() => items.map((s) => s.id), [items])
  const emptyMessage = hasActiveFilters
    ? 'No hay ventas que coincidan con los filtros.'
    : 'No hay ventas registradas.'

  useSaleStatusInvalidation(visibleSaleIds)

  function updateFilters(next: Partial<SalesFiltersState>) {
    setFilters((prev) => ({ ...prev, ...next, page: next.page ?? 1 }))
  }

  return (
    <div className="flex flex-col gap-6 overflow-y-auto p-6 lg:p-8">

      {/* ── Header ── */}
      <div className="flex items-start justify-between gap-6">
        <header>
          <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
            Ventas
          </p>
          <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">
            Historial de ventas
          </h1>
          <p className="mt-1 text-[13.5px] text-muted-foreground">
            Consulta, filtra y descarga el registro de ventas del negocio.
          </p>
        </header>

        <div className="flex shrink-0 items-center gap-2 pt-0.5">
          {canExportSales && (
            <CsvExportButton
              endpoint="/api/sales/export"
              filename={`ventas_${new Date().toISOString().slice(0, 10)}.csv`}
              queryParams={{
                dateFrom: filters.dateFrom || undefined,
                dateTo: filters.dateTo || undefined,
              }}
            />
          )}
          <Button asChild size="sm" variant="secondary">
            <Link to="/pos">
              <ShoppingCart size={14} />
              Abrir POS
            </Link>
          </Button>
        </div>
      </div>

      {/* ── Table card ── */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">

        {/* Toolbar: filtros + conteo + refresh */}
        <div className="flex flex-wrap items-end gap-x-4 gap-y-3 border-b border-gray-100 px-5 py-4">
          <div className="min-w-0 flex-1">
            <SalesFilters
              disabled={sales.isFetching}
              filters={filters}
              onChange={updateFilters}
              onReset={() => setFilters(initialFilters)}
            />
          </div>

          <div className="flex shrink-0 items-center gap-3">
            <span className="text-[12px] font-medium tabular-nums text-muted-foreground">
              {sales.isLoading ? '—' : `${totalItems} ${totalItems === 1 ? 'venta' : 'ventas'}`}
            </span>
            <button
              aria-label="Actualizar ventas"
              className={cn(
                'flex h-8 w-8 items-center justify-center rounded-lg border border-gray-200 bg-white',
                'text-gray-500 transition hover:border-gray-300 hover:text-gray-800',
                'disabled:cursor-not-allowed disabled:opacity-40',
              )}
              disabled={sales.isFetching}
              onClick={() => sales.refetch()}
              type="button"
            >
              <RefreshCw className={sales.isFetching ? 'animate-spin' : ''} size={13} />
            </button>
          </div>
        </div>

        {/* Tabla */}
        <SalesTable
          emptyMessage={emptyMessage}
          isError={sales.isError}
          isLoading={sales.isLoading}
          onRetry={() => sales.refetch()}
          sales={items}
        />

        {/* Pie: paginación */}
        <div className="flex flex-wrap items-center justify-between gap-3 border-t border-gray-100 px-5 py-3">
          <p className="text-[12px] font-medium text-muted-foreground">
            Página <span className="font-semibold text-gray-800">{filters.page}</span> de{' '}
            <span className="font-semibold text-gray-800">{totalPages || 1}</span>
          </p>

          <div className="flex items-center gap-2">
            <select
              className={cn(
                'h-8 rounded-lg border border-gray-200 bg-white px-2.5',
                'text-[12px] font-medium text-gray-800 outline-none transition hover:border-gray-300',
              )}
              onChange={(e) => updateFilters({ pageSize: Number(e.target.value) })}
              value={filters.pageSize}
            >
              {pageSizes.map((n) => (
                <option key={n} value={n}>{n} / pág.</option>
              ))}
            </select>

            <div className="flex items-center gap-1">
              <button
                className={cn(
                  'flex h-8 items-center gap-1 rounded-lg border border-gray-200 bg-white px-3',
                  'text-[12px] font-medium text-gray-700 transition hover:border-gray-300 hover:text-gray-900',
                  'disabled:cursor-not-allowed disabled:opacity-35',
                )}
                disabled={!canGoPrevious}
                onClick={() => updateFilters({ page: filters.page - 1 })}
                type="button"
              >
                <ChevronLeft size={13} />
                Ant.
              </button>
              <button
                className={cn(
                  'flex h-8 items-center gap-1 rounded-lg border border-gray-200 bg-white px-3',
                  'text-[12px] font-medium text-gray-700 transition hover:border-gray-300 hover:text-gray-900',
                  'disabled:cursor-not-allowed disabled:opacity-35',
                )}
                disabled={!canGoNext}
                onClick={() => updateFilters({ page: filters.page + 1 })}
                type="button"
              >
                Sig.
                <ChevronRight size={13} />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
