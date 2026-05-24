import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'
import type {
  DailyClosingDetail,
  DailyClosingPagedResult,
  DailyClosingPreview,
} from '../types'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const dailyClosingApi = {
  async preview(date: string, branchId: string): Promise<DailyClosingPreview> {
    const query = new URLSearchParams({ date, branchId })
    const response = await httpClient<DailyClosingPreview>(
      `/api/daily-closing/preview?${query.toString()}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },

  async create(params: { date: string; branchId: string; notes?: string }): Promise<DailyClosingDetail> {
    const response = await httpClient<DailyClosingDetail>(
      `/api/daily-closing`,
      { accessToken: getAccessToken(), method: 'POST', body: JSON.stringify(params) },
    )
    return response.data!
  },

  async close(closingId: string, params: { cashCounted: number; notes?: string }): Promise<DailyClosingDetail> {
    const response = await httpClient<DailyClosingDetail>(
      `/api/daily-closing/${closingId}/close`,
      { accessToken: getAccessToken(), method: 'POST', body: JSON.stringify(params) },
    )
    return response.data!
  },

  async getList(params: {
    branchId?: string
    dateFrom?: string
    dateTo?: string
    page?: number
    pageSize?: number
  }): Promise<DailyClosingPagedResult> {
    const query = new URLSearchParams()
    query.set('page', String(params.page ?? 1))
    query.set('pageSize', String(params.pageSize ?? 20))
    if (params.branchId) query.set('branchId', params.branchId)
    if (params.dateFrom) query.set('dateFrom', params.dateFrom)
    if (params.dateTo) query.set('dateTo', params.dateTo)
    const response = await httpClient<DailyClosingPagedResult>(
      `/api/daily-closing?${query.toString()}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },

  async getDetail(closingId: string): Promise<DailyClosingDetail> {
    const response = await httpClient<DailyClosingDetail>(
      `/api/daily-closing/${closingId}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },
}
