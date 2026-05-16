import type { PropsWithChildren } from 'react'
import { useEffect } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/modules/auth/authStore'

export function ProtectedRoute({ children }: Readonly<PropsWithChildren>) {
  const session = useAuthStore((state) => state.session)
  const clearSession = useAuthStore((state) => state.clearSession)
  const location = useLocation()
  const isExpired = isSessionExpired(session?.expiresAt)

  useEffect(() => {
    if (session && isExpired) {
      clearSession()
    }
  }, [clearSession, isExpired, session])

  if (!session || isExpired) {
    return <Navigate replace state={{ from: location }} to="/login" />
  }

  return <>{children}</>
}

function isSessionExpired(expiresAt: string | undefined): boolean {
  if (!expiresAt) {
    return true
  }

  const expiresAtTime = Date.parse(expiresAt)

  return Number.isNaN(expiresAtTime) || expiresAtTime <= Date.now()
}
