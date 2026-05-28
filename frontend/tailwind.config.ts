import type { Config } from 'tailwindcss'

// All design tokens are defined as CSS custom properties in src/index.css (:root).
// This file references those variables so that legacy utility aliases (bg-surface,
// text-steel, etc.) keep working. To change the palette, edit index.css — not here.
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        border:          'var(--border)',
        background:      'var(--background)',
        foreground:      'var(--foreground)',
        muted:           'var(--muted)',
        surface:         'var(--card)',
        'surface-subtle':'var(--muted)',
        primary:         'var(--primary)',
        brand:           'var(--brand)',
        accent:          'var(--accent)',
        steel:           'var(--muted-foreground)',
        warning:         '#92400e',  // amber-800 — static semantic color
        danger:          'var(--destructive)',
      },
      boxShadow: {
        // Drop-shadow elevation helpers (distinct from ring-style shadow-control in index.css)
        panel:   '0 1px 3px rgb(79 70 229 / 0.06)',
        control: '0 1px 3px rgb(79 70 229 / 0.08)',
      },
    },
  },
  plugins: [],
} satisfies Config
