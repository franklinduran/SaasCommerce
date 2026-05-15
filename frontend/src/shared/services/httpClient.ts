import type { ApiError, ApiResponse } from '@/shared/types/api'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'
const normalizedApiBaseUrl = apiBaseUrl.replace(/\/$/, '')

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

  const response = await fetch(buildUrl(path), {
    ...options,
    headers,
  })

  const payload = await readPayload<T>(response)

  if (payload === null || !response.ok || !payload.isSuccess) {
    const message =
      payload === null ? 'Request failed' : payload.error?.message ?? 'Request failed'
    const error = payload === null ? null : payload.error

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

function buildUrl(path: string): string {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`

  if (normalizedApiBaseUrl.endsWith('/api') && normalizedPath.startsWith('/api/')) {
    return `${normalizedApiBaseUrl}${normalizedPath.slice(4)}`
  }

  return `${normalizedApiBaseUrl}${normalizedPath}`
}
