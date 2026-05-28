import type { InputHTMLAttributes } from 'react'
import { forwardRef } from 'react'
import { cn } from '@/shared/utils/cn'

type InputProps = Readonly<InputHTMLAttributes<HTMLInputElement>>

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { className, ...props },
  ref,
) {
  return (
    <input
      ref={ref}
      className={cn(
        'h-10 w-full min-w-0 rounded-md bg-card px-3 text-sm font-medium text-foreground shadow-control outline-none transition placeholder:text-muted-foreground focus:shadow-control-focus focus:ring-2 focus:ring-ring/15 disabled:cursor-not-allowed disabled:bg-muted disabled:text-muted-foreground',
        className,
      )}
      {...props}
    />
  )
})
