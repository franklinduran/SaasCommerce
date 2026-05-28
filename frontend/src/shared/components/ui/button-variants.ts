import { cva } from 'class-variance-authority'

export const buttonVariants = cva(
  'inline-flex min-w-0 items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/25 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:shadow-none',
  {
    variants: {
      variant: {
        default:
          'bg-primary text-primary-foreground shadow-sm hover:bg-primary-hover active:bg-brand-hover/90 active:text-primary-foreground disabled:bg-muted disabled:text-muted-foreground',
        secondary:
          'bg-card text-foreground shadow-sm ring-1 ring-border hover:bg-secondary hover:text-foreground active:bg-secondary/80 active:text-foreground disabled:bg-muted disabled:text-muted-foreground disabled:ring-border',
        ghost:
          'bg-transparent text-foreground hover:bg-muted hover:text-foreground active:bg-secondary active:text-foreground disabled:text-muted-foreground',
        outline:
          'bg-card text-foreground shadow-sm ring-1 ring-border hover:bg-secondary hover:text-foreground active:bg-secondary/80 active:text-foreground disabled:bg-muted disabled:text-muted-foreground disabled:ring-border',
        destructive:
          'bg-red-600 text-white shadow-sm hover:bg-red-700 active:bg-red-800 focus-visible:ring-red-600/25 disabled:bg-red-100 disabled:text-red-700',
      },
      size: {
        default: 'h-10 px-4',
        sm:      'h-9 px-3',
        icon:    'h-9 w-9 px-0',
      },
    },
    defaultVariants: {
      variant: 'default',
      size:    'default',
    },
  },
)
