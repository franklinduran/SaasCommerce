import { ChevronLeft, ChevronRight, RefreshCw, ShoppingCart } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { SalesFilters } from '@/modules/sales/components/SalesFilters'
import { SalesTable } from '@/modules/sales/components/SalesTable'
import { useSales, useSaleStatusInvalidation } from '@/modules/sales/hooks/useSales'
import type { SalesFilters as SalesFiltersState } from '@/modules/sales/types/salesTypes'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

const initialFilters: SalesFiltersState = {
  dateFrom: '',
  dateTo: '',
  page: 1,
  pageSize: 10,
  query: '',
  status: '',
}

const pageSizes = [10, 25, 50]

export function SalesPage() {
  const [filters, setFilters] = useState<SalesFiltersState>(initialFilters)
  const sales = useSales(filters)
  const items = useMemo(() => sales.data?.items ?? [], [sales.data?.items])
  const totalItems = sales.data?.totalItems ?? 0
  const totalPages = sales.data?.totalPages ?? 0
  const canGoPrevious = sales.data?.hasPreviousPage ?? filters.page > 1
  const canGoNext = sales.data?.hasNextPage ?? (totalPages > 0 && filters.page < totalPages)
  const hasActiveFilters =
    Boolean(filters.dateFrom) ||
    Boolean(filters.dateTo) ||
    Boolean(filters.query.trim()) ||
    Boolean(filters.status)
  const visibleSaleIds = useMemo(() => items.map((sale) => sale.id), [items])
  const emptyMessage = hasActiveFilters
    ? 'No hay ventas que coincidan con los filtros.'
    : 'No hay ventas registradas.'

  useSaleStatusInvalidation(visibleSaleIds)

  function updateFilters(next: Partial<SalesFiltersState>) {
    setFilters((current) => ({
      ...current,
      ...next,
      page: next.page ?? 1,
    }))
  }

  function resetFilters() {
    setFilters(initialFilters)
  }

  return (
    <section className="space-y-6 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">Ventas</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Historial de ventas</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Consulta ventas, revisa estados y abre recibos simples.
          </p>
        </div>
        <Button asChild>
          <Link to="/pos">
            <ShoppingCart size={16} />
            Abrir POS
          </Link>
        </Button>
      </div>

      <Card className="rounded-md">
        <CardHeader>
          <SalesFilters
            disabled={sales.isFetching}
            filters={filters}
            onChange={updateFilters}
            onReset={resetFilters}
            onRetry={() => sales.refetch()}
          />
        </CardHeader>
      </Card>

      <Card className="overflow-hidden rounded-md">
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <h3 className="text-base font-semibold text-stone-950">Listado de ventas</h3>
            <p className="text-sm font-medium text-stone-600">{totalItems} ventas encontradas</p>
          </div>
          <Button disabled={sales.isFetching} onClick={() => sales.refetch()} variant="ghost">
            <RefreshCw size={16} />
            Reintentar
          </Button>
        </CardHeader>

        <SalesTable
          emptyMessage={emptyMessage}
          isError={sales.isError}
          isLoading={sales.isLoading}
          onRetry={() => sales.refetch()}
          sales={items}
        />

        <div className="flex flex-col gap-4 border-t border-stone-200 px-5 py-4 lg:flex-row lg:items-center lg:justify-between">
          <p className="text-sm font-medium text-stone-600">
            <span>Pagina {filters.page} de {totalPages || 1}</span>
            <span aria-hidden="true" className="px-1">/</span>
            <span>{totalItems} registros</span>
          </p>
          <div className="flex flex-wrap items-center gap-3">
            <select
              className="h-9 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none"
              onChange={(event) => updateFilters({ pageSize: Number(event.target.value) })}
              value={filters.pageSize}
            >
              {pageSizes.map((pageSize) => (
                <option key={pageSize} value={pageSize}>
                  {pageSize} por pagina
                </option>
              ))}
            </select>
            <Button
              disabled={!canGoPrevious}
              onClick={() => updateFilters({ page: filters.page - 1 })}
              size="sm"
              variant="secondary"
            >
              <ChevronLeft size={15} />
              Anterior
            </Button>
            <Button
              disabled={!canGoNext}
              onClick={() => updateFilters({ page: filters.page + 1 })}
              size="sm"
              variant="secondary"
            >
              Siguiente
              <ChevronRight size={15} />
            </Button>
          </div>
        </div>
      </Card>
    </section>
  )
}
