import { useState } from 'react'
import { ArrowLeft, SlidersHorizontal } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { AdjustInventoryDialog } from '@/modules/inventory/components/AdjustInventoryDialog'
import { InventoryMovementList } from '@/modules/inventory/components/InventoryMovementList'
import { StockStatusBadge } from '@/modules/inventory/components/StockStatusBadge'
import { useInventoryProductDetail, useInventoryRealtimeInvalidation } from '@/modules/inventory/hooks/useInventory'
import { Button } from '@/shared/components/ui/button'

export function InventoryProductDetailPage() {
  const { productId } = useParams()
  const [isAdjustOpen, setIsAdjustOpen] = useState(false)
  const detail = useInventoryProductDetail(productId)
  useInventoryRealtimeInvalidation(productId)

  if (detail.isLoading) {
    return <section className="p-6 text-sm font-medium text-stone-600 lg:p-8">Cargando detalle de inventario...</section>
  }

  if (detail.isError || !detail.data) {
    return (
      <section className="space-y-4 p-6 lg:p-8">
        <Link className="inline-flex items-center gap-2 text-sm font-semibold text-stone-700" to="/inventory">
          <ArrowLeft size={16} />
          Volver
        </Link>
        <p className="rounded-md bg-red-50 p-4 text-sm font-medium text-red-700 ring-1 ring-red-200">
          No se pudo cargar el detalle del producto.
        </p>
      </section>
    )
  }

  const product = detail.data

  return (
    <section className="space-y-6 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div>
          <Link className="mb-4 inline-flex items-center gap-2 text-sm font-semibold text-stone-700" to="/inventory">
            <ArrowLeft size={16} />
            Volver
          </Link>
          <h2 className="text-2xl font-semibold text-stone-950">{product.productName}</h2>
          <p className="mt-1 text-sm font-medium text-stone-600">{product.sku} · {product.unitOfMeasure}</p>
        </div>
        <Button onClick={() => setIsAdjustOpen(true)} type="button">
          <SlidersHorizontal size={16} />
          Ajustar inventario
        </Button>
      </div>

      <div className="grid gap-3 lg:grid-cols-3">
        {product.branches.map((branch) => (
          <article className="rounded-md bg-white p-4 shadow-sm ring-1 ring-stone-200" key={branch.branchId}>
            <div className="flex items-start justify-between gap-3">
              <div>
                <p className="text-sm font-semibold text-stone-950">{branch.branchName}</p>
                <p className="mt-2 text-2xl font-semibold text-stone-950">{branch.currentStock}</p>
                <p className="text-xs font-medium text-stone-500">Minimo {branch.minimumStock ?? '-'}</p>
              </div>
              <StockStatusBadge status={branch.status} />
            </div>
          </article>
        ))}
      </div>

      {product.alerts.length > 0 && (
        <div className="rounded-md bg-amber-50 p-4 text-sm font-semibold text-amber-800 ring-1 ring-amber-200">
          {product.alerts.length} alerta(s) de stock bajo activas.
        </div>
      )}

      <div>
        <h3 className="mb-3 text-lg font-semibold text-stone-950">Historial de movimientos</h3>
        <InventoryMovementList movements={product.recentMovements} />
      </div>

      {isAdjustOpen && <AdjustInventoryDialog onClose={() => setIsAdjustOpen(false)} productId={product.productId} />}
    </section>
  )
}
