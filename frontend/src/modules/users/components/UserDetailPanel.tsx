import { zodResolver } from '@hookform/resolvers/zod'
import {
  AlertTriangle,
  CheckCircle2,
  KeyRound,
  Mail,
  Phone,
  ShieldCheck,
  ShieldOff,
  Trash2,
  X,
} from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { cn } from '@/shared/utils/cn'
import { RoleSelect } from './RoleSelect'
import { ResetPasswordDialog } from './ResetPasswordDialog'
import { usersApi } from '../services/usersApi'
import { updateUserSchema, type UpdateUserFormData } from '../schemas/userSchemas'
import type { ResetPasswordResponse, UserDetail } from '../types'

type UserDetailPanelProps = {
  userId: string
  onClose: () => void
  onUpdated: () => void
}

const roleLabels: Record<string, string> = {
  Admin: 'Administrador',
  Supervisor: 'Supervisor',
  Cashier: 'Cajero',
  InventoryManager: 'Gerente de Inventario',
  PurchasingManager: 'Gerente de Compras',
  ReadOnly: 'Solo lectura',
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('') || '?'
}

export function UserDetailPanel({ userId, onClose, onUpdated }: Readonly<UserDetailPanelProps>) {
  const [user, setUser] = useState<UserDetail | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const [pendingRole, setPendingRole] = useState<string>('')
  const [showDisableDialog, setShowDisableDialog] = useState(false)
  const [showResetDialog, setShowResetDialog] = useState(false)
  const [resetPasswordResponse, setResetPasswordResponse] = useState<ResetPasswordResponse | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    reset,
  } = useForm<UpdateUserFormData>({
    resolver: zodResolver(updateUserSchema),
  })

  const applyUser = useCallback((data: UserDetail) => {
    setUser(data)
    setPendingRole(data.role)
    reset({ fullName: data.fullName, phone: data.phone ?? '' })
    setError(null)
  }, [reset])

  const loadUser = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await usersApi.getUserById(userId)
      applyUser(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cargar usuario')
    } finally {
      setIsLoading(false)
    }
  }, [applyUser, userId])

  useEffect(() => {
    let cancelled = false

    async function fetchUser() {
      try {
        const data = await usersApi.getUserById(userId)
        if (!cancelled) {
          applyUser(data)
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Error al cargar usuario')
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    }

    void fetchUser()

    return () => {
      cancelled = true
    }
  }, [applyUser, userId])

  // Clear notice after a few seconds
  useEffect(() => {
    if (!notice) return
    const timer = window.setTimeout(() => setNotice(null), 3500)
    return () => window.clearTimeout(timer)
  }, [notice])

  async function handleProfileSubmit(data: UpdateUserFormData) {
    setIsSaving(true)
    try {
      await usersApi.updateUser(userId, data)
      await loadUser()
      setNotice('Datos del perfil actualizados.')
      onUpdated()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al actualizar el usuario')
    } finally {
      setIsSaving(false)
    }
  }

  async function handleChangeRole() {
    if (!user || pendingRole === user.role) return
    setIsSaving(true)
    try {
      await usersApi.updateUserRole(userId, pendingRole)
      await loadUser()
      setNotice('Rol actualizado.')
      onUpdated()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cambiar el rol')
    } finally {
      setIsSaving(false)
    }
  }

  async function handleDisable() {
    setIsSaving(true)
    try {
      await usersApi.disableUser(userId)
      await loadUser()
      setShowDisableDialog(false)
      setNotice('Usuario desactivado.')
      onUpdated()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al desactivar')
    } finally {
      setIsSaving(false)
    }
  }

  async function handleReactivate() {
    setIsSaving(true)
    try {
      await usersApi.activateUser(userId)
      await loadUser()
      setNotice('Usuario reactivado.')
      onUpdated()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al reactivar')
    } finally {
      setIsSaving(false)
    }
  }

  async function handleResetPassword() {
    setIsSaving(true)
    try {
      const response = await usersApi.resetPassword(userId)
      setResetPasswordResponse(response)
      setShowResetDialog(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al resetear contrasena')
    } finally {
      setIsSaving(false)
    }
  }

  if (isLoading) {
    return <DetailSkeleton />
  }

  if (!user) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
        <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          {error ?? 'Usuario no encontrado'}
        </div>
        <Button onClick={onClose} size="sm" type="button" variant="secondary">
          Volver
        </Button>
      </div>
    )
  }

  const roleLabel = roleLabels[user.role] ?? user.role
  const isRoleDirty = pendingRole !== user.role

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="sticky top-0 z-10 border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-center gap-3">
            <div className="relative shrink-0">
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-stone-900 text-base font-semibold text-white shadow-sm">
                {initials(user.fullName)}
              </div>
              <span
                aria-hidden="true"
                className={cn(
                  'absolute -bottom-0.5 -right-0.5 h-3.5 w-3.5 rounded-full ring-2 ring-white',
                  user.isActive ? 'bg-emerald-500' : 'bg-stone-300',
                )}
              />
            </div>
            <div className="min-w-0">
              <h3 className="truncate text-lg font-semibold text-stone-950">{user.fullName}</h3>
              <div className="mt-1 flex flex-wrap items-center gap-2 text-xs font-medium text-stone-500">
                <span className="inline-flex items-center gap-1">
                  <Mail size={12} />
                  {user.email}
                </span>
                {user.phone && (
                  <span className="inline-flex items-center gap-1">
                    <Phone size={12} />
                    {user.phone}
                  </span>
                )}
              </div>
              <div className="mt-2 flex flex-wrap items-center gap-1.5">
                <span className="inline-flex items-center gap-1 rounded-full bg-stone-100 px-2 py-0.5 text-[11px] font-semibold text-stone-700 ring-1 ring-stone-200">
                  {roleLabel}
                </span>
                <span
                  className={cn(
                    'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[11px] font-semibold ring-1',
                    user.isActive
                      ? 'bg-emerald-50 text-emerald-700 ring-emerald-200'
                      : 'bg-stone-100 text-stone-600 ring-stone-200',
                  )}
                >
                  <span
                    aria-hidden="true"
                    className={cn(
                      'h-1.5 w-1.5 rounded-full',
                      user.isActive ? 'bg-emerald-500' : 'bg-stone-400',
                    )}
                  />
                  {user.isActive ? 'Activo' : 'Inactivo'}
                </span>
                {user.mustChangePassword && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-semibold text-amber-700 ring-1 ring-amber-200">
                    Debe cambiar clave
                  </span>
                )}
              </div>
            </div>
          </div>
          <Button
            aria-label="Cerrar panel"
            className="shrink-0 lg:hidden"
            onClick={onClose}
            size="icon"
            type="button"
            variant="ghost"
          >
            <X size={16} />
          </Button>
        </div>

        {notice && (
          <div className="mt-3 flex items-center gap-2 rounded-md bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-800 ring-1 ring-emerald-200">
            <CheckCircle2 size={15} />
            {notice}
          </div>
        )}
        {error && (
          <div className="mt-3 flex items-center gap-2 rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
            <AlertTriangle size={15} />
            {error}
          </div>
        )}
      </div>

      <div className="min-h-0 flex-1 space-y-4 p-4 sm:p-6 lg:p-8">
        <Section
          description="Nombre y telefono de contacto del usuario."
          title="Perfil"
        >
          <form className="space-y-4" onSubmit={handleSubmit(handleProfileSubmit)}>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-1.5">
                <Label htmlFor="fullName">Nombre completo</Label>
                <Input disabled={isSaving} id="fullName" {...register('fullName')} />
                {errors.fullName && (
                  <p className="text-sm font-semibold text-red-700">{errors.fullName.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="phone">Telefono</Label>
                <Input disabled={isSaving} id="phone" placeholder="Opcional" {...register('phone')} />
                {errors.phone && (
                  <p className="text-sm font-semibold text-red-700">{errors.phone.message}</p>
                )}
              </div>
            </div>
            <div className="flex justify-end gap-2 border-t border-stone-100 pt-3">
              <Button
                disabled={isSaving || !isDirty}
                onClick={() => reset({ fullName: user.fullName, phone: user.phone ?? '' })}
                size="sm"
                type="button"
                variant="ghost"
              >
                Descartar
              </Button>
              <Button disabled={isSaving || !isDirty} size="sm" type="submit">
                {isSaving ? 'Guardando...' : 'Guardar cambios'}
              </Button>
            </div>
          </form>
        </Section>

        <Section
          description="Permisos y nivel de acceso dentro del negocio."
          title="Rol y permisos"
        >
          <div className="space-y-3">
            <div className="space-y-1.5">
              <Label htmlFor="role">Rol asignado</Label>
              <RoleSelect disabled={isSaving} onValueChange={setPendingRole} value={pendingRole} />
            </div>
            {isRoleDirty && (
              <div className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-amber-200 bg-amber-50 px-3 py-2">
                <p className="text-sm font-semibold text-amber-800">
                  Cambio pendiente: {roleLabels[pendingRole] ?? pendingRole}
                </p>
                <div className="flex gap-2">
                  <Button
                    disabled={isSaving}
                    onClick={() => setPendingRole(user.role)}
                    size="sm"
                    type="button"
                    variant="ghost"
                  >
                    Cancelar
                  </Button>
                  <Button disabled={isSaving} onClick={handleChangeRole} size="sm" type="button">
                    Aplicar
                  </Button>
                </div>
              </div>
            )}
          </div>
        </Section>

        <Section
          description="Acciones que afectan el acceso de este usuario al sistema."
          icon={<AlertTriangle className="text-amber-600" size={16} />}
          title="Seguridad"
          tone="amber"
        >
          <div className="grid gap-2 sm:grid-cols-2">
            <ActionRow
              description="Genera una contrasena temporal. El usuario debera cambiarla al iniciar sesion."
              icon={<KeyRound size={16} />}
              label="Resetear contrasena"
            >
              <Button disabled={isSaving} onClick={handleResetPassword} size="sm" type="button" variant="secondary">
                Generar nueva
              </Button>
            </ActionRow>

            {user.isActive ? (
              <ActionRow
                description="El usuario no podra iniciar sesion hasta ser reactivado."
                icon={<ShieldOff size={16} />}
                label="Desactivar acceso"
                tone="danger"
              >
                <Button
                  disabled={isSaving}
                  onClick={() => setShowDisableDialog(true)}
                  size="sm"
                  type="button"
                  variant="destructive"
                >
                  Desactivar
                </Button>
              </ActionRow>
            ) : (
              <ActionRow
                description="Restaura el acceso del usuario al sistema."
                icon={<ShieldCheck size={16} />}
                label="Reactivar acceso"
              >
                <Button
                  disabled={isSaving}
                  onClick={handleReactivate}
                  size="sm"
                  type="button"
                  variant="secondary"
                >
                  Reactivar
                </Button>
              </ActionRow>
            )}
          </div>
        </Section>

        <Section description="Datos de auditoria de la cuenta." title="Detalles">
          <dl className="grid gap-3 sm:grid-cols-2">
            <div>
              <dt className="text-xs font-semibold uppercase text-stone-500">Creado</dt>
              <dd className="mt-1 text-sm font-medium text-stone-900">{formatDate(user.createdAt)}</dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-stone-500">Ultima actualizacion</dt>
              <dd className="mt-1 text-sm font-medium text-stone-900">{formatDate(user.updatedAt)}</dd>
            </div>
          </dl>
        </Section>
      </div>

      <AlertDialog onOpenChange={setShowDisableDialog} open={showDisableDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Desactivar usuario</AlertDialogTitle>
            <AlertDialogDescription>
              <strong>{user.fullName}</strong> no podra iniciar sesion. Puedes reactivarlo cuando lo necesites.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setShowDisableDialog(false)}>
              Cancelar
            </AlertDialogCancel>
            <AlertDialogAction
              disabled={isSaving}
              onClick={handleDisable}
              variant="destructive"
            >
              <Trash2 size={14} />
              Desactivar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {resetPasswordResponse && (
        <ResetPasswordDialog
          isLoading={false}
          onConfirm={() => {
            setShowResetDialog(false)
            setResetPasswordResponse(null)
            setNotice('Contrasena temporal generada.')
            void loadUser()
          }}
          onOpenChange={setShowResetDialog}
          open={showResetDialog}
          temporaryPassword={resetPasswordResponse.temporaryPassword}
        />
      )}
    </div>
  )
}

type SectionProps = {
  children: React.ReactNode
  description?: string
  icon?: React.ReactNode
  title: string
  tone?: 'default' | 'amber'
}

function Section({ children, description, icon, title, tone = 'default' }: Readonly<SectionProps>) {
  const toneClass = tone === 'amber' ? 'ring-amber-200' : 'ring-stone-200'

  return (
    <section
      className={cn(
        'rounded-md bg-white p-4 ring-1 sm:p-5',
        toneClass,
      )}
    >
      <header className="mb-4 flex items-start gap-2.5">
        {icon && <span className="mt-0.5 shrink-0">{icon}</span>}
        <div className="min-w-0">
          <h4 className="text-sm font-semibold text-stone-950">{title}</h4>
          {description && (
            <p className="mt-0.5 text-xs font-medium text-stone-500">{description}</p>
          )}
        </div>
      </header>
      {children}
    </section>
  )
}

type ActionRowProps = {
  children: React.ReactNode
  description: string
  icon: React.ReactNode
  label: string
  tone?: 'default' | 'danger'
}

function ActionRow({ children, description, icon, label, tone = 'default' }: Readonly<ActionRowProps>) {
  const iconClass = tone === 'danger'
    ? 'bg-red-100 text-red-700 ring-red-200'
    : 'bg-white text-stone-700 ring-stone-200'

  return (
    <div
      className={cn(
        'flex flex-col gap-3 rounded-md border p-3',
        tone === 'danger' ? 'border-red-200 bg-red-50/40' : 'border-stone-200 bg-stone-50/60',
      )}
    >
      <div className="flex items-start gap-2.5">
        <span
          className={cn(
            'mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-md ring-1',
            iconClass,
          )}
        >
          {icon}
        </span>
        <div className="min-w-0 flex-1">
          <p className="text-sm font-semibold text-stone-900">{label}</p>
          <p className="mt-0.5 text-xs font-medium text-stone-500">{description}</p>
        </div>
      </div>
      <div className="flex justify-end">{children}</div>
    </div>
  )
}

function DetailSkeleton() {
  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-center gap-3">
          <div className="h-12 w-12 shrink-0 rounded-full bg-stone-100" />
          <div className="min-w-0 flex-1 space-y-2">
            <div className="h-4 w-1/3 rounded bg-stone-100" />
            <div className="h-3 w-1/2 rounded bg-stone-100" />
          </div>
        </div>
      </div>
      <div className="space-y-4 p-4 sm:p-6 lg:p-8">
        <div className="h-32 rounded-md bg-white ring-1 ring-stone-200" />
        <div className="h-24 rounded-md bg-white ring-1 ring-stone-200" />
        <div className="h-32 rounded-md bg-white ring-1 ring-stone-200" />
      </div>
    </div>
  )
}

// ── Helpers ──────────────────────────────────────────────────────────────────

function formatDate(iso: string): string {
  try {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(new Date(iso))
  } catch {
    return iso
  }
}
