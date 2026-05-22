import { z } from 'zod'

export const createInventoryTransferItemSchema = z.object({
  productId: z.string().min(1, 'Selecciona un producto'),
  productName: z.string().optional(),
  quantity: z
    .number({ error: 'Ingresa una cantidad válida' })
    .positive('La cantidad debe ser mayor a 0'),
})

export const createInventoryTransferSchema = z
  .object({
    sourceBranchId: z.string().min(1, 'Selecciona la sucursal de origen'),
    targetBranchId: z.string().min(1, 'Selecciona la sucursal de destino'),
    note: z.string().max(500, 'Máximo 500 caracteres').optional().or(z.literal('')),
    items: z
      .array(createInventoryTransferItemSchema)
      .min(1, 'Agrega al menos un producto'),
  })
  .refine((data) => data.sourceBranchId !== data.targetBranchId, {
    message: 'La sucursal de origen y destino no pueden ser iguales',
    path: ['targetBranchId'],
  })

export type CreateInventoryTransferFormValues = z.infer<typeof createInventoryTransferSchema>
