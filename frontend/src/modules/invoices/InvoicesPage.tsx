import { RefreshCw, Search } from 'lucide-react'
import { useState } from 'react'
import { InvoicesTable } from '@/modules/invoices/components/InvoicesTable'
import { useInvoiceRealtimeInvalidation, useInvoices } from '@/modules/invoices/hooks/useInvoices'
import type { InvoiceFilters } from '@/modules/invoices/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

const defaultFilters: InvoiceFilters = {
  dateFrom: '',
  dateTo: '',
  page: 1,
  pageSize: 10,
  query: '',
  status: '',
}

export function InvoicesPage() {
  const [filters, setFilters] = useState(defaultFilters)
  const invoices = useInvoices(filters)

  useInvoiceRealtimeInvalidation()

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
        <div>
          <p className="text-sm font-semibold uppercase text-stone-500">Documentos</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Recibos internos</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            {invoices.data?.totalItems ?? 0} recibos registrados
          </p>
        </div>
        <Button disabled={invoices.isLoading} onClick={() => invoices.refetch()} type="button" variant="secondary">
          <RefreshCw size={16} />
          Reintentar
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_160px_160px_180px]">
            <label className="relative block">
              <span className="sr-only">Buscar recibo</span>
              <Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" size={16} />
              <input
                className={inputClassName + ' pl-9'}
                onChange={(event) => setFilters((current) => ({ ...current, page: 1, query: event.target.value }))}
                placeholder="Buscar recibo o venta"
                value={filters.query}
              />
            </label>
            <input
              aria-label="Fecha desde"
              className={inputClassName}
              onChange={(event) => setFilters((current) => ({ ...current, dateFrom: event.target.value, page: 1 }))}
              type="date"
              value={filters.dateFrom}
            />
            <input
              aria-label="Fecha hasta"
              className={inputClassName}
              onChange={(event) => setFilters((current) => ({ ...current, dateTo: event.target.value, page: 1 }))}
              type="date"
              value={filters.dateTo}
            />
            <select
              aria-label="Estado"
              className={inputClassName}
              onChange={(event) => setFilters((current) => ({ ...current, page: 1, status: event.target.value }))}
              value={filters.status}
            >
              <option value="">Todos</option>
              <option value="Issued">Emitidos</option>
              <option value="Cancelled">Cancelados</option>
            </select>
          </div>
        </CardHeader>
        <InvoicesTable
          invoices={invoices.data?.items ?? []}
          isError={invoices.isError}
          isLoading={invoices.isLoading}
        />
      </Card>

      {invoices.data && invoices.data.totalPages > 1 && (
        <div className="flex items-center justify-end gap-2">
          <Button
            disabled={!invoices.data.hasPreviousPage}
            onClick={() => setFilters((current) => ({ ...current, page: Math.max(1, current.page - 1) }))}
            type="button"
            variant="secondary"
          >
            Anterior
          </Button>
          <Button
            disabled={!invoices.data.hasNextPage}
            onClick={() => setFilters((current) => ({ ...current, page: current.page + 1 }))}
            type="button"
            variant="secondary"
          >
            Siguiente
          </Button>
        </div>
      )}
    </section>
  )
}

const inputClassName =
  'h-11 w-full rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'
