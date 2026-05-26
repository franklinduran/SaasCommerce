import { beforeEach, describe, expect, it, vi } from 'vitest'
import { auditLogService } from '@/modules/audit/auditLogService'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'audit-token' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('auditLogService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'audit-1', items: [] } })
  })

  it('builds audit log query parameters and trims optional filters', async () => {
    const result = await auditLogService.getAuditLogs({
      action: ' Updated ',
      dateFrom: '2026-05-01',
      dateTo: '2026-05-25',
      entityName: ' Invoice ',
      page: 2,
      pageSize: 25,
      userId: ' user-1 ',
    })

    const [url, options] = vi.mocked(httpClient).mock.calls[0]
    const parsed = new URL(url, 'http://local.test')

    expect(result).toEqual({ id: 'audit-1', items: [] })
    expect(parsed.pathname).toBe('/api/audit-logs')
    expect(parsed.searchParams.get('page')).toBe('2')
    expect(parsed.searchParams.get('pageSize')).toBe('25')
    expect(parsed.searchParams.get('userId')).toBe('user-1')
    expect(parsed.searchParams.get('action')).toBe('Updated')
    expect(parsed.searchParams.get('entityName')).toBe('Invoice')
    expect(parsed.searchParams.get('dateFrom')).toBe(new Date('2026-05-01T00:00:00').toISOString())
    expect(parsed.searchParams.get('dateTo')).toBe(new Date('2026-05-25T23:59:59.999').toISOString())
    expect(options).toEqual({ accessToken: 'audit-token' })
  })

  it('omits blank optional filters and fetches details by id', async () => {
    await auditLogService.getAuditLogs({
      action: ' ',
      dateFrom: '',
      dateTo: '',
      entityName: '',
      page: 1,
      pageSize: 10,
      userId: '',
    })
    await auditLogService.getAuditLogById('audit-2')

    expect(httpClient).toHaveBeenNthCalledWith(
      1,
      '/api/audit-logs?page=1&pageSize=10',
      { accessToken: 'audit-token' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(
      2,
      '/api/audit-logs/audit-2',
      { accessToken: 'audit-token' },
    )
  })
})
