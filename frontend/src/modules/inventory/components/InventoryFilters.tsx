import { Search } from 'lucide-react'
import type { StockFilters } from '@/modules/inventory/types'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

export function InventoryFilters({
  filters,
  onChange,
}: Readonly<{
  filters: StockFilters
  onChange: (values: Partial<StockFilters>) => void
}>) {
  const statusValue = filters.outOfStockOnly ? 'out' : filters.lowStockOnly ? 'low' : '_'

  return (
    <div className="grid gap-3 lg:grid-cols-[minmax(240px,1fr)_160px_160px_130px]">
      <div className="space-y-1.5">
        <Label>Buscar</Label>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" size={16} />
          <input
            className="h-10 w-full rounded-md bg-white pl-9 pr-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
            onChange={(event) => onChange({ search: event.target.value })}
            placeholder="Producto, SKU o codigo"
            value={filters.search}
          />
        </div>
      </div>

      <div className="space-y-1.5">
        <Label>Estado</Label>
        <Select
          value={statusValue}
          onValueChange={(v) =>
            onChange({ lowStockOnly: v === 'low', outOfStockOnly: v === 'out' })
          }
        >
          <SelectTrigger><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="_">Todos</SelectItem>
            <SelectItem value="low">Stock bajo</SelectItem>
            <SelectItem value="out">Agotados</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-1.5">
        <Label>Tipo</Label>
        <Select
          value={filters.productType || '_'}
          onValueChange={(v) => onChange({ productType: v === '_' ? '' : v })}
        >
          <SelectTrigger><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="_">Todos</SelectItem>
            <SelectItem value="Simple">Simple</SelectItem>
            <SelectItem value="Service">Servicio</SelectItem>
            <SelectItem value="Weighed">Pesado</SelectItem>
            <SelectItem value="Composite">Combo</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-1.5">
        <Label>Por pagina</Label>
        <Select
          value={String(filters.pageSize)}
          onValueChange={(v) => onChange({ pageSize: Number(v) })}
        >
          <SelectTrigger><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="10">10</SelectItem>
            <SelectItem value="25">25</SelectItem>
            <SelectItem value="50">50</SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>
  )
}
