import { zodResolver } from '@hookform/resolvers/zod'
import { Eye, EyeOff, UserPlus } from 'lucide-react'
import { useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Button } from '@/shared/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { RoleSelect } from './RoleSelect'
import { usersApi } from '../services/usersApi'
import { createUserSchema, type CreateUserFormData } from '../schemas/userSchemas'

type CreateUserDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: (newUserId?: string) => void
}

export function CreateUserDialog({ open, onOpenChange, onSuccess }: Readonly<CreateUserDialogProps>) {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showPassword, setShowPassword] = useState(false)

  const {
    register,
    handleSubmit,
    control,
    setValue,
    formState: { errors },
    reset,
  } = useForm<CreateUserFormData>({
    resolver: zodResolver(createUserSchema),
    defaultValues: {
      email: '',
      fullName: '',
      password: '',
      role: 'Cashier',
    },
  })

  function resetDialogState() {
    reset({ email: '', fullName: '', password: '', role: 'Cashier' })
    setError(null)
    setShowPassword(false)
  }

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) {
      resetDialogState()
    }
    onOpenChange(nextOpen)
  }

  const role = useWatch({ control, name: 'role' })

  async function onSubmit(data: CreateUserFormData) {
    setIsLoading(true)
    setError(null)
    try {
      const response = await usersApi.createUser(data)
      resetDialogState()
      onSuccess(response.userId)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al crear usuario')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog onOpenChange={handleOpenChange} open={open}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <div className="flex items-start gap-3">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
              <UserPlus size={18} />
            </span>
            <div>
              <DialogTitle>Crear usuario</DialogTitle>
              <DialogDescription>
                Crea una nueva cuenta y asigna un rol con los permisos correspondientes.
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <form className="space-y-4" id="create-user-form" onSubmit={handleSubmit(onSubmit)}>
          {error && (
            <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
              {error}
            </div>
          )}

          <div className="space-y-1.5">
            <Label htmlFor="fullName">Nombre completo</Label>
            <Input
              disabled={isLoading}
              id="fullName"
              placeholder="Juan Perez"
              {...register('fullName')}
            />
            {errors.fullName && (
              <p className="text-sm font-semibold text-red-700">{errors.fullName.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="email">Correo electronico</Label>
            <Input
              disabled={isLoading}
              id="email"
              placeholder="juan@negocio.com"
              type="email"
              {...register('email')}
            />
            {errors.email && (
              <p className="text-sm font-semibold text-red-700">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="password">Contrasena inicial</Label>
            <div className="relative">
              <Input
                className="pr-11"
                disabled={isLoading}
                id="password"
                placeholder="Minimo 8 caracteres"
                type={showPassword ? 'text' : 'password'}
                {...register('password')}
              />
              <Button
                aria-label={showPassword ? 'Ocultar contrasena' : 'Mostrar contrasena'}
                className="absolute right-2 top-1/2 h-8 w-8 -translate-y-1/2"
                onClick={() => setShowPassword((v) => !v)}
                size="icon"
                type="button"
                variant="ghost"
              >
                {showPassword ? <EyeOff size={15} /> : <Eye size={15} />}
              </Button>
            </div>
            {errors.password ? (
              <p className="text-sm font-semibold text-red-700">{errors.password.message}</p>
            ) : (
              <p className="text-xs font-medium text-stone-500">
                El usuario podra cambiarla al iniciar sesion.
              </p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="role">Rol</Label>
            <RoleSelect
              disabled={isLoading}
              onValueChange={(value) => setValue('role', value, { shouldValidate: true })}
              value={role}
            />
            {errors.role && (
              <p className="text-sm font-semibold text-red-700">{errors.role.message}</p>
            )}
          </div>
        </form>

        <DialogFooter>
          <Button
            disabled={isLoading}
            onClick={() => handleOpenChange(false)}
            type="button"
            variant="secondary"
          >
            Cancelar
          </Button>
          <Button disabled={isLoading} form="create-user-form" type="submit">
            <UserPlus size={15} />
            {isLoading ? 'Creando...' : 'Crear usuario'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
