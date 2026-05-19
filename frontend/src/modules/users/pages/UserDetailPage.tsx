import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs'
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogHeader, AlertDialogTitle } from '@/shared/components/ui/alert-dialog'
import { RoleSelect } from '../components/RoleSelect'
import { ResetPasswordDialog } from '../components/ResetPasswordDialog'
import { usersApi } from '../services/usersApi'
import { updateUserSchema, type UpdateUserFormData } from '../schemas/userSchemas'
import type { UserDetail, ResetPasswordResponse } from '../types'

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

  useEffect(() => {
    if (!userId) return
    loadUser()
  }, [userId])

  const loadUser = async () => {
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
  }

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

  if (isLoading) {
    return <div className="h-96 bg-stone-100 rounded-lg animate-pulse" />
  }

  if (!user) {
    return <div className="text-red-600">Usuario no encontrado</div>
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-stone-900">{user.fullName}</h1>
        <p className="text-stone-600 mt-1">{user.email}</p>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      <Tabs defaultValue="profile" className="max-w-2xl">
        <TabsList>
          <TabsTrigger value="profile">Perfil</TabsTrigger>
          <TabsTrigger value="roles">Roles</TabsTrigger>
          <TabsTrigger value="danger">Zona de riesgo</TabsTrigger>
        </TabsList>

        <TabsContent value="profile" className="space-y-6">
          <form onSubmit={handleSubmit(onSubmitProfile)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="fullName">Nombre completo</Label>
              <Input id="fullName" {...register('fullName')} disabled={isSaving} />
              {errors.fullName && <p className="text-sm text-red-600">{errors.fullName.message}</p>}
            </div>

            <div className="space-y-2">
              <Label htmlFor="phone">Teléfono</Label>
              <Input id="phone" {...register('phone')} disabled={isSaving} />
              {errors.phone && <p className="text-sm text-red-600">{errors.phone.message}</p>}
            </div>

            <Button type="submit" disabled={isSaving}>
              {isSaving ? 'Guardando...' : 'Guardar cambios'}
            </Button>
          </form>
        </TabsContent>

        <TabsContent value="roles" className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="role">Rol actual</Label>
            <RoleSelect value={newRole} onValueChange={setNewRole} disabled={isSaving} />
          </div>

          {newRole !== user.role && (
            <Button onClick={handleChangeRole} disabled={isSaving}>
              {isSaving ? 'Actualizando...' : 'Cambiar rol'}
            </Button>
          )}
        </TabsContent>

        <TabsContent value="danger" className="space-y-4">
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <h3 className="font-semibold text-red-900 mb-2">Zona de riesgo</h3>
            <p className="text-sm text-red-800 mb-4">Estas acciones no se pueden deshacer.</p>

            <div className="space-y-2">
              <Button
                variant="outline"
                className="w-full border-amber-200 text-amber-700 hover:bg-amber-50"
                onClick={handleResetPassword}
                disabled={isSaving}
              >
                {isSaving ? 'Generando...' : 'Resetear contraseña'}
              </Button>

              {!user.isActive && (
                <Button
                  variant="outline"
                  className="w-full border-emerald-200 text-emerald-700 hover:bg-emerald-50"
                  onClick={async () => {
                    if (!userId) return
                    setIsSaving(true)
                    try {
                      await usersApi.activateUser(userId)
                      await loadUser()
                    } catch (err) {
                      setError(err instanceof Error ? err.message : 'Error')
                    } finally {
                      setIsSaving(false)
                    }
                  }}
                  disabled={isSaving}
                >
                  Reactivar usuario
                </Button>
              )}

              {user.isActive && (
                <Button
                  variant="outline"
                  className="w-full border-red-200 text-red-700 hover:bg-red-50"
                  onClick={() => setShowDisableDialog(true)}
                  disabled={isSaving}
                >
                  Desactivar usuario
                </Button>
              )}
            </div>
          </div>
        </TabsContent>
      </Tabs>

      <AlertDialog open={showDisableDialog} onOpenChange={setShowDisableDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Desactivar usuario?</AlertDialogTitle>
            <AlertDialogDescription>
              Este usuario no podrá iniciar sesión. Puedes reactivarlo después.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="flex justify-end gap-2">
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleDisableUser} disabled={isSaving} className="bg-red-600 hover:bg-red-700">
              Desactivar
            </AlertDialogAction>
          </div>
        </AlertDialogContent>
      </AlertDialog>

      {resetPassword && (
        <ResetPasswordDialog
          open={showResetDialog}
          onOpenChange={setShowResetDialog}
          temporaryPassword={resetPassword.temporaryPassword}
          isLoading={false}
          onConfirm={() => {
            setShowResetDialog(false)
            setResetPassword(null)
            loadUser()
          }}
        />
      )}
    </div>
  )
}
