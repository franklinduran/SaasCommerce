import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'
import type { CurrentUserPermissionsResponse, PermissionCode } from '@/shared/types/permissions'

async function fetchPermissions(accessToken: string): Promise<CurrentUserPermissionsResponse> {
  const response = await httpClient<CurrentUserPermissionsResponse>('/api/me/permissions', {
    accessToken,
  })

  return response.data!
}

export function useCurrentUserPermissions() {
  const session = useAuthStore((state) => state.session)

  return useQuery({
    queryKey: ['me', 'permissions'],
    queryFn: () => fetchPermissions(session!.accessToken),
    enabled: !!session?.accessToken,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  })
}

export function useHasPermission(permission: PermissionCode): boolean {
  const { data } = useCurrentUserPermissions()

  return data?.permissions.includes(permission) ?? false
}

export function useHasAnyPermission(permissions: PermissionCode[]): boolean {
  const { data } = useCurrentUserPermissions()

  if (!data) return false

  return permissions.some((p) => data.permissions.includes(p))
}
