# Limitaciones Conocidas — ComercioFlow RD MVP v1.0

**Fecha:** 2026-05-24  
**Estado:** Documentadas para release candidate

---

## Seguridad

### L-SEC-01: Refresh tokens almacenados en texto plano
**Descripción:** Los refresh tokens se almacenan como hash SHA-256 en PostgreSQL, pero el valor original se transmite en el cuerpo de la respuesta de login/refresh. No se usa PKCE ni almacenamiento cifrado adicional.  
**Riesgo:** Medio — Un acceso a la base de datos expone los hashes, pero no los tokens originales.  
**Mitigación actual:** Tokens de corta vida (15 min access, 7 días refresh), revocación inmediata en logout.  
**Plan:** Evaluar uso de bcrypt para tokens de refresh en v1.1.

### L-SEC-02: Sin 2FA (autenticación de dos factores)
**Descripción:** El sistema solo soporta autenticación por email/password. No hay TOTP, SMS, ni llaves de hardware.  
**Riesgo:** Medio para cuentas de Owner/Admin.  
**Plan:** Agregar TOTP en v1.2.

### L-SEC-03: Sin rotación automática de JWT secret
**Descripción:** El `Jwt__Key` se configura en variables de entorno y debe rotarse manualmente. Un leak del key invalida todos los tokens activos, pero no hay mecanismo automático de rotación.  
**Plan:** Evaluar uso de Key Management Service (AWS KMS/Azure Key Vault) en producción.

---

## Funcional

### L-FUN-01: Facturación no fiscal (sin comprobantes DGII)
**Descripción:** Los comprobantes generados son internos. No cumplen con el formato 606/607 ni la secuencia de NCF requerida por la DGII dominicana.  
**Impacto:** Negocios que requieren facturación fiscal (contribuyentes del ITBIS) no pueden usar el módulo de facturación para sus declaraciones.  
**Plan:** Módulo de facturación fiscal en v2.0.

### L-FUN-02: Sin impresión directa de tickets POS
**Descripción:** El sistema no se integra con impresoras térmicas POS via WebUSB, Bluetooth ni comandos ESC/POS.  
**Impacto:** El cajero debe imprimir desde el navegador (PDF) o mostrar el recibo en pantalla.  
**Plan:** Integración con PrintNode o WebUSB en v1.2.

### L-FUN-03: Sin importación masiva de productos
**Descripción:** Los productos deben crearse uno a uno desde la interfaz o via API. No hay importación por CSV/Excel.  
**Impacto:** Negocios con catálogos grandes (>50 productos) requieren carga inicial manual.  
**Plan:** Importación CSV en v1.1.

### L-FUN-04: Renovación de suscripción manual
**Descripción:** Cuando el trial o período activo vence, el negocio queda en estado Expired. No hay renovación automática ni integración con pasarela de pagos.  
**Impacto:** Requiere intervención manual del administrador del SaaS para reactivar.  
**Plan:** Integración Stripe en v2.0.

### L-FUN-05: Cierre diario sin reversión
**Descripción:** Una vez cerrado el día, no puede reabrirse ni modificarse. Los errores requieren ajustes manuales en el día siguiente.  
**Impacto:** Bajo — corrección via ajuste de caja en el siguiente día.  
**Plan:** Función de "reapertura con auditoría" en v1.2.

### L-FUN-06: Productos pesados sin fraccionamiento decimal en POS
**Descripción:** Los productos de tipo Weighed (por libra/kg) pueden registrarse con cantidades decimales en el POS, pero la UI no tiene integración con balanza física.  
**Plan:** Integración con balanza serial/USB en v2.0.

### L-FUN-07: Sin soporte offline
**Descripción:** El sistema requiere conexión a internet constante. Cortes de luz/internet interrumpen las operaciones de caja.  
**Impacto:** Alto en zonas rurales o con servicio eléctrico irregular.  
**Plan:** PWA con modo offline para POS básico en v2.0.

---

## Técnico

### L-TEC-01: Sin CI/CD configurado
**Descripción:** No hay pipeline de CI/CD automatizado (GitHub Actions, GitLab CI). El despliegue es manual via Docker Compose.  
**Plan:** Configurar pipeline en v1.1 antes del primer cliente de producción.

### L-TEC-02: Base de datos sin clustering
**Descripción:** PostgreSQL corre en un solo contenedor sin réplicas ni failover. Un fallo del servidor implica downtime total.  
**Plan:** Migrar a Supabase, RDS Multi-AZ o PostgreSQL con streaming replication en producción.

### L-TEC-03: Logs sin centralización
**Descripción:** Serilog escribe a consola y archivo. No hay agregación central (Elastic, Loki, Datadog).  
**Plan:** Integrar con servicio de logs centralizado en v1.1.

### L-TEC-04: Sin health checks con auto-recovery
**Descripción:** El endpoint `/api/health` existe pero Docker no tiene `healthcheck` configurado con restart automático en todos los servicios.  
**Mitigación actual:** Docker Compose `restart: unless-stopped` está configurado.

### L-TEC-05: Migraciones manuales sin rollback
**Descripción:** Las migraciones de EF Core son manuales (sin ModelSnapshot). El rollback de una migración requiere escribir el SQL inverso manualmente.  
**Riesgo:** Bajo para MVP — todas las migraciones han sido probadas.

---

## Rendimiento

### L-PERF-01: Sin caché de respuestas
**Descripción:** No hay Redis u otro sistema de caché. Todas las consultas van directamente a PostgreSQL.  
**Impacto:** Bajo para MVP (1 negocio demo). Para producción con decenas de negocios, se necesitará caché.  
**Plan:** Evaluar Redis para endpoints de catálogo y reportes en v1.1.

### L-PERF-02: Reportes sin materialización
**Descripción:** Los reportes de rentabilidad y cierre diario recalculan todo en tiempo real desde los datos de ventas. Con volúmenes grandes (>10,000 ventas/mes), las consultas pueden ser lentas.  
**Plan:** Vistas materializadas o tablas de agregación en v1.2.

---

## Matriz de Prioridades para v1.1

| Limitación | Prioridad | Esfuerzo |
|------------|-----------|----------|
| L-FUN-03 (importación CSV) | Alta | Media |
| L-TEC-01 (CI/CD) | Alta | Baja |
| L-TEC-02 (DB clustering) | Alta | Alta |
| L-SEC-02 (2FA) | Media | Media |
| L-FUN-02 (impresión POS) | Media | Alta |
| L-FUN-05 (reapertura cierre) | Baja | Baja |
| L-PERF-01 (caché Redis) | Media | Media |
