import { Search } from 'lucide-react'
import type { StockFilters } from '@/modules/inventory/types'

const controlClassName =
  'h-10 w-full rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15'

export function InventoryFilters({
  filters,
  onChange,
}: Readonly<{
  filters: StockFilters
  onChange: (values: Partial<StockFilters>) => void
}>) {
  const statusFilterValue = getStatusFilterValue(filters)

  return (
    <div className="grid gap-3 rounded-md bg-white p-4 shadow-sm ring-1 ring-stone-200 lg:grid-cols-[minmax(240px,1fr)_160px_160px_130px]">
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Buscar</span>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" size={16} />
          <input
            className="h-10 w-full rounded-md bg-white pl-9 pr-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
            onChange={(event) => onChange({ search: event.target.value })}
            placeholder="Producto, SKU o codigo"
            value={filters.search}
          />
        </div>
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Estado</span>
        <select
          className={controlClassName}
          onChange={(event) => {
            onChange({
              lowStockOnly: event.target.value === 'low',
              outOfStockOnly: event.target.value === 'out',
            })
          }}
          value={statusFilterValue}
        >
          <option value="">Todos</option>
          <option value="low">Stock bajo</option>
          <option value="out">Agotados</option>
        </select>
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Sucursal ID</span>
        <input
          className={controlClassName}
          onChange={(event) => onChange({ branchId: event.target.value })}
          placeholder="Opcional"
          value={filters.branchId}
        />
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Por pagina</span>
        <select
          className={controlClassName}
          onChange={(event) => onChange({ pageSize: Number(event.target.value) })}
          value={filters.pageSize}
        >
          <option value={10}>10</option>
          <option value={25}>25</option>
          <option value={50}>50</option>
        </select>
      </label>
    </div>
  )
}

function getStatusFilterValue(filters: StockFilters) {
  if (filters.outOfStockOnly) {
    return 'out'
  }

  return filters.lowStockOnly ? 'low' : ''
}
