export type BetaFeedbackCategory =
  | 'Bug'
  | 'Improvement'
  | 'Question'
  | 'DataError'
  | 'SaleIssue'
  | 'InventoryIssue'
  | 'PurchaseIssue'
  | 'CashIssue'

export type BetaFeedbackStatus = 'New' | 'InReview' | 'Accepted' | 'Rejected' | 'Resolved'

export type BetaFeedback = {
  id: string
  businessId: string
  userId: string
  category: BetaFeedbackCategory
  status: BetaFeedbackStatus
  title: string
  description: string
  contextUrl?: string | null
  reviewNote?: string | null
  createdAt: string
  updatedAt: string
  reviewedAt?: string | null
  reviewedByUserId?: string | null
}

export type BetaFeedbackListResponse = {
  items: BetaFeedback[]
  totalItems: number
  page: number
  pageSize: number
  totalPages: number
}

export type BetaFeedbackFilters = {
  category: '' | BetaFeedbackCategory
  page: number
  pageSize: number
  status: '' | BetaFeedbackStatus
}

export type CreateBetaFeedbackRequest = {
  category: BetaFeedbackCategory
  title: string
  description: string
  contextUrl?: string
}

export type UpdateBetaFeedbackStatusRequest = {
  status: BetaFeedbackStatus
  reviewNote?: string
}

export type BetaFeedbackRealtimeNotification = {
  businessId: string
  feedbackId?: string
}
