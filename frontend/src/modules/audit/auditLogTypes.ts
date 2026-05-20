export interface AuditLogItem {
  auditLogId: string
  userId: string | null
  userFullName: string | null
  action: string
  entityName: string
  entityId: string | null
  description: string | null
  ipAddress: string | null
  createdAt: string
}

export interface AuditLogDetail extends AuditLogItem {
  userAgent: string | null
  correlationId: string | null
  metadataJson: string | null
}

export interface AuditLogListResponse {
  items: AuditLogItem[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface AuditLogFilters {
  dateFrom: string
  dateTo: string
  userId: string
  action: string
  entityName: string
  page: number
  pageSize: number
}
