import type { ApiError, ApiResponse } from '@/shared/types/api'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

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

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    headers,
  })

  const payload = (await response.json()) as ApiResponse<T>

  if (!response.ok || !payload.isSuccess) {
    const message = payload.error?.message ?? 'Request failed'
    throw new HttpClientError(message, response.status, payload.error)
  }

  return payload
}
