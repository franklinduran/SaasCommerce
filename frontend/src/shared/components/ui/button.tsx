import { Slot } from '@radix-ui/react-slot'
import { cva, type VariantProps } from 'class-variance-authority'
import type { ButtonHTMLAttributes } from 'react'
import { cn } from '@/shared/utils/cn'

export const buttonVariants = cva(
  'inline-flex h-10 min-w-0 items-center justify-center gap-2 whitespace-nowrap rounded-md px-4 text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-stone-900/25 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:shadow-none',
  {
    variants: {
      variant: {
        default:
          'bg-stone-900 text-white shadow-sm hover:bg-stone-800 active:bg-stone-950 disabled:bg-stone-200 disabled:text-stone-600',
        secondary:
          'bg-white text-stone-900 shadow-sm ring-1 ring-stone-300 hover:bg-stone-50 hover:text-stone-950 active:bg-stone-100 active:text-stone-950 disabled:bg-stone-100 disabled:text-stone-600 disabled:ring-stone-200',
        ghost:
          'bg-transparent text-stone-700 hover:bg-stone-100 hover:text-stone-950 active:bg-stone-200 active:text-stone-950 disabled:text-stone-500',
        outline:
          'bg-white text-stone-900 shadow-sm ring-1 ring-stone-300 hover:bg-stone-50 hover:text-stone-950 active:bg-stone-100 active:text-stone-950 disabled:bg-stone-100 disabled:text-stone-600 disabled:ring-stone-200',
        destructive:
          'bg-red-600 text-white shadow-sm hover:bg-red-700 active:bg-red-800 focus-visible:ring-red-600/25 disabled:bg-red-100 disabled:text-red-700',
      },
      size: {
        default: 'h-10 px-4',
        sm:      'h-8 px-3 text-xs',
        icon:    'h-10 w-10 px-0',
      },
    },
    defaultVariants: {
      variant: 'default',
      size:    'default',
    },
  },
)

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> &
  VariantProps<typeof buttonVariants> & {
    asChild?: boolean
  }

export function Button({ className, variant, size, asChild = false, ...props }: ButtonProps) {
  const Comp = asChild ? Slot : 'button'
  return (
    <Comp className={cn(buttonVariants({ variant, size, className }))} {...props} />
  )
}
