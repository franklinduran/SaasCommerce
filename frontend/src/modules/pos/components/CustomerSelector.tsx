import { Loader2, Plus, Search, X } from 'lucide-react'
import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createCustomer } from '@/modules/customers/services/customersApi'
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
  const [showForm, setShowForm] = useState(false)

  function handleCreated(customerId: string) {
    setShowForm(false)
    onCustomerChange(customerId)
  }

  return (
    <div>
      {/* Section header */}
      <div className="flex items-center gap-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          Cliente
        </p>
        <div className="h-px flex-1 bg-border" />
        <button
          aria-label={showForm ? 'Cancelar nuevo cliente' : 'Crear cliente rápido'}
          className={cn(
            'flex h-6 items-center gap-1 rounded-md border border-gray-200 bg-white px-2 text-[11px] font-medium text-gray-600 shadow-sm transition',
            'hover:border-gray-300 hover:text-gray-900',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/20',
          )}
          onClick={() => setShowForm((v) => !v)}
          type="button"
        >
          {showForm
            ? <><X aria-hidden="true" size={11} strokeWidth={2} /> Cancelar</>
            : <><Plus aria-hidden="true" size={11} strokeWidth={2} /> Nuevo</>}
        </button>
        <span
          className={cn(
            'text-[11px] font-semibold',
            isCreditPayment ? 'text-amber-600' : 'text-muted-foreground',
          )}
        >
          {isCreditPayment ? 'Obligatorio' : 'Opcional'}
        </span>
      </div>

      {/* Quick-create form */}
      {showForm && (
        <QuickCreateForm
          onCancel={() => setShowForm(false)}
          onCreated={handleCreated}
        />
      )}

      {/* Search + select (hidden while form is open) */}
      {!showForm && (
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
      )}
    </div>
  )
}

// ─── Quick-create form ─────────────────────────────────────────────────────────

function QuickCreateForm({
  onCancel,
  onCreated,
}: Readonly<{ onCancel: () => void; onCreated: (id: string) => void }>) {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [phone, setPhone] = useState('')
  const [errors, setErrors] = useState<{ firstName?: string; lastName?: string }>({})
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: () =>
      createCustomer({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phone: phone.trim() || null,
        email: null,
        isActive: true,
      }),
    onSuccess: async (customer) => {
      await queryClient.invalidateQueries({ queryKey: ['pos-customers'] })
      onCreated(customer.id)
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const next: typeof errors = {}
    if (!firstName.trim()) next.firstName = 'Requerido'
    if (!lastName.trim()) next.lastName = 'Requerido'
    if (Object.keys(next).length) { setErrors(next); return }
    setErrors({})
    mutation.mutate()
  }

  const inputClass = cn(
    'h-9 w-full rounded-lg border bg-white px-3',
    'text-[13px] font-medium text-gray-900 outline-none transition',
    'placeholder:font-normal placeholder:text-gray-400',
    'hover:border-gray-300',
    'focus:border-primary focus:ring-2 focus:ring-primary/15',
  )

  return (
    <form className="mt-3 space-y-2" onSubmit={handleSubmit}>
      {/* Nombre + Apellido */}
      <div className="grid grid-cols-2 gap-2">
        <div>
          <input
            aria-label="Nombre"
            autoFocus
            className={cn(inputClass, errors.firstName ? 'border-red-300' : 'border-gray-200')}
            onChange={(e) => { setFirstName(e.target.value); setErrors((p) => ({ ...p, firstName: undefined })) }}
            placeholder="Nombre *"
            type="text"
            value={firstName}
          />
          {errors.firstName && (
            <p className="mt-0.5 text-[11px] text-destructive">{errors.firstName}</p>
          )}
        </div>
        <div>
          <input
            aria-label="Apellido"
            className={cn(inputClass, errors.lastName ? 'border-red-300' : 'border-gray-200')}
            onChange={(e) => { setLastName(e.target.value); setErrors((p) => ({ ...p, lastName: undefined })) }}
            placeholder="Apellido *"
            type="text"
            value={lastName}
          />
          {errors.lastName && (
            <p className="mt-0.5 text-[11px] text-destructive">{errors.lastName}</p>
          )}
        </div>
      </div>

      {/* Phone */}
      <input
        aria-label="Teléfono del cliente"
        className={cn(inputClass, 'border-gray-200')}
        inputMode="tel"
        onChange={(e) => setPhone(e.target.value)}
        placeholder="Teléfono (opcional)"
        type="tel"
        value={phone}
      />

      {/* API error */}
      {mutation.isError && (
        <p className="text-[11.5px] font-medium text-destructive">
          No se pudo crear el cliente. Intenta de nuevo.
        </p>
      )}

      {/* Actions */}
      <div className="flex gap-2">
        <button
          className={cn(
            'flex h-9 flex-1 items-center justify-center gap-1.5 rounded-lg bg-primary text-[13px] font-semibold text-white transition',
            'hover:bg-primary/90',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/25',
            'disabled:cursor-not-allowed disabled:opacity-50',
          )}
          disabled={mutation.isPending}
          type="submit"
        >
          {mutation.isPending
            ? <><Loader2 aria-hidden="true" className="animate-spin" size={13} /> Creando…</>
            : <><Plus aria-hidden="true" size={13} /> Crear cliente</>}
        </button>
        <button
          className={cn(
            'flex h-9 items-center justify-center rounded-lg border border-gray-200 bg-white px-3 text-[13px] font-medium text-gray-600 transition',
            'hover:border-gray-300 hover:text-gray-900',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/20',
          )}
          onClick={onCancel}
          type="button"
        >
          Cancelar
        </button>
      </div>
    </form>
  )
}

// ─── Credit info ────────────────────────────────────────────────────────────────

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
