import * as SelectPrimitive from '@radix-ui/react-select'
import { CheckIcon, ChevronDownIcon, ChevronUpIcon } from 'lucide-react'
import { cn } from '@/shared/utils/cn'

export function Select(props: Readonly<React.ComponentProps<typeof SelectPrimitive.Root>>) {
  return <SelectPrimitive.Root {...props} />
}

export function SelectGroup(props: Readonly<React.ComponentProps<typeof SelectPrimitive.Group>>) {
  return <SelectPrimitive.Group {...props} />
}

export function SelectValue(props: Readonly<React.ComponentProps<typeof SelectPrimitive.Value>>) {
  return <SelectPrimitive.Value {...props} />
}

export function SelectTrigger({
  className,
  children,
  ...props
}: Readonly<React.ComponentProps<typeof SelectPrimitive.Trigger>>) {
  return (
    <SelectPrimitive.Trigger
      className={cn(
        'flex h-10 w-full min-w-0 items-center justify-between gap-2 rounded-md bg-card px-3 text-sm font-medium text-foreground shadow-control outline-none transition',
        'placeholder:text-muted-foreground',
        'hover:bg-muted active:bg-secondary',
        'focus:shadow-control-focus focus:ring-2 focus:ring-ring/15',
        'disabled:cursor-not-allowed disabled:bg-muted disabled:text-muted-foreground',
        '[&>span]:truncate [&>span]:text-left',
        className,
      )}
      {...props}
    >
      {children}
      <SelectPrimitive.Icon asChild>
        <ChevronDownIcon className="size-4 shrink-0 text-muted-foreground" />
      </SelectPrimitive.Icon>
    </SelectPrimitive.Trigger>
  )
}

export function SelectScrollUpButton({
  className,
  ...props
}: Readonly<React.ComponentProps<typeof SelectPrimitive.ScrollUpButton>>) {
  return (
    <SelectPrimitive.ScrollUpButton
      className={cn('flex cursor-default items-center justify-center py-1', className)}
      {...props}
    >
      <ChevronUpIcon className="size-4 text-muted-foreground" />
    </SelectPrimitive.ScrollUpButton>
  )
}

export function SelectScrollDownButton({
  className,
  ...props
}: Readonly<React.ComponentProps<typeof SelectPrimitive.ScrollDownButton>>) {
  return (
    <SelectPrimitive.ScrollDownButton
      className={cn('flex cursor-default items-center justify-center py-1', className)}
      {...props}
    >
      <ChevronDownIcon className="size-4 text-muted-foreground" />
    </SelectPrimitive.ScrollDownButton>
  )
}

export function SelectContent({
  className,
  children,
  position = 'popper',
  ...props
}: Readonly<React.ComponentProps<typeof SelectPrimitive.Content>>) {
  return (
    <SelectPrimitive.Portal>
      <SelectPrimitive.Content
        className={cn(
          'relative z-50 max-h-96 min-w-[8rem] overflow-hidden rounded-xl border border-border bg-card text-foreground shadow-lg',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
          'data-[side=bottom]:slide-in-from-top-2 data-[side=left]:slide-in-from-right-2',
          'data-[side=right]:slide-in-from-left-2 data-[side=top]:slide-in-from-bottom-2',
          position === 'popper' &&
            'data-[side=bottom]:translate-y-1 data-[side=top]:-translate-y-1',
          className,
        )}
        position={position}
        {...props}
      >
        <SelectScrollUpButton />
        <SelectPrimitive.Viewport
          className={cn(
            'p-1',
            position === 'popper' &&
              'h-[var(--radix-select-trigger-height)] w-full min-w-[var(--radix-select-trigger-width)]',
          )}
        >
          {children}
        </SelectPrimitive.Viewport>
        <SelectScrollDownButton />
      </SelectPrimitive.Content>
    </SelectPrimitive.Portal>
  )
}

export function SelectLabel({
  className,
  ...props
}: Readonly<React.ComponentProps<typeof SelectPrimitive.Label>>) {
  return (
    <SelectPrimitive.Label
      className={cn('px-2 py-1.5 text-xs font-semibold text-muted-foreground', className)}
      {...props}
    />
  )
}

export function SelectItem({
  className,
  children,
  ...props
}: Readonly<React.ComponentProps<typeof SelectPrimitive.Item>>) {
  return (
    <SelectPrimitive.Item
      className={cn(
        'relative flex w-full cursor-default select-none items-center rounded-md py-2 pl-8 pr-2 text-sm font-medium text-foreground outline-none',
        'focus:bg-muted focus:text-foreground data-[highlighted]:bg-muted data-[highlighted]:text-foreground',
        'data-[state=checked]:bg-secondary data-[state=checked]:text-secondary-foreground',
        'data-[disabled]:pointer-events-none data-[disabled]:text-muted-foreground',
        className,
      )}
      {...props}
    >
      <span className="absolute left-2 flex size-4 items-center justify-center">
        <SelectPrimitive.ItemIndicator>
          <CheckIcon className="size-4" />
        </SelectPrimitive.ItemIndicator>
      </span>
      <SelectPrimitive.ItemText>{children}</SelectPrimitive.ItemText>
    </SelectPrimitive.Item>
  )
}

export function SelectSeparator({
  className,
  ...props
}: Readonly<React.ComponentProps<typeof SelectPrimitive.Separator>>) {
  return (
    <SelectPrimitive.Separator
      className={cn('-mx-1 my-1 h-px bg-muted', className)}
      {...props}
    />
  )
}
