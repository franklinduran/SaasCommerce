import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'
import type {
  CashRegisterDetail,
  CloseCashRegisterRequest,
  CloseCashRegisterResponse,
  DailyCashRegisterSummary,
  OpenCashRegisterRequest,
  OpenCashRegisterResponse,
  RegisterMovementRequest,
} from '../types'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const cashRegisterApi = {
  async open(request: OpenCashRegisterRequest): Promise<OpenCashRegisterResponse> {
    const response = await httpClient<OpenCashRegisterResponse>('/api/cash-registers/open', {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify(request),
    })
    return response.data!
  },

  async getActive(): Promise<CashRegisterDetail | null> {
    try {
      const response = await httpClient<CashRegisterDetail | null>('/api/cash-registers/active', {
        accessToken: getAccessToken(),
      })
      return response.data ?? null
    } catch {
      return null
    }
  },

  async registerMovement(id: string, request: RegisterMovementRequest): Promise<void> {
    await httpClient(`/api/cash-registers/${id}/movements`, {
      accessToken: getAccessToken(),
      method: 'POST',
      body: JSON.stringify({ type: request.type, amount: request.amount, reason: request.reason }),
    })
  },

  async close(id: string, request: CloseCashRegisterRequest): Promise<CloseCashRegisterResponse> {
    const response = await httpClient<CloseCashRegisterResponse>(
      `/api/cash-registers/${id}/close`,
      {
        accessToken: getAccessToken(),
        method: 'POST',
        body: JSON.stringify(request),
      },
    )
    return response.data!
  },

  async getDailySummary(params: {
    date?: string
    branchId?: string
  }): Promise<DailyCashRegisterSummary> {
    const query = new URLSearchParams()
    if (params.date) query.set('date', params.date)
    if (params.branchId) query.set('branchId', params.branchId)
    const response = await httpClient<DailyCashRegisterSummary>(
      `/api/cash-registers/daily-summary?${query.toString()}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },
}
