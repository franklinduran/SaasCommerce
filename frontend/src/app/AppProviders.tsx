import { QueryClientProvider } from '@tanstack/react-query'
import type { PropsWithChildren } from 'react'
import { Toaster } from 'sonner'
import { useCashSessionRealtimeInvalidation } from '@/modules/cash/hooks/useCash'
import { useNotificationsRealtimeInvalidation } from '@/modules/notifications/hooks/useNotifications'
import { useRealtime } from '@/shared/hooks/useRealtime'
import { queryClient } from '@/shared/services/queryClient'

export function AppProviders({ children }: Readonly<PropsWithChildren>) {
  return (
    <QueryClientProvider client={queryClient}>
      <RealtimeSessionBridge />
      {children}
      <Toaster position="top-right" richColors closeButton duration={4000} />
    </QueryClientProvider>
  )
}

function RealtimeSessionBridge() {
  useRealtime()
  useCashSessionRealtimeInvalidation()
  useNotificationsRealtimeInvalidation()

  return null
}
