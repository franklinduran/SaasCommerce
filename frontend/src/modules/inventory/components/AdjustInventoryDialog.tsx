import { zodResolver } from '@hookform/resolvers/zod'
import { Package, SlidersHorizontal } from 'lucide-react'
import { Controller, useForm } from 'react-hook-form'
import { useCreateInventoryAdjustmentMutation } from '@/modules/inventory/hooks/useInventory'
import {
  adjustInventorySchema,
  type AdjustInventoryFormInput,
  type AdjustInventoryFormValues,
} from '@/modules/inventory/schemas/adjustInventorySchema'
import { Button } from '@/shared/components/ui/button'
import { Drawer } from '@/shared/components/ui/drawer'
import { HttpClientError } from '@/shared/services/httpClient'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const controlClassName =
  'h-11 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15'

export function AdjustInventoryDialog({
  onClose,
  productId,
  productName,
}: Readonly<{
  onClose: () => void
  productId?: string
  productName?: string
}>) {
  const adjustment = useCreateInventoryAdjustmentMutation()
  const {
    control,
    formState: { errors },
    handleSubmit,
    register,
  } = useForm<AdjustInventoryFormInput, undefined, AdjustInventoryFormValues>({
    resolver: zodResolver(adjustInventorySchema),
    defaultValues: {
      note: '',
      productId: productId ?? '',
      quantity: 1,
      reason: 'ManualAdjustment',
    },
  })
  const errorMessage = adjustment.error instanceof HttpClientError ? adjustment.error.message : null

  async function onSubmit(values: AdjustInventoryFormValues) {
    await adjustment.mutateAsync(values)
    onClose()
  }

  return (
    <Drawer onClose={onClose} size="md" subtitle="Inventario" title="Ajustar inventario">
      <form className="grid gap-5" onSubmit={handleSubmit(onSubmit)}>
        {productName ? (
          <div>
            <span className="mb-1.5 block text-sm font-semibold text-stone-900">Producto</span>
            <div className="flex h-11 items-center gap-2 rounded-md bg-stone-50 px-3 ring-1 ring-stone-200">
              <Package className="shrink-0 text-stone-500" size={16} />
              <span className="truncate text-sm font-semibold text-stone-900">{productName}</span>
            </div>
          </div>
        ) : (
          <label className="block">
            <span className="mb-1.5 block text-sm font-semibold text-stone-900">Producto ID</span>
            <input className={controlClassName} placeholder="guid del producto" {...register('productId')} />
            {errors.productId && (
              <span className="mt-2 block text-sm font-medium text-red-700">{errors.productId.message}</span>
            )}
          </label>
        )}

        <label className="block">
          <span className="mb-1.5 block text-sm font-semibold text-stone-900">Cantidad</span>
          <input className={controlClassName} step="0.001" type="number" {...register('quantity')} />
          {errors.quantity && (
            <span className="mt-2 block text-sm font-medium text-red-700">{errors.quantity.message}</span>
          )}
        </label>

        <div>
          <span className="mb-1.5 block text-sm font-semibold text-stone-900">Tipo de ajuste</span>
          <Controller
            control={control}
            name="reason"
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="InitialStock">Entrada inicial</SelectItem>
                  <SelectItem value="ManualAdjustment">Ajuste manual</SelectItem>
                  <SelectItem value="PurchaseEntry">Entrada por compra</SelectItem>
                  <SelectItem value="Return">Devolucion</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
        </div>

        <label className="block">
          <span className="mb-1.5 block text-sm font-semibold text-stone-900">Motivo</span>
          <input
            className={controlClassName}
            placeholder="Conteo fisico, merma, correccion..."
            {...register('note')}
          />
          {errors.note && (
            <span className="mt-2 block text-sm font-medium text-red-700">{errors.note.message}</span>
          )}
        </label>

        <Button disabled={adjustment.isPending} type="submit">
          <SlidersHorizontal size={16} />
          {adjustment.isPending ? 'Aplicando...' : 'Aplicar ajuste'}
        </Button>

        {errorMessage && (
          <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700 ring-1 ring-red-200">
            {errorMessage}
          </p>
        )}
      </form>
    </Drawer>
  )
}
