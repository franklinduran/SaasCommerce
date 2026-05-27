# Programa de Beta Interna — ComercioFlow MVP

**Versión:** 1.0  
**Etapa:** 36 — Beta Interna / Hardening MVP  
**Fecha de inicio:** 2026-06-01  
**Duración:** 4 semanas  

---

## 1. Objetivo del Programa

La beta interna tiene como objetivo validar el MVP de ComercioFlow en condiciones reales de operación con negocios piloto seleccionados. Se busca:

- Confirmar que los flujos críticos de negocio funcionan de extremo a extremo sin intervención técnica.
- Identificar fricción de UX que no sea evidente en pruebas automatizadas.
- Detectar comportamientos inesperados bajo carga real y datos reales.
- Recolectar feedback cuantitativo y cualitativo antes del lanzamiento público.

---

## 2. Perfil del Negocio Piloto

Los negocios piloto deben cumplir:

| Criterio | Requisito |
|----------|-----------|
| Tipo de negocio | Comercio minorista (abarrotería, farmacia, ferretería, ropa) |
| Tamaño | 1-3 sucursales |
| Volumen de ventas | 20-200 transacciones/día |
| Personal | Al menos 1 cajero + 1 administrador |
| Disponibilidad | Operar con el sistema ≥ 4 horas/día durante la beta |
| Retroalimentación | Comprometido a completar formulario de feedback semanal |

---

## 3. Módulos Incluidos en la Beta

### Módulos DISPONIBLES para beta

| Módulo | Funcionalidades incluidas |
|--------|--------------------------|
| **Autenticación** | Login, logout, refresh token, roles (Admin, Gerente, Cajero) |
| **Catálogo** | Crear/editar productos simples y servicios, búsqueda, SKU/barcode |
| **Inventario** | Ajustes de stock, transferencias entre sucursales, alertas de stock bajo |
| **POS / Ventas** | Venta en efectivo/tarjeta/transferencia, descuentos, vuelto |
| **Ventas fiadas** | Crédito a clientes, límite de crédito, registro de deudas |
| **Abonos** | Pagos parciales y totales de fiados |
| **Clientes** | CRUD de clientes, historial de compras, balance de crédito |
| **Devoluciones** | Devolución parcial/total, nota de crédito |
| **Compras** | Registro de compras, recepción de mercancía |
| **Caja registradora** | Apertura/cierre, movimientos de entrada/salida, resumen diario |
| **Cierre diario** | Resumen consolidado de operaciones del día |
| **Reportes** | Ventas, rentabilidad, inventario, exportación CSV |
| **Notificaciones** | Alertas de stock bajo, pagos de fiados (tiempo real) |

### Módulos NO disponibles en esta beta

| Módulo | Motivo |
|--------|--------|
| E-commerce / Tienda online | Fuera del alcance MVP |
| Integraciones contables externas | Planificado post-MVP |
| App móvil nativa | Planificado post-beta |
| Multi-moneda | Planificado post-MVP |
| Facturación fiscal electrónica (DGII/SAT) | En desarrollo |

---

## 4. Flujos Principales Cubiertos

Los siguientes flujos son considerados **críticos** y deben funcionar sin errores durante la beta:

1. **Flujo de venta completo:** Abrir caja → POS → agregar productos → cobrar → cerrar caja
2. **Flujo de fiado:** POS → seleccionar cliente → registrar como fiado → cobro posterior
3. **Flujo de inventario:** Recibir compra → verificar stock → alerta stock bajo
4. **Flujo de devolución:** Buscar venta → devolver ítems → ajuste de inventario automático
5. **Flujo de reportes:** Seleccionar período → ver métricas → exportar datos

---

## 5. Usuarios y Roles

### Roles disponibles

| Rol | Descripción | Permisos clave |
|-----|-------------|----------------|
| **Administrador** | Dueño/gerente del negocio | Acceso completo, configuración, usuarios |
| **Gerente** | Supervisor de sucursal | Ventas, inventario, reportes, caja |
| **Cajero** | Operador de POS | POS, caja (con permiso), ventas |

### Credenciales de prueba para beta interna

| Usuario | Contraseña | Rol |
|---------|------------|-----|
| `admin@[negocio].com` | `Admin123!` | Administrador |
| `cajero@[negocio].com` | `Cajero123!` | Cajero |

> **Nota:** Las credenciales reales se entregan a cada negocio piloto al activar su cuenta.

---

## 6. Datos de Prueba Iniciales

Al activar un negocio piloto se provisiona automáticamente:

- **1 sucursal principal** (Sucursal Central)
- **3 productos de ejemplo** (café, azúcar, aceite)
- **2 usuarios** (admin + cajero)
- **Inventario inicial:** 10 unidades por producto

Los datos de prueba pueden ser modificados o eliminados libremente por el negocio piloto.

---

## 7. Riesgos Conocidos

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| Pérdida de datos en caso de error de migración BD | Baja | Alto | Backups automáticos cada 6 horas |
| Latencia alta en notificaciones SignalR | Media | Bajo | Reconexión automática implementada |
| Problemas de concurrencia en POS multi-cajero | Media | Medio | Saga transaccional implementada |
| Token JWT expirado sin refresco automático | Baja | Medio | Auto-refresh implementado en frontend |
| Error en cálculo de caja por desfase horario | Baja | Alto | Todo en UTC, frontend convierte a hora local |
| Datos de un negocio visibles por otro (multi-tenant) | Muy baja | Crítico | Filtrado por BusinessId en todas las queries |

---

## 8. Cómo Reportar Errores

### Durante la beta, los errores se reportan mediante:

1. **Formulario de feedback:** [docs/pilots/feedback-form.md](pilots/feedback-form.md)
2. **Canal de soporte:** WhatsApp del equipo técnico (número entregado al activar cuenta)
3. **Email:** soporte@comercioflow.com

### Información requerida en cada reporte:

- Descripción del problema
- Pasos para reproducirlo
- Captura de pantalla (si aplica)
- Fecha y hora del incidente
- Módulo afectado
- Mensaje de error (si lo hay)

### Prioridades de respuesta:

| Severidad | Descripción | Tiempo de respuesta |
|-----------|-------------|---------------------|
| 🔴 Crítico | Sistema no funciona, pérdida de datos | < 2 horas |
| 🟠 Alto | Flujo crítico de negocio bloqueado | < 8 horas |
| 🟡 Medio | Función afectada pero hay workaround | < 24 horas |
| 🟢 Bajo | Mejora de UX o problema menor | < 72 horas |

---

## 9. Criterios de Graduación de Beta

La beta se considera exitosa y el MVP listo para lanzamiento público cuando:

### Criterios técnicos (medibles)
- [ ] 0 bugs críticos abiertos
- [ ] < 3 bugs de alta prioridad abiertos
- [ ] Tiempo de carga promedio < 2 segundos en conexión estándar
- [ ] Health checks `/health/live` y `/health/ready` retornan "Healthy" de manera continua
- [ ] SonarQube Quality Gate: `new_coverage ≥ 80%`, `new_violations = 0`

### Criterios de negocio (feedback)
- [ ] ≥ 2 negocios piloto completan 4 semanas sin solicitar pausa
- [ ] NPS (Net Promoter Score) ≥ 7 de los piloto
- [ ] ≥ 80% de flujos críticos calificados como "satisfactorios" en checklist
- [ ] Documentación de onboarding validada por al menos 1 negocio piloto

### Criterios de proceso
- [ ] Checklist beta ejecutado sin ítems fallidos críticos
- [ ] Runbook de operación documentado y validado
- [ ] Guía de despliegue ejecutada exitosamente en entorno limpio

---

## 10. Comunicaciones del Programa

| Evento | Frecuencia | Responsable |
|--------|------------|-------------|
| Check-in con piloto | Semanal (lunes) | Product Manager |
| Revisión de métricas | Semanal (viernes) | Tech Lead |
| Actualización de versión | Según necesidad | DevOps |
| Reporte de estado beta | Semanal | Tech Lead → stakeholders |

---

## 11. Estructura del Equipo Beta

| Rol | Responsabilidad |
|-----|----------------|
| Product Manager | Coordinación de pilotos, recolección de feedback |
| Tech Lead | Revisión de bugs, arquitectura, decisiones técnicas |
| Desarrollador Backend | Fixes de backend, health checks, performance |
| Desarrollador Frontend | Fixes de UI/UX, accesibilidad |
| QA | Ejecución de checklist, validación de fixes |

---

*Este documento es confidencial y para uso interno del equipo ComercioFlow.*
