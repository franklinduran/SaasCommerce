import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  createBetaFeedback,
  getBetaFeedback,
  updateBetaFeedbackStatus,
} from '@/modules/beta-feedback/services/betaFeedbackService'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('betaFeedbackService', () => {
  afterEach(() => {
    useAuthStore.getState().clearSession()
    vi.clearAllMocks()
  })

  it('gets feedback with filters and auth token', async () => {
    setSession()
    vi.mocked(httpClient).mockResolvedValueOnce({
      data: { items: [], page: 1, pageSize: 20, totalItems: 0, totalPages: 0 },
      error: null,
      isSuccess: true,
    })

    await getBetaFeedback({ category: 'Bug', page: 1, pageSize: 20, status: 'New' })

    expect(httpClient).toHaveBeenCalledWith('/api/beta/feedback?page=1&pageSize=20&category=Bug&status=New', {
      accessToken: 'feedback-token',
    })
  })

  it('creates feedback and updates status', async () => {
    setSession()
    vi.mocked(httpClient).mockResolvedValue({
      data: { id: 'feedback-1' },
      error: null,
      isSuccess: true,
    })

    await createBetaFeedback({
      category: 'SaleIssue',
      description: 'La venta queda procesando.',
      title: 'Venta procesando',
    })
    await updateBetaFeedbackStatus('feedback-1', { reviewNote: 'Listo', status: 'Resolved' })

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/beta/feedback', expect.objectContaining({
      accessToken: 'feedback-token',
      method: 'POST',
    }))
    expect(JSON.parse(vi.mocked(httpClient).mock.calls[0][1]!.body as string)).toEqual({
      category: 'SaleIssue',
      description: 'La venta queda procesando.',
      title: 'Venta procesando',
    })
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/beta/feedback/feedback-1/status', expect.objectContaining({
      accessToken: 'feedback-token',
      method: 'PUT',
    }))
  })
})

function setSession() {
  useAuthStore.getState().setSession({
    accessToken: 'feedback-token',
    expiresAt: '2027-01-01T00:00:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: 'branch-1',
      businessId: 'business-1',
      email: 'admin@test.com',
      fullName: 'Admin',
      id: 'user-1',
      roles: ['Admin'],
    },
  })
}
