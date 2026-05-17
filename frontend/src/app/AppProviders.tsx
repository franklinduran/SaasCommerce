import { QueryClientProvider } from '@tanstack/react-query'
import type { PropsWithChildren } from 'react'
import { useRealtime } from '@/shared/hooks/useRealtime'
import { queryClient } from '@/shared/services/queryClient'

export function AppProviders({ children }: Readonly<PropsWithChildren>) {
  return (
    <QueryClientProvider client={queryClient}>
      <RealtimeSessionBridge />
      {children}
    </QueryClientProvider>
  )
}

function RealtimeSessionBridge() {
  useRealtime()

  return null
}
