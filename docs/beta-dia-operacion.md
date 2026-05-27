# Guía del Día de Operación — Beta Interna ComercioFlow

**Versión:** 1.0  
**Etapa:** 36 — Beta Interna / Hardening MVP  
**Fecha:** 2026-05-27  
**Rol:** Tester QA / Piloto de negocio  

---

## Descripción

Este documento guía un escenario completo de un día de operación de un negocio minorista ficticio usando ComercioFlow. Cada paso incluye las acciones exactas a realizar y el resultado esperado. Es la prueba de extremo a extremo más completa del sistema.

**Duración estimada:** 45-60 minutos  
**Prerrequisito:** Sistema desplegado y corriendo ([ver guía de despliegue](deployment-beta.md))  

---

## Contexto del Escenario

> **Negocio:** Abarrotería "El Buen Precio"  
> **Sucursal:** Central (única)  
> **Personal:** Dueño (Admin) + Cajero  
> **Fecha de prueba:** Usar la fecha actual  
> **Inventario inicial disponible:** Café (10 unid), Azúcar (10 unid), Aceite (5 unid)  

---

## Paso 1 — Verificar Sistema (Admin)

**Actor:** Administrador  
**Duración estimada:** 2 minutos  

### Acción
1. Abrir el navegador y acceder a `http://localhost:5173`
2. Verificar que la página de login carga correctamente
3. Hacer login con `admin@test.com` / `Admin123!`

### Resultado esperado
- Dashboard carga con métricas del día (ventas: $0, caja: sin abrir)
- No aparecen errores en consola del navegador
- Badge de notificaciones visible en el menú

### Checklist
- [ ] Login exitoso sin errores
- [ ] Dashboard muestra métricas del día actuales
- [ ] Nombre del negocio correcto en el encabezado

---

## Paso 2 — Preparar Inventario (Admin)

**Actor:** Administrador  
**Duración estimada:** 5 minutos  

### Acción
1. Ir a **Inventario** en el menú lateral
2. Verificar stock de: Café (10), Azúcar (10), Aceite (5)
3. Si el stock es diferente, hacer ajuste:  
   - Click en el producto → Ajustar Stock → ingresar cantidad → Motivo: "Inventario inicial"

### Resultado esperado
- Stock mostrado correctamente por producto
- Ajuste guarda sin error
- Movimiento de inventario registrado en historial del producto

### Checklist
- [ ] Stock de Café visible: 10 unidades
- [ ] Stock de Azúcar visible: 10 unidades
- [ ] Stock de Aceite visible: 5 unidades

---

## Paso 3 — Abrir Caja (Cajero)

**Actor:** Cajero  
**Duración estimada:** 2 minutos  

### Acción
1. Ir a **Caja** en el menú
2. Click en "Abrir Caja"
3. Ingresar: Monto inicial = `$500.00`, Notas = "Apertura turno mañana"
4. Confirmar apertura

### Resultado esperado
- Caja aparece como "Abierta" en la pantalla
- Dashboard actualiza indicador de caja activa
- Cajero puede ahora usar el POS

### Checklist
- [ ] Caja abierta con monto $500.00
- [ ] Estado "Abierta" visible
- [ ] Timestamp de apertura correcto

---

## Paso 4 — Primera Venta en Efectivo (Cajero)

**Actor:** Cajero  
**Duración estimada:** 3 minutos  

### Acción
1. Ir a **POS** en el menú
2. Buscar "Café" en el buscador de productos
3. Click en Café → agrega 2 unidades al carrito
4. Buscar "Azúcar" → agregar 1 unidad
5. Verificar total (según precios configurados)
6. Click en "Cobrar" → seleccionar "Efectivo"
7. Ingresar monto recibido = `$200.00`
8. Confirmar venta

### Resultado esperado
- Venta registrada con número de transacción
- Vuelto calculado y mostrado
- Stock: Café = 8, Azúcar = 9
- Venta aparece en historial de ventas

### Checklist
- [ ] Venta procesada sin error
- [ ] Número de transacción asignado
- [ ] Vuelto correcto mostrado
- [ ] Stock actualizado automáticamente

---

## Paso 5 — Venta Fiada a Cliente (Cajero)

**Actor:** Cajero  
**Duración estimada:** 4 minutos  

### Acción
1. En **Clientes**, verificar si existe "María García" o crear nuevo cliente:
   - Nombre: `María García`, Teléfono: `809-555-0101`, Límite crédito: `$2,000`
2. Ir al **POS**
3. Agregar: Aceite (1 unidad) + Azúcar (2 unidades)
4. Antes de cobrar, buscar y seleccionar cliente "María García"
5. Seleccionar método de pago "Fiado"
6. Confirmar venta

### Resultado esperado
- Venta registrada como "Fiada"
- Balance de María García muestra deuda
- Stock: Aceite = 4, Azúcar = 7

### Checklist
- [ ] Cliente "María García" creado o encontrado
- [ ] Venta fiada registrada
- [ ] Balance del cliente actualizado
- [ ] Stock descontado correctamente

---

## Paso 6 — Registrar Movimiento de Caja (Cajero)

**Actor:** Cajero  
**Duración estimada:** 2 minutos  

### Acción
1. Ir a **Caja**
2. Con la caja abierta, click en "Registrar Movimiento"
3. Tipo: `Salida (CashOut)`, Monto: `$50.00`, Motivo: "Pago de proveedor de bebidas"
4. Confirmar

### Resultado esperado
- Movimiento registrado en el historial de la caja
- Balance esperado de caja actualizado (apertura $500 - $50 = $450 + ventas efectivo)

### Checklist
- [ ] Movimiento CashOut registrado
- [ ] Motivo guardado
- [ ] Historial muestra el movimiento

---

## Paso 7 — Registrar Compra a Proveedor (Admin)

**Actor:** Administrador  
**Duración estimada:** 4 minutos  

### Acción
1. Ir a **Compras** → "Nueva Compra"
2. Proveedor: "Distribuidora Central"
3. Agregar ítem: Café, 20 unidades, precio costo $15.00/unidad
4. Total compra: $300.00
5. Guardar compra

### Resultado esperado
- Compra registrada en estado "Ordered"
- Aparece en la lista de compras

### Checklist
- [ ] Compra registrada correctamente
- [ ] Estado "Ordered"
- [ ] Total correcto

---

## Paso 8 — Recibir Mercancía (Admin)

**Actor:** Administrador  
**Duración estimada:** 2 minutos  

### Acción
1. En **Compras**, buscar la compra del Paso 7
2. Click en "Recibir Mercancía"
3. Confirmar recepción completa

### Resultado esperado
- Compra cambia a estado "Received"
- Stock de Café se actualiza: 8 + 20 = 28 unidades
- Movimiento de inventario visible en detalle del producto

### Checklist
- [ ] Compra en estado "Received"
- [ ] Stock de Café = 28 unidades
- [ ] Movimiento de inventario registrado

---

## Paso 9 — Registrar Abono de Cliente (Admin/Cajero)

**Actor:** Cajero  
**Duración estimada:** 2 minutos  

### Acción
1. Ir a **Clientes** → buscar "María García"
2. Ver balance de deuda del Paso 5
3. Click en "Registrar Abono"
4. Ingresar monto = $50.00, Método: Efectivo
5. Confirmar

### Resultado esperado
- Balance de María García reducido en $50
- Abono aparece en historial de transacciones del cliente
- Notificación de pago en tiempo real visible en panel

### Checklist
- [ ] Abono registrado
- [ ] Balance del cliente actualizado
- [ ] Notificación SignalR recibida (si dos sesiones abiertas)

---

## Paso 10 — Realizar Devolución (Cajero)

**Actor:** Cajero  
**Duración estimada:** 3 minutos  

### Acción
1. Ir a **Ventas** → buscar la venta del Paso 4
2. Click en "Devolver"
3. Seleccionar solo el Azúcar (1 unidad)
4. Motivo: "Producto dañado"
5. Confirmar devolución

### Resultado esperado
- Devolución registrada
- Stock de Azúcar incrementa: 7 + 1 = 8 unidades
- Nota de crédito generada

### Checklist
- [ ] Devolución parcial registrada
- [ ] Stock de Azúcar = 8 unidades
- [ ] Nota de crédito disponible

---

## Paso 11 — Verificar Reportes del Día (Admin)

**Actor:** Administrador  
**Duración estimada:** 3 minutos  

### Acción
1. Ir a **Reportes** → "Ventas"
2. Seleccionar fecha de hoy como rango
3. Verificar que aparecen las ventas de los pasos 4 y 5
4. Click en "Exportar CSV"
5. Verificar que el archivo se descarga y tiene datos

### Resultado esperado
- Reporte muestra ventas del día
- Totales por método de pago correctos (efectivo + fiado)
- CSV descargado con datos

### Checklist
- [ ] Ventas del día visibles
- [ ] Total en efectivo correcto
- [ ] Total fiado correcto
- [ ] CSV exportado con datos válidos

---

## Paso 12 — Verificar Stock Bajo (Admin)

**Actor:** Administrador  
**Duración estimada:** 2 minutos  

### Acción
1. Ir a **Inventario**
2. Activar filtro "Solo stock bajo"
3. Si Aceite (4 unidades) está bajo el umbral mínimo (5), debe aparecer

### Resultado esperado
- Aceite aparece en la lista de stock bajo (umbral: 5 unidades)
- Badge de notificación refleja alerta de stock bajo

### Checklist
- [ ] Aceite aparece en filtro de stock bajo
- [ ] Notificación de stock bajo en el sistema

---

## Paso 13 — Cerrar Caja (Cajero)

**Actor:** Cajero  
**Duración estimada:** 3 minutos  

### Acción
1. Ir a **Caja**
2. Click en "Cerrar Caja"
3. Ingresar monto contado: contar el dinero físico (para la prueba, usar el monto esperado)
   - Apertura: $500
   - Menos CashOut: -$50
   - Más ventas efectivo del Paso 4: (total de la venta)
   - Monto esperado ≈ $500 - $50 + ventas
4. Notas: "Cierre turno mañana"
5. Confirmar cierre

### Resultado esperado
- Caja cerrada con diferencia calculada
- Si monto contado = esperado → DifferenceType = "Balanced"
- Resumen de caja disponible en historial

### Checklist
- [ ] Caja cerrada exitosamente
- [ ] DifferenceType mostrado (Balanced/Shortage/Surplus)
- [ ] Resumen de caja disponible

---

## Paso 14 — Cierre Diario (Admin)

**Actor:** Administrador  
**Duración estimada:** 3 minutos  

### Acción
1. Ir a **Cierre Diario**
2. Verificar el resumen del día de hoy:
   - Total ventas
   - Total cobrado
   - Total fiado
   - Devoluciones
3. Si el estado es "Pendiente", hacer click en "Cerrar Día"
4. Confirmar

### Resultado esperado
- Cierre diario registrado con resumen consolidado
- Estado "Cerrado"
- Exportar reporte de cierre si disponible

### Checklist
- [ ] Resumen del día correcto
- [ ] Cierre diario procesado
- [ ] Estado "Cerrado"

---

## Verificación Final del Sistema

Después de completar los 14 pasos, verificar el estado del sistema:

### Health checks (en terminal)

```bash
curl http://localhost:5000/health/live
# Esperado: {"isSuccess":true,"data":"Live",...}

curl http://localhost:5000/health/ready
# Esperado: {"isSuccess":true,"data":{"status":"Healthy",...}}
```

### Checklist final

- [ ] Todos los 14 pasos completados sin errores críticos
- [ ] Health check `/health/live` → HTTP 200
- [ ] Health check `/health/ready` → HTTP 200, status "Healthy"
- [ ] No hay errores en logs del servidor
- [ ] Dashboard del día muestra métricas finales correctas

---

## Registro de Resultados

| Paso | Descripción | Resultado | Observaciones |
|------|-------------|-----------|---------------|
| 1 | Verificar sistema | ☐ OK / ☐ FAIL | |
| 2 | Preparar inventario | ☐ OK / ☐ FAIL | |
| 3 | Abrir caja | ☐ OK / ☐ FAIL | |
| 4 | Venta en efectivo | ☐ OK / ☐ FAIL | |
| 5 | Venta fiada | ☐ OK / ☐ FAIL | |
| 6 | Movimiento de caja | ☐ OK / ☐ FAIL | |
| 7 | Registrar compra | ☐ OK / ☐ FAIL | |
| 8 | Recibir mercancía | ☐ OK / ☐ FAIL | |
| 9 | Registrar abono | ☐ OK / ☐ FAIL | |
| 10 | Devolución parcial | ☐ OK / ☐ FAIL | |
| 11 | Verificar reportes | ☐ OK / ☐ FAIL | |
| 12 | Verificar stock bajo | ☐ OK / ☐ FAIL | |
| 13 | Cerrar caja | ☐ OK / ☐ FAIL | |
| 14 | Cierre diario | ☐ OK / ☐ FAIL | |

**Tester:** __________________ **Fecha:** __________________ **Versión:** __________________

**Observaciones generales:**

_______________________________________________

_______________________________________________

_______________________________________________
