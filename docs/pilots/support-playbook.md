# Playbook de Soporte — Piloto ComercioFlow RD

**Versión:** 1.0  
**Para:** Equipo de Soporte Técnico

---

## Flujo General de un Piloto

```
1. Prospecto → 2. Demo → 3. Crear negocio → 4. Onboarding → 5. Seguimiento → 6. Conversión
```

---

## Scripts de Conversación

### Primer Contacto

> "Hola [nombre], habla [agente] de ComercioFlow RD. ¿Tienes 30 minutos para una demo rápida de cómo podemos ayudar a [nombre del negocio] a manejar su inventario y ventas de manera más eficiente?"

### Confirmación de Onboarding

> "Le vamos a crear su acceso ahora mismo. Voy a necesitar: el nombre legal del negocio, su RNC o cédula, un teléfono de contacto, y el email que usará para acceder al sistema. Le llegará su usuario y contraseña en los próximos minutos."

### Seguimiento Día 7

> "Hola [nombre], ¿cómo ha ido con ComercioFlow esta semana? ¿Pudo registrar sus productos? ¿Ha tenido alguna dificultad? Queremos asegurarnos de que el sistema esté funcionando bien para [negocio]."

---

## Problemas Comunes y Soluciones

### "No puedo iniciar sesión"

1. Verificar que el email es correcto (case-insensitive)
2. Pedir reset de contraseña si se olvidó
3. Verificar que el período de trial no haya vencido
4. Si el trial venció, escalar a equipo para extensión de prueba

### "No aparecen mis productos"

1. Verificar que la importación CSV se completó (ver `/products`)
2. Si hubo errores, revisar la tabla de errores de la importación
3. Verificar que el archivo CSV tiene las columnas correctas
4. Recomendar descargar la plantilla y volver a intentar

### "El inventario no se actualizó"

1. Verificar que la importación incluyó `StockQuantity > 0`
2. Ir a `/inventory` y verificar los productos
3. Si no hay stock, hacer ajuste manual desde `/inventory`

### "No veo las ventas del día"

1. Verificar que la sesión de caja esté abierta (ir a `/cash`)
2. Si la sesión está cerrada, abrir una nueva
3. Verificar en `/sales` que las ventas estén registradas

### "El sistema está lento"

1. Pedir que limpien el caché del navegador
2. Verificar la conexión a internet del cliente
3. Si persiste, escalar a equipo técnico con Business ID y hora del incidente

### "No puedo imprimir el ticket"

1. Verificar que el navegador permite ventanas emergentes
2. Usar Chrome o Firefox (evitar Safari)
3. Verificar que la impresora está configurada en el navegador

---

## Escalamiento

| Nivel | Cuándo | Canal |
|-------|--------|-------|
| L1 (Soporte) | Dudas de uso, contraseñas | WhatsApp/Email |
| L2 (Técnico) | Errores del sistema, datos incorrectos | Slack interno |
| L3 (Dev) | Bugs confirmados, pérdida de datos | Ticket en Jira |

---

## Extensión del Período de Trial

Si un cliente necesita más tiempo:

1. Solicitar aprobación al coordinador
2. Acceder a `/admin/pilot-businesses` con credenciales de Admin SaaS
3. Registrar la extensión manualmente en la bitácora
4. Notificar al cliente

> **Nota:** La extensión manual aún no está automatizada en v1. Se gestiona de forma ad-hoc durante el piloto.

---

## Métricas a Registrar

Por cada cliente piloto, registrar semanalmente:

| Métrica | Descripción |
|---------|-------------|
| Login count | Cuántas veces ha iniciado sesión |
| Ventas registradas | Total de ventas en el período |
| Productos en catálogo | Cuántos productos tiene cargados |
| NPS (día 14) | Puntuación del formulario de feedback |
| Conversión | ¿Continuó pagando? |

---

## Contactos del Equipo

| Rol | Nombre | Canal |
|-----|--------|-------|
| Soporte L1 | TBD | WhatsApp grupal |
| Técnico L2 | TBD | Slack #soporte |
| Desarrollador | TBD | Slack #dev |
| Coordinador | TBD | Email directo |
