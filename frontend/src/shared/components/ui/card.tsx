import type { HTMLAttributes } from 'react'
import { cn } from '@/shared/utils/cn'

export function Card({ className, ...props }: Readonly<HTMLAttributes<HTMLDivElement>>) {
  return (
    <div
      className={cn('min-w-0 rounded-xl bg-white p-0 shadow-sm ring-1 ring-stone-200', className)}
      {...props}
    />
  )
}

export function CardHeader({ className, ...props }: Readonly<HTMLAttributes<HTMLDivElement>>) {
  return <div className={cn('p-4 pb-3 sm:p-5 sm:pb-3', className)} {...props} />
}

export function CardContent({ className, ...props }: Readonly<HTMLAttributes<HTMLDivElement>>) {
  return <div className={cn('p-4 pt-0 sm:p-5 sm:pt-0', className)} {...props} />
}
