import { useState, type ReactNode } from 'react'
import { Boxes, History, Search, SlidersHorizontal, X } from 'lucide-react'
import { InventoryAdjustmentForm } from '@/modules/inventory/components/InventoryAdjustmentForm'
import { useInventoryMovementsQuery, useStockQuery } from '@/modules/inventory/hooks/useInventory'
import type { MovementFilters, StockFilters } from '@/modules/inventory/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

const pageSizes = [10, 25, 50]
const controlClassName =
  'h-11 w-full rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15'

const initialStockFilters: StockFilters = {
  search: '',
  lowStockOnly: false,
  productType: '',
  categoryId: '',
  page: 1,
  pageSize: 10,
  sortBy: 'productId',
  sortDirection: 'asc',
}

const initialMovementFilters: MovementFilters = {
  productId: '',
  movementType: '',
  dateFrom: '',
  dateTo: '',
  page: 1,
  pageSize: 10,
  sortBy: 'createdAt',
  sortDirection: 'desc',
}

export function InventoryPage() {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false)
  const [stockFilters, setStockFilters] = useState(initialStockFilters)
  const [movementFilters, setMovementFilters] = useState(initialMovementFilters)
  const stock = useStockQuery(stockFilters)
  const movements = useInventoryMovementsQuery(movementFilters)
  const stockItems = stock.data?.items ?? []
  const movementItems = movements.data?.items ?? []

  function updateStockFilters(values: Partial<StockFilters>) {
    setStockFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  function updateMovementFilters(values: Partial<MovementFilters>) {
    setMovementFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <section className="space-y-6 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">Stock</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Inventario</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Controla existencias y movimientos sin mezclar reglas comerciales del catalogo.
          </p>
        </div>
        <Button onClick={() => setIsDrawerOpen(true)} type="button">
          <SlidersHorizontal size={16} />
          Ajustar inventario
        </Button>
      </div>

      <StockFiltersBar filters={stockFilters} onChange={updateStockFilters} />

      <Card className="overflow-hidden">
        <CardHeader>
          <div className="flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-md bg-stone-100 text-stone-900">
              <Boxes size={19} />
            </span>
            <div>
              <h3 className="text-base font-semibold text-stone-950">Existencias</h3>
              <p className="text-sm font-medium text-stone-600">{stock.data?.totalItems ?? 0} productos con stock</p>
            </div>
          </div>
        </CardHeader>
        <div className="overflow-x-auto">
          <table className="w-full min-w-190 text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
              <tr>
                <th className="px-5 py-3">Producto</th>
                <th className="px-5 py-3">SKU / Barcode</th>
                <th className="px-5 py-3">Unidad</th>
                <th className="px-5 py-3 text-right">Cantidad</th>
                <th className="px-5 py-3">Estado</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200">
              {stock.isLoading && <PlaceholderRows columns={5} />}
              {stock.isError && (
                <ErrorRow columns={5} onRetry={() => void stock.refetch()} text="No se pudo cargar el stock." />
              )}
              {!stock.isLoading && !stock.isError && stockItems.length === 0 && (
                <EmptyRow columns={5} text="No hay existencias con los filtros actuales." />
              )}
              {!stock.isLoading && !stock.isError && stockItems.map((item) => (
                <tr className="bg-white hover:bg-stone-50" key={item.id}>
                  <td className="px-5 py-4">
                    <p className="font-semibold text-stone-950">{item.productName}</p>
                    <p className="mt-1 font-mono text-xs font-medium text-stone-500">{item.productId}</p>
                  </td>
                  <td className="px-5 py-4">
                    <p className="font-semibold text-stone-800">{item.sku ?? 'Sin SKU'}</p>
                    <p className="mt-1 text-xs font-medium text-stone-500">{item.barcode ?? 'Sin barcode'}</p>
                  </td>
                  <td className="px-5 py-4 font-semibold text-stone-700">{item.unitOfMeasure ?? '-'}</td>
                  <td className="px-5 py-4 text-right text-base font-semibold text-stone-950">{item.quantity}</td>
                  <td className="px-5 py-4">
                    <StockBadge isLowStock={item.isLowStock} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <Pagination
          hasNextPage={stock.data?.hasNextPage ?? false}
          hasPreviousPage={stock.data?.hasPreviousPage ?? false}
          onChangePage={(page) => updateStockFilters({ page })}
          onChangePageSize={(pageSize) => updateStockFilters({ pageSize })}
          page={stockFilters.page}
          pageSize={stockFilters.pageSize}
          totalPages={stock.data?.totalPages ?? 0}
        />
      </Card>

      <Card className="overflow-hidden">
        <CardHeader>
          <div className="flex items-center gap-2">
            <History className="text-stone-700" size={18} />
            <div>
              <h3 className="text-base font-semibold text-stone-950">Movimientos recientes</h3>
              <p className="text-sm font-medium text-stone-600">{movements.data?.totalItems ?? 0} movimientos</p>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <MovementFiltersBar filters={movementFilters} onChange={updateMovementFilters} />
        </CardContent>
        <div className="overflow-x-auto">
          <table className="w-full min-w-170 text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
              <tr>
                <th className="px-5 py-3">Razon</th>
                <th className="px-5 py-3">Producto</th>
                <th className="px-5 py-3 text-right">Cantidad</th>
                <th className="px-5 py-3 text-right">Stock anterior</th>
                <th className="px-5 py-3 text-right">Nuevo stock</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200">
              {movements.isLoading && <PlaceholderRows columns={5} />}
              {movements.isError && (
                <ErrorRow columns={5} onRetry={() => void movements.refetch()} text="No se pudo cargar movimientos." />
              )}
              {!movements.isLoading && !movements.isError && movementItems.length === 0 && (
                <EmptyRow columns={5} text="No hay movimientos con los filtros actuales." />
              )}
              {!movements.isLoading && !movements.isError && movementItems.map((movement) => (
                <tr className="bg-white hover:bg-stone-50" key={movement.id}>
                  <td className="px-5 py-4 font-semibold text-stone-950">{movement.reason}</td>
                  <td className="px-5 py-4 font-mono text-xs font-semibold text-stone-600">{movement.productId}</td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-700">{movement.quantity}</td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-700">{movement.previousStock}</td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-950">{movement.newStock}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <Pagination
          hasNextPage={movements.data?.hasNextPage ?? false}
          hasPreviousPage={movements.data?.hasPreviousPage ?? false}
          onChangePage={(page) => updateMovementFilters({ page })}
          onChangePageSize={(pageSize) => updateMovementFilters({ pageSize })}
          page={movementFilters.page}
          pageSize={movementFilters.pageSize}
          totalPages={movements.data?.totalPages ?? 0}
        />
      </Card>

      {isDrawerOpen && (
        <InventoryDrawer onClose={() => setIsDrawerOpen(false)}>
          <InventoryAdjustmentForm />
        </InventoryDrawer>
      )}
    </section>
  )
}

function StockFiltersBar({
  filters,
  onChange,
}: Readonly<{
  filters: StockFilters
  onChange: (values: Partial<StockFilters>) => void
}>) {
  return (
    <Card>
      <CardContent className="grid gap-4 p-5 lg:grid-cols-[minmax(260px,1fr)_180px_160px_auto]">
        <label className="block">
          <span className="mb-2 block text-sm font-semibold text-stone-900">Buscar</span>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" size={16} />
            <input
              className="h-11 w-full rounded-md bg-white pl-9 pr-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
              onChange={(event) => onChange({ search: event.target.value })}
              placeholder="Nombre, SKU o barcode"
              value={filters.search}
            />
          </div>
        </label>
        <label className="block">
          <span className="mb-2 block text-sm font-semibold text-stone-900">Tipo</span>
          <select className={controlClassName} onChange={(event) => onChange({ productType: event.target.value })} value={filters.productType}>
            <option value="">Todos</option>
            <option value="Simple">Simple</option>
            <option value="Weighed">Pesado</option>
            <option value="Composite">Compuesto</option>
            <option value="VariantChild">Variante</option>
          </select>
        </label>
        <label className="block">
          <span className="mb-2 block text-sm font-semibold text-stone-900">Cantidad</span>
          <select
            className={controlClassName}
            onChange={(event) => onChange({ lowStockOnly: event.target.value === 'low' })}
            value={filters.lowStockOnly ? 'low' : ''}
          >
            <option value="">Todo el stock</option>
            <option value="low">Bajo stock</option>
          </select>
        </label>
        <label className="block">
          <span className="mb-2 block text-sm font-semibold text-stone-900">Por pagina</span>
          <PageSizeSelect onChange={(pageSize) => onChange({ pageSize })} pageSize={filters.pageSize} />
        </label>
      </CardContent>
    </Card>
  )
}

function MovementFiltersBar({
  filters,
  onChange,
}: Readonly<{
  filters: MovementFilters
  onChange: (values: Partial<MovementFilters>) => void
}>) {
  return (
    <div className="grid gap-4 lg:grid-cols-[minmax(260px,1fr)_170px_160px_160px_140px]">
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Producto ID</span>
        <input className={controlClassName} onChange={(event) => onChange({ productId: event.target.value })} value={filters.productId} />
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Tipo</span>
        <select className={controlClassName} onChange={(event) => onChange({ movementType: event.target.value })} value={filters.movementType}>
          <option value="">Todos</option>
          <option value="InitialLoad">Carga inicial</option>
          <option value="Purchase">Compra</option>
          <option value="Sale">Venta</option>
          <option value="Adjustment">Ajuste</option>
          <option value="Return">Devolucion</option>
          <option value="ManualCorrection">Correccion</option>
        </select>
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Desde</span>
        <input className={controlClassName} onChange={(event) => onChange({ dateFrom: event.target.value })} type="date" value={filters.dateFrom} />
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Hasta</span>
        <input className={controlClassName} onChange={(event) => onChange({ dateTo: event.target.value })} type="date" value={filters.dateTo} />
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Por pagina</span>
        <PageSizeSelect onChange={(pageSize) => onChange({ pageSize })} pageSize={filters.pageSize} />
      </label>
    </div>
  )
}

function PageSizeSelect({
  onChange,
  pageSize,
}: Readonly<{
  onChange: (pageSize: number) => void
  pageSize: number
}>) {
  return (
    <select className={controlClassName} onChange={(event) => onChange(Number(event.target.value))} value={pageSize}>
      {pageSizes.map((size) => (
        <option key={size} value={size}>{size}</option>
      ))}
    </select>
  )
}

function Pagination({
  hasNextPage,
  hasPreviousPage,
  onChangePage,
  onChangePageSize,
  page,
  pageSize,
  totalPages,
}: Readonly<{
  hasNextPage: boolean
  hasPreviousPage: boolean
  onChangePage: (page: number) => void
  onChangePageSize: (pageSize: number) => void
  page: number
  pageSize: number
  totalPages: number
}>) {
  return (
    <div className="flex flex-col gap-3 border-t border-stone-200 bg-white px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm font-medium text-stone-600">
        Pagina {page} de {totalPages || 1}
      </p>
      <div className="flex items-center gap-2">
        <PageSizeSelect onChange={onChangePageSize} pageSize={pageSize} />
        <Button disabled={!hasPreviousPage} onClick={() => onChangePage(page - 1)} type="button" variant="secondary">
          Anterior
        </Button>
        <Button disabled={!hasNextPage} onClick={() => onChangePage(page + 1)} type="button" variant="secondary">
          Siguiente
        </Button>
      </div>
    </div>
  )
}

const placeholderRowIds = ['inventory-placeholder-1', 'inventory-placeholder-2', 'inventory-placeholder-3', 'inventory-placeholder-4']

function PlaceholderRows({ columns }: Readonly<{ columns: number }>) {
  return placeholderRowIds.map((id) => (
    <tr key={id}>
      <td className="px-5 py-4" colSpan={columns}>
        <div className="h-4 w-full rounded bg-stone-100" />
      </td>
    </tr>
  ))
}

function EmptyRow({ columns, text }: Readonly<{ columns: number; text: string }>) {
  return (
    <tr>
      <td className="px-5 py-10 text-center text-sm font-medium text-stone-600" colSpan={columns}>
        {text}
      </td>
    </tr>
  )
}

function ErrorRow({
  columns,
  onRetry,
  text,
}: Readonly<{
  columns: number
  onRetry: () => void
  text: string
}>) {
  return (
    <tr>
      <td className="px-5 py-10 text-center" colSpan={columns}>
        <p className="text-sm font-medium text-red-700">{text}</p>
        <Button className="mt-3" onClick={onRetry} type="button" variant="secondary">
          Reintentar
        </Button>
      </td>
    </tr>
  )
}

function StockBadge({ isLowStock }: Readonly<{ isLowStock: boolean }>) {
  const className = isLowStock
    ? 'rounded-full bg-amber-50 px-2.5 py-1 text-xs font-semibold text-amber-700 ring-1 ring-amber-200'
    : 'rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200'

  return <span className={className}>{isLowStock ? 'Bajo stock' : 'Disponible'}</span>
}

function InventoryDrawer({
  children,
  onClose,
}: Readonly<{
  children: ReactNode
  onClose: () => void
}>) {
  return (
    <div className="fixed inset-0 z-50 bg-stone-950/20">
      <div className="absolute inset-y-0 right-0 flex w-full max-w-3xl flex-col bg-white shadow-xl ring-1 ring-stone-200">
        <div className="flex h-16 shrink-0 items-center justify-between border-b border-stone-200 px-6">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Inventario</p>
            <h3 className="text-lg font-semibold text-stone-950">Ajustar inventario</h3>
          </div>
          <Button aria-label="Cerrar drawer" onClick={onClose} size="icon" type="button" variant="ghost">
            <X size={18} />
          </Button>
        </div>
        <div className="min-h-0 flex-1 overflow-y-auto p-6">
          {children}
        </div>
      </div>
    </div>
  )
}
