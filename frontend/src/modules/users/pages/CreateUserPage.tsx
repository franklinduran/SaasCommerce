import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'
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
    <section className="space-y-5 p-6 lg:p-8">
      <div>
        <Link
          className="inline-flex items-center gap-2 text-sm font-semibold text-stone-600 hover:text-stone-950"
          to="/users"
        >
          <ArrowLeft size={16} />
          Usuarios
        </Link>
        <h2 className="mt-2 text-2xl font-semibold text-stone-950">Crear usuario</h2>
        <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
          Registra un nuevo usuario en el negocio. Recibirá una contraseña inicial que deberá cambiar al primer acceso.
        </p>
      </div>

      <Card className="max-w-2xl">
        <CardContent className="p-6">
          <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
            {error && (
              <div className="rounded-md border border-red-200 bg-red-50 p-3">
                <p className="text-sm font-semibold text-red-700">{error}</p>
              </div>
            )}

            <div className="space-y-1.5">
              <Label htmlFor="fullName">Nombre completo *</Label>
              <Input
                disabled={isLoading}
                id="fullName"
                placeholder="Juan Pérez"
                {...register('fullName')}
              />
              {errors.fullName && (
                <p className="text-sm font-semibold text-red-700">{errors.fullName.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="email">Email *</Label>
              <Input
                disabled={isLoading}
                id="email"
                placeholder="juan@example.com"
                type="email"
                {...register('email')}
              />
              {errors.email && (
                <p className="text-sm font-semibold text-red-700">{errors.email.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="password">Contraseña inicial *</Label>
              <Input
                disabled={isLoading}
                id="password"
                placeholder="Mínimo 8 caracteres"
                type="password"
                {...register('password')}
              />
              {errors.password && (
                <p className="text-sm font-semibold text-red-700">{errors.password.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="role">Rol *</Label>
              <RoleSelect
                disabled={isLoading}
                onValueChange={(value) => setValue('role', value)}
                value={role}
              />
              {errors.role && (
                <p className="text-sm font-semibold text-red-700">{errors.role.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-3 border-t border-stone-200 pt-5 sm:flex-row sm:items-center sm:justify-end">
              <Button
                disabled={isLoading}
                onClick={() => navigate('/users')}
                type="button"
                variant="secondary"
              >
                Cancelar
              </Button>
              <Button disabled={isLoading} type="submit">
                {isLoading ? 'Creando...' : 'Crear usuario'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </section>
  )
}
