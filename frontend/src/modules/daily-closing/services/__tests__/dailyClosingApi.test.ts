import { beforeEach, describe, expect, it, vi } from 'vitest'
import { dailyClosingApi } from '@/modules/daily-closing/services/dailyClosingApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'closing-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('dailyClosingApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'closing-result' } })
  })

  it('loads previews, lists and details', async () => {
    await dailyClosingApi.preview('2026-05-25', 'branch-1')
    await dailyClosingApi.getList({ branchId: 'branch-1', dateFrom: '2026-05-01', dateTo: '2026-05-25', page: 2, pageSize: 50 })
    await dailyClosingApi.getList({})
    await dailyClosingApi.getDetail('closing-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/daily-closing/preview?date=2026-05-25&branchId=branch-1', { accessToken: 'closing-token' })
    expect(httpClient).toHaveBeenNthCalledWith(
      2,
      '/api/daily-closing?page=2&pageSize=50&branchId=branch-1&dateFrom=2026-05-01&dateTo=2026-05-25',
      { accessToken: 'closing-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/daily-closing?page=1&pageSize=20', { accessToken: 'closing-token' })
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/daily-closing/closing-1', { accessToken: 'closing-token' })
  })

  it('creates and closes daily closings', async () => {
    await dailyClosingApi.create({ branchId: 'branch-1', date: '2026-05-25', notes: 'ok' })
    await dailyClosingApi.close('closing-1', { cashCounted: 100, notes: 'done' })

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/daily-closing', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/daily-closing/closing-1/close', expect.objectContaining({ method: 'POST' }))
  })
})
