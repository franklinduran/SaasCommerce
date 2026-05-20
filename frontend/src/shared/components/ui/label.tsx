import type { LabelHTMLAttributes, ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

export function Label({
  children,
  className,
  ...props
}: Readonly<LabelHTMLAttributes<HTMLLabelElement>> & Readonly<{ children?: ReactNode }>) {
  return (
    <label
      className={cn('text-sm font-semibold text-stone-800', className)}
      {...props}
    >
      {children}
    </label>
  )
}
