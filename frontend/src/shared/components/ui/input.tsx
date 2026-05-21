import type { InputHTMLAttributes } from 'react'
import { forwardRef } from 'react'
import { cn } from '@/shared/utils/cn'

type InputProps = InputHTMLAttributes<HTMLInputElement>

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { className, ...props },
  ref,
) {
  return (
    <input
      ref={ref}
      className={cn(
        'h-10 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15 disabled:cursor-not-allowed disabled:bg-stone-100 disabled:text-stone-600',
        className,
      )}
      {...props}
    />
  )
})
