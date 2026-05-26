import type { HTMLAttributes } from 'react'
import { cn } from '@/shared/utils/cn'

export function Alert({ className, ...props }: Readonly<HTMLAttributes<HTMLDivElement>>) {
  return (
    <div
      className={cn(
        'relative w-full rounded-lg border px-4 py-3 text-sm [&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&>svg]:text-current [&>:last-child]:mb-0 [&>div+div]:ml-7',
        className,
      )}
      {...props}
    />
  )
}

export function AlertTitle({
  children,
  className,
  ...props
}: Readonly<HTMLAttributes<HTMLHeadingElement>>) {
  return (
    <h5 className={cn('mb-1 font-medium leading-tight', className)} {...props}>
      {children}
    </h5>
  )
}

export function AlertDescription({
  className,
  ...props
}: Readonly<HTMLAttributes<HTMLDivElement>>) {
  return <div className={cn('text-sm [&_p]:leading-relaxed', className)} {...props} />
}
