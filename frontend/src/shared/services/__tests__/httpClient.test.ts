import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { HttpClientError, httpClient } from '@/shared/services/httpClient'

const SESSION = {
  accessToken: 'jwt',
  expiresAt: '2099-01-01T00:00:00Z',
  refreshToken: 'refresh',
  user: {
    branchId: '22222222-2222-2222-2222-222222222222',
    businessId: '11111111-1111-1111-1111-111111111111',
    email: 'owner@test.com',
    fullName: 'Owner',
    id: '33333333-3333-3333-3333-333333333333',
    roles: ['Owner'],
  },
}

function makeJsonResponse(status: number, ok: boolean, body: unknown) {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => body,
    ok,
    status,
  }
}

describe('httpClient', () => {
  let replaceSpy: ReturnType<typeof vi.fn>

  beforeEach(() => {
    replaceSpy = vi.fn()
    vi.stubGlobal('location', { replace: replaceSpy })
    useAuthStore.getState().setSession(SESSION)
  })

  afterEach(() => {
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('resolves successfully for a 200 response', async () => {
    vi.stubGlobal('fetch', vi.fn(async () =>
      makeJsonResponse(200, true, { isSuccess: true, data: { value: 42 }, error: null, correlationId: 'x' }),
    ))

    const result = await httpClient<{ value: number }>('/api/test')
    expect(result.data?.value).toBe(42)
  })

  it('throws HttpClientError for non-2xx responses', async () => {
    vi.stubGlobal('fetch', vi.fn(async () =>
      makeJsonResponse(400, false, {
        isSuccess: false,
        data: null,
        error: { code: 'VALIDATION_ERROR', message: 'Bad input' },
        correlationId: 'x',
      }),
    ))

    await expect(httpClient('/api/test')).rejects.toThrow(HttpClientError)
  })

  it('clears session and redirects to /login on 401 for protected endpoints', async () => {
    vi.stubGlobal('fetch', vi.fn(async () =>
      makeJsonResponse(401, false, {
        isSuccess: false,
        data: null,
        error: { code: 'UNAUTHORIZED', message: 'Authentication is required.' },
        correlationId: 'x',
      }),
    ))

    await expect(httpClient('/api/products')).rejects.toThrow(HttpClientError)

    expect(useAuthStore.getState().session).toBeNull()
    expect(replaceSpy).toHaveBeenCalledWith('/login')
  })

  it('does NOT clear session on 401 for /api/auth/login (wrong password)', async () => {
    vi.stubGlobal('fetch', vi.fn(async () =>
      makeJsonResponse(401, false, {
        isSuccess: false,
        data: null,
        error: { code: 'UNAUTHORIZED', message: 'Invalid credentials.' },
        correlationId: 'x',
      }),
    ))

    await expect(httpClient('/api/auth/login', { method: 'POST', body: '{}' })).rejects.toThrow(HttpClientError)

    // Session should still be set — login 401 means wrong credentials, not expired token
    expect(useAuthStore.getState().session).not.toBeNull()
    expect(replaceSpy).not.toHaveBeenCalled()
  })

  it('sends Authorization header when accessToken is provided', async () => {
    let capturedHeaders: Headers | undefined

    vi.stubGlobal('fetch', vi.fn(async (_url: string, init: RequestInit) => {
      capturedHeaders = new Headers(init.headers as HeadersInit)
      return makeJsonResponse(200, true, { isSuccess: true, data: null, error: null, correlationId: 'x' })
    }))

    await httpClient('/api/me', { accessToken: 'my-token' })

    expect(capturedHeaders?.get('Authorization')).toBe('Bearer my-token')
  })
})
