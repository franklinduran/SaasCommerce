# Etapa 22 — Plan de Validación Real Multi-sucursal

**Fecha de inicio:** 2026-05-22  
**Objetivo:** Validar flujo end-to-end de transferencias multi-sucursal en ambiente real  
**Estado:** 🔄 En Ejecución

---

## 📋 Pre-requisitos

- [ ] Docker Compose levantado (PostgreSQL, RabbitMQ, API, Worker, Web)
- [ ] API respondiendo en http://localhost:8080
- [ ] Frontend accesible en http://localhost:5173
- [ ] RabbitMQ Management en http://localhost:15672
- [ ] PostgreSQL en localhost:5433

---

## 🧪 Datos de Prueba Necesarios

```
BusinessId: {se genera en registro}
Usuario: admin@test.com / Admin123!

Sucursales:
- Sucursal A: "Almacén Principal" (MAIN)
- Sucursal B: "Tienda Centro" (CENTER)

Productos:
- Arroz Selecto 10 lb (SKU: ARROZ-001)
- Aceite Crisol 1 galón (SKU: ACEITE-001)
- Leche Rica 1 litro (SKU: LECHE-001)

Stock Inicial:
- Sucursal A: Arroz=50, Aceite=30, Leche=25
- Sucursal B: Arroz=5, Aceite=10, Leche=20
```

---

## ✅ HU-22.1 — Validar Creación y Gestión de Sucursales

### Backend/API

**Endpoint:** `POST /api/branches`

```bash
# Crear sucursal principal
curl -X POST http://localhost:8080/api/branches \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Almacén Principal",
    "code": "MAIN",
    "isMain": true
  }'

# Crear sucursal secundaria
curl -X POST http://localhost:8080/api/branches \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Tienda Centro",
    "code": "CENTER",
    "isMain": false
  }'
```

**Validaciones:**
- [ ] Status 201 Created
- [ ] Response contiene `id`, `name`, `code`, `isMain`, `isActive`
- [ ] `BusinessId` no viene en request (servidor lo añade)
- [ ] No se duplica código dentro del mismo BusinessId

**Endpoint:** `GET /api/branches`

```bash
curl -X GET http://localhost:8080/api/branches \
  -H "Authorization: Bearer {token}"
```

**Validaciones:**
- [ ] Status 200 OK
- [ ] Array contiene ambas sucursales
- [ ] Cada sucursal tiene `code` y `isMain` correcto

**Endpoint:** `PUT /api/branches/{id}`

```bash
curl -X PUT http://localhost:8080/api/branches/{branchId} \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Almacén Principal Actualizado",
    "isActive": true
  }'
```

**Validaciones:**
- [ ] Status 200 OK
- [ ] Cambios aplicados

### Frontend - `/branches`

**Validaciones:**
- [ ] [ ] Carga sin errores de consola
- [ ] [ ] Muestra tabla con sucursales
- [ ] [ ] Botón "Crear sucursal" funciona
- [ ] [ ] Formulario valida nombre y código requeridos
- [ ] [ ] Badge "Principal" aparece en sucursal isMain=true
- [ ] [ ] Estado cargando, error y vacío se muestran correctamente
- [ ] [ ] Editar sucursal abre diálogo
- [ ] [ ] Activar/desactivar sucursal funciona

---

## ✅ HU-22.2 — Validar Inventario Separado por Sucursal

### Backend/API

**Endpoint:** `GET /api/inventory?branchId={branchId}`

```bash
# Inventario Sucursal A
curl -X GET "http://localhost:8080/api/inventory?branchId={branchAId}" \
  -H "Authorization: Bearer {token}"

# Inventario Sucursal B
curl -X GET "http://localhost:8080/api/inventory?branchId={branchBId}" \
  -H "Authorization: Bearer {token}"
```

**Validaciones:**
- [ ] Sucursal A muestra su stock (Arroz=50, Aceite=30, Leche=25)
- [ ] Sucursal B muestra su stock (Arroz=5, Aceite=10, Leche=20)
- [ ] Stocks NO se mezclan
- [ ] Si enviamos `branchId` de otro BusinessId → 403 Forbidden

### Frontend - `/inventory`

**Validaciones:**
- [ ] Muestra selector de sucursal
- [ ] Cambiar sucursal actualiza listado
- [ ] Columna "Stock" muestra cantidad correcta por sucursal
- [ ] Filtro "Bajo Stock" aplica por sucursal

---

## ✅ HU-22.3 — Validar Transferencia Exitosa

### Backend/API

**Endpoint:** `POST /api/inventory-transfers`

```bash
curl -X POST http://localhost:8080/api/inventory-transfers \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "sourceBranchId": "{branchAId}",
    "targetBranchId": "{branchBId}",
    "items": [
      {
        "productId": "{productId}",
        "quantity": 10
      }
    ],
    "note": "Transferencia de validación"
  }'
```

**Validaciones Después de Crear:**
- [ ] Status 201 Created
- [ ] TransferId generado
- [ ] Estado inicial = "Pending"
- [ ] Evento publicado en Outbox

**Validaciones Después de ~5 seg (Worker procesa):**
- [ ] Estado cambia a "Completed"
- [ ] Stock Sucursal A: Arroz=40 (bajó 10)
- [ ] Stock Sucursal B: Arroz=15 (subió 10)
- [ ] Movimientos creados:
  - TransferOut en Sucursal A
  - TransferIn en Sucursal B

```bash
# Verificar movimientos
curl -X GET "http://localhost:8080/api/inventory/movements?branchId={branchAId}" \
  -H "Authorization: Bearer {token}"
```

### Frontend - `/inventory-transfers`

**Validaciones:**
- [ ] Carga sin errores
- [ ] Botón "Nueva transferencia" funciona
- [ ] Formulario permite seleccionar:
  - Sucursal origen
  - Sucursal destino
  - Productos y cantidades
- [ ] Transferencia aparece en listado con estado "Pending"
- [ ] Después de procesar, estado cambia a "Completed" sin recargar
- [ ] Abrir detalle muestra productos y cantidades

---

## ✅ HU-22.4 — Validar Transferencia Fallida

### Backend/API

**Crear transferencia con stock insuficiente:**

```bash
curl -X POST http://localhost:8080/api/inventory-transfers \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "sourceBranchId": "{branchAId}",
    "targetBranchId": "{branchBId}",
    "items": [
      {
        "productId": "{productId}",
        "quantity": 100  # Más que stock disponible (40)
      }
    ]
  }'
```

**Validaciones:**
- [ ] Transferencia se crea (estado Pending)
- [ ] Worker detecta stock insuficiente
- [ ] Estado cambia a "Failed"
- [ ] Stock NO cambia (Sucursal A sigue en 40)
- [ ] NO se crean movimientos
- [ ] Logs muestran "Insufficient stock"

### Frontend

**Validaciones:**
- [ ] Transferencia muestra estado "Failed"
- [ ] Badge roja en listado
- [ ] Detalle muestra razón del fallo

---

## ✅ HU-22.5 — Validar Idempotencia

### Procedimiento

1. Obtener evento de transferencia exitosa (HU-22.3)
2. Simular reprocesamiento (puede ser manual o mediante RabbitMQ)
3. Verificar que NO se duplican efectos

**Validaciones:**
- [ ] Procesamiento duplicado NO crea segundo TransferOut
- [ ] Procesamiento duplicado NO crea segundo TransferIn
- [ ] Stock final permanece igual (no se duplica)
- [ ] Logs indican "Message already processed" o "Idempotent key already exists"
- [ ] Consumer termina sin error

---

## ✅ HU-22.6 — Validar SignalR

### Setup

1. Abrir Frontend con DevTools abierto
2. Monitorear Network → WS (WebSocket)
3. Crear transferencia desde otra pestaña o usuario

### Validaciones

**Evento:** `inventoryTransfer.created`
- [ ] Se recibe cuando se crea transferencia
- [ ] Payload contiene: `id`, `status`, `sourceBranchId`, `targetBranchId`

**Evento:** `inventoryTransfer.completed`
- [ ] Se recibe cuando Worker completa transferencia
- [ ] Frontend actualiza estado sin recargar

**Evento:** `inventory.updated`
- [ ] Se recibe cuando stock cambia
- [ ] Frontend invalida query de inventario
- [ ] Listado se refresca

**Grupos correctos:**
- [ ] Mensaje enviado a `business-{businessId}`
- [ ] Mensaje enviado a `branch-{sourceBranchId}`
- [ ] Mensaje enviado a `branch-{targetBranchId}`
- [ ] NO es broadcast global

---

## ✅ HU-22.7 — Documentar Evidencia

Ver: `docs/testing/Etapa-22-validacion-real-multisucursal.md` (generado después de todas las pruebas)

---

## 🔍 Checklists Técnicos

### Comandos Útiles para Debugging

```bash
# Ver logs de API
docker logs saascommerce-api -f

# Ver logs de Worker
docker logs saascommerce-worker -f

# Conectar a PostgreSQL
psql -h localhost -p 5433 -U saas_rd_user -d saas_rd_db

# Ver colas RabbitMQ
# Ir a: http://localhost:15672 (guest/guest)

# Limpiar RabbitMQ (si es necesario)
docker exec saascommerce-rabbitmq rabbitmqctl purge_queue inventory.transfers
```

### Queries SQL Útiles

```sql
-- Ver todas las sucursales
SELECT id, business_id, "name", code, is_main, is_active 
FROM branches 
ORDER BY is_main DESC;

-- Ver stock por sucursal
SELECT si.id, p.name, p.sku, si.quantity, b.code 
FROM stock_items si
JOIN products p ON si.product_id = p.id
JOIN branches b ON si.branch_id = b.id
ORDER BY b.code, p.name;

-- Ver movimientos de inventario
SELECT im.id, p.name, im.reason, im.quantity, b.code, im.created_at 
FROM inventory_movements im
JOIN products p ON im.product_id = p.id
JOIN branches b ON im.branch_id = b.id
ORDER BY im.created_at DESC;

-- Ver transferencias
SELECT id, source_branch_id, target_branch_id, status, created_at 
FROM inventory_transfers 
ORDER BY created_at DESC;

-- Ver Outbox (eventos pendientes)
SELECT id, aggregate_type, event_type, created_at, processed_at 
FROM outbox_messages 
WHERE processed_at IS NULL;

-- Ver Inbox (idempotencia)
SELECT id, aggregate_id, event_type, processed_at 
FROM inbox_messages 
ORDER BY processed_at DESC LIMIT 10;
```

---

## 📊 Estado de Validación

| HU | Estado | Notas |
|----|--------|-------|
| HU-22.1 | ⏳ Pendiente | Esperando Docker |
| HU-22.2 | ⏳ Pendiente | Esperando Docker |
| HU-22.3 | ⏳ Pendiente | Esperando Docker |
| HU-22.4 | ⏳ Pendiente | Esperando Docker |
| HU-22.5 | ⏳ Pendiente | Esperando Docker |
| HU-22.6 | ⏳ Pendiente | Esperando Docker |
| HU-22.7 | ⏳ Pendiente | Esperando Docker |

---

## 🚀 Próximos Pasos

1. ✅ Levantar Docker
2. → Validar healthchecks
3. → HU-22.1: Crear sucursales
4. → HU-22.2: Validar inventario separado
5. → HU-22.3: Transferencia exitosa
6. → HU-22.4: Transferencia fallida
7. → HU-22.5: Idempotencia
8. → HU-22.6: SignalR
9. → HU-22.7: Documentar
