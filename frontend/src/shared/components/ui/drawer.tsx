import type { ReactNode } from 'react'
import { X } from 'lucide-react'
import { cn } from '@/shared/utils/cn'
import { Button } from './button'

const sizeClasses = {
  sm: 'max-w-sm',
  md: 'max-w-xl',
  lg: 'max-w-3xl',
  xl: 'max-w-5xl',
} as const

type DrawerProps = {
  children: ReactNode
  onClose: () => void
  size?: keyof typeof sizeClasses
  subtitle?: string
  title: string
}

export function Drawer({ children, onClose, size = 'lg', subtitle, title }: Readonly<DrawerProps>) {
  return (
    <div className="fixed inset-0 z-50">
      <button
        aria-label="Cerrar panel"
        className="absolute inset-0 h-full w-full cursor-default bg-stone-950/20"
        onClick={onClose}
        type="button"
      />
      <dialog
        aria-modal="true"
        className={cn(
          'absolute inset-y-0 right-0 m-0 flex h-full w-full min-w-0 flex-col border-0 bg-white p-0 shadow-xl ring-1 ring-stone-200',
          sizeClasses[size],
        )}
        open
      >
        <div className="flex h-16 shrink-0 items-center justify-between gap-3 border-b border-stone-200 px-4 sm:px-6">
          <div>
            {subtitle && (
              <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">
                {subtitle}
              </p>
            )}
            <h3 className="text-lg font-semibold text-stone-950">{title}</h3>
          </div>
          <Button aria-label="Cerrar" onClick={onClose} size="icon" type="button" variant="ghost">
            <X size={18} />
          </Button>
        </div>
        <div className="min-h-0 flex-1 overflow-y-auto p-4 sm:p-6">{children}</div>
      </dialog>
    </div>
  )
}
