import { Building2, ChevronRight } from 'lucide-react'
import type { Supplier } from '@/modules/suppliers/types'
import { cn } from '@/shared/utils/cn'

type SupplierListItemProps = {
  supplier: Supplier
  selected: boolean
  onClick: () => void
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('') || '?'
}

export function SupplierListItem({ supplier, selected, onClick }: Readonly<SupplierListItemProps>) {
  return (
    <li>
      <button
        aria-current={selected ? 'true' : undefined}
        className={cn(
          'group flex w-full items-start gap-3 px-3 py-3 text-left transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-stone-900/25',
          selected
            ? 'bg-stone-900 text-white shadow-sm hover:bg-stone-900 active:bg-stone-950'
            : 'text-stone-900 hover:bg-stone-50 active:bg-stone-100',
        )}
        onClick={onClick}
        type="button"
      >
        <div
          className={cn(
            'flex h-10 w-10 shrink-0 items-center justify-center rounded-md text-sm font-semibold ring-1',
            selected
              ? 'bg-white text-stone-900 ring-white/30'
              : 'bg-white text-stone-700 ring-stone-200 group-hover:ring-stone-300',
          )}
        >
          {supplier.name ? (
            initials(supplier.name)
          ) : (
            <Building2 size={16} />
          )}
        </div>

        <div className="min-w-0 flex-1">
          <div className="flex items-center justify-between gap-2">
            <p className={cn('truncate text-sm font-semibold', selected ? 'text-white' : 'text-stone-900')}>
              {supplier.name}
            </p>
            <ChevronRight
              aria-hidden="true"
              className={cn(
                'shrink-0 transition-transform',
                selected ? 'text-white/80' : 'text-stone-300 group-hover:translate-x-0.5 group-hover:text-stone-500',
              )}
              size={14}
            />
          </div>
          <p className={cn('truncate text-xs font-medium', selected ? 'text-stone-200' : 'text-stone-500')}>
            {supplier.phone ?? supplier.email ?? 'Sin contacto'}
          </p>
          <div className="mt-1.5 flex items-center gap-1.5">
            {supplier.rnc && (
              <span className={cn(
                'rounded-full px-1.5 py-0.5 font-mono text-[10px] font-semibold ring-1',
                selected ? 'bg-white/10 text-white ring-white/20' : 'bg-stone-100 text-stone-700 ring-stone-200',
              )}>
                {supplier.rnc}
              </span>
            )}
            {!supplier.isActive && (
              <span className={cn(
                'rounded-full px-1.5 py-0.5 text-[10px] font-semibold ring-1',
                selected ? 'bg-white/10 text-white ring-white/20' : 'bg-stone-100 text-stone-600 ring-stone-200',
              )}>
                Inactivo
              </span>
            )}
          </div>
        </div>
      </button>
    </li>
  )
}
