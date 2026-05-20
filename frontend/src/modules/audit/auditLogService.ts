import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'
import type { AuditLogDetail, AuditLogFilters, AuditLogListResponse } from './auditLogTypes'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

function toIso(value: string, endOfDay = false): string | null {
  if (!value) return null
  return endOfDay
    ? new Date(`${value}T23:59:59.999`).toISOString()
    : new Date(`${value}T00:00:00`).toISOString()
}

export const auditLogService = {
  async getAuditLogs(filters: AuditLogFilters): Promise<AuditLogListResponse> {
    const params = new URLSearchParams({
      page: String(filters.page),
      pageSize: String(filters.pageSize),
    })

    const dateFrom = toIso(filters.dateFrom)
    const dateTo = toIso(filters.dateTo, true)
    if (dateFrom) params.set('dateFrom', dateFrom)
    if (dateTo) params.set('dateTo', dateTo)
    if (filters.userId.trim()) params.set('userId', filters.userId.trim())
    if (filters.action.trim()) params.set('action', filters.action.trim())
    if (filters.entityName.trim()) params.set('entityName', filters.entityName.trim())

    const response = await httpClient<AuditLogListResponse>(
      `/api/audit-logs?${params.toString()}`,
      { accessToken: getAccessToken() },
    )

    return response.data!
  },

  async getAuditLogById(id: string): Promise<AuditLogDetail> {
    const response = await httpClient<AuditLogDetail>(`/api/audit-logs/${id}`, {
      accessToken: getAccessToken(),
    })

    return response.data!
  },
}
