import type { PropsWithChildren } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'

export function ProtectedRoute({ children }: PropsWithChildren) {
  const session = useAuthStore((state) => state.session)
  const location = useLocation()

  if (!session) {
    return <Navigate replace state={{ from: location }} to="/auth" />
  }

  return <>{children}</>
}
