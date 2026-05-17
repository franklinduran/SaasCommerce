import { PackageSearch, Plus, RefreshCw } from 'lucide-react'
import type { POSProduct } from '@/modules/pos/types/posTypes'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'

type ProductGridProps = {
  isError: boolean
  isLoading: boolean
  onAddProduct: (product: POSProduct) => void
  onRetry: () => void
  products: POSProduct[]
}

export function ProductGrid({
  isError,
  isLoading,
  onAddProduct,
  onRetry,
  products,
}: Readonly<ProductGridProps>) {
  if (isLoading) {
    return (
      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
        {skeletonIds.map((id) => (
          <Card className="h-32 animate-pulse bg-stone-100" key={id} />
        ))}
      </div>
    )
  }

  if (isError) {
    return (
      <Card>
        <CardContent className="flex min-h-52 flex-col items-center justify-center gap-3 p-8 text-center">
          <PackageSearch aria-hidden="true" className="text-red-500" size={28} />
          <p className="text-sm font-semibold text-red-700">No se pudo cargar el catalogo.</p>
          <Button onClick={onRetry} type="button" variant="secondary">
            <RefreshCw aria-hidden="true" size={16} />
            Reintentar
          </Button>
        </CardContent>
      </Card>
    )
  }

  if (products.length === 0) {
    return (
      <Card>
        <CardContent className="flex min-h-52 flex-col items-center justify-center gap-2 p-8 text-center">
          <PackageSearch aria-hidden="true" className="text-stone-500" size={28} />
          <p className="text-sm font-semibold text-stone-800">No hay productos para vender.</p>
          <p className="max-w-sm text-sm font-medium text-stone-500">
            Ajusta la busqueda o activa productos en el catalogo.
          </p>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
      {products.map((product) => (
        <Card className="overflow-hidden" key={product.id}>
          <div className="flex h-full flex-col p-4">
            <div className="min-h-0 flex-1">
              <div className="flex items-start justify-between gap-3">
                <div className="min-w-0">
                  <h3 className="truncate text-base font-semibold text-stone-950">
                    {product.name}
                  </h3>
                  <p className="mt-1 truncate font-mono text-xs font-semibold text-stone-500">
                    {product.sku}
                  </p>
                </div>
                <span className="shrink-0 rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200">
                  Activo
                </span>
              </div>
              <div className="mt-4 flex items-end justify-between gap-3">
                <div>
                  <p className="text-xs font-semibold uppercase text-stone-500">Precio</p>
                  <p className="mt-1 text-lg font-semibold text-stone-950">
                    {formatMoney(product.salePrice)}
                  </p>
                </div>
                <span className="rounded-md bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-700 ring-1 ring-stone-200">
                  {product.trackInventory ? 'Stock' : 'Servicio'}
                </span>
              </div>
            </div>
            <Button
              aria-label={`Agregar ${product.name}`}
              className="mt-4 w-full"
              onClick={() => onAddProduct(product)}
              type="button"
            >
              <Plus aria-hidden="true" size={16} />
              Agregar
            </Button>
          </div>
        </Card>
      ))}
    </div>
  )
}

const skeletonIds = ['pos-product-1', 'pos-product-2', 'pos-product-3', 'pos-product-4']

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
