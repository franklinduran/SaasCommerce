import { create } from 'zustand'

type AppState = {
  businessName: string
  sidebarCollapsed: boolean
  setBusinessName: (businessName: string) => void
  toggleSidebar: () => void
}

export const useAppStore = create<AppState>((set) => ({
  businessName: 'SaasCommerce RD',
  sidebarCollapsed: false,
  setBusinessName: (businessName) => set({ businessName }),
  toggleSidebar: () =>
    set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
}))
