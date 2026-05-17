import { zodResolver } from '@hookform/resolvers/zod'
import { SlidersHorizontal, X } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { useCreateInventoryAdjustmentMutation } from '@/modules/inventory/hooks/useInventory'
import {
  adjustInventorySchema,
  type AdjustInventoryFormInput,
  type AdjustInventoryFormValues,
} from '@/modules/inventory/schemas/adjustInventorySchema'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'

const controlClassName =
  'h-11 w-full rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15'

export function AdjustInventoryDialog({
  onClose,
  productId,
}: Readonly<{
  onClose: () => void
  productId?: string
}>) {
  const adjustment = useCreateInventoryAdjustmentMutation()
  const {
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
    <div className="fixed inset-0 z-50 bg-stone-950/25">
      <div className="absolute inset-y-0 right-0 flex w-full max-w-xl flex-col bg-white shadow-xl ring-1 ring-stone-200">
        <div className="flex h-16 items-center justify-between border-b border-stone-200 px-6">
          <h3 className="text-lg font-semibold text-stone-950">Ajustar inventario</h3>
          <Button aria-label="Cerrar ajuste" onClick={onClose} size="icon" type="button" variant="ghost">
            <X size={18} />
          </Button>
        </div>
        <form className="grid gap-5 p-6" onSubmit={handleSubmit(onSubmit)}>
          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-stone-900">Producto ID</span>
            <input className={controlClassName} placeholder="guid del producto" {...register('productId')} />
            {errors.productId && <span className="mt-2 block text-sm font-medium text-red-700">{errors.productId.message}</span>}
          </label>
          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-stone-900">Cantidad</span>
            <input className={controlClassName} step="0.001" type="number" {...register('quantity')} />
            {errors.quantity && <span className="mt-2 block text-sm font-medium text-red-700">{errors.quantity.message}</span>}
          </label>
          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-stone-900">Tipo</span>
            <select className={controlClassName} {...register('reason')}>
              <option value="InitialStock">Entrada inicial</option>
              <option value="ManualAdjustment">Ajuste manual</option>
              <option value="PurchaseEntry">Entrada por compra</option>
              <option value="Return">Devolucion</option>
            </select>
          </label>
          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-stone-900">Motivo</span>
            <input className={controlClassName} placeholder="Conteo fisico, merma, correccion..." {...register('note')} />
            {errors.note && <span className="mt-2 block text-sm font-medium text-red-700">{errors.note.message}</span>}
          </label>
          <Button disabled={adjustment.isPending} type="submit">
            <SlidersHorizontal size={16} />
            {adjustment.isPending ? 'Aplicando' : 'Ajustar'}
          </Button>
          {errorMessage && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700 ring-1 ring-red-200">
              {errorMessage}
            </p>
          )}
        </form>
      </div>
    </div>
  )
}
