import { z } from 'zod'

export const customerSchema = z.object({
  firstName: z.string().trim().min(1, 'El nombre es obligatorio.'),
  lastName: z.string().trim().min(1, 'El apellido es obligatorio.'),
  phone: z.string().trim().nullable().optional(),
  email: z.string().trim().nullable().optional(),
  isActive: z.boolean().optional(),
})

export const registerCustomerPaymentSchema = z.object({
  amount: z.number().positive('El monto debe ser mayor que cero.'),
  note: z.string().trim().optional(),
})
