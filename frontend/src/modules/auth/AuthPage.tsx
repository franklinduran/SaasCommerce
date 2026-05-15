import { Navigate } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'
import { LoginForm } from '@/modules/auth/components/LoginForm'

export function AuthPage() {
  const session = useAuthStore((state) => state.session)

  if (session) {
    return <Navigate replace to="/" />
  }

  return (
    <main className="grid min-h-screen place-items-center bg-background px-5 py-10">
      <LoginForm />
    </main>
  )
}
