import {
  ChevronLeft,
  ChevronRight,
  Images,
  ImagePlus,
  Pencil,
  Plus,
  RefreshCw,
  Search,
  SlidersHorizontal,
  ToggleLeft,
  ToggleRight,
} from 'lucide-react'
import { useRef, useMemo, useState } from 'react'
import { ProductForm } from '@/modules/products/components/ProductForm'
import {
  useActivateProductMutation,
  useCategoriesQuery,
  useDeactivateProductMutation,
  useProductsRealtimeInvalidation,
  useProductsQuery,
  useSeedProductImagesMutation,
  useUploadProductImageMutation,
} from '@/modules/products/hooks/useProducts'
import type { Product, ProductFilters } from '@/modules/products/types'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'
import { Drawer } from '@/shared/components/ui/drawer'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { CsvExportButton } from '@/shared/components/CsvExportButton'
import { useHasPermission } from '@/shared/hooks/usePermissions'
import { Permission } from '@/shared/types/permissions'

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
    sortBy: 'name',
    sortDirection: 'asc',
  })
  const [drawerMode, setDrawerMode] = useState<'create' | 'edit' | null>(null)
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
  const [pendingDeactivate, setPendingDeactivate] = useState<Product | null>(null)
  const [savedMessage, setSavedMessage] = useState<string | null>(null)
  const [uploadingProductId, setUploadingProductId] = useState<string | null>(null)
  const seedImagesMutation = useSeedProductImagesMutation()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const uploadImageMutation = useUploadProductImageMutation()
  const products = useProductsQuery(filters)
  const categories = useCategoriesQuery()
  const activateProduct = useActivateProductMutation()
  const deactivateProduct = useDeactivateProductMutation()
  const canExportProducts = useHasPermission(Permission.ProductsExport)
  const items = products.data?.items ?? []
  const totalItems = products.data?.totalItems ?? 0
  const totalPages = products.data?.totalPages ?? 0
  const canGoPrevious = products.data?.hasPreviousPage ?? filters.page > 1
  const canGoNext = products.data?.hasNextPage ?? (totalPages > 0 && filters.page < totalPages)

  const drawerTitle = useMemo(
    () => (drawerMode === 'edit' ? 'Editar producto' : 'Crear producto'),
    [drawerMode],
  )
  useProductsRealtimeInvalidation()

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

  function handleToggleStatus(product: Product) {
    if (product.isActive) {
      setPendingDeactivate(product)
      return
    }

    activateProduct.mutate(product.id, {
      onSuccess: () => setSavedMessage('Producto activado correctamente.'),
    })
  }

  function handleUploadImageClick(productId: string) {
    setUploadingProductId(productId)
    fileInputRef.current?.click()
  }

  async function handleFileSelected(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    if (!file || !uploadingProductId) return

    event.target.value = ''
    uploadImageMutation.mutate(
      { productId: uploadingProductId, file },
      {
        onSuccess: () => setSavedMessage('Imagen actualizada correctamente.'),
        onError: (err) => setSavedMessage(`Error: ${err instanceof Error ? err.message : 'No se pudo subir la imagen.'}`),
        onSettled: () => setUploadingProductId(null),
      },
    )
  }

  async function confirmDeactivate() {
    if (!pendingDeactivate) return
    await deactivateProduct.mutateAsync(pendingDeactivate.id)
    setSavedMessage('Producto desactivado correctamente.')
    setPendingDeactivate(null)
  }

  return (
    <section className="space-y-6 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">Catalogo</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Productos</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Busca, filtra y administra productos, servicios, unidades, codigos y precios.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          {canExportProducts && (
            <CsvExportButton
              endpoint="/api/products/export"
              filename={`productos_${new Date().toISOString().slice(0, 10)}.csv`}
            />
          )}
          {import.meta.env.DEV && (
            <Button
              disabled={seedImagesMutation.isPending}
              onClick={() =>
                seedImagesMutation.mutate(undefined, {
                  onSuccess: (result) =>
                    setSavedMessage(
                      result.updated > 0
                        ? `${result.updated} imágenes pobladas correctamente.`
                        : 'Todos los productos ya tienen imagen.',
                    ),
                  onError: () => setSavedMessage('Error al poblar imágenes. ¿MinIO está corriendo?'),
                })
              }
              type="button"
              variant="secondary"
            >
              <Images size={16} />
              {seedImagesMutation.isPending ? 'Poblando…' : 'Poblar imágenes'}
            </Button>
          )}
          <Button onClick={openCreateDrawer}>
            <Plus size={16} />
            Crear producto
          </Button>
        </div>
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
            <Select
              value={filters.productType || '_'}
              onValueChange={(v) => updateFilters({ productType: v === '_' ? '' : v })}
            >
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                {productTypes.map((type) => (
                  <SelectItem key={type.value || '_'} value={type.value || '_'}>
                    {type.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={filters.isActive || '_'}
              onValueChange={(v) => updateFilters({ isActive: v === '_' ? '' : v })}
            >
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="_">Todos</SelectItem>
                <SelectItem value="true">Activos</SelectItem>
                <SelectItem value="false">Inactivos</SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={filters.categoryId || '_'}
              onValueChange={(v) => updateFilters({ categoryId: v === '_' ? '' : v })}
            >
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="_">Todas las categorias</SelectItem>
                {categories.data?.map((category) => (
                  <SelectItem key={category.id} value={category.id}>
                    {category.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
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
          <table className="w-full min-w-260 text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
              <tr>
                <th className="px-5 py-3">Imagen</th>
                <th className="px-5 py-3">Producto</th>
                <th className="px-5 py-3">Tipo</th>
                <th className="px-5 py-3">SKU / Barcode</th>
                <th className="px-5 py-3">Unidad</th>
                <th className="px-5 py-3 text-right">Precio</th>
                <th className="px-5 py-3">Stock</th>
                <th className="px-5 py-3">Estado</th>
                <th className="px-5 py-3 text-right">Acciones</th>
                <th className="px-5 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200">
              {products.isLoading && <SkeletonRows />}
              {products.isError && (
                <tr>
                  <td className="px-5 py-10 text-center" colSpan={10}>
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
                  <td className="px-5 py-12 text-center" colSpan={10}>
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
                    {product.imageUrl ? (
                      <img
                        alt={product.name}
                        className="h-10 w-10 rounded-lg object-cover ring-1 ring-stone-200"
                        src={product.imageUrl}
                      />
                    ) : (
                      <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-stone-100 ring-1 ring-stone-200">
                        <ImagePlus aria-hidden="true" className="text-stone-400" size={14} />
                      </div>
                    )}
                  </td>
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
                    <div className="flex justify-end gap-2">
                      <Button onClick={() => openEditDrawer(product)} size="sm" type="button" variant="secondary">
                        <Pencil size={14} />
                        Editar
                      </Button>
                      <Button
                        disabled={activateProduct.isPending || deactivateProduct.isPending}
                        onClick={() => handleToggleStatus(product)}
                        size="sm"
                        type="button"
                        variant="ghost"
                      >
                        {product.isActive ? <ToggleLeft size={14} /> : <ToggleRight size={14} />}
                        {product.isActive ? 'Desactivar' : 'Activar'}
                      </Button>
                    </div>
                  </td>
                  <td className="px-5 py-4">
                    <Button
                      disabled={uploadImageMutation.isPending && uploadingProductId === product.id}
                      onClick={() => handleUploadImageClick(product.id)}
                      size="sm"
                      title="Subir imagen"
                      type="button"
                      variant="ghost"
                    >
                      <ImagePlus size={14} />
                      Imagen
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="flex flex-col gap-4 border-t border-stone-200 px-5 py-4 lg:flex-row lg:items-center lg:justify-between">
          <p className="text-sm font-medium text-stone-600">
            <span>Pagina {filters.page} de {totalPages || 1}</span>
            <span aria-hidden="true" className="px-1">/</span>
            <span>{totalItems} registros</span>
          </p>
          <div className="flex flex-wrap items-center gap-3">
            <Select
              value={String(filters.pageSize)}
              onValueChange={(v) => updateFilters({ pageSize: Number(v) })}
            >
              <SelectTrigger className="h-9"><SelectValue /></SelectTrigger>
              <SelectContent>
                {pageSizes.map((pageSize) => (
                  <SelectItem key={pageSize} value={String(pageSize)}>
                    {pageSize} por pagina
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
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

      <input
        accept="image/jpeg,image/png,image/webp"
        aria-hidden="true"
        className="hidden"
        onChange={handleFileSelected}
        ref={fileInputRef}
        type="file"
      />

      {drawerMode && (
        <Drawer onClose={closeDrawer} size="xl" subtitle="Catalogo" title={drawerTitle}>
          <ProductForm
            onSaved={handleSaved}
            product={drawerMode === 'edit' ? selectedProduct : null}
          />
        </Drawer>
      )}

      <AlertDialog open={Boolean(pendingDeactivate)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Desactivar producto</AlertDialogTitle>
            <AlertDialogDescription>
              <strong>{pendingDeactivate?.name}</strong> no se eliminara, pero no podra venderse ni usarse en el POS mientras este inactivo.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setPendingDeactivate(null)}>
              Cancelar
            </AlertDialogCancel>
            <AlertDialogAction
              disabled={deactivateProduct.isPending}
              onClick={confirmDeactivate}
              variant="destructive"
            >
              Desactivar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </section>
  )
}

const skeletonRowIds = [
  'product-skeleton-1',
  'product-skeleton-2',
  'product-skeleton-3',
  'product-skeleton-4',
  'product-skeleton-5',
  'product-skeleton-6',
]

function SkeletonRows() {
  return skeletonRowIds.map((id) => (
    <tr key={id}>
      <td className="px-5 py-4" colSpan={10}>
        <div className="h-4 w-full rounded bg-stone-100" />
      </td>
    </tr>
  ))
}

function StatusBadge({ isActive }: Readonly<{ isActive: boolean }>) {
  const className = isActive
    ? 'rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200'
    : 'rounded-full bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-700 ring-1 ring-stone-200'

  return (
    <span className={className}>
      {isActive ? 'Activo' : 'Inactivo'}
    </span>
  )
}
