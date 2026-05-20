import type { ReactNode, SelectHTMLAttributes } from 'react'
import { createContext, forwardRef, useContext, useMemo } from 'react'

// ── Context ──────────────────────────────────────────────────────────────────

type SelectCtx = {
  value: string
  onValueChange?: (value: string) => void
}

const SelectContext = createContext<SelectCtx>({ value: '' })

// ── Select root ───────────────────────────────────────────────────────────────

export function Select({
  children,
  disabled: _disabled,
  onValueChange,
  value = '',
}: Readonly<{
  children: ReactNode
  disabled?: boolean
  onValueChange?: (value: string) => void
  value?: string
}>) {
  const ctxValue = useMemo<SelectCtx>(
    () => ({ onValueChange, value }),
    [onValueChange, value],
  )

  return <SelectContext.Provider value={ctxValue}>{children}</SelectContext.Provider>
}

// ── SelectTrigger ─────────────────────────────────────────────────────────────

type NativeSelectProps = SelectHTMLAttributes<HTMLSelectElement>

export const SelectTrigger = forwardRef<
  HTMLSelectElement,
  NativeSelectProps & Readonly<{ children?: ReactNode }>
>(function SelectTrigger({ children: _children, className = '', ...props }, ref) {
  const ctx = useContext(SelectContext)

  return (
    <select
      ref={ref}
      className={`h-10 w-full rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-300 outline-none focus:ring-2 focus:ring-stone-900/20 ${className}`}
      onChange={(e) => ctx.onValueChange?.(e.target.value)}
      value={ctx.value}
      {...props}
    />
  )
})

// ── SelectValue (placeholder only) ───────────────────────────────────────────

export function SelectValue(_props: Readonly<{ placeholder?: string }>) {
  return null
}

// ── SelectContent / SelectItem ────────────────────────────────────────────────

export function SelectContent({ children }: Readonly<{ children: ReactNode }>) {
  return <>{children}</>
}

export function SelectItem({
  children,
  value,
}: Readonly<{ children: ReactNode; value: string }>) {
  return <option value={value}>{children}</option>
}

// ── SelectLabel ───────────────────────────────────────────────────────────────

export function SelectLabel({ children }: Readonly<{ children: ReactNode }>) {
  return <optgroup label={typeof children === 'string' ? children : undefined} />
}

// ── SelectGroup ───────────────────────────────────────────────────────────────

export function SelectGroup({ children }: Readonly<{ children: ReactNode }>) {
  return <>{children}</>
}

// ── SelectSeparator ───────────────────────────────────────────────────────────

export function SelectSeparator() {
  return null
}
