import { ShoppingBag } from 'lucide-react'
import type { ReactNode } from 'react'

/**
 * Shared auth/onboarding split layout — full-bleed.
 *
 * - Mobile / md: form panel only, fills viewport (`h-dvh`).
 * - lg+: architecture photo hero on the left, white form panel on the right.
 *   Edge-to-edge full viewport, no floating card.
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

// ─── Hero panel — premium architecture photo with overlay ─────────────────────

const HERO_PHOTO_URL =
  'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1400&q=80'

function AuthHeroPanel() {
  return (
    <div className="relative hidden overflow-hidden lg:flex lg:w-[42%] lg:shrink-0 lg:flex-col">

      {/* Photo background */}
      <img
        alt=""
        aria-hidden="true"
        className="absolute inset-0 h-full w-full object-cover"
        src={HERO_PHOTO_URL}
      />

      {/* Dual-layer gradient overlay */}
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

        {/* Testimonial — text directly on photo, no card */}
        <div className="text-white">
          <blockquote className="text-[1.65rem] font-bold leading-[1.2] tracking-tight">
            "Simplemente tiene todas<br />
            las herramientas que mi<br />
            equipo necesita"
          </blockquote>
          <p className="mt-6 text-[15px] font-semibold">Karen Yue</p>
          <p className="mt-1 text-[13px] text-white/70">
            Directora de Tecnología de Marketing Digital
          </p>

          {/* Carousel dots: 1 wide + 2 small */}
          <div className="mt-6 flex items-center gap-2">
            <span className="h-1.5 w-6 rounded-full bg-white" />
            <span className="h-1.5 w-1.5 rounded-full bg-white/35" />
            <span className="h-1.5 w-1.5 rounded-full bg-white/35" />
          </div>
        </div>

      </div>
    </div>
  )
}
