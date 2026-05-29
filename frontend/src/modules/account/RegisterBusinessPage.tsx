import { zodResolver } from '@hookform/resolvers/zod'
import { Eye, EyeOff, Info, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { Button } from '@/shared/components/ui/button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { HttpClientError } from '@/shared/services/httpClient'
import { AuthAltHeader, AuthSplitLayout } from '@/modules/account/components/AuthSplitLayout'
import { useRegisterBusinessMutation } from '@/modules/account/hooks/useRegisterBusinessMutation'
import { useSubscriptionPlans } from '@/modules/subscription/hooks/useSubscription'
import { cn } from '@/shared/utils/cn'

// ─── Validation schema ────────────────────────────────────────────────────────

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
  planId: z.string().min(1, 'Selecciona un plan'),
  phonePrimary: z.string().min(1, 'Telefono principal requerido'),
  phoneSecondary: z.string().optional(),
}).superRefine((values, context) => {
  const normalizedIdentification = values.identificationNumber.replace(/\D/g, '')

  if (values.identificationType === 'Cedula' && normalizedIdentification.length !== 11) {
    context.addIssue({ code: 'custom', message: 'La cedula debe tener 11 digitos', path: ['identificationNumber'] })
  }
  if (values.identificationType === 'Rnc' && normalizedIdentification.length !== 9) {
    context.addIssue({ code: 'custom', message: 'El RNC debe tener 9 digitos', path: ['identificationNumber'] })
  }
  if (values.identificationType === 'Passport' && values.identificationNumber.replace(/\s/g, '').length < 5) {
    context.addIssue({ code: 'custom', message: 'El pasaporte debe tener al menos 5 caracteres', path: ['identificationNumber'] })
  }
})

type RegisterBusinessFormValues = z.infer<typeof registerBusinessSchema>

// ─── Step configuration ───────────────────────────────────────────────────────

const STEP_LABELS = ['Negocio', 'Administrador', 'Confirmación']
const STEP_TITLES: Record<number, string> = {
  1: 'Información del negocio',
  2: 'Datos del administrador',
  3: 'Confirmación',
}
const STEP_FIELDS: Record<number, (keyof RegisterBusinessFormValues)[]> = {
  1: ['planId', 'businessName', 'branchName', 'identificationType', 'identificationNumber', 'phonePrimary'],
  2: ['ownerFullName', 'email', 'password'],
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function RegisterBusinessPage() {
  const [currentStep, setCurrentStep] = useState(1)
  const [showPassword, setShowPassword] = useState(false)
  const navigate = useNavigate()
  const registerBusiness = useRegisterBusinessMutation()
  const plansQuery = useSubscriptionPlans()
  const plans = Array.isArray(plansQuery.data) ? plansQuery.data : []

  const {
    control,
    formState: { errors },
    getValues,
    handleSubmit,
    register,
    trigger,
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
      planId: '',
      phonePrimary: '',
      phoneSecondary: '',
    },
  })

  const errorMessage =
    registerBusiness.error instanceof HttpClientError ? registerBusiness.error.message : null

  async function handleContinue() {
    const fields = STEP_FIELDS[currentStep]
    if (!fields) return
    const isValid = await trigger(fields)
    if (isValid) setCurrentStep((s) => s + 1)
  }

  async function onSubmit(values: RegisterBusinessFormValues) {
    // Defensive guard: only submit when explicitly on the last step.
    // Prevents accidental submissions during step transitions (Continuar →
    // re-render → button DOM reused with type="submit").
    if (currentStep !== STEP_LABELS.length) return

    await registerBusiness.mutateAsync({
      branchName: values.branchName,
      businessName: values.businessName,
      email: values.email,
      identificationNumber: emptyToNull(values.identificationNumber),
      identificationType: emptyToNull(values.identificationType),
      ownerFullName: values.ownerFullName,
      password: values.password,
      planId: values.planId,
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
    <AuthSplitLayout>

      <AuthAltHeader
        actionLabel="Iniciar sesión"
        prompt="¿Ya tienes una cuenta?"
        to="/login"
      />

      {/* Content — vertically centered, no scroll */}
      <div className="flex flex-1 flex-col items-center justify-center overflow-y-auto px-10 lg:px-14">
        <div className="w-full max-w-[540px]">

          {/* Heading */}
          <h1 className="text-[2.125rem] font-bold leading-[1.05] tracking-tight text-gray-900">
            Crea tu comercio
          </h1>
          <p className="mt-2 text-[14.5px] text-gray-500">
            Configura tu negocio en pocos pasos
          </p>

          {/* Step indicator */}
          <StepIndicator current={currentStep} labels={STEP_LABELS} />

          {/* Current step title */}
          <p className="mb-4 text-[13.5px] font-semibold text-primary">
            Paso {currentStep} de {STEP_LABELS.length} · {STEP_TITLES[currentStep]}
          </p>

          {/* Form */}
          <form id="register-form" onSubmit={handleSubmit(onSubmit)}>
            {currentStep === 1 && (
              <Step1
                control={control}
                errors={errors}
                plans={plans}
                plansQuery={plansQuery}
                register={register}
              />
            )}
            {currentStep === 2 && (
              <Step2
                errors={errors}
                register={register}
                showPassword={showPassword}
                onTogglePassword={() => setShowPassword((v) => !v)}
              />
            )}
            {currentStep === 3 && (
              <Step3 errorMessage={errorMessage} plans={plans} values={getValues()} />
            )}

            {/* Wizard navigation — inside the form, aligned right like Iniciar sesión */}
            <div className="mt-6 flex items-center justify-end gap-3">
              {currentStep > 1 && (
                <Button
                  className="h-10 px-5"
                  onClick={() => setCurrentStep((s) => s - 1)}
                  type="button"
                  variant="outline"
                >
                  Atrás
                </Button>
              )}
              {currentStep < STEP_LABELS.length ? (
                <Button
                  className="h-10 px-6 font-semibold"
                  disabled={plansQuery.isLoading && currentStep === 1}
                  onClick={handleContinue}
                  type="button"
                >
                  Continuar
                </Button>
              ) : (
                <Button
                  className="h-10 px-6 font-semibold"
                  disabled={registerBusiness.isPending}
                  onClick={handleSubmit(onSubmit)}
                  type="button"
                >
                  {registerBusiness.isPending ? 'Creando comercio...' : 'Crear comercio'}
                </Button>
              )}
            </div>
          </form>

        </div>
      </div>

      {/* Footer — copyright left, support links right */}
      <footer className="flex shrink-0 items-center justify-between border-t border-gray-200 px-10 py-5 lg:px-14">
        <span className="text-[12px] text-gray-400">© 2026 ComercioFlow</span>
        <nav aria-label="Ayuda" className="flex items-center gap-5 text-[13px] text-gray-500">
          <a className="transition hover:text-gray-900" href="#soporte">Soporte</a>
          <a className="transition hover:text-gray-900" href="#preguntas">Preguntas frecuentes</a>
          <a className="transition hover:text-gray-900" href="#terminos">Términos</a>
        </nav>
      </footer>

    </AuthSplitLayout>
  )
}

// ─── Step indicator ───────────────────────────────────────────────────────────
// Lines are interleaved BETWEEN circles via flatMap — never absolute behind them.

function StepIndicator({ current, labels }: Readonly<{ current: number; labels: string[] }>) {
  return (
    <div className="mb-3 mt-7 flex items-start">
      {labels.flatMap((label, i) => {
        const num = i + 1
        const isActive = num === current
        const isDone = num < current

        const step = (
          <div key={num} className="flex shrink-0 flex-col items-center">
            <div
              className={cn(
                'flex h-8 w-8 items-center justify-center rounded-full text-[13px] font-semibold transition-colors',
                isActive || isDone
                  ? 'bg-primary text-white'
                  : 'bg-white text-gray-400 ring-1 ring-gray-300',
              )}
            >
              {num}
            </div>
            <span
              className={cn(
                'mt-2 whitespace-nowrap text-[12px] transition-colors',
                isActive ? 'font-semibold text-primary' : 'font-medium text-gray-400',
              )}
            >
              {label}
            </span>
          </div>
        )

        if (i < labels.length - 1) {
          // mt-4 = 16px = half of h-8 (32px) → centers the line on circles
          return [
            step,
            <div
              key={`line-${i}`}
              className="mt-4 h-px flex-1 bg-gray-200"
            />,
          ]
        }
        return [step]
      })}
    </div>
  )
}

// ─── Step 1: Business info ────────────────────────────────────────────────────

type Step1Props = {
  control: ReturnType<typeof useForm<RegisterBusinessFormValues>>['control']
  errors: ReturnType<typeof useForm<RegisterBusinessFormValues>>['formState']['errors']
  plans: { id: string; name: string; monthlyPrice: number }[]
  plansQuery: { isLoading: boolean; isError: boolean; refetch: () => void }
  register: ReturnType<typeof useForm<RegisterBusinessFormValues>>['register']
}

function Step1({ control, errors, plans, plansQuery, register }: Readonly<Step1Props>) {
  return (
    <div className="grid gap-x-4 gap-y-3.5 md:grid-cols-2">

      <Field error={errors.planId?.message} label="Plan">
        <Controller
          control={control}
          name="planId"
          render={({ field }) => (
            <Select
              disabled={plansQuery.isLoading || plansQuery.isError}
              value={field.value}
              onValueChange={field.onChange}
            >
              <SelectTrigger className="h-10 w-full rounded-lg border border-gray-200 bg-white text-sm font-medium text-gray-900 shadow-none transition focus:border-primary focus:ring-2 focus:ring-primary/15 data-[placeholder]:font-normal data-[placeholder]:text-gray-400">
                <SelectValue placeholder={plansQuery.isLoading ? 'Cargando planes...' : 'Selecciona un plan'} />
              </SelectTrigger>
              <SelectContent>
                {plans.map((plan) => (
                  <SelectItem key={plan.id} value={plan.id}>
                    {plan.name} - {formatMoney(plan.monthlyPrice)} / mes
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
        {plansQuery.isLoading && (
          <span className="mt-1.5 flex items-center gap-1.5 text-xs text-gray-400">
            <Loader2 className="animate-spin" size={11} /> Cargando planes
          </span>
        )}
        {plansQuery.isError && (
          <span className="mt-1.5 flex items-center gap-2 text-xs font-medium text-red-600">
            No se pudieron cargar los planes.{' '}
            <button className="underline hover:text-red-800" type="button" onClick={() => void plansQuery.refetch()}>
              Reintentar
            </button>
          </span>
        )}
      </Field>

      <Field error={errors.businessName?.message} label="Nombre del comercio">
        <FormInput placeholder="Colmado La Fe" {...register('businessName')} />
      </Field>

      <Field error={errors.branchName?.message} label="Sucursal principal">
        <FormInput {...register('branchName')} />
      </Field>

      <Field error={errors.identificationType?.message} label="Tipo de identificación">
        <Controller
          control={control}
          name="identificationType"
          render={({ field }) => (
            <Select value={field.value} onValueChange={field.onChange}>
              <SelectTrigger className="h-10 w-full rounded-lg border border-gray-200 bg-white text-sm font-medium text-gray-900 shadow-none transition focus:border-primary focus:ring-2 focus:ring-primary/15">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Cedula">Cédula</SelectItem>
                <SelectItem value="Rnc">RNC</SelectItem>
                <SelectItem value="Passport">Pasaporte</SelectItem>
              </SelectContent>
            </Select>
          )}
        />
      </Field>

      <Field error={errors.identificationNumber?.message} label="Numero de identificacion">
        <FormInput placeholder="Cédula, RNC o pasaporte" {...register('identificationNumber')} />
      </Field>

      <Field error={errors.phonePrimary?.message} label="Telefono principal">
        <FormInput placeholder="8090000000" {...register('phonePrimary')} />
      </Field>

      <div className="col-span-full mt-1 flex items-center gap-3 rounded-xl border border-gray-200 bg-white px-4 py-3">
        <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full border border-gray-200 text-gray-400">
          <Info aria-hidden="true" size={12} />
        </span>
        <p className="text-[13px] text-gray-500">
          El plan queda asociado al comercio desde el inicio.
        </p>
      </div>

    </div>
  )
}

// ─── Step 2: Admin info ───────────────────────────────────────────────────────

type Step2Props = {
  errors: ReturnType<typeof useForm<RegisterBusinessFormValues>>['formState']['errors']
  register: ReturnType<typeof useForm<RegisterBusinessFormValues>>['register']
  showPassword: boolean
  onTogglePassword: () => void
}

function Step2({ errors, register, showPassword, onTogglePassword }: Readonly<Step2Props>) {
  return (
    <div className="grid gap-x-4 gap-y-3.5 md:grid-cols-2">
      <div className="col-span-full">
        <Field error={errors.ownerFullName?.message} label="Administrador">
          <FormInput placeholder="Juan Pérez" {...register('ownerFullName')} />
        </Field>
      </div>
      <Field error={errors.email?.message} label="Correo electronico">
        <FormInput placeholder="admin@colmado.com" type="email" {...register('email')} />
      </Field>
      <Field error={errors.password?.message} label="Contrasena">
        <span className="relative block">
          <FormInput className="pr-12" type={showPassword ? 'text' : 'password'} {...register('password')} />
          <Button
            aria-label={showPassword ? 'Ocultar contrasena' : 'Mostrar contrasena'}
            className="absolute right-1.5 top-1/2 h-7 w-7 -translate-y-1/2 text-gray-400 hover:text-gray-700"
            onClick={onTogglePassword}
            size="icon"
            type="button"
            variant="ghost"
          >
            {showPassword ? <EyeOff size={15} /> : <Eye size={15} />}
          </Button>
        </span>
      </Field>
      <div className="col-span-full">
        <Field label="Telefono secundario">
          <FormInput placeholder="Opcional" {...register('phoneSecondary')} />
        </Field>
      </div>
    </div>
  )
}

// ─── Step 3: Confirmation summary ────────────────────────────────────────────

type Step3Props = {
  errorMessage: string | null
  plans: { id: string; name: string; monthlyPrice: number }[]
  values: Partial<RegisterBusinessFormValues>
}

function Step3({ errorMessage, plans, values }: Readonly<Step3Props>) {
  const plan = plans.find((p) => p.id === values.planId)
  return (
    <div className="space-y-3">
      <SummarySection title="Negocio">
        <SummaryRow label="Plan" value={plan ? `${plan.name} — ${formatMoney(plan.monthlyPrice)}/mes` : '—'} />
        <SummaryRow label="Comercio" value={values.businessName ?? ''} />
        <SummaryRow label="Sucursal" value={values.branchName ?? ''} />
        <SummaryRow
          label="Identificación"
          value={values.identificationNumber ? `${values.identificationType} · ${values.identificationNumber}` : '—'}
        />
        <SummaryRow label="Teléfono" value={values.phonePrimary ?? ''} />
        {values.phoneSecondary && <SummaryRow label="Teléfono 2" value={values.phoneSecondary} />}
      </SummarySection>
      <SummarySection title="Administrador">
        <SummaryRow label="Nombre" value={values.ownerFullName ?? ''} />
        <SummaryRow label="Correo" value={values.email ?? ''} />
        <SummaryRow label="Contrasena" value="••••••••" />
      </SummarySection>
      {errorMessage && (
        <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          {errorMessage}
        </p>
      )}
    </div>
  )
}

function SummarySection({ title, children }: Readonly<{ title: string; children: React.ReactNode }>) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4">
      <p className="mb-2.5 text-[10px] font-semibold uppercase tracking-[0.18em] text-gray-400">
        {title}
      </p>
      <dl className="space-y-2">{children}</dl>
    </div>
  )
}

function SummaryRow({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="flex items-baseline justify-between gap-4 text-sm">
      <dt className="shrink-0 font-medium text-gray-500">{label}</dt>
      <dd className="min-w-0 truncate text-right font-medium text-gray-900">{value}</dd>
    </div>
  )
}

// ─── Reusable primitives ──────────────────────────────────────────────────────

function Field({ children, error, label }: Readonly<{ children: React.ReactNode; error?: string; label: string }>) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-[13px] font-medium text-gray-700">{label}</span>
      {children}
      {error && <span className="mt-1 block text-[11.5px] font-medium text-red-600">{error}</span>}
    </label>
  )
}

const FormInput = ({ className, ...props }: Readonly<React.InputHTMLAttributes<HTMLInputElement>>) => (
  <input
    className={cn(
      'h-10 w-full min-w-0 rounded-lg border border-gray-200 bg-white px-3.5',
      'text-sm font-medium text-gray-900 outline-none transition',
      'placeholder:font-normal placeholder:text-gray-400',
      'hover:border-gray-300',
      'focus:border-primary focus:ring-2 focus:ring-primary/15',
      className,
    )}
    {...props}
  />
)

// ─── Utilities ────────────────────────────────────────────────────────────────

function emptyToNull(value?: string | null): string | null {
  return value && value.trim().length > 0 ? value.trim() : null
}

function formatMoney(value: number): string {
  return new Intl.NumberFormat('es-DO', { currency: 'DOP', maximumFractionDigits: 2, minimumFractionDigits: 2, style: 'currency' }).format(value)
}
