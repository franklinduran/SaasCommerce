import { Plus, Save } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { useProductsQuery } from '@/modules/products/hooks/useProducts'
import { PurchaseItemsTable } from '@/modules/purchases/components/PurchaseItemsTable'
import type { DraftPurchaseItem } from '@/modules/purchases/components/PurchaseItemsTable'
import { PurchaseTotalsSummary } from '@/modules/purchases/components/PurchaseTotalsSummary'
import { useCreatePurchase } from '@/modules/purchases/hooks/usePurchases'
import { purchaseSchema, type PurchaseFormValues } from '@/modules/purchases/schemas/purchaseSchema'
import type { CreatePurchaseRequest } from '@/modules/purchases/types'
import { useSuppliers } from '@/modules/suppliers/hooks/useSuppliers'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { HttpClientError } from '@/shared/services/httpClient'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const inputClass =
  'h-11 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'

export function PurchaseForm() {
  const navigate = useNavigate()
  const createPurchase = useCreatePurchase()
  const suppliers = useSuppliers({
    isActive: 'true',
    page: 1,
    pageSize: 50,
    query: '',
    sortBy: 'name',
    sortDirection: 'asc',
  })
  const products = useProductsQuery({
    categoryId: '',
    isActive: 'true',
    page: 1,
    pageSize: 50,
    productType: '',
    query: '',
    sortBy: 'name',
    sortDirection: 'asc',
  })
  const inventoryProducts = useMemo(
    () => (products.data?.items ?? []).filter((product) => product.trackInventory),
    [products.data?.items],
  )
  const form = useForm<PurchaseFormValues>({
    defaultValues: {
      items: [],
      notes: null,
      purchaseDate: new Date().toISOString().slice(0, 10),
      receiveNow: true,
      supplierId: '',
      supplierInvoiceNumber: null,
    },
  })
  const supplierId = useWatch({ control: form.control, name: 'supplierId' })
  const [items, setItems] = useState<DraftPurchaseItem[]>([])
  const [submitError, setSubmitError] = useState<string | null>(null)
  const total = items.reduce((sum, item) => sum + item.quantity * item.unitCost, 0)

  function addItem() {
    setItems((current) => [...current, { productId: '', quantity: 1, unitCost: 0 }])
  }

  function updateItem(index: number, item: DraftPurchaseItem) {
    setItems((current) => current.map((candidate, itemIndex) => (itemIndex === index ? item : candidate)))
  }

  function removeItem(index: number) {
    setItems((current) => current.filter((_, itemIndex) => itemIndex !== index))
  }

  async function submit() {
    setSubmitError(null)

    const validItems = items.filter((item) => item.productId && item.quantity > 0 && item.unitCost >= 0)
    const values = form.getValues()
    const parsed = purchaseSchema.safeParse({ ...values, items: validItems })

    if (!parsed.success) {
      setSubmitError(parsed.error.issues[0]?.message ?? 'La compra no es valida.')
      return
    }

    const purchaseDate = parsed.data.purchaseDate
    const request: CreatePurchaseRequest = {
      branchId: null,
      items: parsed.data.items,
      notes: toNullable(parsed.data.notes),
      purchaseDate: purchaseDate ? new Date(`${purchaseDate}T12:00:00`).toISOString() : null,
      receiveNow: parsed.data.receiveNow,
      supplierId: parsed.data.supplierId,
      supplierInvoiceNumber: toNullable(parsed.data.supplierInvoiceNumber),
    }

    try {
      const purchase = await createPurchase.mutateAsync(request)

      if (purchase?.purchaseId) {
        navigate(`/purchases/${purchase.purchaseId}`)
      } else {
        navigate('/purchases')
      }
    } catch (caught) {
      const message =
        caught instanceof HttpClientError
          ? caught.error?.message ?? 'No se pudo crear la compra.'
          : 'No se pudo crear la compra.'

      setSubmitError(message)
    }
  }

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="text-sm font-semibold uppercase text-stone-500">Compras</p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Nueva compra</h2>
        </div>
        <Button disabled={createPurchase.isPending} onClick={submit} type="button">
          <Save size={16} />
          Guardar compra
        </Button>
      </div>

      {submitError && (
        <div className="rounded-md bg-red-50 px-4 py-3 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          {submitError}
        </div>
      )}

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_320px]">
        <Card>
          <CardHeader>
            <h3 className="text-base font-semibold text-stone-950">Datos de compra</h3>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            <div className="space-y-1.5">
              <span className="text-sm font-semibold text-stone-700">Proveedor</span>
              <Select
                value={supplierId || '_'}
                onValueChange={(v) => form.setValue('supplierId', v === '_' ? '' : v)}
              >
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Seleccionar proveedor</SelectItem>
                  {suppliers.data?.items.map((supplier) => (
                    <SelectItem key={supplier.id} value={supplier.id}>{supplier.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <label className="space-y-1.5">
              <span className="text-sm font-semibold text-stone-700">Factura proveedor</span>
              <input className={inputClass} {...form.register('supplierInvoiceNumber')} />
            </label>
            <label className="space-y-1.5">
              <span className="text-sm font-semibold text-stone-700">Fecha</span>
              <input className={inputClass} type="date" {...form.register('purchaseDate')} />
            </label>
            <label className="flex h-11 items-center gap-3 self-end rounded-md bg-white px-3 text-sm font-semibold text-stone-800 shadow-sm ring-1 ring-stone-200">
              <input className="h-4 w-4 accent-stone-900" type="checkbox" {...form.register('receiveNow')} />
              <span>Recibir inventario</span>
            </label>
            <label className="space-y-1.5 md:col-span-2">
              <span className="text-sm font-semibold text-stone-700">Notas</span>
              <input className={inputClass} {...form.register('notes')} />
            </label>
          </CardContent>
        </Card>

        <PurchaseTotalsSummary itemCount={items.length} total={total} />
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <h3 className="text-base font-semibold text-stone-950">Productos comprados</h3>
          <Button onClick={addItem} type="button" variant="secondary">
            <Plus size={16} />
            Agregar producto
          </Button>
        </CardHeader>
        <CardContent>
          {items.length === 0 ? (
            <div className="rounded-md bg-stone-50 px-4 py-8 text-center text-sm font-semibold text-stone-600 ring-1 ring-stone-200">
              No hay productos agregados.
            </div>
          ) : (
            <PurchaseItemsTable
              items={items}
              onChange={updateItem}
              onRemove={removeItem}
              products={inventoryProducts}
            />
          )}
        </CardContent>
      </Card>
    </section>
  )
}

function toNullable(value: string | null) {
  const normalized = value?.trim() ?? ''

  return normalized.length > 0 ? normalized : null
}
