import { z } from 'zod'

export const createBranchSchema = z.object({
  name: z.string().min(1, 'El nombre es obligatorio').max(100, 'Máximo 100 caracteres'),
  code: z
    .string()
    .min(1, 'El código es obligatorio')
    .max(20, 'Máximo 20 caracteres')
    .regex(/^[A-Z0-9]+$/, 'Solo letras mayúsculas y números'),
  address: z.string().max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
  phone: z.string().max(30, 'Máximo 30 caracteres').optional().or(z.literal('')),
  isMain: z.boolean(),
})

export const updateBranchSchema = z.object({
  name: z.string().min(1, 'El nombre es obligatorio').max(100, 'Máximo 100 caracteres'),
  address: z.string().max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
  phone: z.string().max(30, 'Máximo 30 caracteres').optional().or(z.literal('')),
})

export type CreateBranchFormValues = z.infer<typeof createBranchSchema>
export type UpdateBranchFormValues = z.infer<typeof updateBranchSchema>
