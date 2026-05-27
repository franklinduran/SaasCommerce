import { describe, expect, it } from 'vitest'
import {
  buildApiUrlForEnv,
  resolveApiBaseUrlForEnv,
  resolveSignalRHubUrlForEnv,
} from '@/shared/services/apiConfig'

describe('apiConfig', () => {
  it('uses localhost only for development defaults', () => {
    expect(resolveApiBaseUrlForEnv({ DEV: true })).toBe('http://localhost:5000')
    expect(resolveSignalRHubUrlForEnv({ DEV: true })).toBe('http://localhost:5000/hubs/realtime')
  })

  it('uses same-origin relative paths for production defaults', () => {
    expect(resolveApiBaseUrlForEnv({ DEV: false })).toBe('')
    expect(resolveSignalRHubUrlForEnv({ DEV: false })).toBe('/hubs/realtime')
    expect(buildApiUrlForEnv('/api/products', { DEV: false })).toBe('/api/products')
  })

  it('normalizes configured API bases with or without /api suffix', () => {
    expect(buildApiUrlForEnv('/api/products', {
      DEV: false,
      VITE_API_BASE_URL: 'https://api.example.com/',
    })).toBe('https://api.example.com/api/products')

    expect(buildApiUrlForEnv('/api/products', {
      DEV: false,
      VITE_API_BASE_URL: 'https://api.example.com/api/',
    })).toBe('https://api.example.com/api/products')
  })

  it('uses an explicit SignalR hub URL when configured', () => {
    expect(resolveSignalRHubUrlForEnv({
      DEV: false,
      VITE_SIGNALR_HUB_URL: 'https://api.example.com/hubs/realtime/',
    })).toBe('https://api.example.com/hubs/realtime')
  })
})
