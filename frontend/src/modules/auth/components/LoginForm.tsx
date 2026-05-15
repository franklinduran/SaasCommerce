import { zodResolver } from '@hookform/resolvers/zod'
import { LockKeyhole, Mail, ShieldCheck } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'
import { useLoginMutation } from '@/modules/auth/hooks/useLoginMutation'

const loginSchema = z.object({
  email: z.string().email('Correo invalido'),
  password: z.string().min(6, 'La contrasena debe tener al menos 6 caracteres'),
})

type LoginFormValues = z.infer<typeof loginSchema>

export function LoginForm() {
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
    navigate('/', { replace: true })
  }

  return (
    <form
      className="w-full max-w-[420px] rounded-lg border border-border bg-white p-6 shadow-sm"
      onSubmit={handleSubmit(onSubmit)}
    >
      <div className="mb-6 flex items-center gap-3">
        <span className="flex h-11 w-11 items-center justify-center rounded-md bg-primary text-white">
          <ShieldCheck aria-hidden="true" size={22} />
        </span>
        <div>
          <h1 className="text-xl font-semibold text-foreground">Iniciar sesion</h1>
          <p className="text-sm text-slate-500">SaasCommerce RD</p>
        </div>
      </div>

      <div className="space-y-4">
        <label className="block">
          <span className="mb-1 block text-sm font-medium text-slate-700">Correo</span>
          <span className="relative block">
            <Mail
              aria-hidden="true"
              className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              size={18}
            />
            <input
              autoComplete="email"
              className="h-11 w-full rounded-md border border-border bg-white pl-10 pr-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
              placeholder="admin@test.com"
              type="email"
              {...register('email')}
            />
          </span>
          {errors.email && (
            <span className="mt-1 block text-sm text-danger">{errors.email.message}</span>
          )}
        </label>

        <label className="block">
          <span className="mb-1 block text-sm font-medium text-slate-700">Contrasena</span>
          <span className="relative block">
            <LockKeyhole
              aria-hidden="true"
              className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              size={18}
            />
            <input
              autoComplete="current-password"
              className="h-11 w-full rounded-md border border-border bg-white pl-10 pr-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
              placeholder="Admin123!"
              type="password"
              {...register('password')}
            />
          </span>
          {errors.password && (
            <span className="mt-1 block text-sm text-danger">{errors.password.message}</span>
          )}
        </label>
      </div>

      {errorMessage && (
        <div className="mt-4 rounded-md border border-danger/30 bg-danger/10 px-3 py-2 text-sm text-danger">
          {errorMessage}
        </div>
      )}

      <Button
        className="mt-6 w-full"
        disabled={loginMutation.isPending}
        type="submit"
      >
        {loginMutation.isPending ? 'Validando' : 'Entrar'}
      </Button>
    </form>
  )
}
