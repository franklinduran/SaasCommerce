# PASO 4: Aplicar Límites de Suscripción en Handlers Existentes

**Fecha:** 2026-05-22  
**Status:** 🔄 En Ejecución

## Resumen

PASO 4 integra el sistema de suscripciones en la lógica de negocio existente, agregando validaciones de límites antes de permitir operaciones.

## Patrón de Modificación

Todos los handlers siguen este patrón:

```csharp
// 1. Inyectar el límite checker
public sealed class CreateXxxHandler(
  // ... otros dependencies ...
  ISubscriptionLimitChecker limitChecker)  // ← Agregar esta línea
{
  // 2. Agregar validación al inicio del HandleCore
  private async Task<Result<XxxResponse>> HandleCoreAsync(...)
  {
    if (currentUser.BusinessId is not Guid businessId)
      return Result.Failure<XxxResponse>(XxxErrors.UserContextRequired);
    
    var tenantId = new BusinessId(businessId);
    
    // ← AGREGAR AQUÍ:
    var limitCheck = await limitChecker.CanCreateXxxAsync(tenantId, cancellationToken);
    if (!limitCheck.IsAllowed)
    {
      return Result.Failure<XxxResponse>(
        new Error("SUBSCRIPTION_LIMIT_REACHED", limitCheck.Message));
    }
    
    // ... continuar con lógica existente ...
  }
}
```

## Handlers a Modificar

### ✅ COMPLETADO
- [x] CreateBranchHandler (`Modules/Tenancy/Application/Branches/CreateBranchHandler.cs`)
  - Usa: `limitChecker.CanCreateBranchAsync()`
  - Error: "SUBSCRIPTION_LIMIT_REACHED" si alcanzó límite

### ⏳ PENDIENTE

#### 1. CreateUserHandler
- **Archivo:** `Modules/Identity/Application/Users/CreateUserHandler.cs`
- **Método:** `limitChecker.CanCreateUserAsync(tenantId, cancellationToken)`
- **Punto de Validación:** Antes de crear el usuario

#### 2. CreateProductHandler
- **Archivo:** `Modules/Catalog/Application/Products/CreateProductHandler.cs`
- **Método:** `limitChecker.CanCreateProductAsync(tenantId, cancellationToken)`
- **Punto de Validación:** Antes de crear el producto

#### 3. CreateSaleHandler (SaleHandlerContext o principal handler)
- **Archivo:** `Modules/Sales/Application/Sales/CreateSaleUseCase.cs` (o handler similar)
- **Método:** `limitChecker.CanCreateSaleAsync(tenantId, cancellationToken)`
- **Nota:** Verifica ventas del mes actual, se resetea automáticamente
- **Punto de Validación:** Antes de crear la venta

#### 4. CreateInventoryTransferHandler
- **Archivo:** `Modules/Inventory/Application/Transfers/CreateInventoryTransferHandler.cs`
- **Método:** `limitChecker.CanUseInventoryTransfersAsync(tenantId, cancellationToken)`
- **Punto de Validación:** Antes de permitir crear transferencia

### Métodos Adicionales (Opcionales para MVP)

Para funcionalidades avanzadas, usar:
- `limitChecker.CanUseAdvancedReportsAsync()` - En handlers de reportes
- `limitChecker.CanUseAuditLogsAsync()` - En handlers de auditoría

## Validación de Features (Alternativa)

Para validaciones más complejas, usar la política de acceso:

```csharp
var accessCheck = await accessPolicy.EnsureCanUseFeatureAsync(
  tenantId, 
  SubscriptionFeature.InventoryTransfers, 
  cancellationToken);

if (accessCheck.IsFailure)
  return Result.Failure<Response>(accessCheck.Error);
```

## Testing

### Test Case 1: Crear rama - Límite alcanzado
```
Plan: BASIC (máx 1 sucursal)
Acción: Intentar crear 2da sucursal
Resultado: Error "SUBSCRIPTION_LIMIT_REACHED"
```

### Test Case 2: Cambiar plan - Límite permite
```
Plan: BASIC → PRO
Acción: Crear 3 sucursales (PRO permite 3)
Resultado: Éxito
```

### Test Case 3: Feature bloqueada
```
Plan: BASIC (sin InventoryTransfers)
Acción: Intentar crear transferencia
Resultado: Error "SUBSCRIPTION_FEATURE_NOT_AVAILABLE"
```

## Notas Técnicas

- Las validaciones son **no-bloqueantes** en lógica existente
- Retornan `SubscriptionLimitCheckResult` con detalles de límite actual vs máximo
- Los límites se validan en **tiempo de ejecución** contra BD actual
- El límite de ventas se resetea automáticamente cada mes

## DI Registration

Los servicios ya están registrados en `Modules/DependencyInjection.cs`:
```csharp
services.AddScoped<ISubscriptionLimitChecker, SubscriptionLimitChecker>();
services.AddScoped<ISubscriptionAccessPolicy, SubscriptionAccessPolicy>();
```

## Checklist de Completitud

- [ ] CreateBranchHandler: Limitar sucursales
- [ ] CreateUserHandler: Limitar usuarios
- [ ] CreateProductHandler: Limitar productos
- [ ] CreateSaleHandler: Limitar ventas/mes
- [ ] CreateInventoryTransferHandler: Bloquear si no permitido
- [ ] Verificar compilación: `dotnet build`
- [ ] Ejecutar tests: `dotnet test --filter Subscription`
- [ ] E2E: Crear 2 sucursales en BASIC → debe fallar en 2da

---

**Próximo:** PASO 5 - Frontend Subscription Module
