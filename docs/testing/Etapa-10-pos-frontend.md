# Etapa 10 - POS Frontend con venta real

## Objetivo

Exponer el primer flujo visible de venta desde React:

```txt
POS -> carrito -> POST /api/sales -> Sale Received -> Saga/Worker -> SignalR + status sync -> Completed/Failed
```

La pantalla vive en `frontend/src/modules/pos` y se monta desde la ruta protegida `/sales`.

## Arquitectura Frontend

```txt
modules/pos/components  UI del POS
modules/pos/hooks       TanStack Query, carrito y SignalR
modules/pos/services    API calls centralizados
modules/pos/store       Zustand para carrito local
modules/pos/types       Contratos TypeScript del POS
modules/pos/pages       POSPage
```

Reglas aplicadas:

```txt
Server state usa TanStack Query.
Estado local del carrito usa Zustand.
HTTP usa shared/services/httpClient.
Realtime usa shared/services/signalrClient.
El frontend no envia BusinessId.
El frontend no envia total ni precio como fuente de verdad.
El total mostrado en POS es estimado; backend calcula el total final.
```

## Flujo De Venta

1. El cajero abre `/sales`.
2. POS carga productos desde `GET /api/catalog/products`.
3. POS carga clientes desde `GET /api/customers`.
4. El cajero agrega productos al carrito.
5. El cajero selecciona cliente opcional.
6. POS valida con Zod que haya `branchId`, `paymentMethod` e items validos.
7. POS llama `POST /api/sales`.
8. API responde `SaleResponse` con `saleId`, `status` y `total`.
9. POS muestra `Received` o `Processing`.
10. `useSaleStatusSubscription` escucha `sale.statusChanged`.
11. Mientras la venta no es terminal, `useSaleForPOS` sincroniza `GET /api/sales/{id}` como respaldo operativo.
12. Si llega `Completed`, POS limpia carrito.
13. Si llega `Failed`, POS conserva carrito y muestra razon.

## SignalR

Evento escuchado:

```txt
sale.statusChanged
```

Validaciones del hook:

```txt
payload.saleId debe coincidir con la venta actual.
payload.businessId debe coincidir con el BusinessId del usuario autenticado.
```

El cliente SignalR centralizado conserva listeners registrados antes de que la conexion termine de iniciar, para evitar perder handlers cuando la pantalla POS monta junto al bridge de sesion.

Nota local: en Docker, API y Worker corren separados. El listener SignalR queda integrado, pero la sincronizacion por `GET /api/sales/{id}` evita que el POS quede detenido en `Received` si el evento final sale desde el Worker antes de existir un backplane realtime compartido entre procesos.

## Tests Agregados

```txt
posCartStore.test.ts
POSComponents.test.tsx
POSPage.test.tsx
```

Cobertura funcional:

```txt
Carrito vacio.
Agregar el mismo producto incrementa cantidad.
Disminuir cantidad elimina al llegar a cero.
Subtotal calculado localmente.
Busqueda de productos.
Estados Completed y Failed.
Carga productos desde API con MSW.
Carga clientes desde API con MSW.
POST /api/sales con carrito valido.
Validacion de carrito vacio.
Error de API al crear venta.
Evento SignalR Completed limpia carrito.
Evento SignalR Failed conserva carrito.
Polling de estado actualiza Failed si no llega realtime.
Request de venta no contiene BusinessId.
```

## Validacion Local

Frontend:

```powershell
npm.cmd --prefix .\frontend run lint
npm.cmd --prefix .\frontend run test
npm.cmd --prefix .\frontend run build
```

Backend:

```powershell
$env:NUGET_PACKAGES='C:\Users\frank\.nuget\packages'
dotnet restore .\SaasCommerce.slnx
dotnet build .\SaasCommerce.slnx --no-restore -m:1 /nr:false -v minimal
dotnet test .\SaasCommerce.slnx --no-build -m:1 /nr:false -v minimal
```

Docker:

```powershell
docker compose up --build -d
```

Evidencia esperada en navegador:

```txt
/sales carga productos.
Agregar producto actualiza carrito.
Procesar venta muestra SaleId y estado Received.
SignalR o la sincronizacion de estado actualiza a Completed o Failed.
Completed limpia carrito.
Failed mantiene carrito.
```
