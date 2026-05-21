import * as LabelPrimitive from '@radix-ui/react-label'
import { cn } from '@/shared/utils/cn'

export function Label({
  className,
  ...props
}: Readonly<React.ComponentProps<typeof LabelPrimitive.Root>>) {
  return (
    <LabelPrimitive.Root
      className={cn('text-sm font-semibold text-stone-800', className)}
      {...props}
    />
  )
}
