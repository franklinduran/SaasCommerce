import { CalendarDays, ChevronDown, RotateCcw, Search, X } from 'lucide-react'
import { useState } from 'react'
import type { DateRange } from 'react-day-picker'
import type { PaymentMethodFilter, SalesFilters, SaleStatus } from '@/modules/sales/types/salesTypes'
import { Calendar } from '@/shared/components/ui/calendar'
import { Popover, PopoverContent, PopoverTrigger } from '@/shared/components/ui/popover'
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

      {/* Date range */}
      <DateRangeInput
        disabled={disabled}
        fromValue={filters.dateFrom}
        toValue={filters.dateTo}
        onFromChange={(v) => onChange({ dateFrom: v })}
        onToChange={(v) => onChange({ dateTo: v })}
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

// ─── Date helpers ─────────────────────────────────────────────────────────────

function parseDateIso(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number)
  return new Date(y, m - 1, d)
}

function toIsoDate(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function fmtShort(iso: string) {
  const [y, m, d] = iso.split('-').map(Number)
  return new Intl.DateTimeFormat('es-DO', { day: 'numeric', month: 'short' }).format(new Date(y, m - 1, d))
}

function fmtLong(iso: string) {
  const [y, m, d] = iso.split('-').map(Number)
  return new Intl.DateTimeFormat('es-DO', { day: 'numeric', month: 'short', year: 'numeric' }).format(new Date(y, m - 1, d))
}

function sameDay(a: Date, b: Date): boolean {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth()    === b.getMonth()    &&
    a.getDate()     === b.getDate()
  )
}

function matchesPreset(range: DateRange | undefined, preset: DateRange): boolean {
  if (!range?.from || !range?.to || !preset.from || !preset.to) return false
  return sameDay(range.from, preset.from) && sameDay(range.to, preset.to)
}

// ─── Presets ──────────────────────────────────────────────────────────────────

function todayRange(): DateRange {
  const t = new Date()
  const d = new Date(t.getFullYear(), t.getMonth(), t.getDate())
  return { from: d, to: d }
}

function yesterdayRange(): DateRange {
  const t = new Date()
  t.setDate(t.getDate() - 1)
  const d = new Date(t.getFullYear(), t.getMonth(), t.getDate())
  return { from: d, to: d }
}

function thisWeekRange(): DateRange {
  const t = new Date()
  const day = t.getDay()
  const diff = day === 0 ? 6 : day - 1
  const from = new Date(t.getFullYear(), t.getMonth(), t.getDate() - diff)
  return { from, to: new Date(t.getFullYear(), t.getMonth(), t.getDate()) }
}

function lastWeekRange(): DateRange {
  const { from: mon } = thisWeekRange()
  return {
    from: new Date(mon.getFullYear(), mon.getMonth(), mon.getDate() - 7),
    to:   new Date(mon.getFullYear(), mon.getMonth(), mon.getDate() - 1),
  }
}

function thisMonthRange(): DateRange {
  const t = new Date()
  return {
    from: new Date(t.getFullYear(), t.getMonth(), 1),
    to:   new Date(t.getFullYear(), t.getMonth(), t.getDate()),
  }
}

function lastMonthRange(): DateRange {
  const t = new Date()
  return {
    from: new Date(t.getFullYear(), t.getMonth() - 1, 1),
    to:   new Date(t.getFullYear(), t.getMonth(), 0),
  }
}

const PRESETS: Array<{ label: string; getRange: () => DateRange }> = [
  { label: 'Hoy',         getRange: todayRange },
  { label: 'Ayer',        getRange: yesterdayRange },
  { label: 'Esta semana', getRange: thisWeekRange },
  { label: 'Sem. pasada', getRange: lastWeekRange },
  { label: 'Este mes',    getRange: thisMonthRange },
  { label: 'Mes pasado',  getRange: lastMonthRange },
]

// ─── DateRangeInput ───────────────────────────────────────────────────────────

type DateRangeInputProps = {
  disabled?: boolean
  fromValue: string
  onFromChange: (v: string) => void
  onToChange: (v: string) => void
  toValue: string
}

function DateRangeInput({
  disabled = false,
  fromValue,
  onFromChange,
  onToChange,
  toValue,
}: Readonly<DateRangeInputProps>) {
  const [open, setOpen]       = useState(false)
  const [pending, setPending] = useState<DateRange | undefined>()
  const [navMonth, setNavMonth] = useState<Date>(() => new Date())

  const confirmed: DateRange = {
    from: fromValue ? parseDateIso(fromValue) : undefined,
    to:   toValue   ? parseDateIso(toValue)   : undefined,
  }

  const hasRange = Boolean(fromValue || toValue)

  const label = fromValue && toValue
    ? `${fmtShort(fromValue)} – ${fmtLong(toValue)}`
    : fromValue
      ? `Desde ${fmtLong(fromValue)}`
      : 'Rango de fechas'

  function handleOpenChange(next: boolean) {
    if (disabled) return
    if (next) {
      const seed = confirmed.from ? confirmed : undefined
      setPending(seed)
      setNavMonth(
        confirmed.from
          ? new Date(confirmed.from.getFullYear(), confirmed.from.getMonth(), 1)
          : new Date(),
      )
    } else {
      // Outside click or Escape: discard without applying
      setPending(undefined)
    }
    setOpen(next)
  }

  function handlePreset(range: DateRange) {
    setPending(range)
    if (range.from) {
      setNavMonth(new Date(range.from.getFullYear(), range.from.getMonth(), 1))
    }
  }

  function handleApply() {
    if (pending?.from) {
      onFromChange(toIsoDate(pending.from))
      onToChange(pending.to ? toIsoDate(pending.to) : toIsoDate(pending.from))
    }
    setPending(undefined)
    setOpen(false)
  }

  function handleCancel() {
    setPending(undefined)
    setOpen(false)
  }

  return (
    <Popover open={open} onOpenChange={handleOpenChange}>
      <PopoverTrigger asChild>
        <div
          aria-label="Filtrar por rango de fechas"
          className={cn(
            'flex h-8 cursor-pointer select-none items-center gap-1.5 rounded-lg border border-gray-200 bg-white px-3 transition',
            'hover:border-gray-300',
            open && 'border-primary ring-2 ring-primary/15',
            disabled && 'pointer-events-none opacity-50',
          )}
          role="button"
          tabIndex={0}
        >
          <CalendarDays aria-hidden="true" className="shrink-0 text-gray-400" size={13} />
          <span className={cn(
            'whitespace-nowrap text-[13px]',
            hasRange ? 'font-medium text-gray-800' : 'font-normal text-gray-400',
          )}>
            {label}
          </span>
          {hasRange ? (
            <button
              aria-label="Limpiar rango de fechas"
              className="flex shrink-0 items-center text-gray-300 transition hover:text-gray-600"
              tabIndex={-1}
              type="button"
              onClick={(e) => {
                e.stopPropagation()
                setPending(undefined)
                onFromChange('')
                onToChange('')
              }}
            >
              <X size={11} />
            </button>
          ) : (
            <ChevronDown
              aria-hidden="true"
              className={cn('shrink-0 text-gray-400 transition-transform duration-150', open && 'rotate-180')}
              size={12}
            />
          )}
        </div>
      </PopoverTrigger>

      <PopoverContent className="p-0 w-auto" align="start">

        {/* Preset shortcuts */}
        <div className="flex flex-wrap gap-1 border-b border-gray-100 p-3">
          {PRESETS.map((p) => {
            const range = p.getRange()
            const active = matchesPreset(pending, range)
            return (
              <button
                key={p.label}
                className={cn(
                  'h-6 rounded-md px-2.5 text-[12px] font-medium transition',
                  active
                    ? 'bg-primary text-white'
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200',
                )}
                onClick={() => handlePreset(range)}
                type="button"
              >
                {p.label}
              </button>
            )
          })}
        </div>

        {/* Calendar */}
        <Calendar
          mode="range"
          numberOfMonths={2}
          selected={pending}
          month={navMonth}
          onMonthChange={setNavMonth}
          onSelect={setPending}
        />

        {/* Footer */}
        <div className="flex justify-end gap-2 border-t border-gray-100 px-3 py-2.5">
          <button
            className="h-7 rounded-lg border border-gray-200 px-3 text-[12.5px] font-medium text-gray-600 transition hover:border-gray-300 hover:text-gray-800"
            onClick={handleCancel}
            type="button"
          >
            Cancelar
          </button>
          <button
            className={cn(
              'h-7 rounded-lg px-3 text-[12.5px] font-medium text-white transition',
              'bg-primary hover:bg-primary-hover',
              'disabled:cursor-not-allowed disabled:opacity-40',
            )}
            disabled={!pending?.from}
            onClick={handleApply}
            type="button"
          >
            Aplicar
          </button>
        </div>

      </PopoverContent>
    </Popover>
  )
}
