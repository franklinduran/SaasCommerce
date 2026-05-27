import { useAuthStore } from '@/modules/auth/authStore'
import { buildApiUrl } from '@/shared/services/apiConfig'
import type { ApiError, ApiResponse } from '@/shared/types/api'

/** Paths that should NOT trigger an automatic session-clear on 401.
 *  A wrong password on the login form returns 401 by design. */
const AUTH_EXEMPT_PATHS = ['/api/auth/login']

type RequestOptions = RequestInit & {
  accessToken?: string
}

export class HttpClientError extends Error {
  public readonly status: number
  public readonly error: ApiError | null

  constructor(message: string, status: number, error: ApiError | null) {
    super(message)
    this.name = 'HttpClientError'
    this.status = status
    this.error = error
  }
}

export async function httpClient<T>(
  path: string,
  options: RequestOptions = {},
): Promise<ApiResponse<T>> {
  const headers = new Headers(options.headers)

  if (!headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  if (options.accessToken) {
    headers.set('Authorization', `Bearer ${options.accessToken}`)
  }

  const response = await fetch(buildApiUrl(path), {
    ...options,
    headers,
  })

  const payload = await readPayload<T>(response)

  if (payload === null || !response.ok || !payload.isSuccess) {
    const message =
      payload === null ? 'Request failed' : payload.error?.message ?? 'Request failed'
    const error = payload === null ? null : payload.error

    // Expired / invalid access token on any non-login endpoint → force re-login
    if (
      response.status === 401 &&
      !AUTH_EXEMPT_PATHS.some((p) => path.startsWith(p))
    ) {
      useAuthStore.getState().clearSession()
      window.location.replace('/login')
    }

    throw new HttpClientError(message, response.status, error)
  }

  return payload
}

async function readPayload<T>(response: Response): Promise<ApiResponse<T> | null> {
  const contentType = response.headers.get('content-type') ?? ''

  if (!contentType.includes('application/json')) {
    return null
  }

  return (await response.json()) as ApiResponse<T>
}
