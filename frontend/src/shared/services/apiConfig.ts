const LOCAL_API_BASE_URL = 'http://localhost:5000'
const DEFAULT_HUB_PATH = '/hubs/realtime'

type RuntimeEnv = {
  DEV?: boolean
  VITE_API_BASE_URL?: string
  VITE_SIGNALR_HUB_URL?: string
}

export function resolveApiBaseUrl(): string {
  const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim()

  if (configuredBaseUrl) {
    return normalizeBaseUrl(configuredBaseUrl)
  }

  return import.meta.env.DEV ? getLocalApiBaseUrl() : ''
}

export function resolveSignalRHubUrl(): string {
  const configuredHubUrl = import.meta.env.VITE_SIGNALR_HUB_URL?.trim()

  if (configuredHubUrl) {
    return normalizeBaseUrl(configuredHubUrl)
  }

  const apiBaseUrl = resolveApiBaseUrl()
  if (!apiBaseUrl) return DEFAULT_HUB_PATH

  const hubBaseUrl = apiBaseUrl.endsWith('/api')
    ? apiBaseUrl.slice(0, -4)
    : apiBaseUrl

  return `${hubBaseUrl}${DEFAULT_HUB_PATH}`
}

export function buildApiUrl(path: string): string {
  return buildApiUrlWithBase(path, resolveApiBaseUrl())
}

export function resolveApiBaseUrlForEnv(env: RuntimeEnv): string {
  const configuredBaseUrl = env.VITE_API_BASE_URL?.trim()

  if (configuredBaseUrl) {
    return normalizeBaseUrl(configuredBaseUrl)
  }

  return env.DEV ? getLocalApiBaseUrl() : ''
}

export function resolveSignalRHubUrlForEnv(env: RuntimeEnv): string {
  const configuredHubUrl = env.VITE_SIGNALR_HUB_URL?.trim()

  if (configuredHubUrl) {
    return normalizeBaseUrl(configuredHubUrl)
  }

  const apiBaseUrl = resolveApiBaseUrlForEnv(env)
  if (!apiBaseUrl) return DEFAULT_HUB_PATH

  const hubBaseUrl = apiBaseUrl.endsWith('/api')
    ? apiBaseUrl.slice(0, -4)
    : apiBaseUrl

  return `${hubBaseUrl}${DEFAULT_HUB_PATH}`
}

export function buildApiUrlForEnv(path: string, env: RuntimeEnv): string {
  const normalizedApiBaseUrl = resolveApiBaseUrlForEnv(env)
  return buildApiUrlWithBase(path, normalizedApiBaseUrl)
}

function buildApiUrlWithBase(path: string, normalizedApiBaseUrl: string): string {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`

  if (normalizedApiBaseUrl.endsWith('/api') && normalizedPath.startsWith('/api/')) {
    return `${normalizedApiBaseUrl}${normalizedPath.slice(4)}`
  }

  return `${normalizedApiBaseUrl}${normalizedPath}`
}

function getLocalApiBaseUrl(): string {
  return LOCAL_API_BASE_URL
}

function normalizeBaseUrl(value: string): string {
  return value.replace(/\/+$/, '')
}
