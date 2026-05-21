import { zodResolver } from '@hookform/resolvers/zod'
import { SlidersHorizontal } from 'lucide-react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'
import { useCreateInventoryAdjustmentMutation } from '@/modules/inventory/hooks/useInventory'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const adjustmentSchema = z.object({
  productId: z.uuid('Producto invalido'),
  quantity: z.coerce.number().refine((value) => value !== 0, 'La cantidad no puede ser cero'),
  reason: z.string().min(1, 'Selecciona una razon'),
})

type AdjustmentFormValues = z.infer<typeof adjustmentSchema>
type AdjustmentFormInput = z.input<typeof adjustmentSchema>

export function InventoryAdjustmentForm() {
  const adjustment = useCreateInventoryAdjustmentMutation()
  const {
    control,
    formState: { errors },
    handleSubmit,
    register,
    reset,
  } = useForm<AdjustmentFormInput, undefined, AdjustmentFormValues>({
    resolver: zodResolver(adjustmentSchema),
    defaultValues: {
      productId: '',
      quantity: 1,
      reason: 'Adjustment',
    },
  })

  const errorMessage =
    adjustment.error instanceof HttpClientError
      ? adjustment.error.message
      : null

  async function onSubmit(values: AdjustmentFormValues) {
    await adjustment.mutateAsync(values)
    reset()
  }

  return (
    <form className="grid gap-5" onSubmit={handleSubmit(onSubmit)}>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Producto ID</span>
        <input
          className="h-11 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
          placeholder="guid del producto"
          {...register('productId')}
        />
        {errors.productId && <span className="mt-2 block text-sm font-medium text-red-700">{errors.productId.message}</span>}
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-semibold text-stone-900">Cantidad</span>
        <input
          className="h-11 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
          step="0.001"
          type="number"
          {...register('quantity')}
        />
        {errors.quantity && <span className="mt-2 block text-sm font-medium text-red-700">{errors.quantity.message}</span>}
      </label>
      <div>
        <span className="mb-2 block text-sm font-semibold text-stone-900">Razon</span>
        <Controller
          control={control}
          name="reason"
          render={({ field }) => (
            <Select value={field.value} onValueChange={field.onChange}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="InitialLoad">Carga inicial</SelectItem>
                <SelectItem value="Adjustment">Ajuste</SelectItem>
                <SelectItem value="ManualCorrection">Correccion manual</SelectItem>
                <SelectItem value="Return">Devolucion</SelectItem>
                <SelectItem value="Purchase">Compra</SelectItem>
              </SelectContent>
            </Select>
          )}
        />
        {errors.reason && <span className="mt-2 block text-sm font-medium text-red-700">{errors.reason.message}</span>}
      </div>
      <div className="flex items-end">
        <Button className="w-full sm:w-auto" disabled={adjustment.isPending} type="submit">
          <SlidersHorizontal size={16} />
          {adjustment.isPending ? 'Aplicando' : 'Ajustar'}
        </Button>
      </div>
      {errorMessage && (
        <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700 ring-1 ring-red-200">
          {errorMessage}
        </p>
      )}
    </form>
  )
}
