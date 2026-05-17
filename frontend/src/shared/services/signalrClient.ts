import * as signalR from '@microsoft/signalr'

const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL ?? 'http://localhost:5000/hubs/realtime'

let connection: signalR.HubConnection | null = null
let currentAccessToken: string | null = null
const listeners = new Map<string, Set<(payload: unknown) => void>>()

type RealtimeHandler<TPayload> = (payload: TPayload) => void

export function createSignalRConnection(accessTokenFactory: () => string | Promise<string>) {
  return new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, { accessTokenFactory })
    .withAutomaticReconnect()
    .build()
}

export async function startRealtimeConnection(accessToken?: string) {
  if (!accessToken) {
    return null
  }

  if (connection && currentAccessToken === accessToken) {
    return connection
  }

  await stopRealtimeConnection()

  currentAccessToken = accessToken
  connection = createSignalRConnection(() => accessToken)
  attachListeners(connection)

  try {
    await connection.start()
    return connection
  } catch (error) {
    connection = null
    currentAccessToken = null
    throw error
  }
}

export async function stopRealtimeConnection() {
  if (!connection) {
    currentAccessToken = null
    return
  }

  const activeConnection = connection
  connection = null
  currentAccessToken = null

  if (activeConnection.state !== signalR.HubConnectionState.Disconnected) {
    await activeConnection.stop()
  }
}

export function onRealtimeEvent<TPayload>(
  eventName: string,
  handler: RealtimeHandler<TPayload>,
) {
  const storedHandlers = listeners.get(eventName) ?? new Set<(payload: unknown) => void>()
  const storedHandler = handler as (payload: unknown) => void

  storedHandlers.add(storedHandler)
  listeners.set(eventName, storedHandlers)
  connection?.on(eventName, storedHandler)
}

export function offRealtimeEvent<TPayload>(
  eventName: string,
  handler: RealtimeHandler<TPayload>,
) {
  const storedHandler = handler as (payload: unknown) => void
  const storedHandlers = listeners.get(eventName)

  storedHandlers?.delete(storedHandler)
  if (storedHandlers?.size === 0) {
    listeners.delete(eventName)
  }

  connection?.off(eventName, storedHandler)
}

function attachListeners(activeConnection: signalR.HubConnection) {
  listeners.forEach((eventHandlers, eventName) => {
    eventHandlers.forEach((handler) => activeConnection.on(eventName, handler))
  })
}
