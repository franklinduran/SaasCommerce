# Etapa 11 - Historial, detalle y recibo simple

## Objetivo

Permitir que el usuario consulte ventas reales, revise el detalle operativo y prepare un recibo simple no fiscal desde el navegador:

```txt
POS -> venta creada -> historial -> detalle -> recibo no fiscal -> imprimir
```

## Endpoints Usados

```txt
GET /api/sales
GET /api/sales/{id}
GET /api/business/current
SignalR: sale.statusChanged
```

Reglas aplicadas:

- `GET /api/sales` y `GET /api/sales/{id}` filtran por `BusinessId` en backend.
- El frontend no envia ni valida `BusinessId`.
- Las respuestas siguen el contrato `ApiResponse`.
- Los totales, subtotales, precios y estados mostrados vienen calculados desde backend.
- React no recalcula reglas de negocio criticas.

## Pantallas Creadas

```txt
/pos              POS real de venta
/sales            Historial de ventas
/sales/:saleId    Detalle de venta y recibo simple
```

La ruta `/sales` queda dedicada al historial. El POS real se mueve a `/pos` para separar creacion de venta y consulta posterior.

## Modulo Frontend

```txt
frontend/src/modules/sales/components
frontend/src/modules/sales/hooks
frontend/src/modules/sales/pages
frontend/src/modules/sales/services
frontend/src/modules/sales/types
frontend/src/modules/sales/utils
```

Componentes principales:

- `SalesFilters`
- `SalesTable`
- `SaleStatusBadge`
- `SaleDetailHeader`
- `SaleItemsTable`
- `SaleReceipt`
- `PrintReceiptButton`

El server state usa TanStack Query y el HTTP centralizado usa `shared/services/httpClient`.

## Filtros Disponibles

```txt
Fecha desde
Fecha hasta
Estado
Busqueda por cliente, codigo o SaleId corto
Paginacion
```

Estados visuales cubiertos:

```txt
Loading
Error
Empty state
Lista con datos
Sin resultados por filtros
```

## Detalle De Venta

El detalle muestra:

```txt
Estado
Fecha
Cliente
Sucursal
Metodo de pago
Items vendidos
Cantidad
Precio unitario
Subtotal por item
Total
Razon de error o cancelacion si aplica
```

No se exponen campos tecnicos internos como `BusinessId`, `UserId` o IDs de infraestructura en la UI.

## Recibo Simple

`SaleReceipt` incluye:

```txt
Nombre del negocio
Sucursal
Fecha
Cliente si existe
Productos
Cantidades
Subtotal
Total
Metodo de pago
Estado de venta
Texto "Recibo no fiscal"
```

`PrintReceiptButton` usa la impresion del navegador (`window.print()`), por lo que el usuario puede imprimir o usar "Guardar como PDF" desde el dialogo del sistema.

No incluido en esta etapa:

```txt
NCF
Comprobante fiscal real
Firma digital
Facturacion electronica
Impuestos fiscales complejos
```

Eso queda reservado para una etapa futura de facturacion.

## Actualizacion Realtime

El historial y el detalle escuchan:

```txt
sale.statusChanged
```

Cuando llega un evento:

- Se descartan eventos sin `BusinessId` coincidente.
- En historial se invalida la consulta solo si la venta visible esta afectada.
- En detalle se invalida la venta abierta si el `saleId` coincide.
- TanStack Query refresca los datos desde API para mantener backend como fuente de verdad.

## Backend

Se agrego un read repository para ventas:

```txt
ISaleReadRepository
EfSaleReadRepository
```

Este read model enriquece `SaleResponse` con datos necesarios para historial y detalle:

```txt
CustomerName
BranchName
Code
Items
ProductName
Sku
UnitPrice
Subtotal
FailureReason
PaymentMethod
CreatedAt
```

La consulta sigue aislada por `BusinessId`; `GET /api/sales/{id}` devuelve `404` cuando la venta no pertenece al negocio actual.

## Tests

Frontend:

```txt
SalesHistoryPage should list sales from API
SalesHistoryPage should show empty state when there are no sales
SalesHistoryPage should show error state when API fails
SalesHistoryPage should filter sales by status
SalesHistoryPage should navigate to sale detail
SaleDetailPage should show sale items
SaleDetailPage should show customer when present
SaleDetailPage should show failure reason when sale failed
SaleReceipt should render business name, items and total
SaleReceipt should show non fiscal receipt label
SaleReceipt should not expose internal technical fields
```

Backend:

```txt
GetSales_ShouldReturnOnlyCurrentBusinessSales
GetSales_ShouldApplyStatusFilter_WhenProvided
GetSales_ShouldApplyDateFilters_WhenProvided
GetSales_ShouldSearchByCustomer_WhenQueryProvided
GetSales_ShouldSearchByShortSaleId_WhenQueryProvided
GetSales_ShouldApplyBranchFilter_WhenProvided
GetSales_ShouldSortByTotalAscending_WhenRequested
GetSales_ShouldSortByTotalDescending_WhenRequested
GetSales_ShouldSortByCreatedAtAscending_WhenRequested
GetSaleById_ShouldReturnSaleWithItems_WhenSaleExists
GetSaleById_ShouldReturnNotFound_WhenSaleBelongsToAnotherBusiness
GetSaleById_ShouldIncludeFailureReason_WhenSaleFailed
```

## Validacion Local

Frontend:

```powershell
npm.cmd run lint --prefix .\comercioflow\frontend
npm.cmd run test --prefix .\comercioflow\frontend
npm.cmd run build --prefix .\comercioflow\frontend
```

Resultado:

```txt
Lint OK
Tests OK: 45 passed
Build OK
```

Backend:

```powershell
dotnet build .\comercioflow\SaasCommerce.slnx --no-restore -v minimal
dotnet test .\comercioflow\SaasCommerce.slnx --no-restore -v minimal
dotnet test .\comercioflow\backend\tests\SaasCommerce.Api.Tests\SaasCommerce.Api.Tests.csproj --no-restore -v minimal
```

Resultado:

```txt
Build OK
Tests OK
API tests OK: 53 passed
```

Quality Gate:

```txt
Status: OK
New coverage: 80.1%
New duplicated lines density: 0.59673%
New violations: 0
```

## Resultado

Al cerrar esta etapa, el usuario puede:

```txt
Crear venta desde POS
Ver esa venta en historial
Filtrar ventas por fecha, estado o busqueda
Abrir el detalle
Revisar productos, cantidades, total, cliente y estado
Imprimir o guardar un recibo simple no fiscal
```
