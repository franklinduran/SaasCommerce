import type { ReactNode } from 'react'

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning'

const variantClasses: Record<BadgeVariant, string> = {
  default: 'bg-stone-900 text-white',
  destructive: 'bg-red-600 text-white',
  outline: 'border border-stone-300 text-stone-700',
  secondary: 'bg-stone-100 text-stone-800',
  success: 'bg-emerald-100 text-emerald-800',
  warning: 'bg-amber-100 text-amber-800',
}

export function Badge({
  children,
  className = '',
  variant = 'default',
}: Readonly<{
  children: ReactNode
  className?: string
  variant?: BadgeVariant
}>) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${variantClasses[variant]} ${className}`}
    >
      {children}
    </span>
  )
}
