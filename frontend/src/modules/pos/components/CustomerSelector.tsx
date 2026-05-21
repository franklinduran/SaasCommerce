import { Search, UserRound } from 'lucide-react'
import type { Customer } from '@/modules/pos/types/posTypes'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
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
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <span className="flex h-10 w-10 items-center justify-center rounded-md bg-stone-100 text-stone-900">
          <UserRound aria-hidden="true" size={18} />
        </span>
        <div>
          <h2 className="text-base font-semibold text-stone-950">Cliente</h2>
          <p className="text-sm font-medium text-stone-600">{isCreditPayment ? 'Obligatorio' : 'Opcional'}</p>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        <label className="relative block">
          <Search
            aria-hidden="true"
            className="absolute left-3 top-1/2 -translate-y-1/2 text-stone-400"
            size={16}
          />
          <input
            aria-label="Buscar clientes"
            className="h-10 w-full min-w-0 rounded-md bg-white pl-9 pr-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
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
          <p className="text-sm font-semibold text-red-700">No se pudieron cargar clientes.</p>
        )}
      </CardContent>
    </Card>
  )
}

function SelectedCustomerCredit({ customer }: Readonly<{ customer?: Customer }>) {
  if (!customer) {
    return null
  }

  const creditLimit = customer.creditLimit ?? 0
  const limit = creditLimit === 0 ? 'Sin limite' : formatMoney(creditLimit)

  return (
    <div className="rounded-md bg-stone-50 p-3 text-sm font-semibold text-stone-700 ring-1 ring-stone-200">
      <p>Balance: {formatMoney(customer.currentBalance ?? 0)}</p>
      <p className="mt-1">Limite: {limit}</p>
      {customer.creditStatus === 'Blocked' && (
        <p className="mt-2 text-red-700">Credito bloqueado</p>
      )}
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
