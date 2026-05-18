# Etapa 16 — Reportes, Dashboard Operativo y Métricas del Negocio

**Fecha:** 2026-05-18  
**Rama:** develop  
**Estado:** ✅ COMPLETADA

---

## Resumen

Implementación completa del módulo de Reportes y Dashboard Operativo. Cubre las 8 HUs especificadas: dashboard en tiempo real, reportes de ventas, facturas, cuentas por cobrar, bajo stock, compras, actualización SignalR e exportación CSV.

---

## Archivos creados / modificados

### Backend — Contratos (DTOs)

| Archivo | Descripción |
|---------|-------------|
| `Modules/Reporting/Contracts/Responses/DashboardSummaryResponse.cs` | Respuesta del dashboard con 7 sub-DTOs |
| `Modules/Reporting/Contracts/Responses/SalesReportResponse.cs` | Reporte de ventas paginado + resumen |
| `Modules/Reporting/Contracts/Responses/InvoiceReportResponse.cs` | Reporte de facturas paginado + resumen |
| `Modules/Reporting/Contracts/Responses/AccountsReceivableReportResponse.cs` | Cuentas por cobrar paginadas + resumen |
| `Modules/Reporting/Contracts/Responses/LowStockReportResponse.cs` | Bajo stock paginado |
| `Modules/Reporting/Contracts/Responses/PurchaseReportResponse.cs` | Compras paginadas + resumen |

### Backend — Application

| Archivo | Descripción |
|---------|-------------|
| `Modules/Reporting/Application/Abstractions/IReportsReadRepository.cs` | Interfaz con 6 métodos de consulta + 5 criterias record |
| `Modules/Reporting/Application/Abstractions/IReportExportService.cs` | Interfaz con 5 métodos de exportación CSV |
| `Modules/Reporting/Application/ReportingErrors.cs` | Error `reporting.user_context_required` |
| `Modules/Reporting/Application/Dashboard/GetDashboardSummaryHandler.cs` | Query + Handler del dashboard |
| `Modules/Reporting/Application/Reports/GetSalesReportHandler.cs` | Query + Handler ventas |
| `Modules/Reporting/Application/Reports/GetInvoiceReportHandler.cs` | Query + Handler facturas |
| `Modules/Reporting/Application/Reports/GetAccountsReceivableReportHandler.cs` | Query + Handler C/C |
| `Modules/Reporting/Application/Reports/GetLowStockReportHandler.cs` | Query + Handler bajo stock |
| `Modules/Reporting/Application/Reports/GetPurchaseReportHandler.cs` | Query + Handler compras |

### Backend — Infrastructure

| Archivo | Descripción |
|---------|-------------|
| `Modules/Reporting/Infrastructure/Persistence/EfReportsReadRepository.cs` | Implementación EF Core con queries cross-module. Dashboard, 5 reportes con filtros y paginación |
| `Modules/Reporting/Infrastructure/Export/CsvReportExportService.cs` | Exportación CSV con StringBuilder. 5 formatos |

### Backend — API y DI

| Archivo | Descripción |
|---------|-------------|
| `Api/Endpoints/ReportEndpointRequests.cs` | 5 clases de request con `[AsParameters]` |
| `Api/Program.cs` | 11 endpoints nuevos (dashboard + 5 reportes + 5 exportaciones CSV) |
| `Modules/DependencyInjection.cs` | 8 registros DI nuevos (repositorio + export + 6 handlers) |

### Tests

| Archivo | Tests | Resultado |
|---------|-------|-----------|
| `tests/SaasCommerce.Modules.Tests/ReportingTests.cs` | 15 unit tests | ✅ 145 total passed |
| `tests/SaasCommerce.Api.Tests/DashboardReportsEndpointTests.cs` | 20 API tests | ✅ 96 total passed |

### Frontend

| Archivo | Descripción |
|---------|-------------|
| `frontend/src/modules/dashboard/types.ts` | Tipos TS: DashboardSummary y sub-tipos |
| `frontend/src/modules/dashboard/services/dashboardApi.ts` | `getDashboardSummary()` con token |
| `frontend/src/modules/dashboard/hooks/useDashboard.ts` | `useDashboardSummary()` + `useDashboardRealtimeInvalidation()` |
| `frontend/src/modules/dashboard/DashboardPage.tsx` | Dashboard real: 4 métricas, 3 tablas recientes, alerta C/C |
| `frontend/src/modules/reports/types.ts` | Tipos TS: 5 reportes + filtros |
| `frontend/src/modules/reports/services/reportsApi.ts` | 5 API calls + 5 URLs de exportación CSV |
| `frontend/src/modules/reports/hooks/useReports.ts` | 5 hooks TanStack Query |
| `frontend/src/modules/reports/ReportsPage.tsx` | Página tabbed con 5 sub-reportes, filtros y exportación |

---

## Endpoints implementados

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/dashboard/summary` | Resumen operativo del día | ✅ JWT |
| GET | `/api/reports/sales` | Reporte de ventas paginado | ✅ JWT |
| GET | `/api/reports/sales/export` | Exportar ventas CSV | ✅ JWT |
| GET | `/api/reports/invoices` | Reporte de facturas paginado | ✅ JWT |
| GET | `/api/reports/invoices/export` | Exportar facturas CSV | ✅ JWT |
| GET | `/api/reports/accounts-receivable` | Cuentas por cobrar paginadas | ✅ JWT |
| GET | `/api/reports/accounts-receivable/export` | Exportar C/C CSV | ✅ JWT |
| GET | `/api/reports/inventory-low-stock` | Bajo stock paginado | ✅ JWT |
| GET | `/api/reports/inventory-low-stock/export` | Exportar bajo stock CSV | ✅ JWT |
| GET | `/api/reports/purchases` | Reporte de compras paginado | ✅ JWT |
| GET | `/api/reports/purchases/export` | Exportar compras CSV | ✅ JWT |

---

## Validación técnica

### Build
```
dotnet build  →  0 errors, 0 warnings (todos los proyectos)
npx tsc --noEmit  →  0 errors
```

### Tests
```
SaasCommerce.Modules.Tests:  145 passed, 0 failed
SaasCommerce.Api.Tests:       96 passed, 0 failed
```

---

## Patrones clave usados

- **Multi-tenancy**: `ICurrentUserService.BusinessId` extraído del JWT. Nunca se acepta del frontend.
- **Cross-module EF**: `AppDbContext.Set<Sale>()`, `Set<Invoice>()`, `Set<Product>()` etc. desde el repositorio de Reporting — posible porque todos los módulos están en el mismo `.csproj`.
- **Value objects en LINQ**: `new BusinessId(id)` construido antes del `Where()` para comparación correcta.
- **SignalR invalidation**: Dashboard escucha 6 eventos (`sale.statusChanged`, `invoice.generated`, `invoice.cancelled`, `payment.registered`, `inventory.lowStock`, `purchase.received`) y invalida la query.
- **CSV export**: `Results.File(bytes, "text/csv", filename)` en endpoints de exportación.
- **Paginación**: Todos los reportes usan `Skip/Take` con `TotalItems/TotalPages/HasNextPage/HasPreviousPage`.

---

## HUs cubiertas

| HU | Descripción | Estado |
|----|-------------|--------|
| HU-16-01 | Dashboard de ventas del día | ✅ |
| HU-16-02 | Reporte de ventas con filtros | ✅ |
| HU-16-03 | Reporte de facturas | ✅ |
| HU-16-04 | Cuentas por cobrar | ✅ |
| HU-16-05 | Reporte de bajo stock | ✅ |
| HU-16-06 | Reporte de compras | ✅ |
| HU-16-07 | Actualización en tiempo real (SignalR) | ✅ |
| HU-16-08 | Exportación CSV de todos los reportes | ✅ |
