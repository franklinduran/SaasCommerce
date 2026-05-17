# Etapa 8.5 - Validacion local integral del MVP construido

## Objetivo

Validar en local el MVP construido hasta Etapa 8 como un sistema integrado, no solo por pruebas unitarias. Esta etapa cubre Auth, Settings, Catalog, Inventory, Outbox, Inbox, Saga, Consumers, SignalR, multi-tenancy, Docker local, PostgreSQL, RabbitMQ, Worker, frontend base y respuestas `ApiResponse`.

La validacion respeta las reglas del proyecto: Clean Architecture, API/Worker separados, eventos en Contracts, Application sin MassTransit/SignalR directo, Outbox para publicacion critica, Inbox para idempotencia, SignalR protegido con JWT y filtrado por `BusinessId`.

## Estado de endpoints expuestos hoy

Endpoints HTTP disponibles:

- `GET /health/live`
- `GET /health/ready`
- `GET /api/version`
- `POST /api/account/register-business`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/me`
- `PUT /api/me/profile`
- `PUT /api/me/password`
- `GET /api/business/current`
- `PUT /api/business/current`
- `GET /api/branches/current`
- `PUT /api/branches/current`
- `POST /api/catalog/categories`
- `PUT /api/catalog/categories/{id}`
- `GET /api/catalog/categories`
- `POST /api/catalog/products`
- `GET /api/catalog/products`
- `GET /api/catalog/products/{id}`
- `PUT /api/catalog/products/{id}`
- `PUT /api/catalog/products/{id}/activate`
- `PUT /api/catalog/products/{id}/deactivate`
- `POST /api/inventory/adjustments`
- `GET /api/inventory/stock`
- `GET /api/inventory/movements`
- `GET /hubs/realtime` mediante SignalR negotiate/websocket

No existen todavia endpoints HTTP backend para:

- Customers
- Sales/POS
- `POST /api/sales`
- `GET /api/sales`
- `POST /api/sales/{id}/cancel`

Sales si existe internamente como dominio, persistencia, casos de uso, saga, outbox/consumers y tests. La validacion local de Sales se hace estimulando `SaleCreatedEventV1` en Outbox hasta que exista endpoint POS.

## Comandos base

```powershell
docker compose up --build -d
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

URLs locales:

- Web: `http://localhost:5173`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- RabbitMQ Management: `http://localhost:15672`
- SonarQube: `http://localhost:9000`

Credenciales seed en Development:

- Email: `admin@test.com`
- Password: `Admin123!`

## Resultado local registrado

Validacion ejecutada el `2026-05-17` en Docker local reconstruido.

Infraestructura:

- PostgreSQL: healthy.
- RabbitMQ: healthy.
- API: up.
- Worker: up.
- Web: up, `http://localhost:5173` responde `200`.
- SonarQube: healthy.
- Logs recientes de API: sin entradas criticas `ERR`/`FTL`.
- Logs recientes de Worker: sin entradas criticas `ERR`/`FTL`.

Migraciones/tablas confirmadas:

- `public.outbox_messages`
- `public.inbox_messages`
- `public.sale_saga_states`
- `sales.sales`
- `sales.sale_items`

Health:

- `GET /health/live`: `200`, `isSuccess=true`.
- `GET /health/ready`: `200`, PostgreSQL healthy y RabbitMQ healthy.
- `GET /health`: `404`. No esta expuesto actualmente.

Auth:

- Login seed OK.
- Registro real de comercio OK con data tipo `Colmado La Bendicion`.
- Login del comercio registrado OK.
- `GET /api/me` sin token: `401 ApiResponse`.
- Login con password incorrecto: `401 ApiResponse`.
- Refresh token valido: OK.

Business/Branch:

- `GET /api/business/current`: OK.
- `GET /api/branches/current`: OK.
- El frontend/API no envia `BusinessId`; se resuelve desde claims/current user.

Catalog:

- Crear categoria: OK.
- Crear producto inventariable: OK.
- Obtener producto por id: OK.
- Listar productos con `pageSize=10`: OK.
- SKU duplicado: `409 PRODUCT_SKU_ALREADY_EXISTS`.
- `pageSize=5`: `400 VALIDATION_ERROR`, porque Products solo acepta `10`, `25` o `50`.

Inventory:

- Ajuste inicial positivo: OK.
- Consulta stock: OK.
- Consulta movimientos: OK.
- Ajuste negativo que deja stock bajo cero: `409 INVENTORY_STOCK_INSUFFICIENT`.

Saga tecnica Sales:

- Se creo una venta tecnica en `sales.sales`.
- Se inserto `SaleCreatedEventV1` en Outbox.
- Worker publico la cadena completa:
  - `SaleCreatedEventV1`
  - `StockValidationRequestedEventV1`
  - `StockValidatedEventV1`
  - `InventoryDeductionRequestedEventV1`
  - `InventoryDeductedEventV1`
  - `PaymentRegistrationRequestedEventV1`
  - `PaymentRegisteredEventV1`
  - `InvoiceGenerationRequestedEventV1`
  - `InvoiceGeneratedEventV1`
  - `SaleCompletedEventV1`
- Estado final de venta: `Completed`.
- Estado final de saga: `Completed`.
- Outbox de la correlacion: todos `Published`, sin `Failed`.
- Inbox de la correlacion: `ValidateStockConsumer`, `DeductInventoryConsumer`, `RegisterPaymentConsumer`, `GenerateInvoiceConsumer`, `SaleCompletedConsumer`.

SignalR:

- Sin token: rechaza con `401 ApiResponse`.
- Con JWT valido: conecta a `/hubs/realtime`.
- Desconexion limpia.

Regresion automatizada:

- `dotnet restore`: OK.
- `dotnet build --no-restore -m:1 /nr:false -v minimal`: OK, 0 warnings.
- `dotnet test --no-build -m:1 /nr:false -v minimal`: OK, 165 tests.
- `npm.cmd run lint`: OK.
- `npm.cmd run test`: OK, 19 tests.
- `npm.cmd run build`: OK.

Matriz HTTP integral:

- Total de requests ejecutados: `38`.
- Requests con resultado esperado: `38`.
- Fallidos inesperados: `0`.
- Incluye health, version, register-business, login, refresh, perfil, password, business, branch, categorias, productos, inventario, errores esperados, Customers pendiente y Sales pendiente.
- Customers/Sales HTTP devuelven `404 NOT_FOUND` como esperado porque los endpoints aun no estan expuestos.

SonarQube:

- CE task: `SUCCESS`.
- Quality Gate: `OK`.
- Open issues: `0`.
- Bugs: `0`.
- Vulnerabilities: `0`.
- Code smells: `0`.
- Security hotspots: `0`.
- Coverage global: `79.0%`.
- Duplicacion global: `0.6%`.

## Requests versionados

Usar los archivos en `requests/` con la extension REST Client de VS Code o Rider HTTP Client:

- `requests/00-health.http`
- `requests/01-auth.http`
- `requests/02-business.http`
- `requests/03-products.http`
- `requests/04-inventory.http`
- `requests/05-customers.http`
- `requests/06-sales.http`
- `requests/07-saga-flow.http`
- `requests/08-idempotency.http`
- `requests/09-signalr.md`

## Validacion tecnica de Saga por Outbox

Hasta que exista `POST /api/sales`, el flujo real de saga puede validarse localmente insertando una venta tecnica en `sales.sales` y un `SaleCreatedEventV1` en `outbox_messages`. Esto no reemplaza el endpoint futuro de POS; solo valida Worker, Outbox, Saga, Consumers e Inbox con infraestructura real.

1. Crear primero un producto inventariable y cargar stock con `requests/03-products.http` y `requests/04-inventory.http`.
2. Tomar el `productId` creado.
3. Ejecutar desde repo root, cambiando `productId` si aplica:

```powershell
$saleId=[guid]::NewGuid().ToString()
$itemId=[guid]::NewGuid().ToString()
$outboxId=[guid]::NewGuid().ToString()
$eventId=[guid]::NewGuid().ToString()
$correlationId=[guid]::NewGuid().ToString()
$productId='<PRODUCT_ID_WITH_STOCK>'
$businessId='11111111-1111-1111-1111-111111111111'
$branchId='22222222-2222-2222-2222-222222222222'
$userId='44444444-4444-4444-4444-444444444444'
$createdAt=(Get-Date).ToUniversalTime().ToString('o')
$payloadObj=[ordered]@{
  eventId=$eventId
  correlationId=$correlationId
  saleId=$saleId
  businessId=$businessId
  branchId=$branchId
  userId=$userId
  items=@(@{ productId=$productId; quantity=1; unitPrice=245.00 })
  total=245.00
  paymentMethod='Cash'
  createdAt=$createdAt
  version=1
}
$payload=($payloadObj | ConvertTo-Json -Depth 10 -Compress).Replace("'", "''")
$eventType='SaasCommerce.Modules.Sales.Contracts.Events.V1.SaleCreatedEventV1, SaasCommerce.Modules, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
$sql=@"
insert into sales.sales ("Id", "BusinessId", "BranchId", "UserId", "Status", "PaymentMethod", "Total", "CreatedAt", "UpdatedAt")
values ('$saleId', '$businessId', '$branchId', '$userId', 'Processing', 'Cash', 245.00, now(), now());
insert into sales.sale_items ("Id", "SaleId", "ProductId", "Quantity", "UnitPrice")
values ('$itemId', '$saleId', '$productId', 1.000, 245.00);
insert into outbox_messages ("Id", "EventId", "CorrelationId", "BusinessId", "EventType", "Payload", "OccurredAt", "PublishedAt", "Attempts", "LastError", "Status", "CreatedAt")
values ('$outboxId', '$eventId', '$correlationId', '$businessId', '$eventType', '$payload', now(), null, 0, null, 'Pending', now());
"@
$sql | docker exec -i saascommerce-postgres psql -U saas_rd_user -d saas_rd_db -v ON_ERROR_STOP=1
```

4. Esperar al menos 30 segundos.
5. Consultar resultado:

```powershell
$query=@"
select 'sale_status=' || "Status" from sales.sales where "Id" = '$saleId';
select 'saga_state=' || "CurrentState" from sale_saga_states where "SaleId" = '$saleId';
select 'outbox:' || "EventType" || '|status=' || "Status" || '|attempts=' || "Attempts" || '|error=' || coalesce("LastError", '') from outbox_messages where "CorrelationId" = '$correlationId' order by "CreatedAt";
select 'inbox:' || "ConsumerName" from inbox_messages where "CorrelationId" = '$correlationId' order by "CreatedAt";
"@
$query | docker exec -i saascommerce-postgres psql -U saas_rd_user -d saas_rd_db -t -A
```

Resultado esperado:

```txt
sale_status=Completed
saga_state=Completed
Todos los mensajes Outbox de la correlacion en Published
Inbox contiene ValidateStockConsumer, DeductInventoryConsumer, RegisterPaymentConsumer, GenerateInvoiceConsumer y SaleCompletedConsumer
```

## Checklist de cierre

- [x] Docker local levanta.
- [x] API responde.
- [x] Worker responde y registra consumers/saga.
- [x] PostgreSQL contiene tablas Outbox, Inbox, Saga y Sales.
- [x] RabbitMQ esta healthy.
- [x] Health live/ready OK.
- [x] Auth login/refresh/protegidos OK.
- [x] Register business OK.
- [x] Settings basico OK.
- [x] Products crear/listar/obtener/errores OK.
- [x] Inventory ajuste/stock/movimientos/errores OK.
- [x] Outbox publica cadena tecnica de Sales.
- [x] Inbox registra consumidores procesados.
- [x] Saga completa flujo feliz tecnico.
- [x] SignalR protegido con JWT.
- [x] ApiResponse observado en exitos y errores.
- [ ] Customers HTTP pendiente: modulo base existe, endpoints no expuestos.
- [ ] Sales HTTP/POS pendiente: infraestructura existe, endpoints no expuestos.

## Hallazgos

1. `GET /health` no existe. Los endpoints reales son `/health/live` y `/health/ready`.
2. `GET /api/catalog/products?pageSize=5` devuelve `VALIDATION_ERROR`; el contrato actual acepta `10`, `25` y `50`.
3. Customers tiene modulo/pagina base, pero no endpoints backend.
4. Sales tiene dominio, saga, outbox/consumers y tests, pero no endpoints HTTP. La validacion local del flujo se hizo por Outbox tecnico.
5. El scanner local de SonarQube finaliza correctamente, pero registra avisos de permisos de JGit sobre `C:\Users\frank\.config\jgit\config`. No bloquea el CE task ni el Quality Gate.

## Comandos de regresion

```powershell
$env:NUGET_PACKAGES='C:\Users\frank\.nuget\packages'
dotnet restore .\SaasCommerce.slnx
dotnet build .\SaasCommerce.slnx --no-restore -m:1 /nr:false -v minimal
dotnet test .\SaasCommerce.slnx --no-build -m:1 /nr:false -v minimal
Set-Location .\frontend
npm.cmd run lint
npm.cmd run test
npm.cmd run build
```
