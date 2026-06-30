import { CalendarDays, RotateCcw, Search, X } from 'lucide-react'
import { useRef } from 'react'
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
      <DateInput
        disabled={disabled}
        label="Desde"
        value={filters.dateFrom}
        onChange={(v) => onChange({ dateFrom: v })}
      />

      {/* Date to */}
      <DateInput
        disabled={disabled}
        label="Hasta"
        value={filters.dateTo}
        onChange={(v) => onChange({ dateTo: v })}
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

// ─── DateInput ────────────────────────────────────────────────────────────────

function formatIsoDate(iso: string): string {
  const [y, m, d] = iso.split('-')
  return `${d}/${m}/${y}`
}

type DateInputProps = {
  disabled?: boolean
  label: string
  onChange: (v: string) => void
  value: string
}

function DateInput({
  disabled = false,
  label,
  onChange,
  value,
}: Readonly<DateInputProps>) {
  const inputRef = useRef<HTMLInputElement>(null)

  return (
    <div
      className={cn(
        'relative flex h-8 w-36 cursor-pointer items-center gap-1.5 rounded-lg border border-gray-200 bg-white px-3 transition',
        'hover:border-gray-300 focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/15',
        disabled && 'pointer-events-none opacity-50',
      )}
      onClick={() => inputRef.current?.showPicker?.()}
    >
      {/* Hidden native input — only for picker behaviour */}
      <input
        ref={inputRef}
        aria-label={label}
        className="pointer-events-none absolute inset-0 h-full w-full opacity-0"
        disabled={disabled}
        tabIndex={-1}
        type="date"
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />

      <CalendarDays aria-hidden="true" className="shrink-0 text-gray-400" size={13} />

      <span className={cn(
        'flex-1 select-none whitespace-nowrap text-[12.5px]',
        value ? 'font-medium text-gray-800' : 'font-normal text-gray-400',
      )}>
        {value ? formatIsoDate(value) : label}
      </span>

      {value && (
        <button
          aria-label={`Quitar ${label.toLowerCase()}`}
          className="relative flex shrink-0 items-center text-gray-300 transition hover:text-gray-500"
          tabIndex={-1}
          type="button"
          onClick={(e) => { e.stopPropagation(); onChange('') }}
        >
          <X size={11} />
        </button>
      )}
    </div>
  )
}
