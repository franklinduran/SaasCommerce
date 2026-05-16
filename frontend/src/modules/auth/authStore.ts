import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'
import type { LoginResponse } from '@/modules/auth/types'

type AuthState = {
  session: LoginResponse | null
  setSession: (session: LoginResponse) => void
  clearSession: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      session: null,
      setSession: (session) => set({ session }),
      clearSession: () => set({ session: null }),
    }),
    {
      name: 'saascommerce-auth',
      storage: createJSONStorage(() => localStorage),
    },
  ),
)
