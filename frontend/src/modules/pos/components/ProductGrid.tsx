import { ImageOff, PackageSearch, Plus, RefreshCw } from 'lucide-react'
import type { POSProduct } from '@/modules/pos/types/posTypes'
import { Button } from '@/shared/components/ui/button'

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
      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
        {skeletonIds.map((id) => (
          <div className="h-64 animate-pulse rounded-2xl bg-muted" key={id} />
        ))}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex min-h-52 flex-col items-center justify-center gap-3 rounded-2xl border border-border bg-card p-8 text-center shadow-sm">
        <PackageSearch aria-hidden="true" className="text-destructive" size={28} />
        <p className="text-sm font-semibold text-destructive">No se pudo cargar el catalogo.</p>
        <Button onClick={onRetry} type="button" variant="secondary">
          <RefreshCw aria-hidden="true" size={16} />
          Reintentar
        </Button>
      </div>
    )
  }

  if (products.length === 0) {
    return (
      <div className="flex min-h-52 flex-col items-center justify-center gap-2 rounded-2xl border border-border bg-card p-8 text-center shadow-sm">
        <PackageSearch aria-hidden="true" className="text-muted-foreground" size={28} />
        <p className="text-sm font-semibold text-foreground">No hay productos para vender.</p>
        <p className="max-w-sm text-sm text-muted-foreground">
          Ajusta la busqueda o activa productos en el catalogo.
        </p>
      </div>
    )
  }

  return (
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
      {products.map((product) => (
        <ProductCard key={product.id} onAdd={onAddProduct} product={product} />
      ))}
    </div>
  )
}

function ProductCard({
  onAdd,
  product,
}: Readonly<{ onAdd: (p: POSProduct) => void; product: POSProduct }>) {
  return (
    <div className="group flex flex-col overflow-hidden rounded-2xl border border-border bg-card shadow-sm transition-shadow hover:shadow-md">
      <ProductImage imageUrl={product.imageUrl} name={product.name} />

      <div className="flex flex-1 flex-col gap-3 p-4">
        <div className="min-w-0 flex-1">
          <div className="flex items-start justify-between gap-2">
            <h3 className="line-clamp-2 text-[13.5px] font-semibold leading-snug text-foreground">
              {product.name}
            </h3>
            <span className="shrink-0 rounded-full bg-emerald-50 px-2 py-0.5 text-[10.5px] font-semibold text-emerald-700 ring-1 ring-emerald-200">
              Activo
            </span>
          </div>
          <p className="mt-1 font-mono text-[11px] font-medium text-muted-foreground">
            {product.sku}
          </p>
        </div>

        <div className="flex items-end justify-between gap-2">
          <div>
            <p className="text-[10.5px] font-semibold uppercase tracking-[0.08em] text-muted-foreground">
              Precio
            </p>
            <p className="mt-0.5 text-lg font-bold tracking-tight text-foreground">
              {formatMoney(product.salePrice)}
            </p>
          </div>
          <span className="rounded-lg bg-muted px-2 py-0.5 text-[11px] font-medium text-muted-foreground ring-1 ring-border">
            {product.trackInventory ? 'Stock' : 'Servicio'}
          </span>
        </div>

        <Button
          aria-label={`Agregar ${product.name}`}
          className="mt-auto w-full"
          onClick={() => onAdd(product)}
          type="button"
        >
          <Plus aria-hidden="true" size={15} />
          Agregar
        </Button>
      </div>
    </div>
  )
}

function ProductImage({
  imageUrl,
  name,
}: Readonly<{ imageUrl: string | null | undefined; name: string }>) {
  if (!imageUrl) {
    return (
      <div className="flex h-36 items-center justify-center bg-muted">
        <ImageOff aria-hidden="true" className="text-muted-foreground/40" size={32} />
      </div>
    )
  }

  return (
    <img
      alt={name}
      className="h-36 w-full object-cover"
      loading="lazy"
      onError={(e) => {
        e.currentTarget.style.display = 'none'
        e.currentTarget.nextElementSibling?.classList.remove('hidden')
      }}
      src={imageUrl}
    />
  )
}

const skeletonIds = [
  'pos-product-1',
  'pos-product-2',
  'pos-product-3',
  'pos-product-4',
  'pos-product-5',
  'pos-product-6',
]

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
