import { RotateCcw, Search, SlidersHorizontal } from 'lucide-react'
import type { SalesFilters, SaleStatus } from '@/modules/sales/types/salesTypes'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const statusOptions: Array<{ label: string; value: string }> = [
  { label: 'Todos los estados', value: '_' },
  { label: 'Recibida',          value: 'Received' },
  { label: 'Procesando',        value: 'Processing' },
  { label: 'Completada',        value: 'Completed' },
  { label: 'Fallida',           value: 'Failed' },
  { label: 'Cancelada',         value: 'Cancelled' },
]

type SalesFiltersProps = {
  disabled?: boolean
  filters: SalesFilters
  onChange: (filters: Partial<SalesFilters>) => void
  onReset: () => void
  onRetry: () => void
}

export function SalesFilters({
  disabled = false,
  filters,
  onChange,
  onReset,
  onRetry,
}: Readonly<SalesFiltersProps>) {
  return (
    <div className="grid min-w-0 gap-3 lg:grid-cols-[minmax(220px,1fr)_160px_160px_180px_auto_auto] lg:items-end">
      <label className="block min-w-0 space-y-1.5">
        <span className="text-xs font-semibold uppercase tracking-wide text-stone-500">Busqueda</span>
        <span className="flex h-10 min-w-0 items-center gap-2 rounded-md bg-white px-3 shadow-[0_0_0_1px_rgb(214_211_209)]">
          <Search aria-hidden="true" className="text-stone-500" size={16} />
          <input
            className="w-full bg-transparent text-sm font-medium text-stone-900 outline-none placeholder:text-stone-400"
            disabled={disabled}
            onChange={(event) => onChange({ query: event.target.value })}
            placeholder="Cliente, codigo o venta"
            value={filters.query}
          />
        </span>
      </label>

      <label className="block min-w-0 space-y-1.5">
        <span className="text-xs font-semibold uppercase tracking-wide text-stone-500">Fecha desde</span>
        <Input
          aria-label="Fecha desde"
          disabled={disabled}
          onChange={(event) => onChange({ dateFrom: event.target.value })}
          type="date"
          value={filters.dateFrom}
        />
      </label>

      <label className="block min-w-0 space-y-1.5">
        <span className="text-xs font-semibold uppercase tracking-wide text-stone-500">Fecha hasta</span>
        <Input
          aria-label="Fecha hasta"
          disabled={disabled}
          onChange={(event) => onChange({ dateTo: event.target.value })}
          type="date"
          value={filters.dateTo}
        />
      </label>

      <div className="block min-w-0 space-y-1.5">
        <span className="text-xs font-semibold uppercase tracking-wide text-stone-500">Estado</span>
        <Select
          disabled={disabled}
          value={filters.status || '_'}
          onValueChange={(v) =>
            onChange({ status: (v === '_' ? '' : v) as '' | SaleStatus })
          }
        >
          <SelectTrigger aria-label="Estado">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {statusOptions.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <Button disabled={disabled} onClick={onRetry} type="button" variant="secondary">
        <SlidersHorizontal size={16} />
        Filtrar
      </Button>
      <Button disabled={disabled} onClick={onReset} type="button" variant="ghost">
        <RotateCcw size={16} />
        Limpiar
      </Button>
    </div>
  )
}
