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
npm run build
```

En Windows con PowerShell restringido, usa `npm.cmd`:

```bash
npm.cmd run dev
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
- PostgreSQL: `localhost:5432`

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

## Definition Of Done

- `dotnet build --no-restore` pasa con 0 warnings.
- `dotnet test --no-build` pasa.
- `npm run build` pasa en `frontend`.
- No se introducen issues nuevos de Sonar en codigo nuevo.
- Las dependencias respetan Clean Architecture.
- API y Worker siguen separados.
- Cada funcionalidad nueva declara su modulo dueño.
- Cualquier acceso a datos futuro considera `BusinessId`.
- Eventos criticos futuros deben pasar por Outbox.
- Consumers criticos deben usar Inbox/idempotencia.
- Cobertura objetivo para codigo nuevo: 80% o mas.
