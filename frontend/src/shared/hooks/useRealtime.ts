import { useEffect } from 'react'
import { useAuthStore } from '@/modules/auth/authStore'
import { startRealtimeConnection, stopRealtimeConnection } from '@/shared/services/signalrClient'

export function useRealtime() {
  const accessToken = useAuthStore((state) => state.session?.accessToken)

  useEffect(() => {
    if (!accessToken) {
      stopRealtimeConnection()
      return undefined
    }

    startRealtimeConnection(accessToken).catch(() => undefined)

    return () => {
      stopRealtimeConnection()
    }
  }, [accessToken])
}
