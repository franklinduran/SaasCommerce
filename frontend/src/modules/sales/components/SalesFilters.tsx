import { RotateCcw, Search } from 'lucide-react'
import type { PaymentMethodFilter, SalesFilters, SaleStatus } from '@/modules/sales/types/salesTypes'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { cn } from '@/shared/utils/cn'

const paymentMethodOptions: Array<{ label: string; value: string }> = [
  { label: 'Todos los métodos', value: '_' },
  { label: 'Efectivo',          value: 'Cash' },
  { label: 'Transferencia',     value: 'Transfer' },
  { label: 'Tarjeta',           value: 'Card' },
  { label: 'Crédito',           value: 'Credit' },
]

const statusOptions: Array<{ label: string; value: string }> = [
  { label: 'Todos los estados', value: '_' },
  { label: 'Recibida',          value: 'Received' },
  { label: 'Procesando',        value: 'Processing' },
  { label: 'Completada',        value: 'Completed' },
  { label: 'Fallida',           value: 'Failed' },
  { label: 'Cancelada',         value: 'Cancelled' },
]

const inputClass = cn(
  'h-8 w-full rounded-lg border border-gray-200 bg-white px-3',
  'text-[13px] font-medium text-gray-900 outline-none transition',
  'placeholder:font-normal placeholder:text-gray-400',
  'hover:border-gray-300 focus:border-primary focus:ring-2 focus:ring-primary/15',
  'disabled:cursor-not-allowed disabled:opacity-50',
)

type SalesFiltersProps = {
  disabled?: boolean
  filters: SalesFilters
  onChange: (filters: Partial<SalesFilters>) => void
  onReset: () => void
}

export function SalesFilters({
  disabled = false,
  filters,
  onChange,
  onReset,
}: Readonly<SalesFiltersProps>) {
  return (
    <div className="flex flex-wrap items-center gap-2">

      {/* Search */}
      <span className={cn(
        'flex h-8 items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 transition',
        'w-full sm:w-56',
        'hover:border-gray-300 focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/15',
        disabled && 'cursor-not-allowed opacity-50',
      )}>
        <Search aria-hidden="true" className="shrink-0 text-gray-400" size={13} />
        <input
          aria-label="Buscar ventas"
          className="w-full bg-transparent text-[13px] font-medium text-gray-900 outline-none placeholder:font-normal placeholder:text-gray-400 disabled:cursor-not-allowed"
          disabled={disabled}
          onChange={(e) => onChange({ query: e.target.value })}
          placeholder="Cliente, código…"
          value={filters.query}
        />
      </span>

      {/* Date from */}
      <input
        aria-label="Fecha desde"
        className={cn(inputClass, 'w-36')}
        disabled={disabled}
        onChange={(e) => onChange({ dateFrom: e.target.value })}
        placeholder="Desde"
        type="date"
        value={filters.dateFrom}
      />

      {/* Date to */}
      <input
        aria-label="Fecha hasta"
        className={cn(inputClass, 'w-36')}
        disabled={disabled}
        onChange={(e) => onChange({ dateTo: e.target.value })}
        placeholder="Hasta"
        type="date"
        value={filters.dateTo}
      />

      {/* Payment method */}
      <Select
        disabled={disabled}
        value={filters.paymentMethod || '_'}
        onValueChange={(v) => onChange({ paymentMethod: (v === '_' ? '' : v) as PaymentMethodFilter })}
      >
        <SelectTrigger
          aria-label="Método de pago"
          className="h-8 w-44 rounded-lg border-gray-200 bg-white text-[13px] font-medium text-gray-900 hover:border-gray-300 focus:ring-primary/15"
        >
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {paymentMethodOptions.map((opt) => (
            <SelectItem key={opt.value} value={opt.value}>
              {opt.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {/* Status */}
      <Select
        disabled={disabled}
        value={filters.status || '_'}
        onValueChange={(v) => onChange({ status: (v === '_' ? '' : v) as '' | SaleStatus })}
      >
        <SelectTrigger
          aria-label="Estado"
          className="h-8 w-44 rounded-lg border-gray-200 bg-white text-[13px] font-medium text-gray-900 hover:border-gray-300 focus:ring-primary/15"
        >
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

      {/* Clear */}
      <button
        className={cn(
          'flex h-8 items-center gap-1.5 rounded-lg border border-gray-200 bg-white px-2.5',
          'text-[12.5px] font-medium text-gray-500 transition',
          'hover:border-gray-300 hover:text-gray-800',
          'disabled:cursor-not-allowed disabled:opacity-40',
        )}
        disabled={disabled}
        onClick={onReset}
        type="button"
      >
        <RotateCcw size={12} />
        Limpiar
      </button>
    </div>
  )
}
