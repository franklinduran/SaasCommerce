import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight, Eye, EyeOff, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useLocation, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'
import { useLoginMutation } from '@/modules/auth/hooks/useLoginMutation'
import { AuthSplitLayout } from '@/modules/account/components/AuthSplitLayout'
import { cn } from '@/shared/utils/cn'

const loginSchema = z.object({
  email: z.email('Correo invalido'),
  password: z.string().min(6, 'La contrasena debe tener al menos 6 caracteres'),
})

type LoginFormValues = z.infer<typeof loginSchema>

export function LoginForm() {
  const [showPassword, setShowPassword] = useState(false)
  const location = useLocation()
  const navigate = useNavigate()
  const loginMutation = useLoginMutation()
  const {
    formState: { errors },
    handleSubmit,
    register,
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  const errorMessage =
    loginMutation.error instanceof HttpClientError
      ? loginMutation.error.message
      : null

  async function onSubmit(values: LoginFormValues) {
    await loginMutation.mutateAsync(values)
    navigate(getRedirectPath(location.state), { replace: true })
  }

  return (
    <AuthSplitLayout>
      {/* Top bar — secondary action: go to register */}
      <header className="flex shrink-0 items-center justify-between px-8 py-5 lg:px-12">
        <span className="text-[13px] text-gray-400">¿No tienes una cuenta?</span>
        <Button
          className="font-medium"
          onClick={() => navigate('/register-business')}
          size="sm"
          variant="outline"
        >
          Crear cuenta
        </Button>
      </header>

      {/* Content — vertically centered */}
      <div className="flex flex-1 flex-col items-center justify-center px-8 py-2 lg:px-12">
        <div className="w-full max-w-[420px]">

          {/* Heading */}
          <div className="mb-7">
            <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-primary">
              Bienvenido
            </p>
            <h1 className="mt-2 text-[2rem] font-bold leading-[1.1] tracking-tight text-gray-900">
              Iniciar sesion
            </h1>
            <p className="mt-1.5 text-[14px] leading-relaxed text-gray-500">
              Accede a tu comercio y gestiona el negocio desde cualquier lugar.
            </p>
          </div>

          <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>

            {/* Email */}
            <label className="block">
              <span className="mb-1.5 block text-[13px] font-medium text-gray-700">
                Correo electronico
              </span>
              <input
                aria-label="Correo electronico"
                autoComplete="email"
                className={inputClass}
                placeholder="correo@negocio.com"
                type="email"
                {...register('email')}
              />
              {errors.email && (
                <span className="mt-1 block text-[11.5px] font-medium text-red-600">
                  {errors.email.message}
                </span>
              )}
            </label>

            {/* Password */}
            <label className="block">
              <div className="mb-1.5 flex items-baseline justify-between">
                <span className="text-[13px] font-medium text-gray-700">Contrasena</span>
                <button
                  className="text-[12px] font-medium text-primary hover:underline"
                  type="button"
                >
                  ¿Olvidaste tu contrasena?
                </button>
              </div>
              <span className="relative block">
                <input
                  aria-label="Contrasena"
                  autoComplete="current-password"
                  className={cn(inputClass, 'pr-12')}
                  placeholder="Ingresa tu contrasena"
                  type={showPassword ? 'text' : 'password'}
                  {...register('password')}
                />
                <Button
                  aria-label={showPassword ? 'Ocultar contrasena' : 'Mostrar contrasena'}
                  className="absolute right-1.5 top-1/2 h-7 w-7 -translate-y-1/2 text-gray-400 hover:text-gray-700"
                  onClick={() => setShowPassword((value) => !value)}
                  size="icon"
                  type="button"
                  variant="ghost"
                >
                  {showPassword ? <EyeOff size={15} /> : <Eye size={15} />}
                </Button>
              </span>
              {errors.password && (
                <span className="mt-1 block text-[11.5px] font-medium text-red-600">
                  {errors.password.message}
                </span>
              )}
            </label>

            {/* Server error */}
            {errorMessage && (
              <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
                {errorMessage}
              </p>
            )}

            {/* Submit — integrated, full-width, sits inside the form */}
            <Button
              className="h-11 w-full gap-2 text-[14px] font-semibold shadow-sm shadow-primary/20"
              disabled={loginMutation.isPending}
              type="submit"
            >
              {loginMutation.isPending ? (
                <>
                  <Loader2 className="animate-spin" size={16} /> Validando...
                </>
              ) : (
                <>
                  Iniciar sesion <ArrowRight size={16} />
                </>
              )}
            </Button>
          </form>

          {/* Divider + secondary action */}
          <div className="mt-7 flex items-center gap-3 text-[12px] text-gray-300">
            <span className="h-px flex-1 bg-gray-200" />
            <span>o</span>
            <span className="h-px flex-1 bg-gray-200" />
          </div>

          <Button
            className="mt-4 h-11 w-full font-medium"
            onClick={() => navigate('/register-business')}
            type="button"
            variant="outline"
          >
            Crear cuenta nueva
          </Button>

        </div>
      </div>
    </AuthSplitLayout>
  )
}

const inputClass =
  'h-10 w-full min-w-0 rounded-lg border border-gray-200 bg-white px-3.5 ' +
  'text-sm font-medium text-gray-900 outline-none transition ' +
  'placeholder:font-normal placeholder:text-gray-400 ' +
  'hover:border-gray-300 ' +
  'focus:border-primary focus:ring-2 focus:ring-primary/15'

function getRedirectPath(state: unknown): string {
  if (!state || typeof state !== 'object' || !('from' in state)) {
    return '/'
  }

  const from = (state as { from?: { pathname?: unknown; search?: unknown } }).from
  const pathname = from?.pathname

  if (typeof pathname !== 'string' || !pathname.startsWith('/')) {
    return '/'
  }

  const search = typeof from?.search === 'string' ? from.search : ''

  return `${pathname}${search}`
}
