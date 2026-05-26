import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { logout } from '@/modules/auth/services/authService'

function makeJsonResponse(status: number, ok: boolean, body: unknown) {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => body,
    ok,
    status,
  }
}

describe('authService.logout', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn(async () =>
      makeJsonResponse(200, true, { isSuccess: true, data: null, error: null, correlationId: 'x' }),
    ))
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('calls POST /api/auth/logout with Bearer token and refresh token in body', async () => {
    await logout('access-token', 'refresh-token')

    const fetchMock = vi.mocked(fetch)
    expect(fetchMock).toHaveBeenCalledTimes(1)

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(String(url)).toContain('/api/auth/logout')
    expect(init.method).toBe('POST')

    const headers = new Headers(init.headers as HeadersInit)
    expect(headers.get('Authorization')).toBe('Bearer access-token')

    const body = JSON.parse(init.body as string)
    expect(body.refreshToken).toBe('refresh-token')
  })
})
