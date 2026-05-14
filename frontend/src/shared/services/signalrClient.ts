import * as signalR from '@microsoft/signalr'

const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL ?? 'http://localhost:5000/hubs/business'

export function createSignalRConnection(accessTokenFactory?: () => string | Promise<string>) {
  const builder = new signalR.HubConnectionBuilder()

  if (accessTokenFactory) {
    return builder
      .withUrl(hubUrl, { accessTokenFactory })
      .withAutomaticReconnect()
      .build()
  }

  return builder.withUrl(hubUrl).withAutomaticReconnect().build()
}
