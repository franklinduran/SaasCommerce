# SaasCommerce RD

SaasCommerce RD es un SaaS para comercios dominicanos. Esta base deja listo el repositorio para construir el MVP con Clean Architecture, SOLID, API y Worker separados, multi-tenancy, mensajeria, tiempo real, Sonar y pruebas automatizadas.

## Arquitectura Base

```txt
backend/
  src/
    Api/       ASP.NET Core Web API
    Worker/    Worker Service para eventos y tareas de fondo
    SharedKernel/
    BuildingBlocks/
    Modules/
      Tenancy/
      Identity/
      Catalog/
      Inventory/
      Sales/
      Customers/
      Billing/
      Payments/
      Reporting/

  tests/
    SaasCommerce.Api.Tests/
    SaasCommerce.Architecture.Tests/
    SaasCommerce.BuildingBlocks.Tests/
    SaasCommerce.Modules.Tests/
    SaasCommerce.SharedKernel.Tests/
    SaasCommerce.Worker.Tests/

frontend/      React + Vite + TypeScript
```

La regla principal esta en `ARCHITECTURE_RULES.md`: ningun cambio debe romper Clean Architecture, separacion por contextos, multi-tenancy ni el flujo orientado a eventos.

La base backend esta pensada como monolito modular preparado para extraer microservicios mas adelante. `Api` y `Worker` son hosts; `Modules` contiene el negocio; `BuildingBlocks` contiene abstracciones e infraestructura compartida; `SharedKernel` queda pequeno y solo tiene conceptos universales.

## Stack Oficial

- Backend: .NET 10, ASP.NET Core Web API, Worker Service, EF Core, PostgreSQL, MassTransit, RabbitMQ, SignalR, FluentValidation, Serilog y JWT.
- Frontend: React, Vite, TypeScript, TailwindCSS, ShadCN-style UI, TanStack Query, Zustand, React Hook Form, Zod y SignalR client.
- Testing: xUnit, FluentAssertions, NSubstitute, NetArchTest, Testcontainers y MassTransit Test Harness.
- Local: Docker Compose con PostgreSQL, RabbitMQ, API, Worker y Web.

## Comandos Principales

Backend:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet run --project backend/src/Api/SaasCommerce.Api.csproj
dotnet run --project backend/src/Worker/SaasCommerce.Worker.csproj
```

Frontend:

```bash
cd frontend
npm install
npm run dev
npm run test
npm run build
```

En Windows con PowerShell restringido, usa `npm.cmd`:

```bash
npm.cmd run dev
npm.cmd run test
npm.cmd run build
```

Docker:

```bash
docker compose up --build
```

Servicios locales:

- Web: `http://localhost:5173`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- RabbitMQ Management: `http://localhost:15672`
- PostgreSQL: `localhost:5433` por defecto en host, `5432` dentro de Docker.

## Variables De Entorno

Usa `.env.example` como referencia para el ambiente local:

```txt
ConnectionStrings__DefaultConnection
RabbitMq__Host
RabbitMq__Username
RabbitMq__Password
Jwt__Secret
Jwt__Issuer
Jwt__Audience
VITE_API_BASE_URL
VITE_SIGNALR_HUB_URL
```

## Autenticacion Y Tenancy

Endpoints base:

```txt
POST /api/auth/login
POST /api/auth/refresh
GET  /api/me
```

Usuario seed para desarrollo:

```txt
Email: admin@test.com
Password: Admin123!
BusinessId: 11111111-1111-1111-1111-111111111111
BranchId: 22222222-2222-2222-2222-222222222222
```

Claims obligatorios del JWT:

```txt
sub
nameidentifier
business_id
branch_id
role
```

Regla de seguridad: `BusinessId`, `BranchId` y `UserId` salen del JWT o de `ICurrentUserService`. El frontend nunca es fuente confiable para esos valores, aunque los envie en un request.

## Reglas Para Agentes IA

- Leer `ARCHITECTURE_RULES.md` antes de modificar codigo.
- Identificar el modulo correcto antes de crear clases, endpoints, consumers o componentes.
- No crear casos de uso en una Application global.
- Todo caso de uso debe vivir en `backend/src/Modules/{ModuleName}/Application`.
- No compartir entidades de dominio entre modulos.
- Compartir integraciones mediante `Contracts`, eventos o read models.
- No poner logica de negocio en API, Worker, controllers, consumers, hubs ni EF Core.
- Centralizar acceso HTTP en `frontend/src/shared/services/httpClient.ts`.
- Centralizar SignalR en `frontend/src/shared/services/signalrClient.ts`.
- Mantener pruebas de arquitectura actualizadas cuando cambien capas o referencias.

## Pruebas De Arquitectura

`backend/tests/SaasCommerce.Architecture.Tests` protege estas reglas:

- SharedKernel no depende de BuildingBlocks, Modules, EF Core, MassTransit ni ASP.NET Core.
- BuildingBlocks.Application no depende de Infrastructure ni frameworks tecnicos.
- Los dominios de modulos no dependen de dominios de otros modulos.
- Los contratos de modulos no dependen de Infrastructure.
- SharedKernel, BuildingBlocks y Modules no dependen de API ni Worker.
- No existe una `SaasCommerce.Application` global para casos de uso.

## Proteccion De Etapa 3

La base modular queda protegida con pruebas ejecutables:

- `Architecture.Tests` valida limites entre modulos, hosts sin dominio interno y ausencia de Application global.
- `Modules.Tests` valida eventos de integracion versionados con `EventId`, `CorrelationId`, `BusinessId`, `OccurredAt` y `Version`.
- `BuildingBlocks.Tests` valida Inbox, `EfInboxStore` e `IdempotentConsumer`.
- `Api.Tests` valida `CorrelationIdMiddleware` y el contrato JSON `isSuccess/data/error`.
- El frontend debe consumir `isSuccess`, `data` y `error`; no debe volver a `succeeded/errors`.

## Proteccion De Etapa 4

- `Tenancy` contiene `Business` y `Branch`.
- `Identity` contiene `User`, `Role`, `RefreshToken`, login, refresh token y consulta de usuario actual.
- `Api` expone `/api/auth/login`, `/api/auth/refresh` y `/api/me` sin logica de negocio.
- `CurrentUserService` lee `UserId`, `BusinessId`, `BranchId` y roles desde claims.
- El frontend tiene `LoginPage`, `LoginForm`, `authService`, `useLoginMutation`, `authStore` y ruta protegida.
- Tests backend validan login correcto, password incorrecto, claims JWT, `/api/me` sin token, `/api/me` con token y lectura de `BusinessId` desde claims.
- Tests frontend validan errores de formulario en `LoginForm`.

## Etapa 5: Catalog E Inventory Base

- `Catalog` contiene `Product`, `Category`, `ProductComponent`, contratos de requests/responses y eventos versionados `ProductCreatedIntegrationEventV1` y `ProductUpdatedIntegrationEventV1`.
- `Inventory` contiene `StockItem`, `InventoryMovement`, razones de movimiento, contratos de ajuste/stock/movimientos y eventos versionados `InventoryAdjustedIntegrationEventV1` y `StockReservedIntegrationEventV1`.
- `Product` soporta tipos `Simple`, `Service`, `Weighed`, `Composite`, `VariantParent` y `VariantChild`.
- `Product` administra datos comerciales reales: descripcion, SKU, barcode, unidad de medida, precio venta, costo, mayorista, precio minimo, impuestos, codigos internos/proveedor, variantes y atributos JSON.
- `Product` define su politica de inventario: `TrackInventory`, `MinimumStock`, `MaximumStock`, `ReorderPoint` y `AllowNegativeStock`.
- `Inventory` administra existencias y movimientos: cantidad anterior, cantidad nueva, cantidad del ajuste, razon, usuario y tenant.
- `Inventory` consulta la politica publica de inventario del producto antes de ajustar stock; servicios y productos sin `TrackInventory` no aceptan ajustes.
- `Inventory` no referencia infraestructura de `Catalog`; usa contratos publicos/read models como frontera.
- La API expone endpoints autenticados:

```txt
POST /api/catalog/products
PUT  /api/catalog/products/{id}
GET  /api/catalog/products/{id}
GET  /api/catalog/products
POST /api/inventory/adjustments
GET  /api/inventory/stock
GET  /api/inventory/movements
```

- El frontend tiene pantallas funcionales para `Productos` e `Inventario`, usando TanStack Query, servicios centralizados, formularios con React Hook Form + Zod, estados loading/error/empty y contrato `isSuccess/data/error`.
- La UI de producto esta organizada por secciones: datos generales, precio/costos, inventario, codigos, impuestos/opciones y opciones avanzadas.
- Tests backend validan creacion de productos, SKU duplicado por tenant, barcode duplicado, producto servicio sin inventario, producto pesado sin unidad `Unit`, ajuste de inventario, movimiento generado y proteccion contra stock negativo.

## Etapa 5.2: UX De Productos Y Paginacion

- `/products` prioriza busqueda, filtros, listado y paginacion; no muestra formularios grandes en el flujo principal.
- Crear y editar productos se hace en un drawer lateral reutilizando el mismo formulario por secciones.
- El listado soporta filtros por `query`, `productType`, `isActive`, `categoryId`, `page` y `pageSize`.
- `GET /api/catalog/products` devuelve `items`, `page`, `pageSize`, `totalItems` y `totalPages`.
- La tabla incluye producto, tipo, SKU/barcode, unidad, precio, politica de stock, estado y acciones.
- La UI incluye skeleton de tabla, empty state, error state con reintento, paginacion anterior/siguiente y selector de 10, 25 o 50 registros.

## Definition Of Done

- `dotnet build --no-restore` pasa con 0 warnings.
- `dotnet test --no-build` pasa.
- `npm run build` pasa en `frontend`.
- `npm run test` pasa en `frontend` cuando hay pruebas de UI.
- No se introducen issues nuevos de Sonar en codigo nuevo.
- Las dependencias respetan Clean Architecture.
- API y Worker siguen separados.
- Cada funcionalidad nueva declara su modulo dueño.
- Cualquier acceso a datos futuro considera `BusinessId`.
- Eventos criticos futuros deben pasar por Outbox.
- Consumers criticos deben usar Inbox/idempotencia.
- Cobertura objetivo para codigo nuevo: 80% o mas.
