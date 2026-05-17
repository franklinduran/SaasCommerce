# SignalR local validation

Endpoint:

```txt
http://localhost:8080/hubs/realtime
```

Expected behavior:

- Without JWT: connection is rejected with `401 ApiResponse`.
- With JWT: connection succeeds.
- Backend assigns groups from claims:
  - `business-{BusinessId}`
  - `branch-{BranchId}`
  - `user-{UserId}`
- Frontend must not send `BusinessId`.
- Business data must not be sent with `Clients.All`.

Manual Node check from repo root:

```powershell
Set-Location .\frontend
node -e "const signalR = await import('@microsoft/signalr'); const api='http://localhost:8080'; const loginRes=await fetch(api+'/api/auth/login',{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify({email:'admin@test.com',password:'Admin123!'})}); const login=await loginRes.json(); let noTokenRejected=false; const unauth=new signalR.HubConnectionBuilder().withUrl(api+'/hubs/realtime').build(); try { await unauth.start(); } catch { noTokenRejected=true; } const conn=new signalR.HubConnectionBuilder().withUrl(api+'/hubs/realtime',{accessTokenFactory:()=>login.data.accessToken}).withAutomaticReconnect().build(); await conn.start(); const connected=conn.state; await conn.stop(); console.log(JSON.stringify({noTokenRejected, connected, stopped: conn.state}));"
```

Observed local result:

```json
{
  "noTokenRejected": true,
  "connected": "Connected",
  "stopped": "Disconnected"
}
```
