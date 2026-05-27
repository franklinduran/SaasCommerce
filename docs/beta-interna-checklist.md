# Checklist de Beta Interna — ComercioFlow MVP

**Versión:** 1.0  
**Etapa:** 36 — Beta Interna / Hardening MVP  
**Fecha:** 2026-05-27  

---

## Instrucciones de uso

Este checklist se ejecuta manualmente por el equipo QA antes de cada sesión de beta.  
Cada ítem debe ser marcado con ✅ (OK), ❌ (Fallo) o ⚠️ (Comportamiento inesperado).  
Los ítems marcados ❌ o ⚠️ deben reportarse en el [formulario de feedback](pilots/feedback-form.md).

---

## 1. Autenticación y Sesión

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 1.1 | Login exitoso | Acceder a `/login` → ingresar `admin@test.com` / `Admin123!` → Submit | Redirige al Dashboard, muestra nombre del negocio | ☐ |
| 1.2 | Login fallido | Ingresar contraseña incorrecta → Submit | Mensaje de error "Credenciales inválidas", no redirige | ☐ |
| 1.3 | Sesión persistente | Login → cerrar pestaña → abrir nueva pestaña | Sigue autenticado, no pide login | ☐ |
| 1.4 | Logout | Click en menú de usuario → "Cerrar sesión" | Redirige a `/login`, limpia token | ☐ |
| 1.5 | Token expirado | Esperar expiración (config 30 min) o manipular token | Redirige automáticamente al login | ☐ |

**BusinessId confirmado:** ☐ Todos los datos mostrados pertenecen al negocio del usuario autenticado.

---

## 2. Dashboard

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 2.1 | Carga inicial | Login → Dashboard | KPIs cargados (ventas del día, stock bajo, caja activa) | ☐ |
| 2.2 | Estado carga | Recargar página lento | Skeleton/spinner visible durante carga | ☐ |
| 2.3 | Sin datos del día | Acceder en fecha sin ventas | Muestra "0" en métricas, no error | ☐ |
| 2.4 | Notificaciones | Verificar badge de notificaciones | Badge muestra conteo correcto | ☐ |

---

## 3. Catálogo de Productos

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 3.1 | Listar productos | Ir a Productos | Lista paginada, filtros disponibles | ☐ |
| 3.2 | Crear producto simple | Nuevo producto → completar formulario → Guardar | Producto creado, aparece en lista | ☐ |
| 3.3 | Validación SKU duplicado | Crear producto con SKU existente | Error: "SKU ya registrado" | ☐ |
| 3.4 | Crear producto servicio | Tipo = Servicio, sin inventario | Creado sin campos de stock | ☐ |
| 3.5 | Editar producto | Click en producto → Editar → modificar precio → Guardar | Precio actualizado en lista | ☐ |
| 3.6 | Buscar producto | Escribir en búsqueda | Filtra en tiempo real | ☐ |
| 3.7 | Estado vacío | Sin productos creados | Mensaje "Sin productos" con botón crear | ☐ |

**BusinessId confirmado:** ☐ Solo muestra productos del negocio activo.

---

## 4. Inventario

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 4.1 | Ver stock | Ir a Inventario | Lista de productos con stock por sucursal | ☐ |
| 4.2 | Ajuste positivo | Seleccionar producto → Ajustar → cantidad positiva | Stock incrementa | ☐ |
| 4.3 | Ajuste negativo | Ajustar con stock insuficiente | Error: "Stock insuficiente" | ☐ |
| 4.4 | Filtro stock bajo | Activar filtro "Stock bajo" | Solo muestra productos bajo umbral | ☐ |
| 4.5 | Transferencia entre sucursales | Crear transferencia → Procesar | Stock descontado en origen, incrementa en destino | ☐ |
| 4.6 | Detalle de producto | Click en producto | Muestra movimientos recientes, stock por sucursal | ☐ |
| 4.7 | Estado vacío | Sin productos con inventario | Mensaje descriptivo | ☐ |

---

## 5. Ventas POS (Punto de Venta)

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 5.1 | Abrir POS | Ir a Ventas → POS | Interfaz de venta disponible | ☐ |
| 5.2 | Agregar producto por búsqueda | Escribir nombre/SKU en buscador | Producto aparece en carrito | ☐ |
| 5.3 | Agregar por código de barras | Escanear/escribir código | Producto agrega en carrito | ☐ |
| 5.4 | Modificar cantidad | Click en qty en carrito → cambiar | Total recalculado | ☐ |
| 5.5 | Venta en efectivo | Completar venta → Pago efectivo → monto exacto | Venta registrada, recibo disponible | ☐ |
| 5.6 | Venta con cambio | Pago efectivo → monto mayor al total | Muestra vuelto correcto | ☐ |
| 5.7 | Venta con tarjeta | Completar venta → Pago tarjeta | Venta registrada | ☐ |
| 5.8 | Descuento por línea | Click en ítem → aplicar descuento % | Total recalculado con descuento | ☐ |
| 5.9 | Cancelar venta | Vaciar carrito → Cancelar | Carrito limpio, sin venta registrada | ☐ |
| 5.10 | Venta sin caja abierta | POS sin caja activa | Mensaje de advertencia, no procesa | ☐ |

**BusinessId confirmado:** ☐ Ventas asociadas al negocio/sucursal del cajero.

---

## 6. Ventas Fiadas (Crédito a Clientes)

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 6.1 | Venta fiada | POS → seleccionar cliente → Fiado | Venta registrada, balance deudor del cliente aumenta | ☐ |
| 6.2 | Cliente sin crédito suficiente | Fiado → monto supera límite de crédito | Error con límite de crédito | ☐ |
| 6.3 | Ver fiados del cliente | Ir a Clientes → seleccionar cliente → Fiados | Lista de deudas activas | ☐ |

---

## 7. Gestión de Clientes

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 7.1 | Crear cliente | Clientes → Nuevo → completar datos | Cliente creado | ☐ |
| 7.2 | Buscar cliente | Escribir nombre/teléfono | Filtrado en tiempo real | ☐ |
| 7.3 | Ver detalle | Click en cliente | Balance, historial de compras y fiados | ☐ |
| 7.4 | Estado vacío | Sin clientes | Mensaje con botón crear | ☐ |

---

## 8. Abonos a Fiados

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 8.1 | Registrar abono | Cliente con deuda → Registrar Abono → monto | Balance reducido, abono en historial | ☐ |
| 8.2 | Abono que salda deuda | Abono = saldo total | Balance en cero, deuda marcada pagada | ☐ |
| 8.3 | Abono excesivo | Monto mayor a deuda | Error: monto no puede superar deuda | ☐ |
| 8.4 | Notificación SignalR | Registrar abono | Notificación en tiempo real en panel | ☐ |

---

## 9. Devoluciones

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 9.1 | Devolución parcial | Buscar venta → Devolver → seleccionar ítems | Stock restituido, nota de crédito generada | ☐ |
| 9.2 | Devolución completa | Devolver todos los ítems | Venta marcada devuelta, inventario actualizado | ☐ |
| 9.3 | Venta no devolvible | Intentar devolver venta ya devuelta | Error descriptivo | ☐ |

---

## 10. Compras a Proveedores

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 10.1 | Registrar compra | Compras → Nueva → completar → Guardar | Compra registrada | ☐ |
| 10.2 | Recibir mercancía | Compra en estado "Ordered" → Recibir | Stock incrementa automáticamente | ☐ |
| 10.3 | Ver historial | Ir a Compras | Lista con filtros de fecha | ☐ |
| 10.4 | Estado vacío | Sin compras | Mensaje descriptivo | ☐ |

---

## 11. Caja Registradora

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 11.1 | Apertura de caja | Caja → Abrir → monto inicial | Caja activa visible en POS | ☐ |
| 11.2 | Movimiento de entrada | Caja abierta → Movimiento → CashIn → monto | Movimiento registrado | ☐ |
| 11.3 | Movimiento de salida | CashOut → monto + motivo | Movimiento registrado | ☐ |
| 11.4 | Cierre de caja | Caja → Cerrar → monto contado | Diferencia calculada, caja cerrada | ☐ |
| 11.5 | Resumen diario | Caja → Resumen del día | Lista de cajas abiertas/cerradas del día | ☐ |
| 11.6 | Doble apertura | Intentar abrir segunda caja con usuario que ya tiene caja | Error: "Ya hay una caja abierta" | ☐ |

---

## 12. Reportes

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 12.1 | Reporte de ventas | Reportes → Ventas → seleccionar rango | Tabla y totales | ☐ |
| 12.2 | Exportar ventas CSV | Botón exportar | Descarga archivo CSV válido | ☐ |
| 12.3 | Reporte de rentabilidad | Reportes → Rentabilidad | Margen bruto por producto | ☐ |
| 12.4 | Reporte de inventario | Reportes → Inventario → stock bajo | Lista con productos en alerta | ☐ |
| 12.5 | Sin datos en rango | Seleccionar fecha futura | Tabla vacía con mensaje, no error | ☐ |

---

## 13. Notificaciones en Tiempo Real (SignalR)

| # | Flujo | Pasos | Resultado esperado | Estado |
|---|-------|-------|--------------------|--------|
| 13.1 | Notificación stock bajo | Ajustar stock al mínimo | Notificación badge aparece en < 5 seg | ☐ |
| 13.2 | Notificación pago de fiado | Registrar abono desde otro usuario | Notificación visible | ☐ |
| 13.3 | Reconexión automática | Desconectar red brevemente | Reconecta sin recargar página | ☐ |

---

## 14. Permisos por Rol

| # | Rol | Verificar que NO puede | Estado |
|---|-----|------------------------|--------|
| 14.1 | Cajero | Acceder a Usuarios, Reportes avanzados | ☐ |
| 14.2 | Gerente | Acceder a configuración de negocio | ☐ |
| 14.3 | Admin | Acceso completo a todos los módulos | ☐ |
| 14.4 | Endpoint protegido | Llamar `/api/cash-registers/open` sin token | HTTP 401 Unauthorized | ☐ |
| 14.5 | Permiso faltante | Llamar endpoint con token sin permiso | HTTP 403 Forbidden | ☐ |

---

## 15. Aislamiento Multi-Tenancy (BusinessId)

| # | Verificación | Pasos | Estado |
|---|-------------|-------|--------|
| 15.1 | Datos de negocio A no visibles en negocio B | Crear datos en negocio A → login con usuario negocio B → verificar listas | ☐ |
| 15.2 | Productos | Lista de productos filtrada por negocio activo | ☐ |
| 15.3 | Ventas | Historial de ventas filtrado por negocio | ☐ |
| 15.4 | Clientes | Base de clientes aislada por negocio | ☐ |
| 15.5 | Reportes | Métricas y totales solo del negocio autenticado | ☐ |

---

## Resumen de Sesión

| Métrica | Valor |
|---------|-------|
| Fecha de prueba | |
| Tester | |
| Versión API | |
| Total ítems | 75 |
| ✅ OK | |
| ❌ Fallos | |
| ⚠️ Comportamiento inesperado | |
| Issues reportados (links) | |

**Firma de cierre:** ______________ **Fecha:** ______________
