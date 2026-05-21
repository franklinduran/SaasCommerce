import {
  ChevronLeft,
  ChevronRight,
  Eye,
  Plus,
  RefreshCw,
  RotateCcw,
  Search,
  Truck,
} from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { PurchaseStatusBadge } from '@/modules/purchases/components/PurchaseStatusBadge'
import { usePurchaseRealtimeInvalidation, usePurchases } from '@/modules/purchases/hooks/usePurchases'
import type { PurchaseFilters } from '@/modules/purchases/types'
import { formatMoney } from '@/modules/purchases/utils/formatMoney'
import { useSuppliers } from '@/modules/suppliers/hooks/useSuppliers'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const initialFilters: PurchaseFilters = {
  dateFrom: '',
  dateTo: '',
  page: 1,
  pageSize: 25,
  query: '',
  sortBy: 'purchaseDate',
  sortDirection: 'desc',
  status: '',
  supplierId: '',
}

const pageSizes = [10, 25, 50]

export function PurchasesPage() {
  const [filters, setFilters] = useState<PurchaseFilters>(initialFilters)
  const purchases = usePurchases(filters)
  const suppliers = useSuppliers({
    isActive: 'true',
    page: 1,
    pageSize: 50,
    query: '',
    sortBy: 'name',
    sortDirection: 'asc',
  })

  usePurchaseRealtimeInvalidation()

  const items = useMemo(() => purchases.data?.items ?? [], [purchases.data?.items])
  const supplierItems = suppliers.data?.items ?? []
  const hasFilters = Boolean(
    filters.query ||
      filters.status ||
      filters.supplierId ||
      filters.dateFrom ||
      filters.dateTo,
  )

  const stats = useMemo(() => {
    const received = items.filter((purchase) => purchase.status === 'Received').length
    const draft = items.filter((purchase) => purchase.status === 'Draft').length

    return {
      draft,
      received,
      totalAmount: purchases.data?.totalPurchased ?? 0,
      totalItems: purchases.data?.totalItems ?? 0,
    }
  }, [items, purchases.data?.totalItems, purchases.data?.totalPurchased])

  function updateFilters(values: Partial<PurchaseFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  function resetFilters() {
    setFilters(initialFilters)
  }

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
        <div>
          <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
            <Truck size={13} />
            Abastecimiento
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Compras</h2>
          <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
            Administra ordenes de compra, recepciones y costos sin perder el contexto del inventario.
          </p>
        </div>

        <div className="grid gap-2 sm:flex sm:flex-wrap sm:justify-end">
          <Button
            className="w-full sm:w-auto"
            disabled={purchases.isFetching}
            onClick={() => void purchases.refetch()}
            type="button"
            variant="secondary"
          >
            <RefreshCw className={purchases.isFetching ? 'animate-spin' : undefined} size={16} />
            Refrescar
          </Button>
          <Button asChild className="w-full sm:w-auto" variant="secondary">
            <Link to="/suppliers">Proveedores</Link>
          </Button>
          <Button asChild className="w-full sm:w-auto">
            <Link to="/purchases/new">
              <Plus size={16} />
              Nueva compra
            </Link>
          </Button>
        </div>
      </div>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Compras" value={String(stats.totalItems)} />
        <MetricCard label="Recibidas" tone="success" value={String(stats.received)} />
        <MetricCard label="En borrador" tone="warning" value={String(stats.draft)} />
        <MetricCard label="Monto total" value={formatMoney(stats.totalAmount)} />
      </div>

      <Card>
        <CardHeader className="border-b border-stone-200">
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <h3 className="text-base font-semibold text-stone-950">Filtros de compras</h3>
              <p className="mt-1 text-sm font-medium text-stone-600">
                Busca por proveedor, factura, estado o rango de fechas.
              </p>
            </div>
            <Button
              className="w-full sm:w-auto"
              disabled={!hasFilters}
              onClick={resetFilters}
              type="button"
              variant="ghost"
            >
              <RotateCcw size={16} />
              Limpiar
            </Button>
          </div>
        </CardHeader>
        <CardContent className="pt-4 sm:pt-5">
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-[minmax(260px,1fr)_220px_180px_170px_170px] xl:items-end">
            <label className="block min-w-0 space-y-1.5">
              <span className="text-sm font-semibold text-stone-800">Busqueda</span>
              <span className="relative block">
                <Search
                  aria-hidden="true"
                  className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400"
                  size={15}
                />
                <Input
                  className="pl-9"
                  onChange={(event) => updateFilters({ query: event.target.value })}
                  placeholder="Factura o proveedor"
                  value={filters.query}
                />
              </span>
            </label>

            <FilterField label="Proveedor">
              <Select
                value={filters.supplierId || '_'}
                onValueChange={(value) => updateFilters({ supplierId: value === '_' ? '' : value })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos los proveedores" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Todos los proveedores</SelectItem>
                  {supplierItems.map((supplier) => (
                    <SelectItem key={supplier.id} value={supplier.id}>
                      {supplier.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </FilterField>

            <FilterField label="Estado">
              <Select
                value={filters.status || '_'}
                onValueChange={(value) => updateFilters({ status: value === '_' ? '' : value })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Todos</SelectItem>
                  <SelectItem value="Draft">Borrador</SelectItem>
                  <SelectItem value="Received">Recibidas</SelectItem>
                  <SelectItem value="Cancelled">Canceladas</SelectItem>
                </SelectContent>
              </Select>
            </FilterField>

            <FilterField label="Desde">
              <Input
                onChange={(event) => updateFilters({ dateFrom: event.target.value })}
                type="date"
                value={filters.dateFrom}
              />
            </FilterField>

            <FilterField label="Hasta">
              <Input
                onChange={(event) => updateFilters({ dateTo: event.target.value })}
                type="date"
                value={filters.dateTo}
              />
            </FilterField>
          </div>
        </CardContent>
      </Card>

      <Card className="overflow-hidden">
        <div className="border-b border-stone-200 px-4 py-3 sm:px-5">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h3 className="text-base font-semibold text-stone-950">Historial de compras</h3>
              <p className="mt-1 text-sm font-medium text-stone-600">
                {purchases.data?.totalItems ?? 0} registros encontrados
              </p>
            </div>
            {purchases.isFetching && !purchases.isLoading && (
              <p className="text-sm font-semibold text-stone-500">Actualizando...</p>
            )}
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full min-w-[900px] text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase tracking-wide text-stone-500">
              <tr>
                <th className="px-5 py-3">Compra</th>
                <th className="px-5 py-3">Proveedor</th>
                <th className="px-5 py-3">Fecha</th>
                <th className="px-5 py-3 text-right">Total</th>
                <th className="px-5 py-3">Estado</th>
                <th className="px-5 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200 bg-white">
              {purchases.isLoading && <SkeletonRows />}
              {purchases.isError && (
                <tr>
                  <td className="px-5 py-12 text-center" colSpan={6}>
                    <p className="text-sm font-semibold text-red-700">No se pudieron cargar las compras.</p>
                    <Button
                      className="mt-3"
                      onClick={() => void purchases.refetch()}
                      size="sm"
                      type="button"
                      variant="secondary"
                    >
                      Reintentar
                    </Button>
                  </td>
                </tr>
              )}
              {!purchases.isLoading && !purchases.isError && items.length === 0 && (
                <tr>
                  <td className="px-5 py-12 text-center" colSpan={6}>
                    <p className="text-sm font-semibold text-stone-900">
                      {hasFilters ? 'Sin resultados' : 'Aun no hay compras'}
                    </p>
                    <p className="mt-1 text-sm font-medium text-stone-500">
                      {hasFilters
                        ? 'Ajusta los filtros para encontrar la compra que buscas.'
                        : 'Registra una compra para iniciar el control de abastecimiento.'}
                    </p>
                    {!hasFilters && (
                      <Button asChild className="mt-4">
                        <Link to="/purchases/new">
                          <Plus size={16} />
                          Nueva compra
                        </Link>
                      </Button>
                    )}
                  </td>
                </tr>
              )}
              {items.map((purchase) => (
                <tr className="hover:bg-stone-50" key={purchase.purchaseId}>
                  <td className="px-5 py-4">
                    <p className="font-mono text-sm font-semibold text-stone-950">{purchase.code}</p>
                    <p className="mt-1 text-xs font-medium text-stone-500">
                      {purchase.supplierInvoiceNumber ?? 'Sin factura'}
                    </p>
                  </td>
                  <td className="px-5 py-4 font-medium text-stone-800">{purchase.supplierName}</td>
                  <td className="px-5 py-4 text-stone-600">{formatDate(purchase.purchaseDate)}</td>
                  <td className="px-5 py-4 text-right font-semibold tabular-nums text-stone-950">
                    {formatMoney(purchase.total)}
                  </td>
                  <td className="px-5 py-4">
                    <PurchaseStatusBadge status={purchase.status} />
                  </td>
                  <td className="px-5 py-4 text-right">
                    <Button asChild size="sm" variant="secondary">
                      <Link to={`/purchases/${purchase.purchaseId}`}>
                        <Eye size={14} />
                        Ver detalle
                      </Link>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {purchases.data && (
          <div className="flex flex-col gap-3 border-t border-stone-200 px-5 py-3 lg:flex-row lg:items-center lg:justify-between">
            <p className="text-sm font-medium text-stone-500">
              Pagina {filters.page} de {purchases.data.totalPages || 1}
            </p>
            <div className="grid gap-2 sm:flex sm:items-center sm:justify-end">
              <Select
                value={String(filters.pageSize)}
                onValueChange={(value) => updateFilters({ pageSize: Number(value) })}
              >
                <SelectTrigger className="h-9 w-full sm:w-[140px]">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {pageSizes.map((pageSize) => (
                    <SelectItem key={pageSize} value={String(pageSize)}>
                      {pageSize} por pagina
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button
                disabled={!purchases.data.hasPreviousPage}
                onClick={() => updateFilters({ page: filters.page - 1 })}
                size="sm"
                type="button"
                variant="secondary"
              >
                <ChevronLeft size={14} />
                Anterior
              </Button>
              <Button
                disabled={!purchases.data.hasNextPage}
                onClick={() => updateFilters({ page: filters.page + 1 })}
                size="sm"
                type="button"
                variant="secondary"
              >
                Siguiente
                <ChevronRight size={14} />
              </Button>
            </div>
          </div>
        )}
      </Card>
    </section>
  )
}

function FilterField({
  children,
  label,
}: Readonly<{
  children: React.ReactNode
  label: string
}>) {
  return (
    <div className="block min-w-0 space-y-1.5">
      <span className="text-sm font-semibold text-stone-800">{label}</span>
      {children}
    </div>
  )
}

function MetricCard({
  label,
  tone = 'default',
  value,
}: Readonly<{
  label: string
  tone?: 'default' | 'success' | 'warning'
  value: string
}>) {
  const toneClass = {
    default: 'bg-white text-stone-900 ring-stone-200',
    success: 'bg-emerald-50 text-emerald-800 ring-emerald-200',
    warning: 'bg-amber-50 text-amber-800 ring-amber-200',
  }[tone]

  return (
    <div className={`rounded-xl px-4 py-3 ring-1 ${toneClass}`}>
      <p className="text-xs font-semibold uppercase tracking-wide">{label}</p>
      <p className="mt-2 text-xl font-semibold tabular-nums">{value}</p>
    </div>
  )
}

const skeletonKeys = ['p1', 'p2', 'p3', 'p4', 'p5']

function SkeletonRows() {
  return (
    <>
      {skeletonKeys.map((key) => (
        <tr key={key}>
          <td className="px-5 py-4" colSpan={6}>
            <div className="h-4 w-full rounded bg-stone-100" />
          </td>
        </tr>
      ))}
    </>
  )
}

function formatDate(value: string) {
  try {
    return new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium' }).format(new Date(value))
  } catch {
    return value
  }
}
