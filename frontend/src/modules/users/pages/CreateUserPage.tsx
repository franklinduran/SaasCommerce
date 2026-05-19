import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { RoleSelect } from '../components/RoleSelect'
import { usersApi } from '../services/usersApi'
import { createUserSchema, type CreateUserFormData } from '../schemas/userSchemas'

export function CreateUserPage() {
  const navigate = useNavigate()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<CreateUserFormData>({
    resolver: zodResolver(createUserSchema),
  })

  const role = watch('role')

  const onSubmit = async (data: CreateUserFormData) => {
    setIsLoading(true)
    setError(null)

    try {
      await usersApi.createUser(data)
      navigate('/users')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al crear usuario')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-stone-900">Crear usuario</h1>
        <p className="text-stone-600 mt-1">Registra un nuevo usuario en tu negocio</p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="max-w-md space-y-6">
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-sm text-red-700">{error}</p>
          </div>
        )}

        <div className="space-y-2">
          <Label htmlFor="fullName">Nombre completo *</Label>
          <Input id="fullName" placeholder="Juan Pérez" {...register('fullName')} disabled={isLoading} />
          {errors.fullName && <p className="text-sm text-red-600">{errors.fullName.message}</p>}
        </div>

        <div className="space-y-2">
          <Label htmlFor="email">Email *</Label>
          <Input id="email" type="email" placeholder="juan@example.com" {...register('email')} disabled={isLoading} />
          {errors.email && <p className="text-sm text-red-600">{errors.email.message}</p>}
        </div>

        <div className="space-y-2">
          <Label htmlFor="password">Contraseña inicial *</Label>
          <Input id="password" type="password" placeholder="Mínimo 8 caracteres" {...register('password')} disabled={isLoading} />
          {errors.password && <p className="text-sm text-red-600">{errors.password.message}</p>}
        </div>

        <div className="space-y-2">
          <Label htmlFor="role">Rol *</Label>
          <RoleSelect
            value={role}
            onValueChange={(value) => setValue('role', value)}
            disabled={isLoading}
          />
          {errors.role && <p className="text-sm text-red-600">{errors.role.message}</p>}
        </div>

        <div className="flex gap-2 pt-4">
          <Button type="button" variant="outline" onClick={() => navigate('/users')} disabled={isLoading}>
            Cancelar
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading ? 'Creando...' : 'Crear usuario'}
          </Button>
        </div>
      </form>
    </div>
  )
}
