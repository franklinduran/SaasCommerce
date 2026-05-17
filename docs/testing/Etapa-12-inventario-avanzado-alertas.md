# Etapa 12 - Inventario avanzado y alertas realtime

## Objetivo

Cerrar el ciclo posterior a ventas:

```txt
Venta -> validacion de stock -> descuento idempotente -> movimientos -> bajo stock -> SignalR
```

## Endpoints

```txt
GET  /api/inventory
GET  /api/inventory/products/{productId}
GET  /api/inventory/stock
GET  /api/inventory/movements
POST /api/inventory/adjustments
```

Reglas:

- Todas las consultas filtran por `BusinessId`.
- `POST /api/inventory/adjustments` usa `BusinessId` y sucursal desde el usuario, salvo `BranchId` explicito permitido para ajuste operativo.
- No se permite stock negativo cuando el producto no permite negativo.
- La API solo traduce request a command/query y devuelve `ApiResponse`.

## Backend

Se amplio inventario con:

```txt
SaleId en movimientos para idempotencia
Note en movimientos para motivo operativo
InventoryAdjustedEventV1
LowStockDetectedEventV1
LowStockDetectedNotificationV1
InventoryStockChangedNotificationV1
EfInventoryReadRepository
GetInventoryHandler
GetInventoryProductDetailHandler
InventoryAdjustedConsumer
InventoryDeductedConsumer
LowStockDetectedConsumer
```

Tipos de movimiento aceptados:

```txt
InitialStock
SaleDeduction
ManualAdjustment
PurchaseEntry
Return
```

Tambien se aceptan aliases anteriores como `InitialLoad`, `Sale`, `Adjustment` y `Purchase` para compatibilidad.

## Idempotencia

`DeductSaleInventoryUseCase` consulta movimientos por:

```txt
BusinessId
BranchId
SaleId
```

Si la venta ya fue descontada, no vuelve a crear movimientos ni descuenta stock. Esto protege el flujo ante mensajes duplicados.

## Bajo Stock

Cuando un movimiento deja:

```txt
CurrentStock <= MinimumStock
```

se publica:

```txt
LowStockDetectedEventV1
```

El worker consume ese evento y notifica al grupo del negocio:

```txt
inventory.lowStockDetected
```

Los cambios generales se notifican con:

```txt
inventory.adjusted
inventory.stockChanged
```

## Frontend

Modulo:

```txt
frontend/src/modules/inventory/
```

Pantallas:

```txt
/inventory
/inventory/products/:productId
```

Componentes principales:

```txt
InventoryFilters
InventoryTable
StockStatusBadge
InventoryMovementList
AdjustInventoryDialog
```

Hooks:

```txt
useInventory
useInventoryProductDetail
useAdjustInventory
useInventoryRealtimeInvalidation
```

Al recibir eventos SignalR de inventario, el frontend descarta eventos de otro `BusinessId` e invalida queries de TanStack Query bajo la llave `inventory`.

## Estados Visuales

```txt
Disponible
Stock bajo
Agotado
Loading
Error
Empty state
Detalle por producto
Historial de movimientos
Alertas activas
```

## Tests Cubiertos

Backend:

```txt
InventoryItem_ShouldIncreaseStock_WhenMovementIsEntry
InventoryItem_ShouldDecreaseStock_WhenMovementIsSaleDeduction
InventoryItem_ShouldThrow_WhenStockWouldBeNegative
InventoryItem_ShouldDetectLowStock_WhenCurrentStockIsBelowMinimum
AdjustInventory_ShouldPublishLowStockDetectedEvent_WhenStockFallsBelowMinimum
DeductInventoryForSale_ShouldBeIdempotent_WhenSaleWasAlreadyProcessed
DeductInventoryForSale_ShouldPublishLowStockDetectedEvent_WhenStockFallsBelowMinimum
InventoryRepository_ShouldFilterByBusinessId
InventoryMovementRepository_ShouldNotReturnOtherBusinessMovements
InventoryReadRepository_ShouldReturnLowStockItems
InventoryDeductedConsumer_ShouldProcessEvent_WhenMessageIsValid
InventoryDeductedConsumer_ShouldNotDeductTwice_WhenMessageIsDuplicated
LowStockDetectedConsumer_ShouldNotifyBusinessGroup
GetInventory_ShouldReturnAdjustedStockRows
GetInventoryProductDetail_ShouldReturnStockAndMovements
```

Frontend:

```txt
InventoryPage should render inventory rows
InventoryPage should show low stock badge
AdjustInventoryDialog should validate required fields
InventoryPage should refresh when SignalR stock event arrives
InventoryProductDetailPage should show stock by branch and movements
```

## Validacion Local

Backend:

```powershell
dotnet build .\comercioflow\SaasCommerce.slnx --no-restore -v minimal
dotnet test .\comercioflow\SaasCommerce.slnx --no-restore -v minimal
```

Resultado:

```txt
Build OK
Tests OK: 234 passed
```

Frontend:

```powershell
npm.cmd run lint --prefix .\comercioflow\frontend
npm.cmd run test --prefix .\comercioflow\frontend
npm.cmd run build --prefix .\comercioflow\frontend
```

Resultado:

```txt
Lint OK
Tests OK: 47 passed
Build OK
```

SonarQube:

```txt
Quality Gate OK
New coverage: 80.1%
New duplication: 0.83942%
Open issues nuevos: 0
```

## Resultado

Al cerrar esta etapa, ComercioFlow RD puede:

```txt
Consultar inventario por sucursal
Abrir detalle por producto
Ver stock por sucursal
Ver movimientos recientes
Aplicar ajustes manuales controlados
Descontar inventario por venta sin duplicidad
Detectar stock bajo
Notificar cambios por SignalR al negocio correcto
```
