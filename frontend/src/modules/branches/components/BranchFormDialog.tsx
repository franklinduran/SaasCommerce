import { zodResolver } from '@hookform/resolvers/zod'
import { GitBranch, Pencil } from 'lucide-react'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { BranchStatusBadge } from '@/modules/branches/components/BranchStatusBadge'
import {
  useCreateBranchMutation,
  useUpdateBranchMutation,
} from '@/modules/branches/hooks/useBranches'
import {
  createBranchSchema,
  updateBranchSchema,
} from '@/modules/branches/schemas/branchSchemas'
import type {
  CreateBranchFormValues,
  UpdateBranchFormValues,
} from '@/modules/branches/schemas/branchSchemas'
import type { Branch } from '@/modules/branches/types'
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
import { HttpClientError } from '@/shared/services/httpClient'

type BranchFormDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSaved?: () => void
  branch: Branch | null
}

const branchNameLabel = 'Nombre *'
const branchPhoneLabel = 'Teléfono'
const branchAddressLabel = 'Dirección'
const branchCodeLabel = 'Código *'
const branchCodeHelpText = 'Solo letras mayúsculas y números, máx. 20 caracteres.'

function toCreateDefaults(): CreateBranchFormValues {
  return { address: '', code: '', isMain: false, name: '', phone: '' }
}

function toUpdateDefaults(branch: Branch): UpdateBranchFormValues {
  return {
    address: branch.address ?? '',
    name: branch.name,
    phone: branch.phone ?? '',
  }
}

function toNullable(value?: string) {
  const normalized = value?.trim() ?? ''
  return normalized.length > 0 ? normalized : null
}

function submitButtonLabel(isSubmitting: boolean, isEditing: boolean) {
  if (isSubmitting) return 'Guardando...'
  return isEditing ? 'Guardar cambios' : 'Crear sucursal'
}

export function BranchFormDialog({
  branch,
  onOpenChange,
  onSaved,
  open,
}: Readonly<BranchFormDialogProps>) {
  const isEditing = Boolean(branch)
  const createBranch = useCreateBranchMutation()
  const updateBranch = useUpdateBranchMutation()

  const createForm = useForm<CreateBranchFormValues>({
    defaultValues: toCreateDefaults(),
    resolver: zodResolver(createBranchSchema),
  })

  const updateForm = useForm<UpdateBranchFormValues>({
    defaultValues: branch ? toUpdateDefaults(branch) : { address: '', name: '', phone: '' },
    resolver: zodResolver(updateBranchSchema),
  })

  const { formState: createErrors, handleSubmit: handleCreate, register: registerCreate, reset: resetCreate, setError: setCreateError } = createForm
  const { formState: updateErrors, handleSubmit: handleUpdate, register: registerUpdate, reset: resetUpdate, setError: setUpdateError } = updateForm

  useEffect(() => {
    if (open) {
      if (branch) {
        resetUpdate(toUpdateDefaults(branch))
      } else {
        resetCreate(toCreateDefaults())
      }
    }
  }, [open, branch, resetCreate, resetUpdate])

  async function onCreateSubmit(values: CreateBranchFormValues) {
    try {
      await createBranch.mutateAsync({
        address: toNullable(values.address),
        code: values.code.trim().toUpperCase(),
        isMain: values.isMain,
        name: values.name.trim(),
        phone: toNullable(values.phone),
      })
      onSaved?.()
      onOpenChange(false)
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo crear la sucursal.'
          : 'No se pudo crear la sucursal.'
      setCreateError('name', { message })
    }
  }

  async function onUpdateSubmit(values: UpdateBranchFormValues) {
    if (!branch) return
    try {
      await updateBranch.mutateAsync({
        branchId: branch.id,
        request: {
          address: toNullable(values.address),
          name: values.name.trim(),
          phone: toNullable(values.phone),
        },
      })
      onSaved?.()
      onOpenChange(false)
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo actualizar la sucursal.'
          : 'No se pudo actualizar la sucursal.'
      setUpdateError('name', { message })
    }
  }

  const isSubmitting = createBranch.isPending || updateBranch.isPending

  return (
    <Dialog onOpenChange={onOpenChange} open={open}>
      <DialogContent>
        <DialogHeader>
          <div className="flex items-start gap-3">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
              {isEditing ? <Pencil size={18} /> : <GitBranch size={18} />}
            </span>
            <div>
              <DialogTitle>{isEditing ? 'Editar sucursal' : 'Nueva sucursal'}</DialogTitle>
              <DialogDescription>
                {isEditing
                  ? 'Actualiza el nombre, dirección y teléfono de la sucursal.'
                  : 'Completa los datos para registrar una nueva sucursal.'}
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        {isEditing ? (
          <form
            className="space-y-4"
            id="branch-form"
            onSubmit={handleUpdate(onUpdateSubmit)}
          >
            <div className="flex items-center gap-2">
              <span className="rounded-md bg-stone-100 px-2 py-1 font-mono text-sm font-semibold text-stone-700">
                {branch?.code}
              </span>
              <BranchStatusBadge isActive={branch?.isActive ?? false} isMain={branch?.isMain ?? false} />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="branch-name">{branchNameLabel}</Label>
              <Input disabled={isSubmitting} id="branch-name" {...registerUpdate('name')} />
              {updateErrors.errors.name && (
                <p className="text-sm font-semibold text-red-700">{updateErrors.errors.name.message}</p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-1.5">
                <Label htmlFor="branch-phone">{branchPhoneLabel}</Label>
                <Input disabled={isSubmitting} id="branch-phone" {...registerUpdate('phone')} />
              </div>
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="branch-address">{branchAddressLabel}</Label>
              <Input disabled={isSubmitting} id="branch-address" {...registerUpdate('address')} />
            </div>
          </form>
        ) : (
          <form
            className="space-y-4"
            id="branch-form"
            onSubmit={handleCreate(onCreateSubmit)}
          >
            <div className="space-y-1.5">
              <Label htmlFor="branch-name">{branchNameLabel}</Label>
              <Input disabled={isSubmitting} id="branch-name" {...registerCreate('name')} />
              {createErrors.errors.name && (
                <p className="text-sm font-semibold text-red-700">{createErrors.errors.name.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="branch-code">{branchCodeLabel}</Label>
              <Input
                className="font-mono uppercase"
                disabled={isSubmitting}
                id="branch-code"
                placeholder="SUCURSAL01"
                {...registerCreate('code')}
              />
              <p className="text-xs font-medium text-stone-500">
                {branchCodeHelpText}
              </p>
              {createErrors.errors.code && (
                <p className="text-sm font-semibold text-red-700">{createErrors.errors.code.message}</p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-1.5">
                <Label htmlFor="branch-phone">{branchPhoneLabel}</Label>
                <Input disabled={isSubmitting} id="branch-phone" {...registerCreate('phone')} />
              </div>
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="branch-address">{branchAddressLabel}</Label>
              <Input disabled={isSubmitting} id="branch-address" {...registerCreate('address')} />
            </div>

            <label className="flex h-11 items-center gap-3 rounded-md bg-stone-50 px-3 text-sm font-semibold text-stone-800 ring-1 ring-stone-200">
              <input
                className="h-4 w-4 accent-stone-900"
                disabled={isSubmitting}
                type="checkbox"
                {...registerCreate('isMain')}
              />
              <span>Sucursal principal</span>
            </label>
          </form>
        )}

        <DialogFooter>
          <Button
            disabled={isSubmitting}
            onClick={() => onOpenChange(false)}
            type="button"
            variant="secondary"
          >
            Cancelar
          </Button>
          <Button disabled={isSubmitting} form="branch-form" type="submit">
            {submitButtonLabel(isSubmitting, isEditing)}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
