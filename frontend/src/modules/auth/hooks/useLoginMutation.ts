import { useMutation } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import { login } from '@/modules/auth/services/authService'
import type { LoginRequest } from '@/modules/auth/types'

export function useLoginMutation() {
  const setSession = useAuthStore((state) => state.setSession)

  return useMutation({
    mutationFn: (request: LoginRequest) => login(request),
    onSuccess: (session) => setSession(session),
  })
}
