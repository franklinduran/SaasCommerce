import { Search, UserRound } from 'lucide-react'
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
    <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
      <div className="flex items-center gap-3 border-b border-border px-5 py-4">
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-muted">
          <UserRound aria-hidden="true" className="text-muted-foreground" size={15} strokeWidth={2} />
        </span>
        <div>
          <h2 className="text-[13.5px] font-semibold text-foreground">Cliente</h2>
          <p className={cn('text-[12px]', isCreditPayment ? 'font-semibold text-amber-700' : 'text-muted-foreground')}>
            {isCreditPayment ? 'Obligatorio para fiado' : 'Opcional'}
          </p>
        </div>
      </div>

      <div className="space-y-3 p-4">
        <label className="relative block">
          <Search
            aria-hidden="true"
            className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
            size={14}
          />
          <input
            aria-label="Buscar clientes"
            className={cn(
              'h-9 w-full min-w-0 rounded-xl border border-border bg-background pl-8 pr-3 text-[13px] font-medium text-foreground outline-none transition',
              'placeholder:text-muted-foreground',
              'focus:border-ring focus:ring-2 focus:ring-ring/20',
            )}
            onChange={(event) => onQueryChange(event.target.value)}
            placeholder="Nombre o telefono"
            value={query}
          />
        </label>

        <Select
          disabled={isLoading || isError}
          value={selectedCustomerId ?? '_'}
          onValueChange={(v) => onCustomerChange(v === '_' ? null : v)}
        >
          <SelectTrigger aria-label="Seleccionar cliente"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="_">Sin cliente</SelectItem>
            {customers.map((customer) => (
              <SelectItem key={customer.id} value={customer.id}>
                {customer.fullName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        {selectedCustomerId && (
          <SelectedCustomerCredit
            customer={customers.find((customer) => customer.id === selectedCustomerId)}
          />
        )}

        {isError && (
          <p className="text-[12.5px] font-semibold text-destructive">
            No se pudieron cargar clientes.
          </p>
        )}
      </div>
    </div>
  )
}

function SelectedCustomerCredit({ customer }: Readonly<{ customer?: Customer }>) {
  if (!customer) return null

  const creditLimit = customer.creditLimit ?? 0
  const limit = creditLimit === 0 ? 'Sin limite' : formatMoney(creditLimit)

  return (
    <div className="rounded-xl bg-muted p-3 text-[12.5px] font-medium text-muted-foreground ring-1 ring-border">
      <p>
        Balance: <span className="font-semibold text-foreground">{formatMoney(customer.currentBalance ?? 0)}</span>
      </p>
      <p className="mt-1">
        Limite: <span className="font-semibold text-foreground">{limit}</span>
      </p>
      {customer.creditStatus === 'Blocked' && (
        <p className="mt-2 font-semibold text-destructive">Credito bloqueado</p>
      )}
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
