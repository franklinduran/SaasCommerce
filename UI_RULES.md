# Reglas de UI - SaasCommerce

La interfaz debe ser limpia, sobria y profesional. Usa la paleta Indigo como base visual con un sidebar oscuro (navy/indigo-950), fondo lavanda suave (indigo-50) y brand color indigo-600.

## 0. Sistema de tokens de diseño (HOW TO RETHEME)

**Toda modificacion visual debe hacerse en `frontend/src/index.css` unicamente.**

La arquitectura de tokens es:
```
frontend/src/index.css        ← FUENTE DE VERDAD: define todos los colores en :root
  @theme inline { ... }       ← Mapea vars CSS a utilidades Tailwind (no tocar)
frontend/tailwind.config.ts   ← Solo aliases legacy; referencia CSS vars (no hardcodear hex)
Componentes (.tsx)            ← Solo usan clases semanticas: bg-primary, text-foreground, etc.
```

### Tokens principales disponibles como utilidades Tailwind:

```txt
# Superficie
bg-background       → fondo global (indigo-50 #EEF2FF)
bg-card             → tarjetas / formularios (blanco)
bg-muted            → superficie secundaria (indigo-50)
bg-secondary        → hover suave, chips (indigo-100)

# Texto
text-foreground         → texto principal
text-muted-foreground   → texto secundario / labels
text-card-foreground    → texto dentro de cards

# Accion primaria (brand indigo)
bg-primary              → boton primario, active states
text-primary-foreground → texto sobre primary
hover:bg-primary-hover  → hover del primario

# Bordes y rings
border-border           → bordes de cards, dividers
shadow-control          → ring de inputs normal (0 0 0 1px var(--border))
shadow-control-focus    → ring de inputs en focus (0 0 0 1px var(--ring))
ring-ring/25            → focus ring para botones e interactivos

# Sidebar (dark navy)
bg-sidebar-bg           → fondo sidebar
text-sidebar-fg         → texto sidebar
text-sidebar-muted      → labels de secciones, texto secundario
border-sidebar-border   → dividers dentro del sidebar
bg-sidebar-hover-bg     → hover de items
bg-sidebar-active-bg    → item activo (= brand)
text-sidebar-active-fg  → texto item activo (blanco)
```

### Regla para nuevos componentes:
- Usar SIEMPRE clases semanticas (bg-card, text-foreground, border-border).
- NUNCA hardcodear colores de paleta (bg-indigo-600, text-stone-900, #4F46E5).
- Para cambiar el look del sistema: editar solo `:root` en `index.css`.

## 1. Estilo visual obligatorio

La UI debe sentirse como un SaaS moderno de administracion: clara, espaciosa, ordenada y util. No debe parecer una plantilla generica, colorida o sobrecargada.

Usar:

- Paleta base: `stone`.
- Bordes suaves.
- Sombras sutiles.
- Mucho espacio en blanco.
- Componentes consistentes.
- Tipografia clara.
- Estados visibles: loading, error, empty, success.

No usar:

- Gradientes fuertes.
- Colores neon.
- Fondos oscuros pesados por defecto.
- Cards con sombras exageradas.
- Bordes gruesos innecesarios.
- Demasiados colores compitiendo.
- Tablas apretadas.
- Botones gigantes sin jerarquia.

## 2. Paleta principal

Usar tokens semanticos, no clases de paleta directa:

```txt
Background principal:  bg-background      (indigo-50  #EEF2FF)
Surface/Card:          bg-card            (blanco)
Surface secundaria:    bg-muted           (indigo-50)
Border:                border-border      (indigo-200)
Texto principal:       text-foreground    (near-black con tinte indigo)
Texto secundario:      text-muted-foreground
Hover suave:           hover:bg-muted / hover:bg-secondary
Sidebar fondo:         bg-sidebar-bg      (indigo-950 #1E1B4B)
Sidebar texto:         text-sidebar-fg    (indigo-100)
Sidebar activa:        bg-sidebar-active-bg (indigo-600 #4F46E5)
Sidebar texto activo:  text-sidebar-active-fg (blanco)
```

## 3. Color de accion

El color primario es indigo-600:

```txt
Primary:          bg-primary              (#4F46E5)
Primary hover:    hover:bg-primary-hover  (#4338CA)
Primary text:     text-primary-foreground (blanco)
```

Para acciones criticas o estados:

```txt
Success: emerald
Warning: amber
Danger: red
Info: sky
```

Regla: los colores de estado solo se usan para estados reales, no para decorar.

## 4. Layout base

Toda pantalla interna debe usar esta estructura:

```txt
AppShell
├── Sidebar
├── Topbar
└── MainContent
```

Reglas:

```txt
Sidebar ancho: 260px
Topbar altura: 64px
Main padding: 24px o 32px
Contenido maximo: full width en dashboards y tablas
Cards: rounded-xl o rounded-2xl
```

El dashboard no debe estar centrado como landing page. Debe parecer herramienta de trabajo.

Reglas de scroll:

```txt
AppShell debe ocupar exactamente 100dvh en pantallas internas.
La sidebar nunca debe crecer mas que el alto visible.
La sidebar debe tener scroll interno si su contenido excede el alto disponible.
La topbar debe permanecer fija dentro del shell.
MainContent debe ser el unico contenedor con scroll vertical principal.
El contenido de cada pantalla debe vivir dentro de un contenedor interno para evitar que sidebar/topbar se muevan al hacer scroll.
No usar scroll del body para pantallas internas.
```

## 5. Sidebar

La sidebar usa un tema oscuro (dark navy):

```txt
Fondo:              bg-sidebar-bg      (indigo-950 #1E1B4B)
Texto:              text-sidebar-fg    (indigo-100 #E0E7FF)
Labels de seccion:  text-sidebar-muted
Item hover:         bg-sidebar-hover-bg
Item activo:        bg-sidebar-active-bg (indigo-600)
Texto activo:       text-sidebar-active-fg (blanco)
Bordes/dividers:    border-sidebar-border
```

Reglas:

```txt
Iconos pequenos.
Texto legible sobre fondo oscuro.
Usar solo tokens sidebar-* dentro del sidebar.
No usar clases de paleta directa (indigo-*, stone-*) en el sidebar.
No usar cards dentro del sidebar.
No usar gradientes.
```

## 6. Topbar

La topbar debe mostrar:

```txt
Titulo de pantalla
Breadcrumb opcional
Boton principal de accion
Usuario actual o negocio
```

Estilo:

```txt
Background:     bg-card      (blanco)
Border bottom:  border-border
Texto:          text-foreground / text-muted-foreground
```

## 7. Cards

Usar cards para secciones, metricas y formularios.

```txt
Background: bg-card   (blanco)
Border:     ring-1 ring-border
Radius:     rounded-2xl
Shadow:     shadow-sm
Padding:    p-5 o p-6
```

No hacer cards con colores fuertes. Para resaltar, usar borde o badge.

## 8. Botones

Jerarquia obligatoria (usando variants de button-variants.ts):

```txt
Primary (default):
bg-primary text-primary-foreground hover:bg-primary-hover

Secondary:
bg-card text-foreground ring-1 ring-border hover:bg-secondary

Ghost:
bg-transparent text-foreground hover:bg-muted

Danger (destructive):
bg-red-600 text-white hover:bg-red-700
```

Reglas:

```txt
Solo un boton primary por seccion.
Botones destructivos siempre con confirmacion.
No usar botones de colores diferentes sin razon.
```

## 9. Formularios

Inputs:

```txt
bg-card
shadow-control          (ring indigo-200 normal)
shadow-control-focus    (ring indigo-600 al focus)
text-foreground
placeholder:text-muted-foreground
focus:ring-ring/15
```

Reglas:

```txt
Labels siempre visibles.
Errores debajo del campo.
No usar placeholders como labels.
Campos monetarios alineados y formateados.
Validacion con React Hook Form + Zod.
```

## 10. Tablas

Tablas limpias, no recargadas.

```txt
Header: bg-muted text-muted-foreground
Rows: bg-card
Border: border-border
Hover row: hover:bg-muted
Text: text-foreground
Secondary text: text-muted-foreground
```

Reglas:

```txt
Acciones al final.
Estados con badges.
Paginacion visible.
Empty state cuando no hay datos.
Skeleton mientras carga.
```

## 11. Badges

Usar badges para estados.

```txt
Active: emerald
Inactive: stone
Pending: amber
Failed: red
Processing: sky
Completed: emerald
Cancelled: stone
```

Regla: los badges deben tener fondo suave y texto oscuro.

Ejemplo:

```txt
bg-emerald-50 text-emerald-700 border-emerald-200
```

## 12. Dashboard

El dashboard debe priorizar claridad.

Debe incluir:

```txt
Cards de metricas
Graficas simples
Ultimas ventas
Productos con bajo stock
Accesos rapidos
```

No debe incluir:

```txt
Gradientes decorativos
Ilustraciones innecesarias
Cards gigantes sin datos
Colores aleatorios
```

## 13. Dark mode

No implementar dark mode hasta que la UI clara este estable.

Cuando se implemente:

```txt
Background: stone-950
Surface: stone-900
Border: stone-800
Text: stone-50
Muted: stone-400
Primary: stone-100
Primary text: stone-950
```

## 14. Regla para agentes IA

Antes de crear o modificar UI, debes identificar:

```txt
Modulo afectado
Pantalla afectada
Componentes reutilizables
Estados de loading/error/empty
Validaciones
Responsive behavior
```

No debes crear UI bonita sin consistencia. Debes usar el sistema visual definido: Tailwind Stone, ShadCN UI, componentes pequenos, jerarquia clara y accesibilidad.

## 15. Checklist obligatorio antes de aceptar UI

```txt
Usa tokens semanticos (bg-primary, text-foreground, border-border, etc.)?
Tiene jerarquia visual clara?
Tiene estados loading/error/empty?
Usa componentes reutilizables?
No hay colores aleatorios?
No hay gradientes innecesarios?
Los formularios tienen labels?
Los errores son visibles?
Las tablas tienen hover, empty state y paginacion?
El diseno funciona en laptop y desktop?
No se mezclo logica de negocio en componentes visuales?
```

## Prompt corto para agentes

```txt
Antes de modificar UI, debes seguir las reglas de UI del proyecto. El sistema usa tokens semanticos definidos en index.css — NUNCA hardcodees colores de paleta (indigo-*, stone-*, hex). Usa bg-card para tarjetas blancas, bg-background para fondos lavanda, bg-primary para acciones brand (indigo-600), text-foreground para texto principal, text-muted-foreground para secundario, border-border para dividers. El sidebar usa tokens bg-sidebar-bg / text-sidebar-fg / bg-sidebar-active-bg. Cards usan rounded-2xl. Inputs usan shadow-control / shadow-control-focus. Usa ShadCN UI, estados con emerald/amber/red/sky solo para informacion real, React Hook Form + Zod en formularios, diseno responsive.
```
# Regla de formularios de onboarding

Todo formulario de onboarding debe indicar claramente campos obligatorios.

Reglas:

- Los labels deben estar visibles.
- Los campos requeridos deben marcarse con `*` o texto equivalente.
- Los errores deben aparecer debajo del campo.
- El boton submit debe mostrar estado loading.
- No se debe permitir avanzar visualmente si faltan campos criticos.
- La respuesta de error del backend debe mostrarse de forma clara y segura.

# Regla de settings

Las pantallas de settings deben ser sobrias y operativas.

Reglas:

- Separar perfil, negocio, sucursal y seguridad en secciones pequenas.
- No mezclar datos sensibles con datos operativos.
- Mantener labels visibles y campos requeridos marcados.
- Mostrar loading, error y success por seccion.
- No permitir editar `BusinessId`, `BranchId`, `UserId`, roles ni tokens desde formularios.
- El boton principal de cada seccion debe tener estado loading mientras guarda.
