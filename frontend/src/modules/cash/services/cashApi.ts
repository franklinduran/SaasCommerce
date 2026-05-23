import { useAuthStore } from '@/modules/auth/authStore'
import type {
  CashClosingResult,
  CashMovement,
  CashSession,
  CashSessionsListResult,
  CloseCashSessionRequest,
  OpenCashSessionRequest,
  RegisterCashMovementRequest,
} from '@/modules/cash/types'
import { httpClient } from '@/shared/services/httpClient'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const cashApi = {
  async openSession(request: OpenCashSessionRequest): Promise<CashSession> {
    const response = await httpClient<CashSession>('/api/cash-sessions', {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify(request),
    })
    return response.data!
  },

  async getCurrentSession(): Promise<CashSession | null> {
    try {
      const response = await httpClient<CashSession | null>('/api/cash-sessions/current', {
        accessToken: getAccessToken(),
      })
      return response.data ?? null
    } catch {
      return null
    }
  },

  async getSessionById(id: string): Promise<CashSession> {
    const response = await httpClient<CashSession>(`/api/cash-sessions/${id}`, {
      accessToken: getAccessToken(),
    })
    return response.data!
  },

  async listSessions(params?: {
    branchId?: string
    status?: string
    dateFrom?: string
    dateTo?: string
    page?: number
    pageSize?: number
  }): Promise<CashSessionsListResult> {
    const query = new URLSearchParams()
    if (params?.branchId) query.set('branchId', params.branchId)
    if (params?.status) query.set('status', params.status)
    if (params?.dateFrom) query.set('dateFrom', params.dateFrom)
    if (params?.dateTo) query.set('dateTo', params.dateTo)
    if (params?.page) query.set('page', String(params.page))
    if (params?.pageSize) query.set('pageSize', String(params.pageSize))

    const qs = query.toString()
    const response = await httpClient<CashSessionsListResult>(
      `/api/cash-sessions${qs ? `?${qs}` : ''}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },

  async closeSession(id: string, request: CloseCashSessionRequest): Promise<CashClosingResult> {
    const response = await httpClient<CashClosingResult>(`/api/cash-sessions/${id}/close`, {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify(request),
    })
    return response.data!
  },

  async registerMovement(id: string, request: RegisterCashMovementRequest): Promise<CashMovement> {
    const response = await httpClient<CashMovement>(`/api/cash-sessions/${id}/movements`, {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify(request),
    })
    return response.data!
  },
}
