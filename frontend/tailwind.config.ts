import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        border: '#e7e5e4',
        background: '#fafaf9',
        foreground: '#1c1917',
        muted: '#f5f5f4',
        surface: '#ffffff',
        'surface-subtle': '#fafaf9',
        primary: '#1c1917',
        accent: '#44403c',
        steel: '#57534e',
        warning: '#92400e',
        danger: '#b91c1c',
      },
      boxShadow: {
        panel: '0 1px 2px rgb(28 25 23 / 0.06)',
        control: '0 1px 2px rgb(28 25 23 / 0.08)',
      },
    },
  },
  plugins: [],
} satisfies Config
