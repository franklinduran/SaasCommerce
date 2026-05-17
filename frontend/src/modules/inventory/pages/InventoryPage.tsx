import { useState } from 'react'
import { Boxes, SlidersHorizontal } from 'lucide-react'
import { AdjustInventoryDialog } from '@/modules/inventory/components/AdjustInventoryDialog'
import { InventoryFilters } from '@/modules/inventory/components/InventoryFilters'
import { InventoryTable } from '@/modules/inventory/components/InventoryTable'
import { useInventory, useInventoryRealtimeInvalidation } from '@/modules/inventory/hooks/useInventory'
import type { StockFilters } from '@/modules/inventory/types'
import { Button } from '@/shared/components/ui/button'

const initialFilters: StockFilters = {
  branchId: '',
  categoryId: '',
  lowStockOnly: false,
  outOfStockOnly: false,
  page: 1,
  pageSize: 10,
  productId: '',
  productType: '',
  search: '',
  sortBy: 'productId',
  sortDirection: 'asc',
}

export function InventoryPage() {
  const [filters, setFilters] = useState(initialFilters)
  const [isAdjustOpen, setIsAdjustOpen] = useState(false)
  const inventory = useInventory(filters)
  const items = inventory.data?.items ?? []
  useInventoryRealtimeInvalidation()

  function updateFilters(values: Partial<StockFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="flex items-center gap-2 text-sm font-semibold uppercase text-stone-500">
            <Boxes size={16} />
            Inventario
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Existencias por sucursal</h2>
        </div>
        <Button onClick={() => setIsAdjustOpen(true)} type="button">
          <SlidersHorizontal size={16} />
          Ajustar inventario
        </Button>
      </div>

      <InventoryFilters filters={filters} onChange={updateFilters} />

      <InventoryTable
        error={inventory.isError}
        isLoading={inventory.isLoading}
        items={items}
        onRetry={() => void inventory.refetch()}
      />

      <div className="flex items-center justify-between rounded-md bg-white px-4 py-3 text-sm font-medium text-stone-600 shadow-sm ring-1 ring-stone-200">
        <span>Pagina {filters.page} de {inventory.data?.totalPages ?? 1}</span>
        <div className="flex gap-2">
          <Button disabled={!inventory.data?.hasPreviousPage} onClick={() => updateFilters({ page: filters.page - 1 })} type="button" variant="secondary">
            Anterior
          </Button>
          <Button disabled={!inventory.data?.hasNextPage} onClick={() => updateFilters({ page: filters.page + 1 })} type="button" variant="secondary">
            Siguiente
          </Button>
        </div>
      </div>

      {isAdjustOpen && <AdjustInventoryDialog onClose={() => setIsAdjustOpen(false)} />}
    </section>
  )
}
