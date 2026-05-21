import { ChevronRight, Ban } from 'lucide-react'
import type { Customer } from '@/modules/customers/types'
import { cn } from '@/shared/utils/cn'

type CustomerListItemProps = {
  customer: Customer
  selected: boolean
  onClick: () => void
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('') || '?'
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}

export function CustomerListItem({ customer, selected, onClick }: Readonly<CustomerListItemProps>) {
  const hasBalance = customer.currentBalance > 0
  const isBlocked = customer.creditStatus === 'Blocked'

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
        <div className="relative shrink-0">
          <div
            className={cn(
              'flex h-10 w-10 items-center justify-center rounded-full text-sm font-semibold ring-1',
              selected
                ? 'bg-white text-stone-900 ring-white/30'
                : 'bg-white text-stone-700 ring-stone-200 group-hover:ring-stone-300',
            )}
          >
            {initials(customer.fullName)}
          </div>
          {isBlocked && (
            <span
              aria-hidden="true"
              className={cn(
                'absolute -bottom-0.5 -right-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-red-600 ring-2',
                selected ? 'ring-stone-900' : 'ring-white',
              )}
            >
              <Ban className="text-white" size={9} strokeWidth={3} />
            </span>
          )}
        </div>

        <div className="min-w-0 flex-1">
          <div className="flex items-center justify-between gap-2">
            <p className={cn('truncate text-sm font-semibold', selected ? 'text-white' : 'text-stone-900')}>
              {customer.fullName}
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
            {customer.phone ?? customer.email ?? 'Sin contacto'}
          </p>
          <div className="mt-1.5 flex items-center gap-1.5">
            {hasBalance ? (
              <span className="rounded-full bg-amber-50 px-1.5 py-0.5 text-[10px] font-semibold text-amber-800 ring-1 ring-amber-200 tabular-nums">
                {formatMoney(customer.currentBalance)}
              </span>
            ) : (
              <span className={cn(
                'rounded-full px-1.5 py-0.5 text-[10px] font-semibold ring-1',
                selected ? 'bg-white/10 text-white ring-white/20' : 'bg-stone-100 text-stone-600 ring-stone-200',
              )}>
                Sin deuda
              </span>
            )}
            {isBlocked && (
              <span className="rounded-full bg-red-50 px-1.5 py-0.5 text-[10px] font-semibold text-red-700 ring-1 ring-red-200">
                Bloqueado
              </span>
            )}
            {!customer.isActive && (
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
