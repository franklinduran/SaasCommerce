import type { PropsWithChildren } from 'react'
import { Navigate } from 'react-router-dom'
import { useCurrentUserPermissions } from '@/shared/hooks/usePermissions'
import type { PermissionCode } from '@/shared/types/permissions'

type PermissionRouteProps = PropsWithChildren<{
  /** One or more permissions — user must hold at least one of them. */
  permissions: PermissionCode | PermissionCode[]
}>

/**
 * Guards a route by checking whether the authenticated user holds at least one
 * of the required permissions.  While permissions are loading the route renders
 * nothing (avoids a flash-of-forbidden).  If the user lacks the permission they
 * are redirected to /forbidden.
 */
export function PermissionRoute({ children, permissions }: Readonly<PermissionRouteProps>) {
  const { data, isLoading } = useCurrentUserPermissions()

  // Don't flash /forbidden while the permissions query is in flight
  if (isLoading && !data) {
    return null
  }

  const required = Array.isArray(permissions) ? permissions : [permissions]
  const hasAccess = data?.permissions.some((p) => required.includes(p as PermissionCode)) ?? false

  if (!hasAccess) {
    return <Navigate replace to="/forbidden" />
  }

  return <>{children}</>
}
