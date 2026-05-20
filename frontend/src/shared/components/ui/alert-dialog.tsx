import type { ReactNode } from 'react'

// ── AlertDialog root ──────────────────────────────────────────────────────────

export function AlertDialog({
  children,
  onOpenChange: _onOpenChange,
  open,
}: Readonly<{
  children: ReactNode
  onOpenChange?: (open: boolean) => void
  open?: boolean
}>) {
  if (!open) return null

  return (
    <div aria-modal="true" className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/50" />
      {children}
    </div>
  )
}

// ── AlertDialogContent ────────────────────────────────────────────────────────

export function AlertDialogContent({
  children,
  className = '',
}: Readonly<{ children: ReactNode; className?: string }>) {
  return (
    <div
      className={`relative z-50 w-full max-w-md rounded-xl bg-white p-6 shadow-xl ${className}`}
    >
      {children}
    </div>
  )
}

// ── AlertDialogHeader ─────────────────────────────────────────────────────────

export function AlertDialogHeader({ children }: Readonly<{ children: ReactNode }>) {
  return <div className="mb-4 space-y-1.5">{children}</div>
}

// ── AlertDialogFooter ─────────────────────────────────────────────────────────

export function AlertDialogFooter({ children }: Readonly<{ children: ReactNode }>) {
  return <div className="mt-6 flex justify-end gap-2">{children}</div>
}

// ── AlertDialogTitle ──────────────────────────────────────────────────────────

export function AlertDialogTitle({ children }: Readonly<{ children: ReactNode }>) {
  return <h2 className="text-lg font-semibold text-stone-950">{children}</h2>
}

// ── AlertDialogDescription ────────────────────────────────────────────────────

export function AlertDialogDescription({ children }: Readonly<{ children: ReactNode }>) {
  return <p className="text-sm font-medium text-stone-600">{children}</p>
}

// ── AlertDialogAction ─────────────────────────────────────────────────────────

export function AlertDialogAction({
  children,
  className = '',
  disabled,
  onClick,
}: Readonly<{
  children: ReactNode
  className?: string
  disabled?: boolean
  onClick?: (() => void) | (() => Promise<void>)
}>) {
  return (
    <button
      className={`inline-flex items-center justify-center rounded-md bg-stone-900 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-stone-800 disabled:pointer-events-none disabled:opacity-50 ${className}`}
      disabled={disabled}
      onClick={onClick}
      type="button"
    >
      {children}
    </button>
  )
}

// ── AlertDialogCancel ─────────────────────────────────────────────────────────

export function AlertDialogCancel({
  children,
  onClick,
}: Readonly<{
  children: ReactNode
  onClick?: () => void
}>) {
  return (
    <button
      className="inline-flex items-center justify-center rounded-md border border-stone-300 bg-white px-4 py-2 text-sm font-semibold text-stone-700 shadow-sm hover:bg-stone-50"
      onClick={onClick}
      type="button"
    >
      {children}
    </button>
  )
}
