import type { ReactNode } from 'react'
import { useCurrentUserPermissions } from '@/shared/hooks/usePermissions'
import type { PermissionCode } from '@/shared/types/permissions'

type PermissionGateProps = {
  /** Required permission code(s). If an array, the user needs at least ONE. */
  permission: PermissionCode | PermissionCode[]
  /** Rendered when the user has the required permission. */
  children: ReactNode
  /** Optional fallback rendered when the user lacks the permission. */
  fallback?: ReactNode
}

export function PermissionGate({ permission, children, fallback = null }: PermissionGateProps) {
  const { data, isLoading } = useCurrentUserPermissions()

  if (isLoading || !data) {
    return null
  }

  const permissions = Array.isArray(permission) ? permission : [permission]
  const hasPermission = permissions.some((p) => data.permissions.includes(p))

  if (!hasPermission) {
    return <>{fallback}</>
  }

  return <>{children}</>
}
