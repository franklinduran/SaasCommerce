# Checklist de Onboarding — Primer Cliente

**Versión:** 1.0  
**Para:** Equipo de Soporte ComercioFlow

---

## Antes de la Demo (Preparación)

- [ ] Verificar que el ambiente de producción está activo
- [ ] Confirmar datos del cliente:
  - Nombre del negocio
  - RNC o Cédula
  - Teléfono
  - Email del administrador
  - Nombre completo del admin
- [ ] Acordar fecha/hora para sesión de onboarding (30-45 min)

---

## Paso 1: Crear el Negocio Piloto

**URL:** `/admin/pilot-businesses`

1. Acceder con cuenta de administrador SaaS
2. Completar el formulario:
   - **Nombre del negocio** → Nombre legal exacto
   - **Tipo de ID** → RNC (empresas) o Cédula (persona física)
   - **Número de ID** → RNC/Cédula sin guiones
   - **Teléfono** → 10 dígitos sin guiones (ej: 8091234567)
   - **Nombre de sucursal** → "Sucursal Principal" (editable después)
   - **Nombre del admin** → Nombre completo
   - **Email** → Email real que el cliente usará para acceder
   - **Contraseña temporal** → Mínimo 8 caracteres (se recomienda cambiar en primer login)
3. Hacer clic en "Crear negocio piloto"
4. Guardar los datos de confirmación:
   - Business ID, Branch ID, Admin User ID
   - Fecha de vencimiento del trial (14 días)
5. Comunicar al cliente: email + contraseña temporal

---

## Paso 2: Primera Sesión con el Cliente

### Agenda sugerida (45 min)

| Tiempo | Actividad |
|--------|-----------|
| 5 min | Acceso inicial, cambio de contraseña |
| 10 min | Tour general del sistema |
| 10 min | Importar productos (CSV) |
| 10 min | Abrir primera caja, hacer venta demo |
| 5 min | Revisar reportes básicos |
| 5 min | Preguntas y próximos pasos |

---

## Paso 3: Importar Productos

**URL:** `/products/import`

1. Ir a la sección de importación
2. Descargar la plantilla CSV
3. Completar con los productos del cliente
4. Subir el archivo
5. Revisar resultados: productos importados vs. omitidos
6. Verificar en `/inventory` que el stock fue creado

---

## Paso 4: Verificar Onboarding Completo

**URL:** `/onboarding`

Los pasos se completan automáticamente:
- **Información del negocio** ✅ (creado al registrar el negocio)
- **Catálogo de productos** ✅ (al importar el CSV)
- **Inventario inicial** ✅ (al importar con StockQuantity > 0)
- **Primera sesión de caja** → Pendiente (cliente debe abrir caja)

---

## Paso 5: Primera Venta Demo

1. Ir a `/cash` → Abrir sesión de caja
2. Ir a `/pos` → Registrar una venta de prueba
3. Mostrar el ticket de venta
4. Ir a `/sales` → Ver historial de ventas

---

## Después del Onboarding

- [ ] Compartir el link de feedback: ver `feedback-form.md`
- [ ] Agendar seguimiento en 7 días
- [ ] Documentar preguntas o problemas encontrados
- [ ] Notificar fecha de vencimiento del trial (14 días)
- [ ] Recordatorio en día 10 si no ha decidido continuar

---

## Contactos de Soporte

- **WhatsApp soporte:** +1 (809) 555-DEMO
- **Email:** soporte@comercioflow.com.do
- **Documentación:** https://docs.comercioflow.com.do
