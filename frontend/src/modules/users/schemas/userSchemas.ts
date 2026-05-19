import { z } from 'zod'

export const createUserSchema = z.object({
  fullName: z.string().min(1, 'Nombre es requerido').min(3, 'Mínimo 3 caracteres'),
  email: z.email({ error: 'Email inválido' }),
  password: z.string().min(8, 'Mínimo 8 caracteres'),
  role: z.string().min(1, 'Rol es requerido'),
  defaultBranchId: z.uuid().optional(),
})

export type CreateUserFormData = z.infer<typeof createUserSchema>

export const updateUserSchema = z.object({
  fullName: z.string().min(1, 'Nombre es requerido').min(3, 'Mínimo 3 caracteres'),
  phone: z.string().optional(),
  defaultBranchId: z.uuid().optional(),
})

export type UpdateUserFormData = z.infer<typeof updateUserSchema>
