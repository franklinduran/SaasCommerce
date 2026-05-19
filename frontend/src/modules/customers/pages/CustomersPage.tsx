import { ChevronLeft, ChevronRight, Plus, Search, Users } from 'lucide-react'
import { useState } from 'react'
import { CustomerForm } from '@/modules/customers/components/CustomerForm'
import { CustomerTable } from '@/modules/customers/components/CustomerTable'
import {
  useCreateCustomer,
  useCustomerCreditInvalidation,
  useCustomers,
} from '@/modules/customers/hooks/useCustomers'
import type { CustomerFilters } from '@/modules/customers/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

const initialFilters: CustomerFilters = {
  isActive: 'true',
  page: 1,
  pageSize: 10,
  query: '',
  sortBy: 'fullName',
  sortDirection: 'asc',
}

export function CustomersPage() {
  const [filters, setFilters] = useState(initialFilters)
  const [isCreating, setIsCreating] = useState(false)
  const customers = useCustomers(filters)
  const createCustomer = useCreateCustomer()
  const items = customers.data?.items ?? []
  const totalItems = customers.data?.totalItems ?? 0
  const totalPages = customers.data?.totalPages ?? 0
  const pendingBalance = items.reduce((total, customer) => total + customer.currentBalance, 0)

  useCustomerCreditInvalidation()

  function updateFilters(values: Partial<CustomerFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="flex items-center gap-2 text-sm font-semibold uppercase text-stone-500">
            <Users size={16} />
            Cuentas
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Clientes</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Clientes, fiados, abonos y balances pendientes por negocio.
          </p>
        </div>
        <Button onClick={() => setIsCreating((value) => !value)} type="button">
          <Plus size={16} />
          Crear cliente
        </Button>
      </div>

      {isCreating && (
        <Card>
          <CardHeader>
            <CustomerForm
              isSubmitting={createCustomer.isPending}
              onSubmit={(request) => {
                createCustomer.mutate(request, {
                  onSuccess: () => setIsCreating(false),
                })
              }}
            />
          </CardHeader>
        </Card>
      )}

      <div className="grid gap-4 sm:grid-cols-3">
        <Metric label="Clientes" value={String(totalItems)} />
        <Metric label="Fiados visibles" value={formatMoney(pendingBalance)} />
        <Metric label="Bloqueados" value={String(items.filter((customer) => customer.creditStatus === 'Blocked').length)} />
      </div>

      <Card>
        <CardHeader>
          <div className="grid gap-4 lg:grid-cols-[minmax(260px,1fr)_180px_160px_auto]">
            <div className="flex h-11 items-center gap-2 rounded-md bg-white px-3 shadow-sm ring-1 ring-stone-200">
              <Search aria-hidden="true" className="text-stone-500" size={18} />
              <input
                className="w-full bg-transparent text-sm font-medium text-stone-900 outline-none placeholder:text-stone-400"
                onChange={(event) => updateFilters({ query: event.target.value })}
                placeholder="Nombre o telefono"
                value={filters.query}
              />
            </div>
            <select className={selectClass} onChange={(event) => updateFilters({ isActive: event.target.value })} value={filters.isActive}>
              <option value="">Todos</option>
              <option value="true">Activos</option>
              <option value="false">Inactivos</option>
            </select>
            <select className={selectClass} onChange={(event) => updateFilters({ pageSize: Number(event.target.value) })} value={filters.pageSize}>
              <option value={10}>10 por pagina</option>
              <option value={25}>25 por pagina</option>
              <option value={50}>50 por pagina</option>
            </select>
            <Button onClick={() => customers.refetch()} type="button" variant="secondary">Filtrar</Button>
          </div>
        </CardHeader>
      </Card>

      <Card className="overflow-hidden">
        <CustomerTable
          customers={items}
          isError={customers.isError}
          isLoading={customers.isLoading}
        />
      </Card>

      <div className="flex items-center justify-between rounded-md bg-white px-4 py-3 text-sm font-medium text-stone-600 shadow-sm ring-1 ring-stone-200">
        <span>Pagina {filters.page} de {totalPages || 1}</span>
        <div className="flex gap-2">
          <Button disabled={!customers.data?.hasPreviousPage} onClick={() => updateFilters({ page: filters.page - 1 })} type="button" variant="secondary">
            <ChevronLeft size={16} />
            Anterior
          </Button>
          <Button disabled={!customers.data?.hasNextPage} onClick={() => updateFilters({ page: filters.page + 1 })} type="button" variant="secondary">
            Siguiente
            <ChevronRight size={16} />
          </Button>
        </div>
      </div>
    </section>
  )
}

const selectClass =
  'h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'

function Metric({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="rounded-md bg-white p-4 shadow-sm ring-1 ring-stone-200">
      <p className="text-xs font-semibold uppercase text-stone-500">{label}</p>
      <p className="mt-1 text-xl font-semibold text-stone-950">{value}</p>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
