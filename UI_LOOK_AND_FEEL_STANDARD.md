# UI Look & Feel Standard — ComercioFlow RD

> Documento operativo para mantener una experiencia visual consistente, moderna y premium en todo el frontend de ComercioFlow RD.

---

## 1. Propósito

Este estándar define cómo debe verse, sentirse y comportarse la interfaz de ComercioFlow RD en todas sus pantallas, módulos y flujos. Su objetivo es asegurar que cada elemento de UI mantenga un look & feel consistente, limpio, sutil, moderno y profesional, inspirado en pantallas tipo Adobe/Nucleus: mucho aire visual, bordes suaves, sombras delicadas, tipografía clara, formularios limpios, estados bien definidos y una experiencia fluida.

Este documento debe aplicarse a:

```txt
Login
Registro de comercio
Onboarding
Dashboard
POS
Ventas
Productos
Inventario
Clientes
Compras
Facturas
Reportes
Configuración
Usuarios
Auditoría
Feedback beta
Pantallas empty/error/loading
Modales
Tablas
Formularios
Notificaciones
```

---

## 2. Principio rector de diseño

Toda pantalla debe transmitir:

```txt
Confianza
Claridad
Orden
Ligereza
Modernidad
Velocidad
Control
Profesionalismo SaaS
```

La interfaz no debe sentirse pesada, genérica ni cargada. Debe sentirse como un producto SaaS pulido, con estética limpia, elementos sutiles, contraste controlado y jerarquía visual clara.

---

## 3. Estilo visual oficial

El estilo oficial de ComercioFlow RD es:

```txt
Premium SaaS
Adobe/Nucleus inspired
Split-screen cuando aplique
Minimalista, pero funcional
Suave, claro y moderno
Con acento índigo/púrpura sobrio
Componentes reutilizables y consistentes
```

No se debe usar un estilo visual improvisado por módulo. Cada pantalla nueva o modificada debe reutilizar el sistema visual definido aquí.

---

## 4. Tokens visuales base

### 4.1 Colores

Usar una paleta neutral con acento principal índigo/púrpura.

```txt
Background app:        #F6F7FB / #F8FAFC
Surface principal:     #FFFFFF
Surface secundaria:    #F9FAFB
Border suave:          #E5E7EB
Border activo:         #7C3AED / #6366F1
Texto principal:       #111827
Texto secundario:      #4B5563
Texto muted:           #6B7280
Texto deshabilitado:   #9CA3AF
Primary:               #4F46E5
Primary hover:         #4338CA
Primary soft:          #EEF2FF
Success:               #16A34A
Warning:               #D97706
Danger:                #DC2626
Info:                  #2563EB
```

Reglas:

```txt
No usar colores saturados sin necesidad.
No mezclar demasiados acentos en una misma pantalla.
Usar el color primario solo para acciones principales, foco, pasos activos y énfasis.
Los estados success/warning/danger deben ser visibles, pero sobrios.
```

### 4.2 Tipografía

Usar una tipografía sans-serif moderna, clara y legible.

```txt
Fuente recomendada: Inter, Geist, Manrope o system sans-serif
Heading principal: 28px–36px, font-semibold/bold
Heading sección: 20px–24px, font-semibold
Subheading: 14px–16px, text-muted
Body: 14px–15px
Label: 13px–14px, font-medium
Helper text: 12px–13px
```

Reglas:

```txt
No usar textos grandes sin jerarquía.
No usar más de 3 tamaños principales por pantalla.
No usar párrafos largos dentro de componentes visuales.
Los labels deben ser claros, no decorativos.
```

### 4.3 Radios, sombras y bordes

```txt
Card radius:       20px–28px
Input radius:      10px–14px
Button radius:     10px–14px
Badge radius:      999px
Modal radius:      20px–24px
Border width:      1px
Shadow base:       suave, difusa, sin exageración
```

Reglas:

```txt
Las sombras deben ser sutiles.
No usar sombras duras ni negras fuertes.
Los bordes deben ayudar a separar, no decorar en exceso.
Las cards deben sentirse livianas.
```

---

## 5. Layout oficial

### 5.1 Layout general de aplicación interna

Las pantallas internas deben seguir este patrón:

```txt
App shell
├── Sidebar / navegación principal
├── Topbar contextual
└── Page container
    ├── Page header
    ├── Acción primaria
    ├── Filtros / métricas si aplica
    └── Contenido principal
```

Reglas:

```txt
Usar ancho máximo cuando la lectura lo requiera.
Usar spacing consistente: 24px / 32px / 40px.
No pegar contenido a los bordes.
No mezclar demasiadas cards en una misma fila.
Evitar pantallas visualmente densas.
```

### 5.2 Pantallas públicas y auth

Login, registro, onboarding y recuperación de contraseña deben usar un layout más editorial:

```txt
Split-screen premium
├── Panel visual izquierdo
│   ├── Imagen/fotografía inmersiva
│   ├── Overlay suave
│   ├── Logo superior
│   └── Frase/testimonio inferior
└── Panel derecho
    ├── Formulario limpio
    ├── Progreso por pasos si aplica
    ├── Campos mínimos por paso
    └── CTA claro
```

Reglas:

```txt
Usar split-screen en login, registro y onboarding.
El panel izquierdo debe reforzar marca, confianza y aspiración.
El panel derecho debe priorizar claridad y conversión.
No mostrar formularios largos si se pueden dividir en pasos.
No saturar con cards informativas.
```

---

## 6. Registro y onboarding por pasos

Todo flujo largo debe convertirse en wizard por pasos.

### 6.1 Registro de comercio

Estructura oficial:

```txt
Paso 1: Negocio
- Plan
- Nombre del comercio
- Sucursal principal
- Tipo de identificación
- Número de identificación
- Teléfono principal

Paso 2: Administrador
- Nombre del administrador
- Correo electrónico
- Contraseña
- Confirmar contraseña si aplica
- Teléfono secundario si aplica

Paso 3: Confirmación
- Resumen del comercio
- Resumen del plan
- Resumen del administrador
- Confirmación final
```

### 6.2 Stepper

El stepper debe ser horizontal, sutil y claro.

```txt
Activo: círculo o punto en primary
Inactivo: borde gris suave
Completado: ícono check o color primary suave
Línea: gris suave, primary solo en progreso completado
```

Reglas:

```txt
No usar stepper demasiado grande.
No usar animaciones fuertes.
No ocultar el paso actual.
Mostrar texto como: Paso 1 de 3 · Información del negocio.
```

---

## 7. Componentes base

### 7.1 Botones

Tipos oficiales:

```txt
Primary: acción principal de la pantalla
Secondary: acción alternativa
Ghost: navegación ligera
Danger: acciones destructivas
Icon: acciones compactas
```

Reglas:

```txt
Solo debe haber un botón primary dominante por bloque.
Los botones deben tener altura consistente: 40px–44px.
Los botones destructivos deben pedir confirmación cuando afecten datos críticos.
Los labels deben usar verbos claros: Crear, Guardar, Continuar, Confirmar, Cancelar.
```

### 7.2 Inputs

```txt
Altura: 40px–44px
Borde: 1px suave
Focus: borde primary + ring suave
Placeholder: muted, no demasiado oscuro
Error: mensaje debajo del campo
```

Reglas:

```txt
Siempre usar label visible.
No depender solo del placeholder.
Mostrar mensajes de error claros.
No usar inputs desalineados.
No mezclar estilos de input por módulo.
```

### 7.3 Selects, combobox y filtros

```txt
Select simple: pocas opciones
Combobox: listas largas o búsqueda
Filtros: usar barra o panel sutil, no modales innecesarios
```

Reglas:

```txt
Los filtros deben ser compactos.
Los filtros activos deben ser visibles.
Debe existir opción de limpiar filtros.
```

### 7.4 Cards

Las cards deben agrupar información relacionada.

```txt
Padding: 20px–24px
Radius: 20px–24px
Border: suave
Shadow: opcional y ligera
```

Reglas:

```txt
No meter demasiada información en una card.
No usar cards dentro de cards salvo casos justificados.
No usar bordes y sombras pesadas al mismo tiempo.
```

### 7.5 Tablas

Las tablas deben ser limpias y escaneables.

```txt
Header claro
Filas con hover sutil
Acciones alineadas a la derecha
Estados con badges
Empty state cuando no hay datos
Skeleton/loading cuando carga
```

Reglas:

```txt
No mostrar columnas innecesarias.
No poner acciones críticas sin confirmación.
No usar textos largos sin truncamiento o tooltip.
Las tablas deben funcionar bien en pantallas pequeñas.
```

### 7.6 Badges de estado

Usar badges para estados de negocio.

```txt
Received / Draft: gris/azul suave
Processing: azul/índigo suave
Completed: verde suave
Failed: rojo suave
Cancelled: gris/rojo suave
Pending: ámbar suave
```

Reglas:

```txt
No depender solo del color; incluir texto.
Usar labels consistentes en todo el sistema.
No inventar nombres de estados visuales por pantalla.
```

---

## 8. Estados de interfaz obligatorios

Toda pantalla o componente que consuma datos debe manejar:

```txt
Loading
Empty
Error
Success
Disabled
Submitting
Realtime updating
```

### 8.1 Loading

```txt
Usar skeletons suaves en tablas/cards.
Usar spinners solo en acciones puntuales.
No bloquear toda la pantalla si solo carga una sección.
```

### 8.2 Empty state

Debe incluir:

```txt
Ícono sutil
Título claro
Descripción breve
CTA si aplica
```

Ejemplo:

```txt
No hay productos registrados.
Agrega tu primer producto para comenzar a vender.
[Crear producto]
```

### 8.3 Error state

Debe incluir:

```txt
Mensaje entendible
Acción de reintentar si aplica
Sin stack trace
Sin detalles técnicos innecesarios
```

---

## 9. Formularios oficiales

Todo formulario debe usar:

```txt
React Hook Form
Zod
Componentes compartidos
Mensajes de error consistentes
Estado submitting
Prevención de doble submit
```

Reglas:

```txt
No crear formularios con estado manual si React Hook Form aplica.
No validar solo en backend.
No mostrar errores genéricos cuando el campo puede indicar el problema.
No poner lógica de negocio dentro del componente visual.
```

Los formularios largos deben dividirse en:

```txt
Secciones
Steps
Tabs
Accordion sutil, si aplica
```

---

## 10. Motion e interacción

La animación debe ser mínima y útil.

```txt
Duración: 150ms–250ms
Easing: suave
Usar para hover, focus, apertura de modal, cambio de paso
No usar animaciones llamativas en flujos críticos
```

Reglas:

```txt
La animación no debe impedir velocidad.
No usar rebotes excesivos.
No animar tablas completas con muchos datos.
```

---

## 11. Iconografía

Usar una sola familia de íconos, preferiblemente `lucide-react`.

Reglas:

```txt
Tamaño base: 16px–20px
Stroke consistente
No mezclar estilos filled/outline sin intención
Los íconos deben ayudar a entender, no decorar en exceso
```

---

## 12. Imágenes y paneles visuales

En pantallas auth/onboarding:

```txt
Usar fotografías o composiciones visuales premium.
Aplicar overlay oscuro suave para legibilidad.
Mantener logo arriba.
Usar frase/testimonio abajo.
Evitar imágenes genéricas o de baja calidad.
```

El panel visual no debe competir con el formulario. Debe acompañar la experiencia.

---

## 13. Accesibilidad

Requisitos mínimos:

```txt
Contraste suficiente
Labels visibles
Focus visible
Navegación por teclado
Botones con nombres accesibles
Inputs asociados a labels
Mensajes de error legibles
No depender solo del color
```

Reglas:

```txt
Todo componente nuevo debe ser usable con teclado.
No eliminar outlines sin reemplazo visual.
Los modales deben manejar focus trap.
```

---

## 14. Responsive design

Toda pantalla debe funcionar en:

```txt
Desktop grande
Laptop
Tablet
Móvil cuando aplique
```

Reglas:

```txt
El split-screen puede convertirse en single-column en móvil.
Las tablas deben tener estrategia responsive.
Los formularios de dos columnas pasan a una columna en pantallas pequeñas.
Los CTAs deben quedar visibles y claros.
```

---

## 15. Arquitectura frontend obligatoria

La UI debe respetar la estructura oficial del proyecto:

```txt
apps/web/src/
├── app/
├── modules/
└── shared/
```

Reglas:

```txt
Componentes visuales reutilizables → shared/components
Hooks reutilizables → shared/hooks
Servicios HTTP → shared/services o módulo correspondiente
Tipos compartidos → shared/types
Pantallas de negocio → modules/{module}/pages
Componentes de módulo → modules/{module}/components
Hooks de módulo → modules/{module}/hooks
Schemas de formulario → modules/{module}/schemas
```

No se debe:

```txt
Hacer fetch directo en componentes grandes.
Duplicar componentes visuales iguales.
Mezclar lógica de negocio con presentación.
Crear páginas gigantes.
Copiar formularios completos entre módulos.
```

---

## 16. Componentes compartidos requeridos

El frontend debe tener una base de componentes reutilizables para asegurar consistencia:

```txt
PageShell
PageHeader
SectionHeader
ContentCard
MetricCard
DataTable
StatusBadge
EmptyState
ErrorState
LoadingSkeleton
FormField
PasswordField
SearchInput
FilterBar
ConfirmDialog
ActionMenu
StepWizard
Stepper
AuthSplitLayout
PhotoPanel
```

Cada pantalla debe usar estos componentes antes de crear uno nuevo.

---

## 17. Reglas por módulo

### 17.1 Dashboard

```txt
Métricas en cards limpias.
Gráficas con colores sobrios.
No saturar con demasiados indicadores.
Mostrar últimas actividades y alertas importantes.
```

### 17.2 POS

```txt
Debe priorizar velocidad.
Botones grandes y claros.
Carrito visible.
Estados de venta en tiempo real.
Evitar pasos innecesarios.
```

### 17.3 Productos e inventario

```txt
Tablas limpias.
Filtros rápidos.
Badges para stock bajo.
Acciones claras.
Formularios por secciones.
```

### 17.4 Clientes y fiados

```txt
Información financiera clara.
Alertas sobrias para deuda.
Historial fácil de leer.
No usar colores agresivos salvo riesgo real.
```

### 17.5 Ventas, compras y facturas

```txt
Estados visibles.
Timeline o historial cuando aplique.
Acciones críticas con confirmación.
Montos alineados y legibles.
```

### 17.6 Reportes

```txt
Filtros arriba.
Resumen visual primero.
Tablas o gráficas después.
Exportación visible pero no dominante.
```

### 17.7 Configuración y usuarios

```txt
Agrupar por secciones.
Usar formularios claros.
Evitar páginas largas sin división visual.
Mostrar permisos de forma comprensible.
```

---

## 18. Señales en tiempo real

Cuando una pantalla reciba eventos por SignalR:

```txt
Mostrar cambios sin recargar toda la página.
Usar indicadores sutiles de actualización.
Actualizar badges, tablas y métricas de forma consistente.
Evitar parpadeos visuales.
No mostrar datos de otro BusinessId.
```

Ejemplos:

```txt
Venta pasa de Processing a Completed.
Inventario se actualiza.
Compra cambia a InventoryUpdated.
Feedback beta cambia de estado.
```

---

## 19. Copywriting UI

El texto de la interfaz debe ser:

```txt
Claro
Breve
Humano
Dominicano-neutral
Profesional
Orientado a acción
```

Ejemplos correctos:

```txt
Crear comercio
Continuar
Guardar cambios
Registrar venta
No hay productos registrados
El plan queda asociado al comercio desde el inicio
```

Evitar:

```txt
Mensajes técnicos innecesarios
Textos muy largos en botones
Errores genéricos como “Algo salió mal” sin acción
Anglicismos innecesarios
```

---

## 20. Testing visual y funcional obligatorio

Todo cambio en UI crítica debe incluir o actualizar pruebas.

Pruebas mínimas según caso:

```txt
Component tests con React Testing Library
Integration tests con MSW
Mocks controlados de SignalR si aplica
Playwright para flujos críticos
```

Casos visuales obligatorios:

```txt
Render correcto del formulario
Validaciones visibles
Loading state
Error state
Empty state
Acción primary funcionando
Responsive básico si la pantalla lo requiere
```

---

## 21. Definition of Done UI

Una pantalla o componente de UI no está terminado hasta cumplir:

```txt
Sigue este look & feel.
Usa componentes compartidos cuando existen.
No duplica estilos innecesarios.
Tiene loading, empty y error si consume datos.
Tiene validación con Zod si es formulario.
Usa React Hook Form si es formulario complejo.
Usa TanStack Query para server state.
No hace fetch directo desde componentes grandes.
Respeta BusinessId y permisos desde el flujo seguro.
No expone datos sensibles.
Es responsive según el contexto.
Tiene pruebas cuando es UI crítica.
No introduce issues nuevos de Sonar.
No rompe accesibilidad básica.
```

---

## 22. Checklist antes de aprobar una pantalla

Antes de aprobar cualquier pantalla, validar:

```txt
¿La pantalla se ve consistente con el estándar premium SaaS?
¿Usa spacing y tipografía consistentes?
¿El CTA principal está claro?
¿Hay demasiadas cards, bordes o sombras?
¿Los formularios están divididos si son largos?
¿Los estados loading/error/empty existen?
¿Los colores de estado son consistentes?
¿Los badges usan nombres oficiales?
¿El responsive fue considerado?
¿La lógica de negocio está fuera de componentes visuales?
¿Se usaron hooks/servicios adecuados?
¿La pantalla tiene pruebas si es crítica?
```

---

## 23. Regla final

Ninguna pantalla nueva debe sentirse como una pieza aislada. Todo elemento visual de ComercioFlow RD debe parecer parte del mismo producto: limpio, sutil, moderno, consistente, rápido y confiable.

