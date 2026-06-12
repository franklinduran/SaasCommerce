import { ChevronLeft, ChevronRight, ImageOff, PackageSearch, Plus, RefreshCw } from 'lucide-react'
import type { ProductViewMode } from '@/modules/pos/components/ProductSearch'
import type { POSProduct } from '@/modules/pos/types/posTypes'
import { Button } from '@/shared/components/ui/button'
import { cn } from '@/shared/utils/cn'

type ProductGridProps = {
  cartQuantities: ReadonlyMap<string, number>
  isError: boolean
  isLoading: boolean
  onAddProduct: (product: POSProduct) => void
  onNextPage: () => void
  onPrevPage: () => void
  onRetry: () => void
  page: number
  products: POSProduct[]
  totalPages: number
  view: ProductViewMode
}

export function ProductGrid({
  cartQuantities,
  isError,
  isLoading,
  onAddProduct,
  onNextPage,
  onPrevPage,
  onRetry,
  page,
  products,
  totalPages,
  view,
}: Readonly<ProductGridProps>) {
  // ── Loading ──────────────────────────────────────────────────────────────────
  if (isLoading) {
    return view === 'grid' ? <GridSkeleton /> : <ListSkeleton />
  }

  // ── Error ────────────────────────────────────────────────────────────────────
  if (isError) {
    return (
      <div className="flex min-h-40 flex-col items-center justify-center gap-3 py-10 text-center">
        <PackageSearch aria-hidden="true" className="text-muted-foreground/40" size={32} />
        <p className="text-[13.5px] font-semibold text-foreground">No se pudo cargar el catalogo.</p>
        <Button onClick={onRetry} size="sm" type="button" variant="secondary">
          <RefreshCw aria-hidden="true" size={14} />
          Reintentar
        </Button>
      </div>
    )
  }

  // ── Empty ────────────────────────────────────────────────────────────────────
  if (products.length === 0) {
    return (
      <div className="flex min-h-40 flex-col items-center justify-center gap-2 py-10 text-center">
        <PackageSearch aria-hidden="true" className="text-muted-foreground/40" size={32} />
        <p className="text-[13.5px] font-semibold text-foreground">No hay productos para vender.</p>
        <p className="text-[12.5px] text-muted-foreground">
          Ajusta la búsqueda o activa productos en el catálogo.
        </p>
      </div>
    )
  }

  const hasPrev = page > 1
  const hasNext = page < totalPages
  const showPagination = totalPages > 1

  return (
    <div className="flex flex-col gap-4">
      {view === 'grid' ? (
        <GridView cartQuantities={cartQuantities} onAdd={onAddProduct} products={products} />
      ) : (
        <ListView cartQuantities={cartQuantities} onAdd={onAddProduct} products={products} />
      )}

      {showPagination && (
        <Pagination
          hasNext={hasNext}
          hasPrev={hasPrev}
          onNext={onNextPage}
          onPrev={onPrevPage}
          page={page}
          totalPages={totalPages}
        />
      )}
    </div>
  )
}

// ─── Pagination ────────────────────────────────────────────────────────────────

function Pagination({
  hasNext,
  hasPrev,
  onNext,
  onPrev,
  page,
  totalPages,
}: Readonly<{
  hasNext: boolean
  hasPrev: boolean
  onNext: () => void
  onPrev: () => void
  page: number
  totalPages: number
}>) {
  return (
    <div className="flex items-center justify-between gap-3">
      <p className="text-[12px] text-muted-foreground">
        Página <span className="font-semibold text-foreground">{page}</span> de{' '}
        <span className="font-semibold text-foreground">{totalPages}</span>
      </p>
      <div className="flex gap-1">
        <button
          aria-label="Página anterior"
          className={cn(
            'flex h-8 w-8 items-center justify-center rounded-lg border border-gray-200 bg-white text-gray-500 transition',
            'hover:border-gray-300 hover:text-gray-900',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/20',
            'disabled:cursor-not-allowed disabled:opacity-40',
          )}
          disabled={!hasPrev}
          onClick={onPrev}
          type="button"
        >
          <ChevronLeft aria-hidden="true" size={15} />
        </button>
        <button
          aria-label="Página siguiente"
          className={cn(
            'flex h-8 w-8 items-center justify-center rounded-lg border border-gray-200 bg-white text-gray-500 transition',
            'hover:border-gray-300 hover:text-gray-900',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/20',
            'disabled:cursor-not-allowed disabled:opacity-40',
          )}
          disabled={!hasNext}
          onClick={onNext}
          type="button"
        >
          <ChevronRight aria-hidden="true" size={15} />
        </button>
      </div>
    </div>
  )
}

// ─── Card grid ─────────────────────────────────────────────────────────────────

function GridView({
  cartQuantities,
  onAdd,
  products,
}: Readonly<{ cartQuantities: ReadonlyMap<string, number>; onAdd: (p: POSProduct) => void; products: POSProduct[] }>) {
  return (
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
      {products.map((product) => (
        <ProductCard
          cartQty={cartQuantities.get(product.id) ?? 0}
          key={product.id}
          onAdd={onAdd}
          product={product}
        />
      ))}
    </div>
  )
}

function ProductCard({
  cartQty,
  onAdd,
  product,
}: Readonly<{ cartQty: number; onAdd: (p: POSProduct) => void; product: POSProduct }>) {
  return (
    <div className="group relative flex flex-col overflow-hidden rounded-2xl border border-border bg-card shadow-sm transition-shadow hover:shadow-md">
      {cartQty > 0 && (
        <span className="absolute right-2 top-2 z-10 flex h-6 min-w-6 items-center justify-center rounded-full bg-primary px-1.5 text-[11px] font-bold tabular-nums text-white shadow-sm">
          {cartQty}
        </span>
      )}
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

// ─── Table list ────────────────────────────────────────────────────────────────

function ListView({
  cartQuantities,
  onAdd,
  products,
}: Readonly<{ cartQuantities: ReadonlyMap<string, number>; onAdd: (p: POSProduct) => void; products: POSProduct[] }>) {
  return (
    <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
      {/* Table header */}
      <div className="grid grid-cols-[auto_minmax(0,1fr)_auto_auto_auto] items-center gap-3 border-b border-gray-100 px-4 py-2.5">
        <div className="hidden w-9 sm:block" />
        <p className="text-[11px] font-semibold uppercase tracking-[0.1em] text-muted-foreground">
          Producto
        </p>
        <p className="hidden w-16 text-right text-[11px] font-semibold uppercase tracking-[0.1em] text-muted-foreground md:block">
          Tipo
        </p>
        <p className="w-24 text-[11px] font-semibold uppercase tracking-[0.1em] text-muted-foreground">
          Precio
        </p>
        <div className="w-20" />
      </div>

      {/* Table rows */}
      <div className="divide-y divide-gray-100">
        {products.map((product) => (
          <ProductRow
            cartQty={cartQuantities.get(product.id) ?? 0}
            key={product.id}
            onAdd={onAdd}
            product={product}
          />
        ))}
      </div>
    </div>
  )
}

function ProductRow({
  cartQty,
  onAdd,
  product,
}: Readonly<{ cartQty: number; onAdd: (p: POSProduct) => void; product: POSProduct }>) {
  return (
    <div className={cn(
      'grid grid-cols-[auto_minmax(0,1fr)_auto_auto_auto] items-center gap-3 px-4 py-2.5 transition-colors hover:bg-gray-50/70',
      cartQty > 0 && 'bg-primary/[0.03]',
    )}>
      {/* Thumbnail + cart badge */}
      <div className="relative hidden shrink-0 sm:block">
        <ProductThumbnail imageUrl={product.imageUrl} name={product.name} />
        {cartQty > 0 && (
          <span className="absolute -right-1.5 -top-1.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-bold tabular-nums text-white shadow-sm">
            {cartQty}
          </span>
        )}
      </div>

      {/* Name + SKU */}
      <div className="min-w-0">
        <p className="truncate text-[13px] font-semibold text-foreground">{product.name}</p>
        <p className="mt-0.5 font-mono text-[11px] text-muted-foreground">{product.sku}</p>
      </div>

      {/* Type */}
      <span className="hidden w-16 text-right text-[11px] font-medium text-muted-foreground md:block">
        {product.trackInventory ? 'Stock' : 'Servicio'}
      </span>

      {/* Price */}
      <p className="w-24 text-[13.5px] font-bold tabular-nums tracking-tight text-foreground">
        {formatMoney(product.salePrice)}
      </p>

      {/* Add */}
      <Button
        aria-label={`Agregar ${product.name}`}
        className="w-20"
        onClick={() => onAdd(product)}
        size="sm"
        type="button"
      >
        <Plus aria-hidden="true" size={14} />
        Agregar
      </Button>
    </div>
  )
}

// ─── Image helpers ──────────────────────────────────────────────────────────────

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
    <img alt={name} className="h-36 w-full object-cover" loading="lazy" src={imageUrl} />
  )
}

function ProductThumbnail({
  imageUrl,
  name,
}: Readonly<{ imageUrl: string | null | undefined; name: string }>) {
  if (!imageUrl) {
    return (
      <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-muted">
        <ImageOff aria-hidden="true" className="text-muted-foreground/40" size={14} />
      </div>
    )
  }
  return (
    <img
      alt={name}
      className="h-9 w-9 rounded-lg object-cover ring-1 ring-border"
      loading="lazy"
      src={imageUrl}
    />
  )
}

// ─── Skeletons ──────────────────────────────────────────────────────────────────

function GridSkeleton() {
  return (
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
      {skeletonIds.map((id) => (
        <div className="h-64 animate-pulse rounded-2xl bg-muted" key={id} />
      ))}
    </div>
  )
}

function ListSkeleton() {
  return (
    <div className="divide-y divide-gray-100">
      {skeletonIds.map((id) => (
        <div className="flex items-center gap-3 px-3 py-2.5" key={id}>
          <div className="hidden h-9 w-9 animate-pulse rounded-lg bg-muted sm:block" />
          <div className="flex-1 space-y-2">
            <div className="h-3.5 w-1/2 animate-pulse rounded bg-muted" />
            <div className="h-2.5 w-1/4 animate-pulse rounded bg-muted" />
          </div>
          <div className="h-3.5 w-16 animate-pulse rounded bg-muted" />
          <div className="h-8 w-20 animate-pulse rounded-lg bg-muted" />
        </div>
      ))}
    </div>
  )
}

const skeletonIds = [
  'pos-product-1', 'pos-product-2', 'pos-product-3',
  'pos-product-4', 'pos-product-5', 'pos-product-6',
]

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
