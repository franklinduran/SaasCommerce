import { beforeEach, describe, expect, it, vi } from 'vitest'
import { cashApi } from '@/modules/cash/services/cashApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'cash-token' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('cashApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'cash-result' } })
  })

  it('sends cash session commands with authenticated payloads', async () => {
    await cashApi.openSession({ openingBalance: 100, notes: 'start' })
    await cashApi.closeSession('session-1', { countedCash: 125, notes: 'done' })
    await cashApi.registerMovement('session-1', { amount: 10, description: 'petty cash', movementType: 'Out' })

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/cash-sessions', expect.objectContaining({
      accessToken: 'cash-token',
      method: 'POST',
    }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/cash-sessions/session-1/close', expect.objectContaining({
      accessToken: 'cash-token',
      method: 'POST',
    }))
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/cash-sessions/session-1/movements', expect.objectContaining({
      accessToken: 'cash-token',
      method: 'POST',
    }))
    expect(JSON.parse(vi.mocked(httpClient).mock.calls[0][1]!.body as string)).toEqual({ openingBalance: 100, notes: 'start' })
  })

  it('loads current, detail and filtered cash sessions', async () => {
    await cashApi.getCurrentSession()
    await cashApi.getSessionById('session-1')
    await cashApi.listSessions({
      branchId: 'branch-1',
      dateFrom: '2026-05-01',
      dateTo: '2026-05-25',
      page: 2,
      pageSize: 25,
      status: 'Open',
    })
    await cashApi.listSessions()

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/cash-sessions/current', { accessToken: 'cash-token' })
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/cash-sessions/session-1', { accessToken: 'cash-token' })
    expect(httpClient).toHaveBeenNthCalledWith(
      3,
      '/api/cash-sessions?branchId=branch-1&status=Open&dateFrom=2026-05-01&dateTo=2026-05-25&page=2&pageSize=25',
      { accessToken: 'cash-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/cash-sessions', { accessToken: 'cash-token' })
  })

  it('returns null when current cash session cannot be loaded', async () => {
    vi.mocked(httpClient).mockRejectedValueOnce(new Error('not found'))

    await expect(cashApi.getCurrentSession()).resolves.toBeNull()
  })
})
