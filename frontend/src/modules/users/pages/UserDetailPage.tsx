import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { AlertTriangle, ArrowLeft } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog'
import { RoleSelect } from '../components/RoleSelect'
import { ResetPasswordDialog } from '../components/ResetPasswordDialog'
import { usersApi } from '../services/usersApi'
import { updateUserSchema, type UpdateUserFormData } from '../schemas/userSchemas'
import type { ResetPasswordResponse, UserDetail } from '../types'

export function UserDetailPage() {
  const { userId } = useParams<{ userId: string }>()
  const [user, setUser] = useState<UserDetail | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [newRole, setNewRole] = useState<string>('')
  const [showDisableDialog, setShowDisableDialog] = useState(false)
  const [showResetDialog, setShowResetDialog] = useState(false)
  const [resetPassword, setResetPassword] = useState<ResetPasswordResponse | null>(null)
  const [isSaving, setIsSaving] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<UpdateUserFormData>({
    resolver: zodResolver(updateUserSchema),
  })

  const loadUser = useCallback(async () => {
    if (!userId) return
    setIsLoading(true)
    try {
      const data = await usersApi.getUserById(userId)
      setUser(data)
      setNewRole(data.role)
      reset({
        fullName: data.fullName,
        phone: data.phone,
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cargar usuario')
    } finally {
      setIsLoading(false)
    }
  }, [reset, userId])

  useEffect(() => {
    if (!userId) return
    loadUser()
  }, [loadUser, userId])

  const onSubmitProfile = async (data: UpdateUserFormData) => {
    if (!userId) return
    setIsSaving(true)
    try {
      await usersApi.updateUser(userId, data)
      await loadUser()
      setError(null)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al actualizar')
    } finally {
      setIsSaving(false)
    }
  }

  const handleChangeRole = async () => {
    if (!userId) return
    setIsSaving(true)
    try {
      await usersApi.updateUserRole(userId, newRole)
      await loadUser()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cambiar rol')
    } finally {
      setIsSaving(false)
    }
  }

  const handleDisableUser = async () => {
    if (!userId) return
    setIsSaving(true)
    try {
      await usersApi.disableUser(userId)
      await loadUser()
      setShowDisableDialog(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al desactivar')
    } finally {
      setIsSaving(false)
    }
  }

  const handleResetPassword = async () => {
    if (!userId) return
    setIsSaving(true)
    try {
      const response = await usersApi.resetPassword(userId)
      setResetPassword(response)
      setShowResetDialog(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al resetear contraseña')
    } finally {
      setIsSaving(false)
    }
  }

  const handleReactivate = async () => {
    if (!userId) return
    setIsSaving(true)
    try {
      await usersApi.activateUser(userId)
      await loadUser()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al reactivar')
    } finally {
      setIsSaving(false)
    }
  }

  if (isLoading) {
    return (
      <section className="space-y-5 p-6 lg:p-8">
        <div className="h-7 w-48 rounded bg-stone-100" />
        <Card>
          <CardContent className="space-y-3 p-6">
            <div className="h-10 rounded bg-stone-100" />
            <div className="h-10 rounded bg-stone-100" />
            <div className="h-10 w-40 rounded bg-stone-100" />
          </CardContent>
        </Card>
      </section>
    )
  }

  if (!user) {
    return (
      <section className="space-y-5 p-6 lg:p-8">
        <Link
          className="inline-flex items-center gap-2 text-sm font-semibold text-stone-600 hover:text-stone-950"
          to="/users"
        >
          <ArrowLeft size={16} />
          Usuarios
        </Link>
        <Card className="border-red-200 bg-red-50 ring-red-200">
          <p className="p-5 text-sm font-semibold text-red-700">Usuario no encontrado.</p>
        </Card>
      </section>
    )
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
        <h2 className="mt-2 text-2xl font-semibold text-stone-950">{user.fullName}</h2>
        <p className="mt-2 text-sm font-medium text-stone-600">{user.email}</p>
      </div>

      {error && (
        <Card className="border-red-200 bg-red-50 ring-red-200">
          <p className="p-5 text-sm font-semibold text-red-700">{error}</p>
        </Card>
      )}

      <Tabs className="max-w-3xl" defaultValue="profile">
        <TabsList>
          <TabsTrigger value="profile">Perfil</TabsTrigger>
          <TabsTrigger value="roles">Roles</TabsTrigger>
          <TabsTrigger value="danger">Zona de riesgo</TabsTrigger>
        </TabsList>

        <TabsContent className="mt-4" value="profile">
          <Card>
            <CardContent className="p-6">
              <form className="space-y-5" onSubmit={handleSubmit(onSubmitProfile)}>
                <div className="space-y-1.5">
                  <Label htmlFor="fullName">Nombre completo</Label>
                  <Input disabled={isSaving} id="fullName" {...register('fullName')} />
                  {errors.fullName && (
                    <p className="text-sm font-semibold text-red-700">{errors.fullName.message}</p>
                  )}
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="phone">Teléfono</Label>
                  <Input disabled={isSaving} id="phone" {...register('phone')} />
                  {errors.phone && (
                    <p className="text-sm font-semibold text-red-700">{errors.phone.message}</p>
                  )}
                </div>

                <div className="flex justify-end border-t border-stone-200 pt-5">
                  <Button disabled={isSaving} type="submit">
                    {isSaving ? 'Guardando...' : 'Guardar cambios'}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent className="mt-4" value="roles">
          <Card>
            <CardContent className="space-y-5 p-6">
              <div className="space-y-1.5">
                <Label htmlFor="role">Rol actual</Label>
                <RoleSelect disabled={isSaving} onValueChange={setNewRole} value={newRole} />
              </div>

              {newRole !== user.role && (
                <div className="flex justify-end border-t border-stone-200 pt-5">
                  <Button disabled={isSaving} onClick={handleChangeRole} type="button">
                    {isSaving ? 'Actualizando...' : 'Cambiar rol'}
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent className="mt-4" value="danger">
          <Card>
            <CardHeader className="flex flex-row items-start gap-3">
              <span className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-red-50 text-red-700 ring-1 ring-red-200">
                <AlertTriangle size={18} />
              </span>
              <div>
                <h3 className="text-base font-semibold text-stone-950">Zona de riesgo</h3>
                <p className="mt-1 text-sm font-medium text-stone-600">
                  Estas acciones son irreversibles o afectan el acceso del usuario.
                </p>
              </div>
            </CardHeader>
            <CardContent className="flex flex-col items-stretch gap-3 p-5 pt-0 sm:items-start">
              <Button
                disabled={isSaving}
                onClick={handleResetPassword}
                type="button"
                variant="secondary"
              >
                {isSaving ? 'Generando...' : 'Resetear contraseña'}
              </Button>

              {!user.isActive && (
                <Button
                  disabled={isSaving}
                  onClick={handleReactivate}
                  type="button"
                  variant="secondary"
                >
                  Reactivar usuario
                </Button>
              )}

              {user.isActive && (
                <Button
                  className="bg-red-600 text-white hover:bg-red-700"
                  disabled={isSaving}
                  onClick={() => setShowDisableDialog(true)}
                  type="button"
                >
                  Desactivar usuario
                </Button>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <AlertDialog onOpenChange={setShowDisableDialog} open={showDisableDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Desactivar usuario?</AlertDialogTitle>
            <AlertDialogDescription>
              Este usuario no podrá iniciar sesión. Puedes reactivarlo después.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="flex justify-end gap-2">
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              className="bg-red-600 hover:bg-red-700"
              disabled={isSaving}
              onClick={handleDisableUser}
            >
              Desactivar
            </AlertDialogAction>
          </div>
        </AlertDialogContent>
      </AlertDialog>

      {resetPassword && (
        <ResetPasswordDialog
          isLoading={false}
          onConfirm={() => {
            setShowResetDialog(false)
            setResetPassword(null)
            loadUser()
          }}
          onOpenChange={setShowResetDialog}
          open={showResetDialog}
          temporaryPassword={resetPassword.temporaryPassword}
        />
      )}
    </section>
  )
}
