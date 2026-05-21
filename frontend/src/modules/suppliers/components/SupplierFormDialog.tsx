import { zodResolver } from '@hookform/resolvers/zod'
import { Building2, Pencil } from 'lucide-react'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { useCreateSupplier, useUpdateSupplier } from '@/modules/suppliers/hooks/useSuppliers'
import type { Supplier, SupplierRequest } from '@/modules/suppliers/types'
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

const supplierSchema = z.object({
  address: z.string().optional(),
  email: z.email({ error: 'Correo invalido' }).or(z.literal('')).optional(),
  isActive: z.boolean(),
  name: z.string().min(2, 'Nombre requerido'),
  phone: z.string().optional(),
  rnc: z.string().optional(),
})

type SupplierFormValues = z.infer<typeof supplierSchema>

type SupplierFormDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSaved?: (saved?: Supplier) => void
  supplier: Supplier | null
}

function toDefaults(supplier?: Supplier | null): SupplierFormValues {
  return {
    address: supplier?.address ?? '',
    email: supplier?.email ?? '',
    isActive: supplier?.isActive ?? true,
    name: supplier?.name ?? '',
    phone: supplier?.phone ?? '',
    rnc: supplier?.rnc ?? '',
  }
}

function toNullable(value?: string) {
  const normalized = value?.trim() ?? ''
  return normalized.length > 0 ? normalized : null
}

export function SupplierFormDialog({
  onOpenChange,
  onSaved,
  open,
  supplier,
}: Readonly<SupplierFormDialogProps>) {
  const createSupplier = useCreateSupplier()
  const updateSupplier = useUpdateSupplier()
  const isEditing = Boolean(supplier)

  const {
    formState: { errors },
    handleSubmit,
    register,
    reset,
    setError,
  } = useForm<SupplierFormValues>({
    defaultValues: toDefaults(supplier),
    resolver: zodResolver(supplierSchema),
  })

  useEffect(() => {
    if (open) {
      reset(toDefaults(supplier))
    }
  }, [open, reset, supplier])

  async function onSubmit(values: SupplierFormValues) {
    const request: SupplierRequest = {
      address: toNullable(values.address),
      email: toNullable(values.email),
      name: values.name.trim(),
      phone: toNullable(values.phone),
      rnc: toNullable(values.rnc),
    }

    try {
      if (supplier) {
        const updated = await updateSupplier.mutateAsync({
          request: { ...request, isActive: values.isActive },
          supplierId: supplier.id,
        })
        onSaved?.((updated as Supplier | undefined) ?? supplier)
      } else {
        const created = await createSupplier.mutateAsync(request)
        onSaved?.(created as Supplier | undefined)
      }
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo guardar el proveedor.'
          : 'No se pudo guardar el proveedor.'

      setError('name', { message })
    }
  }

  const isSubmitting = createSupplier.isPending || updateSupplier.isPending
  let submitLabel = 'Crear proveedor'

  if (isSubmitting) {
    submitLabel = 'Guardando...'
  } else if (isEditing) {
    submitLabel = 'Guardar cambios'
  }

  return (
    <Dialog onOpenChange={onOpenChange} open={open}>
      <DialogContent>
        <DialogHeader>
          <div className="flex items-start gap-3">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
              {isEditing ? <Pencil size={18} /> : <Building2 size={18} />}
            </span>
            <div>
              <DialogTitle>{isEditing ? 'Editar proveedor' : 'Crear proveedor'}</DialogTitle>
              <DialogDescription>
                Datos de identificacion, contacto y direccion del proveedor.
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <form className="space-y-4" id="supplier-form" onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-1.5">
            <Label htmlFor="name">Nombre / Razon social *</Label>
            <Input disabled={isSubmitting} id="name" {...register('name')} />
            {errors.name && (
              <p className="text-sm font-semibold text-red-700">{errors.name.message}</p>
            )}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="rnc">RNC / Identificacion fiscal</Label>
              <Input disabled={isSubmitting} id="rnc" {...register('rnc')} />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="phone">Telefono</Label>
              <Input disabled={isSubmitting} id="phone" {...register('phone')} />
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="email">Correo electronico</Label>
            <Input disabled={isSubmitting} id="email" type="email" {...register('email')} />
            {errors.email && (
              <p className="text-sm font-semibold text-red-700">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="address">Direccion</Label>
            <Input disabled={isSubmitting} id="address" {...register('address')} />
          </div>

          {isEditing && (
            <label className="flex h-11 items-center gap-3 rounded-md bg-stone-50 px-3 text-sm font-semibold text-stone-800 ring-1 ring-stone-200">
              <input
                className="h-4 w-4 accent-stone-900"
                disabled={isSubmitting}
                type="checkbox"
                {...register('isActive')}
              />
              <span>Proveedor activo</span>
            </label>
          )}
        </form>

        <DialogFooter>
          <Button
            disabled={isSubmitting}
            onClick={() => onOpenChange(false)}
            type="button"
            variant="secondary"
          >
            Cancelar
          </Button>
          <Button disabled={isSubmitting} form="supplier-form" type="submit">
            {submitLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
