import { Plus, Search, Truck, X } from 'lucide-react'
import type { ReactNode } from 'react'
import { useState } from 'react'
import { SupplierForm } from '@/modules/suppliers/components/SupplierForm'
import { SuppliersTable } from '@/modules/suppliers/components/SuppliersTable'
import { useSuppliers, useUpdateSupplier } from '@/modules/suppliers/hooks/useSuppliers'
import type { Supplier, SupplierFilters } from '@/modules/suppliers/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

const initialFilters: SupplierFilters = {
  isActive: 'true',
  page: 1,
  pageSize: 10,
  query: '',
  sortBy: 'name',
  sortDirection: 'asc',
}

export function SuppliersPage() {
  const [filters, setFilters] = useState(initialFilters)
  const [selectedSupplier, setSelectedSupplier] = useState<Supplier | null>(null)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const suppliers = useSuppliers(filters)
  const updateSupplier = useUpdateSupplier()
  const items = suppliers.data?.items ?? []

  function updateFilters(values: Partial<SupplierFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  function openCreate() {
    setSelectedSupplier(null)
    setDrawerOpen(true)
  }

  function openEdit(supplier: Supplier) {
    setSelectedSupplier(supplier)
    setDrawerOpen(true)
  }

  async function toggleStatus(supplier: Supplier) {
    await updateSupplier.mutateAsync({
      supplierId: supplier.id,
      request: {
        address: supplier.address,
        email: supplier.email,
        isActive: !supplier.isActive,
        name: supplier.name,
        phone: supplier.phone,
        rnc: supplier.rnc,
      },
    })
  }

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="flex items-center gap-2 text-sm font-semibold uppercase text-stone-500">
            <Truck size={16} />
            Compras
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Proveedores</h2>
        </div>
        <Button onClick={openCreate} type="button">
          <Plus size={16} />
          Crear proveedor
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="grid gap-4 lg:grid-cols-[minmax(260px,1fr)_180px_auto]">
            <div className="flex h-11 items-center gap-2 rounded-md bg-white px-3 shadow-sm ring-1 ring-stone-200">
              <Search aria-hidden="true" className="text-stone-500" size={18} />
              <input
                className="w-full bg-transparent text-sm font-medium text-stone-900 outline-none placeholder:text-stone-400"
                onChange={(event) => updateFilters({ query: event.target.value })}
                placeholder="Nombre, RNC, telefono o correo"
                value={filters.query}
              />
            </div>
            <select
              className="h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none"
              onChange={(event) => updateFilters({ isActive: event.target.value })}
              value={filters.isActive}
            >
              <option value="">Todos</option>
              <option value="true">Activos</option>
              <option value="false">Inactivos</option>
            </select>
            <Button onClick={() => suppliers.refetch()} type="button" variant="secondary">
              Filtrar
            </Button>
          </div>
        </CardHeader>
      </Card>

      <SuppliersTable
        error={suppliers.isError}
        isLoading={suppliers.isLoading}
        items={items}
        onEdit={openEdit}
        onRetry={() => suppliers.refetch()}
        onToggle={(supplier) => toggleStatus(supplier)}
      />

      <div className="flex items-center justify-between rounded-md bg-white px-4 py-3 text-sm font-medium text-stone-600 shadow-sm ring-1 ring-stone-200">
        <span>Pagina {filters.page} de {suppliers.data?.totalPages ?? 1}</span>
        <div className="flex gap-2">
          <Button disabled={!suppliers.data?.hasPreviousPage} onClick={() => updateFilters({ page: filters.page - 1 })} type="button" variant="secondary">
            Anterior
          </Button>
          <Button disabled={!suppliers.data?.hasNextPage} onClick={() => updateFilters({ page: filters.page + 1 })} type="button" variant="secondary">
            Siguiente
          </Button>
        </div>
      </div>

      {drawerOpen && (
        <Drawer onClose={() => setDrawerOpen(false)} title={selectedSupplier ? 'Editar proveedor' : 'Crear proveedor'}>
          <SupplierForm onSaved={() => setDrawerOpen(false)} supplier={selectedSupplier} />
        </Drawer>
      )}
    </section>
  )
}

function Drawer({
  children,
  onClose,
  title,
}: Readonly<{
  children: ReactNode
  onClose: () => void
  title: string
}>) {
  return (
    <div className="fixed inset-0 z-50 bg-stone-950/20">
      <div className="absolute inset-y-0 right-0 flex w-full max-w-3xl flex-col bg-white shadow-xl ring-1 ring-stone-200">
        <div className="flex h-16 shrink-0 items-center justify-between border-b border-stone-200 px-6">
          <h3 className="text-lg font-semibold text-stone-950">{title}</h3>
          <Button aria-label="Cerrar drawer" onClick={onClose} size="icon" type="button" variant="ghost">
            <X size={18} />
          </Button>
        </div>
        <div className="min-h-0 flex-1 overflow-y-auto p-6">{children}</div>
      </div>
    </div>
  )
}
