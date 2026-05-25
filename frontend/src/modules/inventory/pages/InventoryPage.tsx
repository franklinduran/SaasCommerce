import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useState } from 'react'
import { AdjustInventoryDialog } from '@/modules/inventory/components/AdjustInventoryDialog'
import { InventoryFilters } from '@/modules/inventory/components/InventoryFilters'
import { InventoryTable } from '@/modules/inventory/components/InventoryTable'
import { useInventory, useInventoryRealtimeInvalidation } from '@/modules/inventory/hooks/useInventory'
import type { StockFilters } from '@/modules/inventory/types'
import { CsvExportButton } from '@/shared/components/CsvExportButton'
import { Button } from '@/shared/components/ui/button'
import { useHasPermission } from '@/shared/hooks/usePermissions'
import { Permission } from '@/shared/types/permissions'

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

type AdjustTarget = { productId: string; productName: string }

export function InventoryPage() {
  const [filters, setFilters] = useState(initialFilters)
  const [adjustTarget, setAdjustTarget] = useState<AdjustTarget | null>(null)
  const canExportInventory = useHasPermission(Permission.InventoryExport)
  const inventory = useInventory(filters)
  const items = inventory.data?.items ?? []
  useInventoryRealtimeInvalidation()

  function updateFilters(values: Partial<StockFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  function handleAdjust(productId: string, productName: string) {
    setAdjustTarget({ productId, productName })
  }

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">Inventario</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Existencias por sucursal</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Busca productos y usa el boton Ajustar para corregir existencias fila por fila.
          </p>
        </div>
        {canExportInventory && (
          <CsvExportButton
            endpoint="/api/inventory/export"
            filename={`inventario_${new Date().toISOString().slice(0, 10)}.csv`}
          />
        )}
      </div>

      <InventoryFilters filters={filters} onChange={updateFilters} />

      <InventoryTable
        error={inventory.isError}
        isLoading={inventory.isLoading}
        items={items}
        onAdjust={handleAdjust}
        onRetry={() => inventory.refetch()}
      />

      <div className="flex flex-col gap-3 rounded-md bg-white px-4 py-3 text-sm font-medium text-stone-600 shadow-sm ring-1 ring-stone-200 sm:flex-row sm:items-center sm:justify-between">
        <span>Pagina {filters.page} de {inventory.data?.totalPages ?? 1}</span>
        <div className="flex flex-wrap gap-2">
          <Button
            disabled={!inventory.data?.hasPreviousPage}
            onClick={() => updateFilters({ page: filters.page - 1 })}
            type="button"
            variant="secondary"
          >
            <ChevronLeft size={16} />
            Anterior
          </Button>
          <Button
            disabled={!inventory.data?.hasNextPage}
            onClick={() => updateFilters({ page: filters.page + 1 })}
            type="button"
            variant="secondary"
          >
            Siguiente
            <ChevronRight size={16} />
          </Button>
        </div>
      </div>

      {adjustTarget && (
        <AdjustInventoryDialog
          onClose={() => setAdjustTarget(null)}
          productId={adjustTarget.productId}
          productName={adjustTarget.productName}
        />
      )}
    </section>
  )
}
