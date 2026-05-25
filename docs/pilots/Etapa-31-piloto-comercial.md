# Etapa 31 — Piloto Comercial y Onboarding del Primer Cliente

**Fecha:** 2026-05-24  
**Estado:** ✅ Completado  
**Objetivo:** Habilitar el proceso de onboarding para negocios piloto en producción

---

## Resumen Ejecutivo

La Etapa 31 convierte el MVP técnico en un producto comercializable. Introduce el flujo de registro de negocios piloto por parte del equipo de SaaS, un wizard de onboarding para guiar al cliente en sus primeros pasos, y la importación masiva de productos desde CSV.

---

## Historias de Usuario Implementadas

### HU-31.1: Crear Negocio Piloto (Admin SaaS)
**Endpoint:** `POST /api/admin/pilot-businesses`  
**Permiso:** `saas.manage_businesses` (solo Admin/Owner de la plataforma)

Permite al equipo de soporte registrar un nuevo negocio en período de prueba de 14 días. Crea:
- `Business` + `Branch` (Sucursal Principal)
- `Role` (Admin) + `User` (administrador del negocio)
- `BusinessSubscription` en estado Trial (14 días)

**Reglas de negocio:**
- Email único en la plataforma
- Número de identificación (RNC/Cédula) único por negocio
- Contraseña mínimo 8 caracteres
- Tipos de identificación válidos: `Rnc`, `Cedula`, `Passport`

### HU-31.2: Wizard de Onboarding
**Endpoints:**
- `GET /api/onboarding/status` — Estado actual de los pasos
- `POST /api/onboarding/steps/{step}/complete` — Marcar paso (retorna estado actual)

Los pasos se completan **automáticamente** cuando el dato existe en el sistema:
| Paso | Cómo se completa |
|------|-----------------|
| BusinessInfo | Siempre verdadero (usuario autenticado) |
| Products | Al crear cualquier producto activo |
| Inventory | Al tener StockItem con Quantity > 0 |
| CashSession | Al abrir la primera sesión de caja |

**URL:** `/onboarding`

### HU-31.3: Importación Masiva de Productos (CSV)
**Endpoints:**
- `GET /api/products/import/template` — Descargar plantilla
- `POST /api/products/import` — Subir archivo CSV

**Formato CSV:**
```
Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
Arroz El Gallo 5lbs,ARR-GALL-5LB,Víveres,175.00,140.00,100
```

**Reglas:**
- Máximo 500 filas por importación
- SKUs duplicados son omitidos (no error fatal)
- Categorías nuevas se crean automáticamente
- `StockQuantity > 0` crea movimiento inicial (InitialStock)
- Filas con errores son omitidas; importación parcial retorna 201

**URL:** `/products/import`

---

## Arquitectura Técnica

### Backend
```
Modules/Identity/Application/PilotBusiness/
  CreatePilotBusinessCommand.cs
  CreatePilotBusinessHandler.cs
  CreatePilotBusinessErrors.cs

Modules/Identity/Application/Onboarding/
  OnboardingStep.cs (enum)
  IOnboardingStatusReader.cs
  GetOnboardingStatusHandler.cs
  CompleteOnboardingStepHandler.cs

Modules/Identity/Infrastructure/Onboarding/
  EfOnboardingStatusReader.cs

Modules/Catalog/Application/Import/
  ImportProductsCommand.cs
  ImportProductsHandler.cs
  ImportProductsErrors.cs

Api/Endpoints/
  AdminEndpointExtensions.cs
  OnboardingEndpointExtensions.cs
  ProductImportEndpointExtensions.cs
```

### Frontend
```
modules/onboarding/       → Wizard de onboarding
modules/admin/            → Registro de negocios piloto
modules/products/pages/   → Importación de productos
```

---

## Tests Implementados

| Tipo | Archivo | Tests |
|------|---------|-------|
| Módulo | PilotBusinessTests.cs | 6 |
| Módulo | ImportProductsTests.cs | 7 |
| API | PilotBusinessEndpointTests.cs | 8 |
| API | ProductImportEndpointTests.cs | 7 |
| Frontend | OnboardingPage.test.tsx | 6 |
| Frontend | PilotBusinessPage.test.tsx | 5 |
| Frontend | ProductImportPage.test.tsx | 5 |
| **Total** | | **44 nuevos tests** |

---

## Checklist de Verificación

- [x] `POST /api/admin/pilot-businesses` retorna 201 con datos del negocio
- [x] Intento con email duplicado retorna `PILOT_BUSINESS_DUPLICATE_EMAIL`
- [x] `GET /api/onboarding/status` retorna estado calculado dinámicamente
- [x] `POST /api/onboarding/steps/{step}/complete` valida nombre de paso
- [x] `GET /api/products/import/template` retorna CSV descargable
- [x] `POST /api/products/import` importa productos y crea stock inicial
- [x] Archivo vacío retorna `IMPORT_EMPTY_FILE`
- [x] SKU duplicado omite fila (no error fatal) y retorna 201
- [x] BusinessId siempre se obtiene del JWT, nunca del frontend
- [x] Todos los tests pasan (836 backend, 162 frontend)
