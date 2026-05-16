import { useMutation } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import { registerBusiness } from '@/modules/account/services/accountService'
import type { RegisterBusinessRequest } from '@/modules/account/types'

export function useRegisterBusinessMutation() {
  const setSession = useAuthStore((state) => state.setSession)

  return useMutation({
    mutationFn: (request: RegisterBusinessRequest) => registerBusiness(request),
    onSuccess: (response) => {
      setSession({
        accessToken: response.accessToken,
        expiresAt: response.expiresAt,
        refreshToken: response.refreshToken,
        user: response.user,
      })
    },
  })
}
