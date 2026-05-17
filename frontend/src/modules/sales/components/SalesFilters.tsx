import { RotateCcw, Search, SlidersHorizontal } from 'lucide-react'
import type { SalesFilters, SaleStatus } from '@/modules/sales/types/salesTypes'
import { Button } from '@/shared/components/ui/button'

const statusOptions: Array<{ label: string; value: '' | SaleStatus }> = [
  { label: 'Todos los estados', value: '' },
  { label: 'Recibida', value: 'Received' },
  { label: 'Procesando', value: 'Processing' },
  { label: 'Completada', value: 'Completed' },
  { label: 'Fallida', value: 'Failed' },
  { label: 'Cancelada', value: 'Cancelled' },
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
    <div className="grid gap-3 lg:grid-cols-[minmax(220px,1fr)_160px_160px_180px_auto_auto]">
      <label className="flex h-11 items-center gap-2 rounded-md bg-white px-3 shadow-sm ring-1 ring-stone-200">
        <Search aria-hidden="true" className="text-stone-500" size={18} />
        <span className="sr-only">Busqueda</span>
        <input
          className="w-full bg-transparent text-sm font-medium text-stone-900 outline-none placeholder:text-stone-400"
          disabled={disabled}
          onChange={(event) => onChange({ query: event.target.value })}
          placeholder="Cliente, codigo o venta"
          value={filters.query}
        />
      </label>

      <label className="sr-only" htmlFor="sales-date-from">
        Fecha desde
      </label>
      <input
        className="h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15"
        disabled={disabled}
        id="sales-date-from"
        onChange={(event) => onChange({ dateFrom: event.target.value })}
        type="date"
        value={filters.dateFrom}
      />

      <label className="sr-only" htmlFor="sales-date-to">
        Fecha hasta
      </label>
      <input
        className="h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15"
        disabled={disabled}
        id="sales-date-to"
        onChange={(event) => onChange({ dateTo: event.target.value })}
        type="date"
        value={filters.dateTo}
      />

      <label className="sr-only" htmlFor="sales-status">
        Estado
      </label>
      <select
        className="h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15"
        disabled={disabled}
        id="sales-status"
        onChange={(event) => onChange({ status: event.target.value as '' | SaleStatus })}
        value={filters.status}
      >
        {statusOptions.map((option) => (
          <option key={option.value || 'all'} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>

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
