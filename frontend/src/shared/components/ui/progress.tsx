import * as ProgressPrimitive from '@radix-ui/react-progress'
import { cn } from '@/shared/utils/cn'

type ProgressProps = React.ComponentProps<typeof ProgressPrimitive.Root>

export function Progress({ className, value, ...props }: Readonly<ProgressProps>) {
  return (
    <ProgressPrimitive.Root
      className={cn(
        'relative h-4 w-full overflow-hidden rounded-full bg-stone-100',
        className,
      )}
      {...props}
    >
      <ProgressPrimitive.Indicator
        className="h-full w-full flex-1 bg-stone-900 transition-all"
        style={{ transform: `translateX(-${100 - (Number(value) || 0)}%)` }}
      />
    </ProgressPrimitive.Root>
  )
}
