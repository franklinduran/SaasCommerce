import { zodResolver } from '@hookform/resolvers/zod'
import {
  Building2,
  KeyRound,
  RefreshCw,
  Settings as SettingsIcon,
  ShieldCheck,
  SlidersHorizontal,
  Store,
  type LucideIcon,
  UserRound,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { OperationalSettingsPanel } from '@/modules/settings/components/OperationalSettingsPanel'
import {
  useChangePasswordMutation,
  useCurrentBranchQuery,
  useCurrentBusinessQuery,
  useMeQuery,
  useUpdateBranchMutation,
  useUpdateBusinessMutation,
  useUpdateProfileMutation,
} from '@/modules/settings/hooks/useSettings'
import type { CurrentBranch, CurrentBusiness, CurrentUser } from '@/modules/settings/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { HttpClientError } from '@/shared/services/httpClient'
import { cn } from '@/shared/utils/cn'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const profileSchema = z.object({
  fullName: z.string().min(2, 'El nombre es requerido.'),
  phone: z.string().optional(),
})

const businessSchema = z
  .object({
    businessName: z.string().min(2, 'El nombre del negocio es requerido.'),
    identificationNumber: z.string().min(1, 'La identificacion es requerida.'),
    identificationType: z.enum(['Cedula', 'Rnc', 'Passport'], {
      message: 'Selecciona un tipo de identificacion.',
    }),
    primaryPhone: z.string().min(1, 'El telefono principal es requerido.'),
    secondaryPhone: z.string().optional(),
  })
  .superRefine((values, context) => {
    const normalized = values.identificationNumber.replace(/\D/g, '')

    if (values.identificationType === 'Cedula' && normalized.length !== 11) {
      context.addIssue({
        code: 'custom',
        message: 'La cedula debe tener 11 digitos.',
        path: ['identificationNumber'],
      })
    }

    if (values.identificationType === 'Rnc' && normalized.length !== 9) {
      context.addIssue({
        code: 'custom',
        message: 'El RNC debe tener 9 digitos.',
        path: ['identificationNumber'],
      })
    }

    if (values.identificationType === 'Passport' && values.identificationNumber.trim().length < 5) {
      context.addIssue({
        code: 'custom',
        message: 'El pasaporte debe tener al menos 5 caracteres.',
        path: ['identificationNumber'],
      })
    }
  })

const branchSchema = z.object({
  address: z.string().optional(),
  name: z.string().min(2, 'El nombre de la sucursal es requerido.'),
  phone: z.string().optional(),
})

const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'La contrasena actual es requerida.'),
  newPassword: z.string().min(8, 'La nueva contrasena debe tener al menos 8 caracteres.'),
})

type ProfileFormValues = z.infer<typeof profileSchema>
type BusinessFormValues = z.infer<typeof businessSchema>
type BranchFormValues = z.infer<typeof branchSchema>
type PasswordFormValues = z.infer<typeof passwordSchema>

type SettingsSection = 'profile' | 'security' | 'business' | 'branch' | 'operational'

const SETTINGS_SECTIONS: Array<{
  description: string
  icon: LucideIcon
  id: SettingsSection
  label: string
}> = [
  { description: 'Tu informacion personal', icon: UserRound, id: 'profile', label: 'Mi perfil' },
  { description: 'Contrasena y acceso', icon: KeyRound, id: 'security', label: 'Seguridad' },
  { description: 'Datos del comercio', icon: Store, id: 'business', label: 'Negocio' },
  { description: 'Sucursal activa', icon: Building2, id: 'branch', label: 'Sucursal' },
  { description: 'Parametros del sistema', icon: SlidersHorizontal, id: 'operational', label: 'Operativo' },
]

export function SettingsPage() {
  const me = useMeQuery()
  const business = useCurrentBusinessQuery()
  const branch = useCurrentBranchQuery()
  const [activeSection, setActiveSection] = useState<SettingsSection>('profile')
  const isLoading = me.isLoading || business.isLoading || branch.isLoading
  const isError = me.isError || business.isError || branch.isError

  function refetchAll() {
    me.refetch()
    business.refetch()
    branch.refetch()
  }

  return (
    <section className="flex min-h-full flex-col">
      <div className="shrink-0 border-b border-stone-200 bg-white px-4 py-5 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
              <SettingsIcon size={13} />
              Configuracion
            </p>
            <h2 className="mt-1 text-2xl font-semibold text-stone-950">Ajustes</h2>
            <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
              Administra tu perfil, el comercio, la sucursal activa, la seguridad y los parametros operativos.
            </p>
          </div>
          <Button disabled={isLoading} onClick={refetchAll} size="sm" type="button" variant="secondary">
            <RefreshCw className={isLoading ? 'animate-spin' : undefined} size={14} />
            Refrescar
          </Button>
        </div>
      </div>

      <div className="grid flex-1 lg:grid-cols-[260px_minmax(0,1fr)]">
        {/* Sidebar nav */}
        <aside className="shrink-0 border-stone-200 bg-white lg:border-r">
          <nav className="flex gap-1 overflow-x-auto p-2 lg:flex-col lg:gap-0.5 lg:p-3">
            {SETTINGS_SECTIONS.map((section) => {
              const isActive = activeSection === section.id
              const Icon = section.icon
              return (
                <button
                  aria-current={isActive ? 'true' : undefined}
                  className={cn(
                    'group flex h-9 shrink-0 items-center gap-2.5 rounded-md px-3 text-left text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-stone-900/25 focus-visible:ring-offset-2',
                    isActive
                      ? 'bg-stone-900 text-white shadow-sm ring-1 ring-stone-900 hover:bg-stone-900 hover:text-white active:bg-stone-950 active:text-white'
                      : 'text-stone-700 hover:bg-stone-100 hover:text-stone-950 active:bg-stone-200',
                  )}
                  key={section.id}
                  onClick={() => setActiveSection(section.id)}
                  type="button"
                >
                  <Icon
                    aria-hidden="true"
                    className={cn(isActive ? 'text-white' : 'text-stone-500 group-hover:text-stone-950')}
                    size={15}
                    strokeWidth={isActive ? 2.25 : 2}
                  />
                  <span className="flex-1 truncate">{section.label}</span>
                </button>
              )
            })}
          </nav>
        </aside>

        {/* Section content */}
        <div className="bg-stone-50 p-4 sm:p-6 lg:p-8">
          {isLoading ? (
            <SettingsSkeleton />
          ) : isError ? (
            <Card>
              <CardContent className="p-5">
                <p className="text-sm font-semibold text-red-700">No se pudieron cargar los ajustes.</p>
                <p className="mt-1 text-sm font-medium text-stone-600">
                  Verifica la sesion o intenta nuevamente.
                </p>
              </CardContent>
            </Card>
          ) : (
            <div className="mx-auto max-w-3xl">
              {activeSection === 'profile' && <ProfileSettingsCard initialValues={me.data} />}
              {activeSection === 'security' && <SecuritySettingsCard />}
              {activeSection === 'business' && <BusinessSettingsCard initialValues={business.data} />}
              {activeSection === 'branch' && <BranchSettingsCard initialValues={branch.data} />}
              {activeSection === 'operational' && <OperationalSettingsPanel />}
            </div>
          )}
        </div>
      </div>
    </section>
  )
}

function ProfileSettingsCard({
  initialValues,
}: Readonly<{ initialValues: CurrentUser | undefined }>) {
  const updateProfile = useUpdateProfileMutation()
  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    values: {
      fullName: initialValues?.fullName ?? '',
      phone: initialValues?.phone ?? '',
    },
  })

  return (
    <SettingsCard
      description="Datos visibles para operaciones internas."
      icon={<UserRound size={18} />}
      title="Mi perfil"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) =>
          updateProfile.mutate({ fullName: values.fullName, phone: emptyToNull(values.phone) }),
        )}
      >
        <Field error={form.formState.errors.fullName?.message} label="Nombre completo *">
          <input className={inputClassName} {...form.register('fullName')} />
        </Field>
        <Field error={form.formState.errors.phone?.message} label="Telefono">
          <input className={inputClassName} {...form.register('phone')} />
        </Field>
        <FormFooter mutation={updateProfile} />
      </form>
    </SettingsCard>
  )
}

function BusinessSettingsCard({
  initialValues,
}: Readonly<{
  initialValues: CurrentBusiness | undefined
}>) {
  const updateBusiness = useUpdateBusinessMutation()
  const form = useForm<BusinessFormValues>({
    mode: 'onChange',
    resolver: zodResolver(businessSchema),
    values: {
      businessName: initialValues?.name ?? '',
      identificationNumber: initialValues?.identificationNumber ?? '',
      identificationType:
        (initialValues?.identificationType as BusinessFormValues['identificationType']) ?? 'Rnc',
      primaryPhone: initialValues?.phones.find((phone) => phone.isPrimary)?.number ?? '',
      secondaryPhone: initialValues?.phones.find((phone) => !phone.isPrimary)?.number ?? '',
    },
  })

  return (
    <SettingsCard
      description="Identificacion y telefonos del tenant actual."
      icon={<Store size={18} />}
      title="Negocio"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) =>
          updateBusiness.mutate({
            businessName: values.businessName,
            identificationNumber: values.identificationNumber,
            identificationType: values.identificationType,
            phones: [
              { isPrimary: true, label: 'Principal', number: values.primaryPhone },
              ...(emptyToNull(values.secondaryPhone)
                ? [{ isPrimary: false, label: 'Secundario', number: values.secondaryPhone! }]
                : []),
            ],
          }),
        )}
      >
        <Field error={form.formState.errors.businessName?.message} label="Nombre del negocio *">
          <input className={inputClassName} {...form.register('businessName')} />
        </Field>
        <div className="grid gap-4 lg:grid-cols-[180px_minmax(0,1fr)]">
          <Field error={form.formState.errors.identificationType?.message} label="Tipo *">
            <Controller
              control={form.control}
              name="identificationType"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Cedula">Cedula</SelectItem>
                    <SelectItem value="Rnc">RNC</SelectItem>
                    <SelectItem value="Passport">Pasaporte</SelectItem>
                  </SelectContent>
                </Select>
              )}
            />
          </Field>
          <Field error={form.formState.errors.identificationNumber?.message} label="Identificacion *">
            <input className={inputClassName} {...form.register('identificationNumber')} />
          </Field>
        </div>
        <div className="grid gap-4 lg:grid-cols-2">
          <Field error={form.formState.errors.primaryPhone?.message} label="Telefono principal *">
            <input className={inputClassName} {...form.register('primaryPhone')} />
          </Field>
          <Field error={form.formState.errors.secondaryPhone?.message} label="Telefono secundario">
            <input className={inputClassName} {...form.register('secondaryPhone')} />
          </Field>
        </div>
        <FormFooter mutation={updateBusiness} />
      </form>
    </SettingsCard>
  )
}

function BranchSettingsCard({
  initialValues,
}: Readonly<{
  initialValues: CurrentBranch | undefined
}>) {
  const updateBranch = useUpdateBranchMutation()
  const form = useForm<BranchFormValues>({
    resolver: zodResolver(branchSchema),
    values: {
      address: initialValues?.address ?? '',
      name: initialValues?.name ?? '',
      phone: initialValues?.phone ?? '',
    },
  })

  return (
    <SettingsCard
      description="Datos operativos de la sucursal activa."
      icon={<Building2 size={18} />}
      title="Sucursal"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) =>
          updateBranch.mutate({
            address: emptyToNull(values.address),
            name: values.name,
            phone: emptyToNull(values.phone),
          }),
        )}
      >
        <Field error={form.formState.errors.name?.message} label="Nombre *">
          <input className={inputClassName} {...form.register('name')} />
        </Field>
        <Field error={form.formState.errors.address?.message} label="Direccion">
          <input className={inputClassName} {...form.register('address')} />
        </Field>
        <Field error={form.formState.errors.phone?.message} label="Telefono">
          <input className={inputClassName} {...form.register('phone')} />
        </Field>
        <FormFooter mutation={updateBranch} />
      </form>
    </SettingsCard>
  )
}

function SecuritySettingsCard() {
  const changePassword = useChangePasswordMutation()
  const form = useForm<PasswordFormValues>({
    resolver: zodResolver(passwordSchema),
    values: {
      currentPassword: '',
      newPassword: '',
    },
  })

  useEffect(() => {
    if (changePassword.isSuccess) {
      form.reset({ currentPassword: '', newPassword: '' })
    }
  }, [changePassword.isSuccess, form])

  return (
    <SettingsCard
      description="Actualiza tu clave sin exponer datos sensibles."
      icon={<KeyRound size={18} />}
      title="Seguridad"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) => changePassword.mutate(values))}
      >
        <Field error={form.formState.errors.currentPassword?.message} label="Contrasena actual *">
          <input className={inputClassName} type="password" {...form.register('currentPassword')} />
        </Field>
        <Field error={form.formState.errors.newPassword?.message} label="Nueva contrasena *">
          <input className={inputClassName} type="password" {...form.register('newPassword')} />
        </Field>
        <FormFooter mutation={changePassword} />
      </form>
    </SettingsCard>
  )
}

function SettingsCard({
  children,
  description,
  icon,
  title,
}: Readonly<{
  children: ReactNode
  description: string
  icon: React.ReactNode
  title: string
}>) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-start gap-3">
        <span className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-stone-100 text-stone-800 ring-1 ring-stone-200">
          {icon}
        </span>
        <div>
          <h3 className="text-base font-semibold text-stone-950">{title}</h3>
          <p className="mt-1 text-sm font-medium text-stone-600">{description}</p>
        </div>
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
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
    <label className="block space-y-1.5">
      <span className="text-sm font-semibold text-stone-800">{label}</span>
      {children}
      {error && <span className="block text-sm font-semibold text-red-700">{error}</span>}
    </label>
  )
}

function FormFooter({
  mutation,
}: Readonly<{ mutation: { error: Error | null; isPending: boolean; isSuccess: boolean } }>) {
  return (
    <div className="flex flex-col gap-3 border-t border-stone-200 pt-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-h-5">
        {mutation.error && (
          <p className="text-sm font-semibold text-red-700">{toErrorMessage(mutation.error)}</p>
        )}
        {mutation.isSuccess && (
          <p className="flex items-center gap-2 text-sm font-semibold text-emerald-700">
            <ShieldCheck size={15} />
            Cambios guardados correctamente.
          </p>
        )}
      </div>
      <Button disabled={mutation.isPending} type="submit">
        {mutation.isPending ? 'Guardando...' : 'Guardar cambios'}
      </Button>
    </div>
  )
}

function SettingsSkeleton() {
  const skeletonIds = ['profile-settings', 'business-settings', 'branch-settings', 'security-settings']

  return (
    <div className="grid gap-6 xl:grid-cols-2">
      {skeletonIds.map((id) => (
        <Card key={id}>
          <CardContent className="space-y-4 p-4 sm:p-6">
            <div className="h-5 w-48 rounded bg-stone-100" />
            <div className="h-10 rounded bg-stone-100" />
            <div className="h-10 rounded bg-stone-100" />
            <div className="h-10 w-40 rounded bg-stone-100" />
          </CardContent>
        </Card>
      ))}
    </div>
  )
}

function emptyToNull(value: string | undefined): string | null {
  return value?.trim() ? value.trim() : null
}

function toErrorMessage(error: Error): string {
  if (error instanceof HttpClientError) {
    return error.error?.message ?? error.message
  }

  return error.message
}

const inputClassName =
  'h-10 w-full min-w-0 rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-300 outline-none placeholder:text-stone-400 focus:ring-2 focus:ring-stone-900/20'
