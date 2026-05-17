import { beforeEach, describe, expect, it, vi } from 'vitest'

const signalRMock = vi.hoisted(() => {
  const connection = {
    start: vi.fn(() => Promise.resolve()),
    stop: vi.fn(() => Promise.resolve()),
    on: vi.fn(),
    off: vi.fn(),
    state: 'Connected',
  }
  const builder = {
    withUrl: vi.fn(() => builder),
    withAutomaticReconnect: vi.fn(() => builder),
    build: vi.fn(() => connection),
  }
  function HubConnectionBuilder() {
    return builder
  }

  return {
    builder,
    connection,
    HubConnectionBuilder: vi.fn(HubConnectionBuilder),
  }
})

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: signalRMock.HubConnectionBuilder,
  HubConnectionState: {
    Disconnected: 'Disconnected',
  },
}))

describe('signalrClient', () => {
  beforeEach(() => {
    vi.resetModules()
    vi.clearAllMocks()
    signalRMock.connection.state = 'Connected'
    signalRMock.connection.start.mockResolvedValue(undefined)
    signalRMock.connection.stop.mockResolvedValue(undefined)
  })

  it('does not connect without token', async () => {
    const { startRealtimeConnection } = await import('@/shared/services/signalrClient')

    const connection = await startRealtimeConnection()

    expect(connection).toBeNull()
    expect(signalRMock.HubConnectionBuilder).not.toHaveBeenCalled()
  })

  it('connects with accessTokenFactory and automatic reconnect', async () => {
    const { startRealtimeConnection } = await import('@/shared/services/signalrClient')

    await startRealtimeConnection('jwt-token')

    expect(signalRMock.HubConnectionBuilder).toHaveBeenCalledTimes(1)
    expect(signalRMock.builder.withUrl).toHaveBeenCalledTimes(1)
    expect(signalRMock.builder.withAutomaticReconnect).toHaveBeenCalledTimes(1)
    expect(signalRMock.connection.start).toHaveBeenCalledTimes(1)

    const withUrlCalls = signalRMock.builder.withUrl.mock.calls as unknown as Array<
      [string, { accessTokenFactory: () => string | Promise<string> }]
    >
    const [, options] = withUrlCalls[0]
    expect(await options.accessTokenFactory()).toBe('jwt-token')
    expect(JSON.stringify(options)).not.toContain('businessId')
  })

  it('disconnects on logout', async () => {
    const { startRealtimeConnection, stopRealtimeConnection } = await import(
      '@/shared/services/signalrClient'
    )

    await startRealtimeConnection('jwt-token')
    await stopRealtimeConnection()

    expect(signalRMock.connection.stop).toHaveBeenCalledTimes(1)
  })

  it('registers realtime ping listener', async () => {
    const { startRealtimeConnection, onRealtimeEvent } = await import(
      '@/shared/services/signalrClient'
    )
    const handler = vi.fn()

    await startRealtimeConnection('jwt-token')
    onRealtimeEvent('realtime.ping', handler)

    expect(signalRMock.connection.on).toHaveBeenCalledWith('realtime.ping', handler)
  })

  it('registers and removes sale status listener', async () => {
    const { startRealtimeConnection, onRealtimeEvent, offRealtimeEvent } = await import(
      '@/shared/services/signalrClient'
    )
    const handler = vi.fn()

    await startRealtimeConnection('jwt-token')
    onRealtimeEvent('sale.statusChanged', handler)
    offRealtimeEvent('sale.statusChanged', handler)

    expect(signalRMock.connection.on).toHaveBeenCalledWith('sale.statusChanged', handler)
    expect(signalRMock.connection.off).toHaveBeenCalledWith('sale.statusChanged', handler)
  })
})
