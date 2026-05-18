# Etapa 15 — Validacion real Docker (develop)

Fecha: 2026-05-18  
Rama: develop  
Ejecutado por: agente automatizado (Claude)

---

## Checklist de validacion end-to-end

### 1. Docker completo levantado ✅

```
saascommerce-web        Up
saascommerce-worker     Up
saascommerce-api        Up
saascommerce-postgres   Up (healthy)
saascommerce-rabbitmq   Up (healthy)
```

Puertos: API:8080, Web:5173, Postgres:5433, RabbitMQ:15672

---

### 2. Migracion PostgreSQL aplicada ✅

Schema `billing.invoices` presente con columnas:
Id, BusinessId, BranchId, SaleId, InvoiceNumber, Subtotal, DiscountTotal, TaxTotal, Total, Status, CreatedAt, UpdatedAt, CancelledAt, Sequence, CustomerId

---

### 3. Crear venta desde POS ✅

Credenciales seed (DevelopmentDataSeeder):
- email: admin@test.com / password: Admin123!
- businessId: 11111111-1111-1111-1111-111111111111
- branchId: 22222222-2222-2222-2222-222222222222

Venta 1:
```
POST /api/sales
→ saleId: 3da8197b-2118-4c44-ba6a-b9022dc463ee
→ status: Received → Completed
→ total: RD$500.00 (qty:1 x 500.00)
```

Venta 2:
```
POST /api/sales
→ saleId: bd47f9d3-d4f0-471e-a7dd-1ea4e43e7810
→ status: Received → Completed
→ total: RD$1000.00 (qty:2 x 500.00)
```

---

### 4. Venta Completed (saga) ✅

Flujo: Sale creada → OutboxMessage publicado por Worker → RabbitMQ → SaleCompletedEventV1 consumido → sale.Status = Completed

Outbox stats al cierre: 240 mensajes publicados, 0 pendientes.

---

### 5. Recibo generado automaticamente ✅

Consumer: `GenerateInvoiceOnSaleCompletedConsumer` (via `IdempotentConsumer<SaleCompletedEventV1>`)  
Handler: `GenerateInvoiceHandler` → crea Invoice con secuencia por businessId

---

### 6. GET /api/invoices ✅

```
GET /api/invoices?page=1&pageSize=10
→ 200 OK
→ totalItems: 2
→ items: [RI-00000002 (1000.00 Issued), RI-00000001 (500.00 Issued)]
```

---

### 7. GET /api/invoices/{id} y GET /api/sales/{saleId}/invoice ✅

```
GET /api/invoices/bf56f9d3-f764-4751-a119-0db5a3787ff6
→ invoiceNumber: RI-00000001, subtotal: 500.00, total: 500.00, status: Issued

GET /api/sales/3da8197b-2118-4c44-ba6a-b9022dc463ee/invoice
→ mismo resultado
```

---

### 8. Lista de recibos en frontend ✅ (verificado por codigo)

Ruta `/invoices` → `InvoicesPage`:
- Tabla con filtros (busqueda, fecha desde/hasta, estado)
- Paginacion
- Boton "Reintentar" con `useInvoices(filters)` via React Query
- `useInvoiceRealtimeInvalidation()` conectado al hub

Frontend activo: peticiones GET al API registradas en logs a las 13:03 UTC.

---

### 9. Detalle de recibo ✅ (verificado por codigo)

Ruta `/invoices/:invoiceId` → `InvoiceDetailPage`:
- Header con numero de recibo e `InvoiceStatusBadge`
- Grid de Subtotal / Descuento / Impuesto / Total
- `InvoiceReceipt` con datos del negocio, items y totales
- Boton "Cancelar" (deshabilitado si status != Issued)

---

### 10. Boton imprimir ✅ (verificado por codigo)

`PrintReceiptButton` → `window.print()`  
Estilos CSS con `print:hidden` ocultan header y botones al imprimir.

```tsx
export function PrintReceiptButton() {
  return (
    <Button onClick={() => window.print()} ...>
      <Printer size={16} />
      Imprimir recibo
    </Button>
  )
}
```

---

### 11. SignalR por business-{businessId} ✅

**Consumer registrado:**
```
[05:18:30 INF] Configured endpoint InvoiceGeneratedRealtime,
               Consumer: SaasCommerce.Api.Realtime.InvoiceGeneratedRealtimeConsumer
```

**WebSocket conectado (desde el frontend):**
```
[05:18:35 INF] Realtime connected.
               ConnectionId=iq62H49B1tAgjg2FpzkBrw
               UserId=44444444-4444-4444-4444-444444444444
               BusinessId=11111111-1111-1111-1111-111111111111
```

**Flujo completo:**
1. `InvoiceGeneratedEventV1` publicado por Billing consumer via RabbitMQ
2. `InvoiceGeneratedRealtimeConsumer` consume el evento
3. `SignalRRealtimeNotifier.NotifyBusinessAsync()` → `hubContext.Clients.Group("business-{guid}").SendAsync("invoice.generated", payload)`
4. Frontend `useInvoiceRealtimeInvalidation()` escucha `invoice.generated` → `queryClient.invalidateQueries`

Nombre de grupo: `business-{businessId:D}` (guion, no dos puntos)

---

### 12. SonarQube ⚠️ (servicio activo, analisis pendiente de token)

Servicio:
```
GET http://localhost:9000/api/system/status
→ {"version":"25.1.0.102122","status":"UP"}
```

Containers: `saascommerce-sonarqube` y `saascommerce-sonarqube-db` (healthy)

Para ejecutar analisis completo:
1. Abrir http://localhost:9000 y generar token de admin
2. Configurar variables de entorno:
   ```powershell
   [Environment]::SetEnvironmentVariable("SONAR_HOST_URL", "http://localhost:9000", "User")
   [Environment]::SetEnvironmentVariable("SONAR_PROJECT_KEY", "saascommerce", "User")
   [Environment]::SetEnvironmentVariable("SONAR_TOKEN", "<token>", "User")
   ```
3. Ejecutar `.\scripts\sonar.ps1`

---

### 13. Evidencia final ✅

**PostgreSQL — billing.invoices:**
```
 InvoiceNumber |  Total  | Status | sale_prefix |         CreatedAt
---------------+---------+--------+-------------+--------------------------
 RI-00000001   |  500.00 | Issued | 3da8197b    | 2026-05-18 05:34:33+00
 RI-00000002   | 1000.00 | Issued | bd47f9d3    | 2026-05-18 12:59:02+00
```

**Outbox/Inbox:**
- outbox_messages: 240 publicados, 0 pendientes
- inbox_messages: 261 entradas (idempotencia activa)

**Resumen:**
- 12/13 items validados OK
- Item 12 (SonarQube analisis): servicio UP, analisis requiere token del usuario
- Sistema de facturacion funcional de punta a punta en Docker real
