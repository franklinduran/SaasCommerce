# Reglas de UI - SaasCommerce

La interfaz debe ser limpia, sobria y profesional. Debe usar como base visual la paleta Tailwind Stone, tomando como referencia UI Colors / Tailwind Colors Stone: https://uicolors.app/tailwind-colors/stone.

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

Usar `stone` como neutral principal:

```txt
Background principal: stone-50
Surface/Card: white
Surface secundaria: stone-100
Border: stone-200
Texto principal: stone-900
Texto secundario: stone-600
Texto muted: stone-500
Hover suave: stone-100
Sidebar fondo recomendado: stone-100 o white
Sidebar activa: stone-900
Sidebar texto activo: white
```

## 3. Color de accion

El color primario debe ser sobrio.

```txt
Primary: stone-900
Primary hover: stone-800
Primary text: white
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

La sidebar debe ser simple:

```txt
Fondo recomendado: stone-100 o white
Texto: stone-700
Texto activo: white
Item hover: stone-200
Item activo: stone-900
Borde derecho: stone-200
```

Reglas:

```txt
Iconos pequenos.
Texto legible.
No usar muchos colores.
No usar cards dentro del sidebar.
No usar gradientes.
No usar negro o fondos oscuros pesados por defecto.
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
Background: white
Border bottom: stone-200
Texto: stone-900
```

## 7. Cards

Usar cards para secciones, metricas y formularios.

```txt
Background: white
Border: stone-200
Radius: rounded-xl
Shadow: shadow-sm o sin shadow
Padding: p-5 o p-6
```

No hacer cards con colores fuertes. Para resaltar, usar borde o badge.

## 8. Botones

Jerarquia obligatoria:

```txt
Primary:
bg-stone-900 text-white hover:bg-stone-800

Secondary:
bg-white text-stone-900 border border-stone-300 hover:bg-stone-50

Ghost:
text-stone-700 hover:bg-stone-100

Danger:
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
bg-white
border-stone-300
text-stone-900
placeholder-stone-400
focus:ring-stone-900
focus:border-stone-900
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
Header: bg-stone-50 text-stone-600
Rows: bg-white
Border: stone-200
Hover row: stone-50
Text: stone-900
Secondary text: stone-500
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
Usa paleta stone?
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
Antes de modificar UI, debes seguir las reglas de UI del proyecto. La interfaz debe usar Tailwind Stone como paleta principal, con estilo SaaS administrativo sobrio, limpio y profesional. No uses colores aleatorios, gradientes fuertes, sombras exageradas ni componentes gigantes. Usa ShadCN UI, cards blancas, bordes stone-200, texto stone-900, secundarios stone-600, primary stone-900, estados con emerald/amber/red/sky solo cuando representen informacion real. Toda pantalla debe tener loading, error y empty states, formularios con React Hook Form + Zod, componentes reutilizables y diseno responsive.
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
