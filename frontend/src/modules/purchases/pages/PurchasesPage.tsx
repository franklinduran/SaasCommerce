import { ChevronLeft, ChevronRight, Eye, Plus, Search, Truck } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useState } from 'react'
import { PurchaseStatusBadge } from '@/modules/purchases/components/PurchaseStatusBadge'
import { usePurchaseRealtimeInvalidation, usePurchases } from '@/modules/purchases/hooks/usePurchases'
import type { PurchaseFilters } from '@/modules/purchases/types'
import { formatMoney } from '@/modules/purchases/utils/formatMoney'
import { useSuppliers } from '@/modules/suppliers/hooks/useSuppliers'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

const initialFilters: PurchaseFilters = {
  dateFrom: '',
  dateTo: '',
  page: 1,
  pageSize: 10,
  query: '',
  sortBy: 'purchaseDate',
  sortDirection: 'desc',
  status: '',
  supplierId: '',
}

export function PurchasesPage() {
  const [filters, setFilters] = useState(initialFilters)
  const purchases = usePurchases(filters)
  const suppliers = useSuppliers({
    isActive: 'true',
    page: 1,
    pageSize: 50,
    query: '',
    sortBy: 'name',
    sortDirection: 'asc',
  })
  const items = purchases.data?.items ?? []
  usePurchaseRealtimeInvalidation()

  function updateFilters(values: Partial<PurchaseFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="flex items-center gap-2 text-sm font-semibold uppercase text-stone-500">
            <Truck size={16} />
            Abastecimiento
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Compras</h2>
        </div>
        <div className="flex gap-2">
          <Link className={secondaryLinkClass} to="/suppliers">Proveedores</Link>
          <Link className={primaryLinkClass} to="/purchases/new">
            <Plus size={16} />
            Nueva compra
          </Link>
        </div>
      </div>

      <Card>
        <CardHeader>
          <div className="grid gap-4 xl:grid-cols-[minmax(240px,1fr)_180px_160px_160px_160px_auto]">
            <div className="flex h-11 items-center gap-2 rounded-md bg-white px-3 shadow-sm ring-1 ring-stone-200">
              <Search aria-hidden="true" className="text-stone-500" size={18} />
              <input
                className="w-full bg-transparent text-sm font-medium text-stone-900 outline-none placeholder:text-stone-400"
                onChange={(event) => updateFilters({ query: event.target.value })}
                placeholder="Factura o proveedor"
                value={filters.query}
              />
            </div>
            <select className={selectClass} onChange={(event) => updateFilters({ supplierId: event.target.value })} value={filters.supplierId}>
              <option value="">Todos los proveedores</option>
              {suppliers.data?.items.map((supplier) => (
                <option key={supplier.id} value={supplier.id}>{supplier.name}</option>
              ))}
            </select>
            <select className={selectClass} onChange={(event) => updateFilters({ status: event.target.value })} value={filters.status}>
              <option value="">Todos</option>
              <option value="Draft">Borrador</option>
              <option value="Received">Recibidas</option>
              <option value="Cancelled">Canceladas</option>
            </select>
            <input className={selectClass} onChange={(event) => updateFilters({ dateFrom: event.target.value })} type="date" value={filters.dateFrom} />
            <input className={selectClass} onChange={(event) => updateFilters({ dateTo: event.target.value })} type="date" value={filters.dateTo} />
            <Button onClick={() => void purchases.refetch()} type="button" variant="secondary">Filtrar</Button>
          </div>
        </CardHeader>
      </Card>

      <div className="grid gap-4 sm:grid-cols-3">
        <Metric label="Compras" value={String(purchases.data?.totalItems ?? 0)} />
        <Metric label="Total comprado" value={formatMoney(purchases.data?.totalPurchased ?? 0)} />
        <Metric label="Pagina" value={`${filters.page}/${purchases.data?.totalPages ?? 1}`} />
      </div>

      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-240 text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
              <tr>
                <th className="px-5 py-3">Compra</th>
                <th className="px-5 py-3">Proveedor</th>
                <th className="px-5 py-3">Fecha</th>
                <th className="px-5 py-3 text-right">Total</th>
                <th className="px-5 py-3">Estado</th>
                <th className="px-5 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200">
              {purchases.isLoading && (
                <tr><td className="px-5 py-8 text-stone-500" colSpan={6}>Cargando compras...</td></tr>
              )}
              {purchases.isError && (
                <tr><td className="px-5 py-8 text-red-700" colSpan={6}>No se pudo cargar compras.</td></tr>
              )}
              {!purchases.isLoading && !purchases.isError && items.length === 0 && (
                <tr><td className="px-5 py-10 text-center text-stone-600" colSpan={6}>No hay compras registradas.</td></tr>
              )}
              {items.map((purchase) => (
                <tr className="bg-white hover:bg-stone-50" key={purchase.purchaseId}>
                  <td className="px-5 py-4">
                    <p className="font-semibold text-stone-950">{purchase.code}</p>
                    <p className="mt-1 text-xs font-medium text-stone-500">{purchase.supplierInvoiceNumber ?? 'Sin factura'}</p>
                  </td>
                  <td className="px-5 py-4 font-semibold text-stone-800">{purchase.supplierName}</td>
                  <td className="px-5 py-4 text-stone-700">{formatDate(purchase.purchaseDate)}</td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-950">{formatMoney(purchase.total)}</td>
                  <td className="px-5 py-4"><PurchaseStatusBadge status={purchase.status} /></td>
                  <td className="px-5 py-4 text-right">
                    <Link className={smallLinkClass} to={`/purchases/${purchase.purchaseId}`}>
                      <Eye size={14} />
                      Ver
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      <div className="flex items-center justify-between rounded-md bg-white px-4 py-3 text-sm font-medium text-stone-600 shadow-sm ring-1 ring-stone-200">
        <span>Pagina {filters.page} de {purchases.data?.totalPages ?? 1}</span>
        <div className="flex gap-2">
          <Button disabled={!purchases.data?.hasPreviousPage} onClick={() => updateFilters({ page: filters.page - 1 })} type="button" variant="secondary">
            <ChevronLeft size={16} />
            Anterior
          </Button>
          <Button disabled={!purchases.data?.hasNextPage} onClick={() => updateFilters({ page: filters.page + 1 })} type="button" variant="secondary">
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
const primaryLinkClass =
  'inline-flex h-10 items-center justify-center gap-2 rounded-md bg-stone-900 px-4 text-sm font-semibold text-white shadow-sm hover:bg-stone-800'
const secondaryLinkClass =
  'inline-flex h-10 items-center justify-center gap-2 rounded-md bg-white px-4 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-300 hover:bg-stone-50'
const smallLinkClass =
  'inline-flex h-8 items-center justify-center gap-2 rounded-md bg-white px-3 text-xs font-semibold text-stone-900 shadow-sm ring-1 ring-stone-300 hover:bg-stone-50'

function Metric({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="rounded-md bg-white p-4 shadow-sm ring-1 ring-stone-200">
      <p className="text-xs font-semibold uppercase text-stone-500">{label}</p>
      <p className="mt-1 text-xl font-semibold text-stone-950">{value}</p>
    </div>
  )
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium' }).format(new Date(value))
}
