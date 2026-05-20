import type { LabelHTMLAttributes, ReactNode } from 'react'

export function Label({
  children,
  className = '',
  ...props
}: Readonly<LabelHTMLAttributes<HTMLLabelElement>> & Readonly<{ children?: ReactNode }>) {
  return (
    <label
      className={`text-sm font-semibold text-stone-800 ${className}`}
      {...props}
    >
      {children}
    </label>
  )
}
