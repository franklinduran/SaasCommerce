import { zodResolver } from '@hookform/resolvers/zod'
import { Check } from 'lucide-react'
import type { ReactNode } from 'react'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { useCreateSupplier, useUpdateSupplier } from '@/modules/suppliers/hooks/useSuppliers'
import type { Supplier, SupplierRequest } from '@/modules/suppliers/types'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'

const supplierSchema = z.object({
  address: z.string().optional(),
  email: z.string().email('Correo invalido').or(z.literal('')).optional(),
  isActive: z.boolean(),
  name: z.string().min(2, 'Nombre requerido'),
  phone: z.string().optional(),
  rnc: z.string().optional(),
})

type SupplierFormValues = z.infer<typeof supplierSchema>

const inputClass =
  'h-11 w-full rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15'

type SupplierFormProps = {
  supplier?: Supplier | null
  onSaved?: () => void
}

export function SupplierForm({ supplier, onSaved }: Readonly<SupplierFormProps>) {
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
    reset(toDefaults(supplier))
  }, [reset, supplier])

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
        await updateSupplier.mutateAsync({
          supplierId: supplier.id,
          request: { ...request, isActive: values.isActive },
        })
      } else {
        await createSupplier.mutateAsync(request)
      }

      onSaved?.()
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo guardar el proveedor.'
          : 'No se pudo guardar el proveedor.'

      setError('name', { message })
    }
  }

  return (
    <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid gap-4 md:grid-cols-2">
        <Field error={errors.name?.message} label="Nombre">
          <input className={inputClass} {...register('name')} />
        </Field>
        <Field error={errors.rnc?.message} label="RNC">
          <input className={inputClass} {...register('rnc')} />
        </Field>
        <Field error={errors.phone?.message} label="Telefono">
          <input className={inputClass} {...register('phone')} />
        </Field>
        <Field error={errors.email?.message} label="Correo">
          <input className={inputClass} {...register('email')} />
        </Field>
        <Field error={errors.address?.message} label="Direccion">
          <input className={inputClass} {...register('address')} />
        </Field>
        {isEditing && (
          <label className="flex h-11 items-center gap-3 rounded-md bg-white px-3 text-sm font-semibold text-stone-800 shadow-sm ring-1 ring-stone-200">
            <input className="h-4 w-4 accent-stone-900" type="checkbox" {...register('isActive')} />
            Activo
          </label>
        )}
      </div>
      <Button disabled={createSupplier.isPending || updateSupplier.isPending} type="submit">
        <Check size={16} />
        Guardar proveedor
      </Button>
    </form>
  )
}

function Field({
  children,
  error,
  label,
}: Readonly<{
  children: ReactNode
  error?: string
  label: string
}>) {
  return (
    <label className="space-y-1.5">
      <span className="text-sm font-semibold text-stone-700">{label}</span>
      {children}
      {error && <span className="text-xs font-semibold text-red-700">{error}</span>}
    </label>
  )
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
