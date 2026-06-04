import { Search } from 'lucide-react'
import type { Customer } from '@/modules/pos/types/posTypes'
import { cn } from '@/shared/utils/cn'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

type CustomerSelectorProps = {
  customers: Customer[]
  isCreditPayment?: boolean
  isError: boolean
  isLoading: boolean
  onCustomerChange: (customerId: string | null) => void
  onQueryChange: (query: string) => void
  query: string
  selectedCustomerId: string | null
}

export function CustomerSelector({
  customers,
  isCreditPayment = false,
  isError,
  isLoading,
  onCustomerChange,
  onQueryChange,
  query,
  selectedCustomerId,
}: Readonly<CustomerSelectorProps>) {
  return (
    <div>
      {/* Section header */}
      <div className="flex items-center gap-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          Cliente
        </p>
        <div className="h-px flex-1 bg-border" />
        <span
          className={cn(
            'text-[11px] font-semibold',
            isCreditPayment ? 'text-amber-600' : 'text-muted-foreground',
          )}
        >
          {isCreditPayment ? 'Obligatorio' : 'Opcional'}
        </span>
      </div>

      {/* Content */}
      <div className="mt-3 space-y-2">
        <label className="relative block">
          <Search
            aria-hidden="true"
            className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
            size={13}
          />
          <input
            aria-label="Buscar clientes"
            className={cn(
              'h-9 w-full min-w-0 rounded-lg border border-gray-200 bg-white pl-8 pr-3',
              'text-[13px] font-medium text-gray-900 outline-none transition',
              'placeholder:font-normal placeholder:text-gray-400',
              'hover:border-gray-300',
              'focus:border-primary focus:ring-2 focus:ring-primary/15',
            )}
            onChange={(e) => onQueryChange(e.target.value)}
            placeholder="Buscar por nombre o teléfono"
            value={query}
          />
        </label>

        <Select
          disabled={isLoading || isError}
          value={selectedCustomerId ?? '_'}
          onValueChange={(v) => onCustomerChange(v === '_' ? null : v)}
        >
          <SelectTrigger aria-label="Seleccionar cliente">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="_">Sin cliente</SelectItem>
            {customers.map((c) => (
              <SelectItem key={c.id} value={c.id}>
                {c.fullName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        {selectedCustomerId && (
          <CustomerCreditInfo
            customer={customers.find((c) => c.id === selectedCustomerId)}
          />
        )}

        {isError && (
          <p className="text-[12px] font-medium text-destructive">
            No se pudieron cargar clientes.
          </p>
        )}
      </div>
    </div>
  )
}

function CustomerCreditInfo({ customer }: Readonly<{ customer?: Customer }>) {
  if (!customer) return null

  const limit = (customer.creditLimit ?? 0) === 0
    ? 'Sin límite'
    : formatMoney(customer.creditLimit ?? 0)

  return (
    <div className="rounded-lg bg-muted px-3 py-2.5 text-[12.5px] text-muted-foreground">
      <div className="flex items-center justify-between gap-2">
        <span>Balance</span>
        <span className="font-semibold tabular-nums text-foreground">
          {formatMoney(customer.currentBalance ?? 0)}
        </span>
      </div>
      <div className="mt-1 flex items-center justify-between gap-2">
        <span>Límite</span>
        <span className="font-semibold text-foreground">{limit}</span>
      </div>
      {customer.creditStatus === 'Blocked' && (
        <p className="mt-2 font-semibold text-destructive">Crédito bloqueado</p>
      )}
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
