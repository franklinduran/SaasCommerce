# Etapa 18 — Validación: Gestión Operativa de Usuarios

**Fecha:** 2026-05-19
**Rama:** develop (worktree `claude/vigorous-heisenberg-f6842e`)
**Estado:** ✅ COMPLETADA — Quality Gate PASS

---

## Resumen

Esta etapa cierra la funcionalidad de gestión multiusuario iniciada con el framework de seguridad de la Etapa 17. Habilita que un **Owner/Admin** cree, edite, active/desactive y resetee passwords de usuarios dentro de su negocio, con eventos de integración, audit trail, flujo de cambio obligatorio de password y un frontend completo.

En paralelo, se realizó un trabajo intensivo de **cobertura de tests** para llevar el `new_coverage` de SonarQube de **71.5% → 85.3%** sin introducir violaciones nuevas.

---

## Alcance funcional

### HUs implementadas

| HU | Descripción | Endpoint / Pantalla |
|----|-------------|---------------------|
| 18.1 | Listar usuarios del negocio | `GET /api/users` + `UsersPage` |
| 18.2 | Ver detalle de usuario | `GET /api/users/{id}` + `UserDetailPage` |
| 18.3 | Crear usuario (Owner/Admin) | `POST /api/users` + `CreateUserPage` |
| 18.4 | Actualizar perfil (nombre/teléfono/sucursal) | `PUT /api/users/{id}` + `UserDetailPage` |
| 18.5 | Cambiar rol del usuario | `PUT /api/users/{id}/role` |
| 18.6 | Desactivar usuario | `PUT /api/users/{id}/disable` |
| 18.7 | Reactivar usuario | `POST /api/users/{id}/activate` |
| 18.8 | Reset de password (temp password) | `POST /api/users/{id}/reset-password` |
| 18.9 | Cambio obligatorio de password (`MustChangePassword`) | Redirección forzada en `AppShell` |

---

## Archivos creados / modificados

### Backend — Domain

| Archivo | Cambio |
|---------|--------|
| `Modules/Identity/Domain/User.cs` | Propiedad `MustChangePassword`, métodos `Activate()` y `ForcePasswordChange()`. `ChangePasswordHash` limpia el flag. |
| `Modules/Identity/Domain/SystemPermissions.cs` | Nuevos: `UsersCreate`, `UsersUpdate`, `UsersResetPassword`. |
| `Modules/Identity/Domain/RolePermissionMatrix.cs` | Los 3 permisos nuevos sólo en `FullAccess` (Owner/Admin). |

### Backend — Contracts

| Archivo | Descripción |
|---------|-------------|
| `Identity/Contracts/Requests/CreateUserRequest.cs` | DTO de creación |
| `Identity/Contracts/Requests/UpdateUserRequest.cs` | DTO de actualización |
| `Identity/Contracts/Responses/UserDetailResponse.cs` | DTO de detalle |
| `Identity/Contracts/Responses/ResetPasswordResponse.cs` | Password temporal devuelta una sola vez |
| `Identity/Contracts/Responses/LoginResponse.cs` | Incluye `MustChangePassword` |
| `Identity/Contracts/Events/V1/UserCreatedIntegrationEventV1.cs` | Evento creación |
| `Identity/Contracts/Events/V1/UserActivatedIntegrationEventV1.cs` | Evento activación |
| `Identity/Contracts/Events/V1/UserDeactivatedIntegrationEventV1.cs` | Evento desactivación |
| `Identity/Contracts/Events/V1/UserRoleChangedIntegrationEventV1.cs` | Evento cambio de rol |
| `Identity/Contracts/Events/V1/UserPasswordResetIntegrationEventV1.cs` | Evento reset de password |

### Backend — Application (handlers nuevos)

| Archivo | Función |
|---------|---------|
| `Identity/Application/Users/GetUserByIdHandler.cs` | Detalle con filtro por BusinessId |
| `Identity/Application/Users/CreateUserHandler.cs` | Valida email único, hash password, role + outbox + audit |
| `Identity/Application/Users/UpdateUserHandler.cs` | Actualiza perfil + audit |
| `Identity/Application/Users/ActivateUserHandler.cs` | Activa usuario + outbox + audit |
| `Identity/Application/Users/ResetUserPasswordHandler.cs` | Temp password crypto-safe, `MustChangePassword = true`, outbox + audit |

### Backend — Application (handlers retrofitted)

| Archivo | Cambio |
|---------|--------|
| `Identity/Application/Users/DisableUserHandler.cs` | + Outbox (`UserDeactivated`) + audit log |
| `Identity/Application/Users/UpdateUserRoleHandler.cs` | + Outbox (`UserRoleChanged`) + audit (captura rol previo) |
| `Identity/Application/Auth/LoginHandler.cs` | Incluye `MustChangePassword` en respuesta |
| `Identity/Application/Auth/RefreshTokenHandler.cs` | Propaga `MustChangePassword` |
| `Identity/Application/Auth/IdentityResponseMapper.cs` | Mapeo de `MustChangePassword` |
| `Identity/Application/Permissions/IdentityPermissionsErrors.cs` | Nuevos errores: `UserAlreadyActive`, `DuplicateEmail`, `PasswordTooShort`, `InvalidEmail`, `CannotCreateOwnerRole`, `CannotResetOwnPassword` |
| `Identity/Application/Users/IUserManagementRepository.cs` | + `EmailExistsInBusinessAsync` |

### Backend — Infrastructure

| Archivo | Cambio |
|---------|--------|
| `Identity/Infrastructure/Persistence/EfUserManagementRepository.cs` | Implementa `EmailExistsInBusinessAsync` |
| `Identity/Infrastructure/Persistence/Configurations/UserConfiguration.cs` | Mapeo de `MustChangePassword` |
| `BuildingBlocks/Infrastructure/Persistence/Migrations/202605180003_UserManagement.cs` | ALTER `identity.users` ADD COLUMN `must_change_password boolean NOT NULL DEFAULT false` |

### Backend — API

| Archivo | Cambio |
|---------|--------|
| `Api/Endpoints/UsersEndpointExtensions.cs` | 5 endpoints nuevos: GET `/{id}`, POST `/`, PUT `/{id}`, POST `/{id}/activate`, POST `/{id}/reset-password` |
| `Api/Program.cs` | Mapeo de mensajes de error `identity.duplicate_email → CONFLICT`, `identity.cannot_*_self → VALIDATION_ERROR`, etc. |
| `Modules/DependencyInjection.cs` | Registro de los 5 handlers nuevos |

### Frontend

| Archivo | Descripción |
|---------|-------------|
| `modules/users/types.ts` | `UserSummary`, `UserDetail`, `CreateUserRequest`, `UpdateUserRequest`, `ResetPasswordResponse` |
| `modules/users/services/usersApi.ts` | `getUsers`, `getUserById`, `createUser`, `updateUser`, `activateUser`, `disableUser`, `resetPassword`, `updateRole` |
| `modules/users/schemas/userSchemas.ts` | Schemas Zod `createUserSchema`, `updateUserSchema` |
| `modules/users/components/UserTable.tsx` | Tabla con nombre, email, rol, estado, acciones |
| `modules/users/components/RoleSelect.tsx` | Select con 6 roles (sin Owner) |
| `modules/users/components/ResetPasswordDialog.tsx` | Confirmación + muestra temp password una sola vez |
| `modules/users/pages/CreateUserPage.tsx` | Form de creación con react-hook-form + Zod |
| `modules/users/pages/UserDetailPage.tsx` | Detalle + editar + danger zone (desactivar / resetear) |
| `modules/users/UsersPage.tsx` | Tabla real con botón "Crear usuario" |
| `app/router.tsx` | Rutas `/users/new`, `/users/:userId`, `/change-password` |
| `app/LazyPages.tsx` | Lazy imports para las páginas de usuarios |
| `app/AppShell.tsx` | Redirige a `/change-password` si `mustChangePassword=true` |
| `modules/auth/authStore.ts` + `auth/types.ts` | Persiste `mustChangePassword` post-login |
| `shared/types/permissions.ts` | 3 permisos nuevos en el objeto `Permission` |

---

## Endpoints

| Método | Ruta | Permiso requerido |
|--------|------|-------------------|
| GET | `/api/users` | `UsersView` |
| GET | `/api/users/{id:guid}` | `UsersView` |
| POST | `/api/users` | `UsersCreate` |
| PUT | `/api/users/{id:guid}` | `UsersUpdate` |
| PUT | `/api/users/{id:guid}/role` | `UsersUpdateRole` |
| PUT | `/api/users/{id:guid}/disable` | `UsersDisable` |
| POST | `/api/users/{id:guid}/activate` | `UsersDisable` |
| POST | `/api/users/{id:guid}/reset-password` | `UsersResetPassword` |
| GET | `/api/audit-logs` | `AuditView` |
| GET | `/api/me/permissions` | autenticado |

---

## Validación de calidad

### Test suite — estado final

| Proyecto | Tests | Resultado |
|----------|-------|-----------|
| `SaasCommerce.SharedKernel.Tests` | 2 | ✅ |
| `SaasCommerce.BuildingBlocks.Tests` | 23 | ✅ |
| `SaasCommerce.Modules.Tests` | 366 | ✅ |
| `SaasCommerce.Worker.Tests` | 24 | ✅ |
| `SaasCommerce.Architecture.Tests` | 28 | ✅ |
| `SaasCommerce.Api.Tests` | 188 | ✅ |
| **Total** | **631** | **0 fallas** |

Comando: `dotnet test SaasCommerce.slnx -c Release` — 631 tests pasan, 0 fallan, 0 ignorados.

### SonarQube — Quality Gate

| Condición | Umbral | Antes | Final | Estado |
|-----------|--------|-------|-------|--------|
| `new_coverage` | ≥ 80 % | 71.5 % | **85.3 %** | ✅ PASS |
| `new_violations` | = 0 | 0 | **0** | ✅ PASS |
| `new_duplicated_lines_density` | ≤ 3 % | 1.45 % | **1.45 %** | ✅ PASS |
| **Overall** | — | ❌ ERROR | **✅ PASS** | — |

- `new_lines_to_cover`: 4 382
- `new_uncovered_lines`: 985 → **643** (−342 líneas cubiertas)

### Test files añadidos para la validación

| Archivo | Tests | Cobertura objetivo |
|---------|-------|--------------------|
| `Identity/UserManagementTests.cs` | 11 | Handlers Etapa 18 (Create/Update/Activate/Reset/GetById) |
| `Identity/IdentityHandlerTests.cs` | 21 | `DisableUserHandler`, `UpdateUserRoleHandler`, `GetCurrentUserPermissionsHandler`, `GetAuditLogsHandler`, `GetUsersHandler`, `PermissionAuthorizationHandler`, `PermissionService` |
| `Identity/AuditLogInfrastructureTests.cs` | 10 | `EfAuditLogWriter`, `EfAuditLogReadRepository` (filtros + paginación) |
| `ReportsReadRepositoryTests.cs` | 12 | `EfReportsReadRepository` (6 consultas + filtros) |
| `CsvReportExportServiceTests.cs` | 6 | `CsvReportExportService` (5 exports + escape) |
| `CustomerQueryUseCaseTests.cs` | 14 | `GetCustomerCreditMovements`, `GetCustomerCreditSummary`, `ListCustomers` (todas las ramas) |
| `SalesUseCaseTests.cs` | 14 | `ListSales`, `ValidateSaleStock` (todas las ramas) |
| `EfRepositoriesTests.cs` | 18 | `EfUserManagement`, `EfInvoice`, `EfCustomerCredit`, `EfProductInventoryPolicy` |
| `DomainEntityTests.cs` | 46 | `Sale`, `Customer`, `Supplier`, `CustomerCreditAccount`, `CustomerCreditMovement` (guard clauses + transiciones) |
| `InventoryDomainTests.cs` | 12 | `Product.UpdateAverageCost`, `InventoryMovement`, `StockItem` |
| `InventoryRepositoryTests.cs` | 6 | `EfInventoryRepository.HasSaleMovementAsync`, `HasPurchaseMovementAsync`, `CountStockAsync` |
| `Api.Tests/ApiHelpersTests.cs` | ~30 | `ToPublicErrorCode`, `ToFailureStatusCode`, `ToApiError` |
| `Api.Tests/ProgramHelpersTests.cs` | 6 | `GetCorsAllowedOrigins`, `GetAllSystemPermissions`, `CanConnectToRabbitMqAsync` |
| `Api.Tests/RealtimeNotificationConsumerTests.cs` (extendido) | +4 | Consumers de `CustomerCredit*`, `Invoice*` |
| Extensiones a `BillingInvoiceTests`, `CatalogInventoryTests`, `CustomersSalesApiWorkflowTests`, `PurchasingTests` | ~30 | `GenerateInvoice` failure paths, `AdjustInventory` guards, créditos, `GetSuppliers/GetPurchases/GetPurchaseById/UpdateSupplier/CreatePurchase` |

---

## Decisiones de diseño

- **Solo Owner/Admin** pueden crear/editar/resetear usuarios. Supervisor sólo tiene `users.view`.
- **Reset password** genera temp password crypto-safe, retornada al admin una sola vez. El usuario es forzado a cambiarla en su próximo login (`MustChangePassword`).
- **`MustChangePassword`** se persiste en `User` y se incluye en `LoginResponse` / `RefreshResponse` para que el frontend redirija.
- **Email uniqueness per-business** — enforced en código (`EmailExistsInBusinessAsync`) y en DB (índice único existente).
- **No se permite** crear/asignar el rol `Owner` desde el endpoint (`identity.cannot_create_owner_role`).
- **No se permite** que un admin se resetee a sí mismo la password (`identity.cannot_reset_own_password`).
- **Audit log** en todas las operaciones mutativas: `user.created`, `user.updated`, `user.role_changed`, `user.activated`, `user.deactivated`, `user.password_reset`.
- **Outbox** para los 5 integration events, consumidos por el módulo de realtime para refrescar la UI.

---

## Mapeo de errores → HTTP

| Código de dominio | Código público | Status |
|-------------------|----------------|--------|
| `identity.duplicate_email` | `CONFLICT` | 409 |
| `identity.user_already_active` | `VALIDATION_ERROR` | 400 |
| `identity.user_already_inactive` | `VALIDATION_ERROR` | 400 |
| `identity.password_too_short` | `VALIDATION_ERROR` | 400 |
| `identity.invalid_email` | `VALIDATION_ERROR` | 400 |
| `identity.cannot_create_owner_role` | `VALIDATION_ERROR` | 400 |
| `identity.cannot_reset_own_password` | `VALIDATION_ERROR` | 400 |
| `identity.cannot_disable_self` | `VALIDATION_ERROR` | 400 |
| `identity.cannot_remove_last_owner` | `VALIDATION_ERROR` | 400 |
| `identity.invalid_role` | `VALIDATION_ERROR` | 400 |
| `identity.user_not_found` | `NOT_FOUND` | 404 |
| `identity.user_context_required` | `TENANT_CONTEXT_MISSING` | 401 |
| `identity.forbidden` | `FORBIDDEN` | 403 |

---

## Comandos de verificación

```bash
# Build limpio
dotnet build SaasCommerce.slnx -c Release   # 0 warnings, 0 errors

# Tests completos
dotnet test SaasCommerce.slnx -c Release    # 631 passed, 0 failed

# Architecture tests (independientes)
dotnet test backend/tests/SaasCommerce.Architecture.Tests/   # 28/28

# Frontend
npx tsc --noEmit                            # 0 errors

# SonarQube
.\scripts\sonar.ps1                         # Quality Gate: PASS
```

---

## Estado del worktree

Los cambios viven en `develop` (worktree `claude/vigorous-heisenberg-f6842e`) **sin commitear** por instrucción explícita del usuario. Pendiente de revisión y commit final. Resumen:

- **Modificados:** 21 archivos (12 backend + 6 frontend + 3 contracts)
- **Nuevos:** 19 archivos (1 migration + 5 handlers + 5 events + 4 DTOs + UI + tests)
- **Pendiente de commit:** todos los cambios listados arriba

---

## Próximos pasos sugeridos

1. **Commit** del trabajo de Etapa 18 dividido en commits temáticos (domain, application, infrastructure, API, frontend, tests).
2. **Aplicar migración** en ambientes downstream: `dotnet ef database update` o despliegue del contenedor.
3. **Verificación funcional en Docker** siguiendo el patrón de `Etapa-15-validacion-real-docker.md` para confirmar el flujo completo extremo a extremo (login → reset → cambio obligatorio → login con nueva password).
4. **Cobertura adicional opcional** — quedan 643 líneas sin cubrir (15 %), principalmente en `DeductSaleInventoryUseCase`, `RegisterCreditSaleUseCase`, `OutboxPublisher`, y los EF repositories de `Sale`/`Purchase`. Si se prioriza llegar a ≥ 95 %, esos son los siguientes targets.
