import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        border: '#dfe5dc',
        background: '#f6f7f4',
        foreground: '#17211c',
        muted: '#eef1eb',
        primary: '#146c43',
        accent: '#0f6e7a',
        warning: '#a16207',
        danger: '#b42318',
      },
      boxShadow: {
        panel: '0 12px 40px rgb(23 33 28 / 0.08)',
      },
    },
  },
  plugins: [],
} satisfies Config
