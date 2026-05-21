import * as TabsPrimitive from '@radix-ui/react-tabs'
import { cn } from '@/shared/utils/cn'

export const Tabs = TabsPrimitive.Root

export function TabsList({
  className,
  ...props
}: React.ComponentProps<typeof TabsPrimitive.List>) {
  return (
    <TabsPrimitive.List
      className={cn(
        'flex max-w-full gap-1 overflow-x-auto rounded-md bg-stone-100 p-1',
        className,
      )}
      {...props}
    />
  )
}

export function TabsTrigger({
  className,
  ...props
}: React.ComponentProps<typeof TabsPrimitive.Trigger>) {
  return (
    <TabsPrimitive.Trigger
      className={cn(
        'flex h-9 flex-1 shrink-0 items-center justify-center rounded-md px-3 text-sm font-semibold transition-colors',
        'text-stone-700 hover:bg-stone-200 hover:text-stone-950 active:bg-stone-300 active:text-stone-950',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-stone-900/25 focus-visible:ring-offset-2',
        'disabled:pointer-events-none disabled:text-stone-400',
        'data-[state=active]:bg-stone-900 data-[state=active]:text-white data-[state=active]:hover:bg-stone-900 data-[state=active]:hover:text-white data-[state=active]:active:bg-stone-950 data-[state=active]:active:text-white',
        'data-[state=active]:shadow-sm data-[state=active]:ring-1 data-[state=active]:ring-stone-900',
        className,
      )}
      {...props}
    />
  )
}

export function TabsContent({
  className,
  ...props
}: React.ComponentProps<typeof TabsPrimitive.Content>) {
  return (
    <TabsPrimitive.Content
      className={cn(
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-stone-900/25 focus-visible:ring-offset-2',
        className,
      )}
      {...props}
    />
  )
}
