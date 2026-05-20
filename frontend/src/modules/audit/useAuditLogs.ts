import { useQuery } from '@tanstack/react-query'
import { auditLogService } from './auditLogService'
import type { AuditLogFilters } from './auditLogTypes'

export const auditLogKeys = {
  all: ['audit-logs'] as const,
  list: (filters: AuditLogFilters) => ['audit-logs', 'list', filters] as const,
  detail: (id: string) => ['audit-logs', 'detail', id] as const,
}

export function useAuditLogs(filters: AuditLogFilters) {
  return useQuery({
    queryFn: () => auditLogService.getAuditLogs(filters),
    queryKey: auditLogKeys.list(filters),
  })
}

export function useAuditLogDetail(id: string | null) {
  return useQuery({
    enabled: Boolean(id),
    queryFn: () => auditLogService.getAuditLogById(id!),
    queryKey: id ? auditLogKeys.detail(id) : auditLogKeys.all,
  })
}
