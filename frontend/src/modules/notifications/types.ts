export type NotificationType =
  | 'LowStock'
  | 'CashDifference'
  | 'SaleFailed'
  | 'InvoiceFailed'
  | 'HighExpense'
  | 'NegativeProfit'
  | 'ProductMissingCost'
  | 'DailyClosingPending'
  | 'CustomerDebtOverdue'

export type NotificationSeverity = 'Info' | 'Warning' | 'Critical'

export type NotificationStatus = 'Unread' | 'Read'

export interface NotificationItem {
  id: string
  type: NotificationType
  severity: NotificationSeverity
  status: NotificationStatus
  title: string
  message: string
  branchId: string | null
  relatedEntityId: string | null
  relatedEntityType: string | null
  createdAt: string
  readAt: string | null
}

export interface NotificationListResponse {
  items: NotificationItem[]
  totalCount: number
  page: number
  pageSize: number
  unreadCount: number
}

export interface UnreadCountResponse {
  unreadCount: number
}
