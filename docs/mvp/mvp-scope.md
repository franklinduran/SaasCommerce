# ComercioFlow RD — Alcance del MVP

**Versión:** 1.0.0-rc  
**Fecha:** 2026-05-24

---

## Funcionalidades Incluidas en MVP ✅

### Autenticación y Seguridad
- Login con email/password, JWT (15 min) + refresh token rotativo
- Logout con revocación de refresh token en servidor
- Rate limiting: login 5/min, refresh 10/min, registro 3/min
- Security headers: X-Content-Type-Options, X-Frame-Options, Referrer-Policy, X-XSS-Protection, Permissions-Policy
- Multi-tenancy: aislamiento completo por BusinessId (extraído de JWT, nunca del frontend)

### Gestión de Usuarios y Roles
- Roles: Owner/Admin (full access), Supervisor, Cashier, Viewer
- Permisos granulares: +40 permisos específicos por módulo
- Gestión de usuarios: crear, editar, asignar roles, desactivar
- Usuarios acotados por sucursal (BranchId en JWT)

### Catálogo de Productos
- Tipos: Simple, Pesado (por peso), Compuesto, Variante
- Unidades: Unidad, Libra, Kilogramo, Gramo, Litro, Mililitro, Caja, Pack, Docena, Metro, Servicio
- Categorías con jerarquía plana
- Precios: venta, costo, mayorista, precio mínimo
- Impuestos ITBIS: Exento, 18%, Reducido
- Componentes (recetas) para productos compuestos
- Búsqueda por nombre, SKU, código de barras
- Activar/desactivar productos

### Inventario
- Seguimiento de stock por producto y sucursal
- Movimientos: InitialStock, PurchaseEntry, SaleDeduction, ManualAdjustment, Return, TransferIn, TransferOut
- Alertas automáticas de stock bajo (notificación + Dashboard)
- Ajustes manuales con razón y usuario auditado
- Historial de movimientos por producto

### Ventas / POS
- Venta desde POS: búsqueda rápida, cantidades, descuentos por ítem
- Tipos de pago: Efectivo, Tarjeta de crédito, Tarjeta de débito, Transferencia
- Clientes opcionales en venta (ventas al contado)
- Devoluciones parciales y totales
- Deducción automática de inventario al completar venta
- Límite mensual de ventas según plan de suscripción

### Clientes y Cuenta Corriente
- Ficha de cliente: nombre, teléfono, email, dirección
- Cuenta corriente (crédito): saldo, historial de movimientos
- Registro de pagos a cuenta corriente
- Alertas de deuda vencida

### Proveedores y Compras
- Ficha de proveedor: nombre, RNC, teléfono, email, dirección
- Órdenes de compra: creación, recepción parcial/total
- Actualización automática de inventario al recibir
- Actualización de costo promedio de producto al recibir
- Historial de compras por proveedor

### Facturación
- Comprobantes internos de venta (no fiscales)
- Reimpresión de comprobante
- Historial de facturas por cliente

### Caja Diaria
- Apertura de caja con monto inicial
- Registro de entradas y salidas manuales
- Cálculo de balance del sistema (opening + in - out)
- Cierre de caja con monto físico contado
- Alerta automática si diferencia > RD$50

### Gastos Operativos
- Registro de gastos con categoría y monto
- Categorías: Alquiler, Servicios, Salarios, Mantenimiento, Otros
- Integración con Cierre Diario
- Reportes de gastos por período

### Cierre Diario
- Resumen de ventas, compras, gastos, caja
- Cálculo de rentabilidad del día
- Estado: Abierto, Cerrado
- Alertas: margen negativo, diferencia de caja > RD$100
- Solo un cierre por sucursal por día

### Rentabilidad
- Margen bruto por producto (precio venta - costo)
- Rentabilidad por sucursal
- Alertas: producto sin costo configurado, margen negativo
- Historial de rentabilidad por período

### Transferencias de Inventario
- Transferencias entre sucursales del mismo negocio
- Estados: Pending, Completed, Cancelled
- Deducción/suma automática de inventario en origen/destino

### Multi-Sucursal
- Creación y gestión de sucursales
- Inventario independiente por sucursal
- Caja independiente por sucursal
- Reportes filtrados por sucursal
- Límites de sucursales según plan de suscripción

### Suscripciones SaaS
- Planes: Basic ($29/mes, 1 sucursal, 2 usuarios, 300 productos), Pro ($99), Premium ($299)
- Trial de 14 días para negocios nuevos
- Validación de límites en tiempo real al crear recursos
- Estados: Trial, Active, PastDue, Suspended, Cancelled, Expired
- Página de suscripción con uso vs. límites

### Notificaciones Operativas
- Tipos: Stock bajo, Falla de venta, Error de factura, Diferencia de caja, Gasto elevado, Margen negativo, Cierre diario pendiente, Deuda de cliente vencida
- Severidades: Info, Warning, Critical
- Bell en header con badge de no leídas
- Página de notificaciones con filtros y paginación
- Marcar como leída (individual o masivo)
- Entrega en tiempo real via SignalR

### Reportes y Dashboard
- Dashboard: KPIs del día, tendencias, alertas rápidas
- Reporte de ventas por período, por producto, por vendedor
- Reporte de inventario: stock actual, movimientos
- Reporte de compras por proveedor
- Reporte de rentabilidad

### Auditoría
- Log de operaciones: usuario, timestamp, recurso, acción
- Filtros por módulo, usuario y fecha
- Solo lectura para Supervisores

---

## No incluido en MVP v1.0 (Roadmap)

- ❌ Integración con pasarela de pagos (Stripe/PayPal)
- ❌ Reportes fiscales DGII (606/607)
- ❌ App móvil nativa
- ❌ Importación masiva de productos (CSV/Excel)
- ❌ Código de barras con scanner físico integrado
- ❌ Impresión directa de tickets POS (Bluetooth/USB)
- ❌ Módulo de recursos humanos / nómina
- ❌ Contabilidad general / asientos
- ❌ Multi-moneda (USD/EUR)
- ❌ E-commerce / tienda online
- ❌ Integración con SISDOM o plataformas DRD
- ❌ Renovación automática de suscripción
- ❌ Dashboard administrativo SaaS (MRR, churn, LTV)
