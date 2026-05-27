import { z } from 'zod'

export const purchaseItemSchema = z.object({
  productId: z.string().min(1, 'Selecciona un producto.'),
  quantity: z.number().positive('La cantidad debe ser mayor que cero.'),
  unitCost: z.number().min(0, 'El costo no puede ser negativo.'),
})

export const purchaseSchema = z.object({
  supplierId: z.string().min(1, 'Selecciona un proveedor.'),
  supplierInvoiceNumber: z.string().trim().nullable(),
  purchaseDate: z.string().nullable(),
  notes: z.string().trim().nullable(),
  receiveNow: z.boolean(),
  items: z.array(purchaseItemSchema).min(1, 'Agrega al menos un producto con cantidad y costo validos.'),
})

export type PurchaseFormValues = z.infer<typeof purchaseSchema>
