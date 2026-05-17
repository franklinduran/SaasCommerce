# Etapa 12.5 - Pruebas reales del MVP actual

## Ambiente usado

Fecha de ejecucion: 2026-05-17

Stack levantado con Docker:

```txt
API: http://localhost:8080
Frontend: http://localhost:5173
PostgreSQL: localhost:5433
RabbitMQ: localhost:5672 / http://localhost:15672
Worker: saascommerce-worker
```

Preparacion ejecutada:

```txt
docker compose down -v
docker compose up -d --build
npm install
dotnet build
dotnet test
npm lint
npm test
npm run build
```

Estado final del ambiente:

```txt
/health/ready: Healthy
PostgreSQL: Healthy
RabbitMQ: Healthy
RabbitMQ queues: 0 ready / 0 unacknowledged
Outbox: 71 Published, 0 Pending, 0 Failed
Frontend HTTP: 200 OK
```

Resultado de SonarQube:

```txt
Analisis ejecutado: OK
Compute Engine: SUCCESS
Quality Gate: OK
New coverage: 80.3%
New duplication: 1.25134%
Open issues nuevos: 0
```

## Datos de prueba

Negocios:

```txt
Negocio A: Colmado La Bendicion
RNC: 000000000
Usuario: admin@colmado.test
Sucursal: Principal
BusinessId: 7b9b26bf-6b89-48c2-8fde-d6ea69fa31c7
BranchId: d291f019-535e-4154-b7de-18dd8c95198d

Negocio B: Minimarket El Sol
RNC: 101010101
Usuario: admin@elsol.test
```

Productos creados para Colmado La Bendicion:

```txt
Arroz Selecto 10 lb
Aceite Crisol 16 oz
Leche Rica 1 litro
Salami Induveca 1 lb
Huevos unidad
Pan de agua
Refresco Coca-Cola 2 litros
Agua Dasani 16 oz
Azucar crema 5 lb
Cafe Santo Domingo
```

Inventario inicial aplicado:

```txt
Arroz Selecto 10 lb: 20 / minimo 5
Aceite Crisol 16 oz: 15 / minimo 4
Leche Rica 1 litro: 12 / minimo 3
Salami Induveca 1 lb: 10 / minimo 2
Huevos unidad: 90 / minimo 20
Pan de agua: 40 / minimo 10
```

## Casos ejecutados

| Caso | Resultado | Evidencia |
| --- | --- | --- |
| 1 - Login y carga inicial | OK | Login real con `admin@colmado.test`; `/api/me` devolvio usuario, negocio y sucursal. Frontend navego al dashboard. |
| 2 - Crear productos | OK | 10 productos dominicanos creados. Precio negativo rechazado con `ApiResponse` de error. Producto de negocio B no aparece para A. |
| 3 - Ajustar inventario manual | OK | Movimientos `InitialStock` creados. Arroz quedo en 20. Historial muestra movimiento inicial. |
| 4 - No permitir stock negativo | OK | Ajuste Arroz `-30` rechazo 409. Stock quedo en 20. No se creo movimiento invalido. |
| 5 - Venta normal POS/API | OK | Venta `CE767350` completo. Arroz 20 -> 18, Aceite 15 -> 14. Total RD$1,000.00. |
| 6 - Venta sin stock suficiente | OK | Venta de Leche x50 quedo `Failed`, motivo `Insufficient stock.`. Stock de Leche quedo en 12. |
| 7 - Idempotencia descuento | OK | Evento duplicado insertado en Outbox para venta `088ABC35`. Stock de Salami quedo en 1 y movimientos `SaleDeduction` para esa venta quedaron en 1. |
| 8 - Stock bajo | OK | Venta de Salami x9 dejo stock 1 <= minimo 2. Se recibio `inventory.lowStockDetected` por SignalR. |
| 9 - Producto agotado | OK | Aceite quedo en 0, estado `OutOfStock`, filtro agotados lo muestra y venta posterior fallo. |
| 10 - Historial de ventas | OK | `/sales` mostro 7 ventas reales, filtros `Completed`, `Failed` y busqueda por SaleId corto funcionaron. |
| 11 - Detalle de venta | OK | `/sales/ce767350-e3ea-413a-abf9-7395de6194c9` mostro items, cantidades, sucursal, total y estado. |
| 12 - Recibo imprimible | OK | Detalle mostro `RECIBO NO FISCAL`, productos, cantidades, subtotal, total y metodo de pago. |
| 13 - Multi-tenancy real | OK | Usuario A no ve producto de B. Usuario B no ve inventario ni venta de A. SignalR no envio evento de A a B. |
| 14 - Worker apagado | OK | Con Worker apagado, venta `771BED4F` quedo `Received` y stock intacto. Al encender Worker, completo y Pan 41 -> 40. |
| 15 - RabbitMQ apagado | OK | Con RabbitMQ apagado, venta `100AD48A` quedo `Received` y stock intacto. Al encender RabbitMQ, completo y Huevos 90 -> 89. |

## Resultados

Validaciones automatizadas:

```txt
dotnet build: OK, 0 warnings, 0 errors
dotnet test: OK, 240 passed
npm lint: OK
npm test: OK, 47 passed
npm run build: OK
SonarQube Quality Gate: OK
SonarQube new coverage: 80.3%
SonarQube new duplication: 1.25134%
SonarQube new issues: 0
```

Validacion frontend headless con Chrome DevTools Protocol:

```txt
Login real: OK
/inventory muestra productos reales: OK
/inventory muestra Stock bajo / Agotado: OK
/sales muestra historial: OK
/sales/:saleId muestra detalle: OK
Recibo no fiscal visible: OK
Console errors: 0
```

Eventos SignalR reales observados:

```txt
sale.statusChanged: Received
sale.statusChanged: Completed
inventory.stockChanged: SaleDeduction
inventory.lowStockDetected
inventory.adjusted
inventory.stockChanged: ManualAdjustment
```

Resiliencia:

```txt
Outbox conserva eventos cuando Worker esta apagado.
Outbox conserva eventos cuando RabbitMQ esta apagado.
Al recuperarse Worker/RabbitMQ, las ventas pendientes se completan sin duplicar descuentos.
RabbitMQ quedo sin mensajes atascados.
Outbox quedo sin Pending/Failed.
```

## Bugs encontrados

### BUG-12.5-001 - SignalR desde Worker no llegaba al navegador

Sintoma:

```txt
El frontend solo recibia sale.statusChanged Received emitido desde API.
No recibia Completed/Failed ni eventos inventory.stockChanged/lowStockDetected generados desde Worker.
```

Causa:

```txt
Los clientes SignalR estan conectados al Hub hospedado por API.
El Worker estaba intentando notificar con IHubContext local, pero ese proceso no tiene las conexiones del navegador.
```

Impacto:

```txt
El flujo de negocio terminaba correctamente, pero la actualizacion realtime visible no se entregaba.
```

## Bugs corregidos

### Correccion BUG-12.5-001

Se agregaron consumidores realtime en el proceso API:

```txt
SaleCompletedRealtimeConsumer
SaleFailedRealtimeConsumer
InventoryAdjustedRealtimeConsumer
InventoryDeductedRealtimeConsumer
LowStockDetectedRealtimeConsumer
```

Archivo:

```txt
backend/src/Api/Realtime/RealtimeNotificationConsumers.cs
```

Registro:

```txt
backend/src/Api/Program.cs
```

Pruebas agregadas:

```txt
backend/tests/SaasCommerce.Api.Tests/RealtimeNotificationConsumerTests.cs
```

Validacion posterior:

```txt
SignalR recibio Completed desde API consumer.
SignalR recibio inventory.stockChanged por venta.
SignalR recibio inventory.lowStockDetected.
SignalR recibio inventory.adjusted por ajuste manual.
```

## Evidencias

Ventas clave:

```txt
CE767350 - Venta normal: Completed, RD$1,000.00
088ABC35 - Venta Salami x9: Completed, genero Stock bajo
771BED4F - Venta creada con Worker apagado: Completed al recuperar Worker
100AD48A - Venta creada con RabbitMQ apagado: Completed al recuperar RabbitMQ
```

Stock final relevante:

```txt
Arroz Selecto 10 lb: 18, Disponible
Aceite Crisol 16 oz: 0, Agotado
Leche Rica 1 litro: 12, Disponible
Salami Induveca 1 lb: 1, Stock bajo
Huevos unidad: 89, Disponible
Pan de agua: 40, Disponible
```

Idempotencia:

```txt
Venta usada: 088abc35-40ab-4a9a-a2f8-3190b29a2af8
Producto: Salami Induveca 1 lb
Stock antes del duplicado: 1
Stock despues del duplicado: 1
Movimientos SaleDeduction para esa venta: 1
Estado venta: Completed
```

Multi-tenancy:

```txt
Producto privado de Minimarket El Sol no aparece para Colmado La Bendicion.
Inventario de Colmado no aparece para Minimarket El Sol.
Venta de Colmado devuelve 404 al consultarse con token de Minimarket El Sol.
Eventos SignalR contienen BusinessId y se entregan al grupo correcto.
```

## Decision final

Decision funcional:

```txt
Etapa 12.5 aprobada funcionalmente.
El flujo Login -> Productos -> Inventario -> POS/Ventas -> Saga -> Descuento -> Historial -> Detalle -> Recibo -> Alertas -> SignalR funciona con datos reales simulados.
```

Decision de calidad:

```txt
Tests automatizados OK.
E2E real OK.
Frontend real OK sin errores de consola.
RabbitMQ/Outbox/Worker OK tras fallos temporales.
SonarQube Quality Gate OK.
New coverage >= 80%.
New duplication <= 3%.
Open issues nuevos = 0.
```

Recomendacion:

```txt
Etapa 12.5 cerrada.
Se puede avanzar a Etapa 13 - Clientes, fiados y cuentas por cobrar.
```
