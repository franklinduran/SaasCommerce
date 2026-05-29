import { ShoppingBag } from 'lucide-react'
import { useEffect, useState, type ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button } from '@/shared/components/ui/button'
import { cn } from '@/shared/utils/cn'

/**
 * Shared auth/onboarding split layout — full-bleed.
 *
 * - Mobile / md: form panel only, fills viewport (`h-dvh`).
 * - lg+: architecture photo hero on the left, white form panel on the right.
 */
export function AuthSplitLayout({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <div className="flex h-dvh overflow-hidden">
      <AuthHeroPanel />
      <div className="flex flex-1 flex-col overflow-hidden bg-white">
        {children}
      </div>
    </div>
  )
}

/**
 * Top-right header used by both auth pages to surface the alternate flow
 * ("¿Ya tienes cuenta?" / "¿No tienes cuenta?"). Centralised so the spacing,
 * typography and button variant stay in lockstep on both pages.
 */
export function AuthAltHeader({
  actionLabel,
  prompt,
  to,
}: Readonly<{ actionLabel: string; prompt: string; to: string }>) {
  const navigate = useNavigate()
  return (
    <header className="flex shrink-0 items-center justify-between px-8 py-5 lg:px-12">
      <span className="text-[13px] text-gray-400">{prompt}</span>
      <Button
        className="font-medium"
        onClick={() => navigate(to)}
        size="sm"
        variant="outline"
      >
        {actionLabel}
      </Button>
    </header>
  )
}

// ─── Hero panel — premium architecture photo with working testimonial slider ─

const HERO_PHOTO_URL =
  'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1400&q=80'

const TESTIMONIALS = [
  {
    name: 'Karen Yue',
    quote: 'Simplemente tiene todas las herramientas que mi equipo necesita',
    role: 'Directora de Tecnología de Marketing Digital',
  },
  {
    name: 'Carlos Méndez',
    quote: 'Manejar inventario y ventas nunca había sido tan simple',
    role: 'Propietario · Colmado La Esperanza',
  },
  {
    name: 'María Reyes',
    quote: 'Nuestro equipo ahorra horas cada semana gracias a la plataforma',
    role: 'Gerente de Operaciones · Boutique Lima',
  },
] as const

const ROTATION_MS = 6000

function AuthHeroPanel() {
  const [active, setActive] = useState(0)

  // Auto-rotate every ROTATION_MS. Resetting the interval each time the
  // user clicks a dot avoids a stale interval firing right after manual nav.
  useEffect(() => {
    const id = window.setInterval(
      () => setActive((i) => (i + 1) % TESTIMONIALS.length),
      ROTATION_MS,
    )
    return () => window.clearInterval(id)
  }, [active])

  const current = TESTIMONIALS[active]

  return (
    <div className="relative hidden overflow-hidden lg:flex lg:w-[42%] lg:shrink-0 lg:flex-col">

      {/* Photo background */}
      <img
        alt=""
        aria-hidden="true"
        className="absolute inset-0 h-full w-full object-cover"
        src={HERO_PHOTO_URL}
      />

      {/* Dual-layer gradient overlay — softens top, deepens bottom for legibility */}
      <div className="absolute inset-x-0 top-0 h-[35%] bg-gradient-to-b from-black/40 to-transparent" />
      <div className="absolute inset-x-0 bottom-0 h-[60%] bg-gradient-to-t from-black/80 via-black/40 to-transparent" />

      {/* Content */}
      <div className="relative z-10 flex h-full flex-col justify-between p-10">

        {/* Logo */}
        <div className="flex items-center gap-2.5 text-white">
          <div className="flex h-9 w-9 items-center justify-center rounded-[10px] bg-white/15 ring-1 ring-inset ring-white/20 backdrop-blur">
            <ShoppingBag aria-hidden="true" size={17} strokeWidth={2.5} />
          </div>
          <span className="text-[16px] font-semibold tracking-tight">Comercio</span>
        </div>

        {/* Testimonial slider — text directly on photo */}
        <div className="text-white">
          {/* key forces remount → restart fade-in animation on slide change */}
          <div className="animate-in fade-in duration-500" key={active}>
            <blockquote className="text-[1.65rem] font-bold leading-[1.2] tracking-tight">
              "{current.quote}"
            </blockquote>
            <p className="mt-6 text-[15px] font-semibold">{current.name}</p>
            <p className="mt-1 text-[13px] text-white/70">{current.role}</p>
          </div>

          {/* Clickable carousel dots */}
          <div
            aria-label="Testimonios"
            className="mt-6 flex items-center gap-2"
            role="tablist"
          >
            {TESTIMONIALS.map((t, i) => (
              <button
                aria-label={`Testimonio de ${t.name}`}
                aria-selected={i === active}
                className={cn(
                  'h-1.5 rounded-full transition-all',
                  i === active ? 'w-6 bg-white' : 'w-1.5 bg-white/35 hover:bg-white/55',
                )}
                key={t.name}
                onClick={() => setActive(i)}
                role="tab"
                type="button"
              />
            ))}
          </div>
        </div>

      </div>
    </div>
  )
}
