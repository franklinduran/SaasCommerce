import type { ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline' | 'success' | 'warning'

const variantClasses: Record<BadgeVariant, string> = {
  default:     'bg-primary text-primary-foreground',
  destructive: 'bg-red-600 text-white',
  outline:     'border border-border text-foreground',
  secondary:   'bg-secondary text-secondary-foreground',
  success:     'bg-emerald-100 text-emerald-800',
  warning:     'bg-amber-100 text-amber-800',
}

export function Badge({
  children,
  className,
  variant = 'default',
}: Readonly<{
  children: ReactNode
  className?: string
  variant?: BadgeVariant
}>) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full border border-transparent px-2.5 py-0.5 text-xs font-semibold',
        variantClasses[variant],
        className,
      )}
    >
      {children}
    </span>
  )
}
