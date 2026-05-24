# Script de Demo — ComercioFlow RD MVP

**Duración estimada:** 20–25 minutos  
**Audiencia:** Dueños de colmados, minimarkets, negocios de retail dominicanos  
**URL:** http://localhost:5173 (Docker local) o dominio de staging

---

## Preparación Previa

1. `docker compose up -d` — asegurarse de que todos los servicios estén corriendo
2. Limpiar DB si es necesario: `docker compose down -v && docker compose up -d`
3. El seeder crea automáticamente: negocio, admin, 20 productos, 5 clientes, 3 proveedores
4. Credenciales: `admin@test.com` / `Admin123!`

---

## Flujo de Demo (20 minutos)

### 1. Login y Dashboard (2 min)

- Ingresar a la URL y hacer login con credenciales de admin
- Mostrar el **Dashboard** principal: métricas del día, ventas recientes, alertas
- Destacar: "Todo en tiempo real, actualizado automáticamente"

**Puntos a destacar:**
- Resumen de ventas del día
- Alertas de stock bajo (ya visibles en notificaciones)
- Indicador de suscripción activa

---

### 2. Catálogo de Productos (3 min)

- Ir a **Productos**
- Mostrar los 20 productos del colmado ya cargados (Arroz El Gallo, Cerveza Presidente, etc.)
- Abrir un producto (ej: Arroz El Gallo 5lbs) y mostrar:
  - Precio de venta: RD$175
  - Costo: RD$140 → Margen: ~20%
  - Stock mínimo y punto de reorden configurados

- **Agregar un producto nuevo** en vivo:
  - Nombre: "Ron Barceló Imperial 750ml"
  - Categoría: Bebidas
  - SKU: RON-BARC-750
  - Precio venta: RD$1,250 / Costo: RD$950
  - Guardar → "¿Ven qué fácil?"

---

### 3. Inventario (2 min)

- Ir a **Inventario**
- Mostrar stock actual por producto
- Hacer un ajuste manual:
  - Producto: Cerveza Presidente
  - Ajuste: +24 unidades (llegó una caja)
  - El sistema actualiza el stock en tiempo real

---

### 4. Nueva Venta — POS (5 min)

- Ir a **POS (Punto de Venta)**
- Crear una venta:
  1. Buscar "Arroz" → agregar 2x Arroz El Gallo = RD$350
  2. Buscar "Cerveza" → agregar 6x Cerveza Presidente = RD$390
  3. Buscar "Leche" → agregar 1x Leche Parmalat = RD$95
  4. **Total: RD$835**
- Seleccionar cliente: "María Altagracia Rodríguez"
- Método de pago: Efectivo, RD$1,000
- Cambio: RD$165
- **Completar venta**

**Mostrar en tiempo real:**
- El stock de los productos se deduce automáticamente
- Aparece notificación si algún producto queda bajo mínimo
- La venta aparece en el dashboard inmediatamente

---

### 5. Clientes y Cuenta Corriente (2 min)

- Ir a **Clientes**
- Mostrar la lista (5 clientes pre-cargados)
- Abrir "Juan Carlos Pérez Sánchez"
- Mostrar historial de compras y balance de cuenta corriente
- "Un colmado maneja crédito — aquí llevamos el control"

---

### 6. Compras y Proveedores (2 min)

- Ir a **Compras**
- Crear una orden de compra:
  - Proveedor: Distribuidora Rojas & Asociados SRL
  - Producto: Arroz El Gallo 5lbs, 50 unidades, costo unitario RD$138
  - Total: RD$6,900
  - Estado: Pendiente
- "Cuando llega la mercancía, la marcamos como recibida y el inventario se actualiza automáticamente"

---

### 7. Caja Diaria (2 min)

- Ir a **Caja**
- Mostrar la sesión de caja abierta (apertura con monto inicial)
- Ver los movimientos del día (la venta del POS ya aparece)
- Mostrar balance del sistema vs. efectivo en caja
- "Al final del día, el cajero reporta el efectivo real y el sistema detecta diferencias"

---

### 8. Reportes y Rentabilidad (2 min)

- Ir a **Reportes**
- Mostrar: Ventas del día/semana/mes
- Ir a **Rentabilidad** → mostrar margen por producto
- "Cerveza Presidente: margen 27.7%. Arroz El Gallo: 20%. ¿Cuáles son sus productos más rentables?"

---

### 9. Notificaciones y Alertas (1 min)

- Mostrar el **ícono de campana** con notificaciones
- Ver notificaciones de stock bajo generadas automáticamente
- "El sistema trabaja para usted incluso cuando usted no está mirando"

---

### 10. Multi-Sucursal y Suscripción (1 min)

- Mostrar **Suscripción** (plan Basic en trial)
- "Con el plan Pro puede tener 3 sucursales. Con Premium, ilimitadas"
- Mencionar: cada sucursal tiene su propio inventario, caja y reportes

---

## Cierre de Demo

**Preguntas clave para el cliente:**
- "¿Cuántos productos maneja su negocio actualmente?"
- "¿Tienen algún sistema para controlar el inventario hoy?"
- "¿Cuántos empleados entran ventas?"

**Propuesta de valor:**
- Control total del negocio desde cualquier dispositivo
- Precios en DOP, adaptado para el mercado dominicano
- Desde RD$1,450/mes (Basic) — menos que un empleado a tiempo parcial
- Soporte local, en español

---

## Preguntas Frecuentes

**¿Funciona sin internet?**  
Actualmente requiere conexión. Una versión offline está en el roadmap.

**¿Puedo importar mis productos actuales?**  
Sí, próximamente con importación CSV. Por ahora se cargan manualmente o via API.

**¿Está disponible en móvil?**  
La web es responsive. Una app nativa está planificada para una versión futura.

**¿Mis datos están seguros?**  
Sí. Cada negocio tiene sus datos completamente aislados (multi-tenant). 
Tokens JWT de 15 minutos, rate limiting en autenticación.

**¿Qué pasa si cancelo la suscripción?**  
Sus datos se conservan por 30 días. Puede exportarlos antes de eliminar la cuenta.
