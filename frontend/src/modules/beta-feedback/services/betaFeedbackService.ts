import { useAuthStore } from '@/modules/auth/authStore'
import type {
  BetaFeedback,
  BetaFeedbackFilters,
  BetaFeedbackListResponse,
  CreateBetaFeedbackRequest,
  UpdateBetaFeedbackStatusRequest,
} from '@/modules/beta-feedback/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getBetaFeedback(filters: BetaFeedbackFilters): Promise<BetaFeedbackListResponse> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  })

  if (filters.category) {
    params.set('category', filters.category)
  }

  if (filters.status) {
    params.set('status', filters.status)
  }

  const response = await httpClient<BetaFeedbackListResponse>(`/api/beta/feedback?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function createBetaFeedback(request: CreateBetaFeedbackRequest): Promise<BetaFeedback> {
  const response = await httpClient<BetaFeedback>('/api/beta/feedback', {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'POST',
  })

  return response.data!
}

export async function updateBetaFeedbackStatus(
  feedbackId: string,
  request: UpdateBetaFeedbackStatusRequest,
): Promise<BetaFeedback> {
  const response = await httpClient<BetaFeedback>(`/api/beta/feedback/${feedbackId}/status`, {
    accessToken: getAccessToken(),
    body: JSON.stringify(request),
    method: 'PUT',
  })

  return response.data!
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
