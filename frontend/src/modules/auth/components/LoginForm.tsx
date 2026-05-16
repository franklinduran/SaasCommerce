import { zodResolver } from '@hookform/resolvers/zod'
import { CircleDollarSign, Eye, EyeOff } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useLocation, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'
import { useLoginMutation } from '@/modules/auth/hooks/useLoginMutation'

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
    defaultValues: {
      email: '',
      password: '',
    },
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
    <form
      className="w-full max-w-[400px] rounded-xl bg-white p-6 shadow-sm ring-1 ring-stone-200"
      onSubmit={handleSubmit(onSubmit)}
    >
      <div className="mb-8 text-center">
        <span className="mx-auto flex h-12 w-12 items-center justify-center rounded-xl bg-stone-900 text-white">
          <CircleDollarSign aria-hidden="true" size={25} strokeWidth={2.4} />
        </span>
        <h1 className="mt-6 text-[28px] font-semibold leading-tight text-foreground">
          Iniciar sesion
        </h1>
      </div>

      <div className="space-y-5">
        <label className="block">
          <span className="mb-2 block text-sm font-medium text-stone-900">
            Correo electronico
          </span>
          <span className="relative block">
            <input
              aria-label="Correo electronico"
              autoComplete="email"
              className="h-11 w-full rounded-md bg-white px-3 text-sm text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
              placeholder="admin@test.com"
              type="email"
              {...register('email')}
            />
          </span>
          {errors.email && (
            <span className="mt-2 block text-sm font-medium text-danger">{errors.email.message}</span>
          )}
        </label>

        <label className="block">
          <span className="mb-2 block text-sm font-medium text-stone-900">Contrasena</span>
          <span className="relative block">
            <input
              aria-label="Contrasena"
              autoComplete="current-password"
              className="h-11 w-full rounded-md bg-white px-3 pr-12 text-sm text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
              placeholder="Admin123!"
              type={showPassword ? 'text' : 'password'}
              {...register('password')}
            />
            <button
              aria-label={showPassword ? 'Ocultar contrasena' : 'Mostrar contrasena'}
              className="absolute right-2 top-1/2 grid h-8 w-8 -translate-y-1/2 place-items-center rounded-md text-stone-600 transition hover:bg-stone-100 hover:text-stone-900 focus:outline-none focus:ring-2 focus:ring-stone-900"
              onClick={() => setShowPassword((value) => !value)}
              type="button"
            >
              {showPassword ? <EyeOff aria-hidden="true" size={21} /> : <Eye aria-hidden="true" size={21} />}
            </button>
          </span>
          {errors.password && (
            <span className="mt-2 block text-sm font-medium text-danger">
              {errors.password.message}
            </span>
          )}
        </label>
      </div>

      {errorMessage && (
        <div className="mt-6 rounded-md bg-danger/10 px-4 py-3 text-sm font-medium text-danger">
          {errorMessage}
        </div>
      )}

      <Button
        className="mt-6 h-11 w-full"
        disabled={loginMutation.isPending}
        type="submit"
      >
        {loginMutation.isPending ? 'Validando' : 'Iniciar sesion'}
      </Button>

      <button
        className="mx-auto mt-4 block rounded-md px-3 py-2 text-sm font-medium text-stone-600 transition hover:text-stone-900 focus:outline-none focus:ring-2 focus:ring-stone-900"
        type="button"
      >
        Olvidaste tu contrasena?
      </button>

      <div className="mt-8 text-center">
        <p className="text-sm font-medium text-stone-600">No tienes una cuenta?</p>
        <Button
          className="mt-4 h-11 w-full"
          onClick={() => navigate('/register-business')}
          type="button"
          variant="outline"
        >
          Crear cuenta nueva
        </Button>
      </div>
    </form>
  )
}

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
