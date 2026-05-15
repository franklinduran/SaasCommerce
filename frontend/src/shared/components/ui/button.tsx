import { cva, type VariantProps } from 'class-variance-authority'
import type { ButtonHTMLAttributes } from 'react'
import { cn } from '@/shared/utils/cn'

const buttonVariants = cva(
  'inline-flex h-10 items-center justify-center gap-2 rounded-md px-4 text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-stone-900 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        default: 'bg-stone-900 text-white shadow-sm hover:bg-stone-800',
        secondary: 'bg-white text-stone-900 shadow-sm ring-1 ring-stone-300 hover:bg-stone-50',
        ghost: 'bg-transparent text-stone-700 hover:bg-stone-100 hover:text-stone-900',
        outline:
          'bg-white text-stone-900 shadow-sm ring-1 ring-stone-300 hover:bg-stone-50',
      },
      size: {
        default: 'h-10 px-4',
        icon: 'h-10 w-10 px-0',
        sm: 'h-8 px-3 text-xs',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'default',
    },
  },
)

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> &
  VariantProps<typeof buttonVariants>

export function Button({ className, variant, size, ...props }: ButtonProps) {
  return (
    <button
      className={cn(buttonVariants({ variant, size, className }))}
      {...props}
    />
  )
}
