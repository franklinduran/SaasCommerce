import {
  ChevronLeft,
  ChevronRight,
  Pencil,
  Plus,
  RefreshCw,
  Search,
  SlidersHorizontal,
  X,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { useMemo, useState } from 'react'
import { ProductForm } from '@/modules/products/components/ProductForm'
import { useProductsQuery } from '@/modules/products/hooks/useProducts'
import type { Product, ProductFilters } from '@/modules/products/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

const productTypes = [
  { label: 'Todos', value: '' },
  { label: 'Simple', value: 'Simple' },
  { label: 'Servicio', value: 'Service' },
  { label: 'Pesado', value: 'Weighed' },
  { label: 'Combo', value: 'Composite' },
  { label: 'Padre', value: 'VariantParent' },
  { label: 'Variante', value: 'VariantChild' },
]

const pageSizes = [10, 25, 50]

export function ProductsPage() {
  const [filters, setFilters] = useState<ProductFilters>({
    categoryId: '',
    isActive: 'true',
    page: 1,
    pageSize: 10,
    productType: '',
    query: '',
  })
  const [drawerMode, setDrawerMode] = useState<'create' | 'edit' | null>(null)
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
  const [savedMessage, setSavedMessage] = useState<string | null>(null)
  const products = useProductsQuery(filters)
  const items = products.data?.items ?? []
  const totalItems = products.data?.totalItems ?? 0
  const totalPages = products.data?.totalPages ?? 0
  const canGoPrevious = filters.page > 1
  const canGoNext = totalPages > 0 && filters.page < totalPages

  const drawerTitle = useMemo(
    () => (drawerMode === 'edit' ? 'Editar producto' : 'Crear producto'),
    [drawerMode],
  )

  function updateFilters(next: Partial<ProductFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      page: next.page ?? 1,
    }))
  }

  function openCreateDrawer() {
    setSelectedProduct(null)
    setDrawerMode('create')
    setSavedMessage(null)
  }

  function openEditDrawer(product: Product) {
    setSelectedProduct(product)
    setDrawerMode('edit')
    setSavedMessage(null)
  }

  function closeDrawer() {
    setDrawerMode(null)
    setSelectedProduct(null)
  }

  function handleSaved() {
    closeDrawer()
    setSavedMessage('Producto guardado correctamente.')
  }

  return (
    <section className="space-y-6 p-6 lg:p-8">
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">Catalogo</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Productos</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Busca, filtra y administra productos, servicios, unidades, codigos y precios.
          </p>
        </div>
        <Button onClick={openCreateDrawer}>
          <Plus size={16} />
          Crear producto
        </Button>
      </div>

      {savedMessage && (
        <div className="rounded-md bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-700 ring-1 ring-emerald-200">
          {savedMessage}
        </div>
      )}

      <Card>
        <CardHeader>
          <div className="grid gap-4 xl:grid-cols-[minmax(260px,1fr)_180px_160px_220px_auto]">
            <div className="flex h-11 items-center gap-2 rounded-md bg-white px-3 shadow-sm ring-1 ring-stone-200">
              <Search aria-hidden="true" className="text-stone-500" size={18} />
              <input
                className="w-full bg-transparent text-sm font-medium text-stone-900 outline-none placeholder:text-stone-400"
                onChange={(event) => updateFilters({ query: event.target.value })}
                placeholder="Buscar por nombre, SKU o barcode"
                value={filters.query}
              />
            </div>
            <select
              className="h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15"
              onChange={(event) => updateFilters({ productType: event.target.value })}
              value={filters.productType}
            >
              {productTypes.map((type) => (
                <option key={type.value} value={type.value}>
                  {type.label}
                </option>
              ))}
            </select>
            <select
              className="h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15"
              onChange={(event) => updateFilters({ isActive: event.target.value })}
              value={filters.isActive}
            >
              <option value="">Todos</option>
              <option value="true">Activos</option>
              <option value="false">Inactivos</option>
            </select>
            <input
              className="h-11 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none placeholder:text-stone-400 focus:ring-2 focus:ring-stone-900/15"
              onChange={(event) => updateFilters({ categoryId: event.target.value })}
              placeholder="Categoria ID"
              value={filters.categoryId}
            />
            <Button onClick={() => products.refetch()} type="button" variant="secondary">
              <SlidersHorizontal size={16} />
              Filtrar
            </Button>
          </div>
        </CardHeader>
      </Card>

      <Card className="overflow-hidden">
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <h3 className="text-base font-semibold text-stone-950">Listado de productos</h3>
            <p className="text-sm font-medium text-stone-600">
              {totalItems} productos encontrados
            </p>
          </div>
          <Button disabled={products.isFetching} onClick={() => products.refetch()} variant="ghost">
            <RefreshCw size={16} />
            Reintentar
          </Button>
        </CardHeader>

        <div className="overflow-x-auto">
          <table className="w-full min-w-[1040px] text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
              <tr>
                <th className="px-5 py-3">Producto</th>
                <th className="px-5 py-3">Tipo</th>
                <th className="px-5 py-3">SKU / Barcode</th>
                <th className="px-5 py-3">Unidad</th>
                <th className="px-5 py-3 text-right">Precio</th>
                <th className="px-5 py-3">Stock</th>
                <th className="px-5 py-3">Estado</th>
                <th className="px-5 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200">
              {products.isLoading && <SkeletonRows />}
              {products.isError && (
                <tr>
                  <td className="px-5 py-10 text-center" colSpan={8}>
                    <div className="mx-auto max-w-sm space-y-3">
                      <p className="text-sm font-semibold text-red-700">No se pudo cargar el catalogo.</p>
                      <Button onClick={() => products.refetch()} variant="secondary">
                        <RefreshCw size={16} />
                        Reintentar
                      </Button>
                    </div>
                  </td>
                </tr>
              )}
              {!products.isLoading && !products.isError && items.length === 0 && (
                <tr>
                  <td className="px-5 py-12 text-center" colSpan={8}>
                    <p className="text-sm font-semibold text-stone-700">No hay productos registrados.</p>
                    <p className="mt-1 text-sm font-medium text-stone-500">
                      Crea el primer producto para comenzar a preparar ventas e inventario.
                    </p>
                  </td>
                </tr>
              )}
              {items.map((product) => (
                <tr className="bg-white hover:bg-stone-50" key={product.id}>
                  <td className="px-5 py-4">
                    <p className="font-semibold text-stone-950">{product.name}</p>
                    <p className="mt-1 text-xs font-medium text-stone-500">{product.description ?? 'Sin descripcion'}</p>
                  </td>
                  <td className="px-5 py-4 font-semibold text-stone-700">{product.productType}</td>
                  <td className="px-5 py-4">
                    <p className="font-mono text-xs font-semibold text-stone-700">{product.sku}</p>
                    <p className="mt-1 text-xs font-medium text-stone-500">{product.barcode ?? 'Sin barcode'}</p>
                  </td>
                  <td className="px-5 py-4 font-semibold text-stone-700">{product.unitOfMeasure}</td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-950">
                    RD$ {product.salePrice.toLocaleString('es-DO', { minimumFractionDigits: 2 })}
                  </td>
                  <td className="px-5 py-4">
                    <span className="rounded-full bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-700 ring-1 ring-stone-200">
                      {product.trackInventory ? 'Controla stock' : 'No aplica'}
                    </span>
                  </td>
                  <td className="px-5 py-4">
                    <StatusBadge isActive={product.isActive} />
                  </td>
                  <td className="px-5 py-4 text-right">
                    <Button onClick={() => openEditDrawer(product)} size="sm" type="button" variant="secondary">
                      <Pencil size={14} />
                      Editar
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="flex flex-col gap-4 border-t border-stone-200 px-5 py-4 lg:flex-row lg:items-center lg:justify-between">
          <p className="text-sm font-medium text-stone-600">
            Pagina {filters.page} de {totalPages || 1} · {totalItems} registros
          </p>
          <div className="flex flex-wrap items-center gap-3">
            <select
              className="h-9 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none"
              onChange={(event) => updateFilters({ pageSize: Number(event.target.value) })}
              value={filters.pageSize}
            >
              {pageSizes.map((pageSize) => (
                <option key={pageSize} value={pageSize}>
                  {pageSize} por pagina
                </option>
              ))}
            </select>
            <Button
              disabled={!canGoPrevious}
              onClick={() => updateFilters({ page: filters.page - 1 })}
              size="sm"
              variant="secondary"
            >
              <ChevronLeft size={15} />
              Anterior
            </Button>
            <Button
              disabled={!canGoNext}
              onClick={() => updateFilters({ page: filters.page + 1 })}
              size="sm"
              variant="secondary"
            >
              Siguiente
              <ChevronRight size={15} />
            </Button>
          </div>
        </div>
      </Card>

      {drawerMode && (
        <ProductDrawer onClose={closeDrawer} title={drawerTitle}>
          <ProductForm
            onSaved={handleSaved}
            product={drawerMode === 'edit' ? selectedProduct : null}
          />
        </ProductDrawer>
      )}
    </section>
  )
}

function SkeletonRows() {
  return Array.from({ length: 6 }).map((_, index) => (
    <tr key={index}>
      <td className="px-5 py-4" colSpan={8}>
        <div className="h-4 w-full rounded bg-stone-100" />
      </td>
    </tr>
  ))
}

function StatusBadge({ isActive }: { isActive: boolean }) {
  return (
    <span className={isActive
      ? 'rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200'
      : 'rounded-full bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-700 ring-1 ring-stone-200'}
    >
      {isActive ? 'Activo' : 'Inactivo'}
    </span>
  )
}

function ProductDrawer({
  children,
  onClose,
  title,
}: {
  children: ReactNode
  onClose: () => void
  title: string
}) {
  return (
    <div className="fixed inset-0 z-50 bg-stone-950/20">
      <div className="absolute inset-y-0 right-0 flex w-full max-w-5xl flex-col bg-white shadow-xl ring-1 ring-stone-200">
        <div className="flex h-16 shrink-0 items-center justify-between border-b border-stone-200 px-6">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Catalogo</p>
            <h3 className="text-lg font-semibold text-stone-950">{title}</h3>
          </div>
          <Button aria-label="Cerrar drawer" onClick={onClose} size="icon" type="button" variant="ghost">
            <X size={18} />
          </Button>
        </div>
        <div className="min-h-0 flex-1 overflow-y-auto p-6">
          {children}
        </div>
      </div>
    </div>
  )
}
