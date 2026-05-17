import { z } from 'zod'

export const adjustInventorySchema = z.object({
  productId: z.uuid('Producto invalido'),
  quantity: z.coerce.number().refine((value) => value !== 0, 'La cantidad no puede ser cero'),
  reason: z.string().min(1, 'Selecciona una razon'),
  note: z.string().max(240, 'El motivo no puede exceder 240 caracteres').optional(),
})

export type AdjustInventoryFormValues = z.infer<typeof adjustInventorySchema>
export type AdjustInventoryFormInput = z.input<typeof adjustInventorySchema>
