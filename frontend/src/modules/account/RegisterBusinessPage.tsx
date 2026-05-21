import { zodResolver } from '@hookform/resolvers/zod'
import { Building2, Eye, EyeOff } from 'lucide-react'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'
import { useRegisterBusinessMutation } from '@/modules/account/hooks/useRegisterBusinessMutation'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const registerBusinessSchema = z.object({
  branchName: z.string().min(2, 'Nombre de sucursal requerido'),
  businessName: z.string().min(2, 'Nombre del comercio requerido'),
  email: z.email('Correo invalido'),
  identificationNumber: z.string().min(1, 'Numero de identificacion requerido'),
  identificationType: z.enum(['Cedula', 'Rnc', 'Passport'], {
    message: 'Tipo de identificacion requerido',
  }),
  ownerFullName: z.string().min(2, 'Nombre del administrador requerido'),
  password: z.string().min(8, 'La contrasena debe tener al menos 8 caracteres'),
  phonePrimary: z.string().min(1, 'Telefono principal requerido'),
  phoneSecondary: z.string().optional(),
}).superRefine((values, context) => {
  const normalizedIdentification = values.identificationNumber.replace(/\D/g, '')

  if (values.identificationType === 'Cedula' && normalizedIdentification.length !== 11) {
    context.addIssue({
      code: 'custom',
      message: 'La cedula debe tener 11 digitos',
      path: ['identificationNumber'],
    })
  }

  if (values.identificationType === 'Rnc' && normalizedIdentification.length !== 9) {
    context.addIssue({
      code: 'custom',
      message: 'El RNC debe tener 9 digitos',
      path: ['identificationNumber'],
    })
  }

  if (
    values.identificationType === 'Passport' &&
    values.identificationNumber.replace(/\s/g, '').length < 5
  ) {
    context.addIssue({
      code: 'custom',
      message: 'El pasaporte debe tener al menos 5 caracteres',
      path: ['identificationNumber'],
    })
  }
})

type RegisterBusinessFormValues = z.infer<typeof registerBusinessSchema>

const inputClass =
  'h-11 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15'

export function RegisterBusinessPage() {
  const [showPassword, setShowPassword] = useState(false)
  const navigate = useNavigate()
  const registerBusiness = useRegisterBusinessMutation()
  const {
    control,
    formState: { errors, isValid },
    handleSubmit,
    register,
  } = useForm<RegisterBusinessFormValues>({
    mode: 'onChange',
    resolver: zodResolver(registerBusinessSchema),
    defaultValues: {
      branchName: 'Sucursal principal',
      businessName: '',
      email: '',
      identificationNumber: '',
      identificationType: 'Cedula',
      ownerFullName: '',
      password: '',
      phonePrimary: '',
      phoneSecondary: '',
    },
  })

  const errorMessage =
    registerBusiness.error instanceof HttpClientError
      ? registerBusiness.error.message
      : null

  async function onSubmit(values: RegisterBusinessFormValues) {
    await registerBusiness.mutateAsync({
      branchName: values.branchName,
      businessName: values.businessName,
      email: values.email,
      identificationNumber: emptyToNull(values.identificationNumber),
      identificationType: emptyToNull(values.identificationType),
      ownerFullName: values.ownerFullName,
      password: values.password,
      phones: [
        { isPrimary: true, label: 'Principal', number: values.phonePrimary },
        ...(emptyToNull(values.phoneSecondary)
          ? [{ isPrimary: false, label: 'Secundario', number: values.phoneSecondary! }]
          : []),
      ],
    })
    navigate('/', { replace: true })
  }

  return (
    <main className="min-h-dvh bg-stone-50 px-4 py-8 text-stone-950">
      <div className="mx-auto flex w-full max-w-3xl flex-col gap-6">
        <div className="text-center">
          <span className="mx-auto flex h-12 w-12 items-center justify-center rounded-md bg-stone-900 text-white">
            <Building2 size={24} />
          </span>
          <h1 className="mt-5 text-3xl font-semibold">Registrar comercio</h1>
          <p className="mt-2 text-sm font-medium text-stone-600">
            Crea el negocio, la sucursal principal y el usuario administrador inicial.
          </p>
        </div>

        <form
          className="rounded-xl bg-white p-5 shadow-sm ring-1 ring-stone-200 sm:p-6"
          onSubmit={handleSubmit(onSubmit)}
        >
          <div className="grid gap-5 md:grid-cols-2">
            <Field error={errors.businessName?.message} label="Nombre del comercio *">
              <input className={inputClass} placeholder="Colmado La Fe" {...register('businessName')} />
            </Field>
            <Field error={errors.branchName?.message} label="Sucursal principal *">
              <input className={inputClass} {...register('branchName')} />
            </Field>
            <Field error={errors.ownerFullName?.message} label="Administrador *">
              <input className={inputClass} placeholder="Juan Perez" {...register('ownerFullName')} />
            </Field>
            <Field error={errors.email?.message} label="Correo electronico *">
              <input className={inputClass} placeholder="admin@colmado.com" type="email" {...register('email')} />
            </Field>
            <Field error={errors.password?.message} label="Contrasena *">
              <span className="relative block">
                <input
                  className={`${inputClass} pr-12`}
                  type={showPassword ? 'text' : 'password'}
                  {...register('password')}
                />
                <Button
                  aria-label={showPassword ? 'Ocultar contrasena' : 'Mostrar contrasena'}
                  className="absolute right-2 top-1/2 h-8 w-8 -translate-y-1/2"
                  onClick={() => setShowPassword((value) => !value)}
                  size="icon"
                  type="button"
                  variant="ghost"
                >
                  {showPassword ? <EyeOff size={19} /> : <Eye size={19} />}
                </Button>
              </span>
            </Field>
            <Field error={errors.identificationType?.message} label="Tipo de identificacion *">
              <Controller
                control={control}
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
            <Field error={errors.identificationNumber?.message} label="Numero de identificacion *">
              <input className={inputClass} placeholder="Cedula, RNC o pasaporte" {...register('identificationNumber')} />
            </Field>
            <Field error={errors.phonePrimary?.message} label="Telefono principal *">
              <input className={inputClass} placeholder="8090000000" {...register('phonePrimary')} />
            </Field>
            <Field label="Telefono secundario">
              <input className={inputClass} placeholder="Opcional" {...register('phoneSecondary')} />
            </Field>
          </div>

          {errorMessage && (
            <p className="mt-5 rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700 ring-1 ring-red-200">
              {errorMessage}
            </p>
          )}

          <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
            <Button onClick={() => navigate('/login')} type="button" variant="secondary">
              Ya tengo cuenta
            </Button>
            <Button disabled={registerBusiness.isPending || !isValid} type="submit">
              {registerBusiness.isPending ? 'Creando comercio' : 'Crear comercio'}
            </Button>
          </div>
        </form>
      </div>
    </main>
  )
}

type FieldProps = {
  children: React.ReactNode
  error?: string
  label: string
}

function Field({ children, error, label }: Readonly<FieldProps>) {
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-semibold text-stone-900">{label}</span>
      {children}
      {error && <span className="mt-2 block text-sm font-medium text-red-700">{error}</span>}
    </label>
  )
}

function emptyToNull(value?: string | null): string | null {
  return value && value.trim().length > 0 ? value.trim() : null
}
