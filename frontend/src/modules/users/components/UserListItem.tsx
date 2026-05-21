import { ChevronRight } from 'lucide-react'
import { cn } from '@/shared/utils/cn'
import type { UserSummary } from '../types'

type UserListItemProps = {
  user: UserSummary
  selected: boolean
  onClick: () => void
}

const roleLabels: Record<string, string> = {
  Admin: 'Administrador',
  Supervisor: 'Supervisor',
  Cashier: 'Cajero',
  InventoryManager: 'Gte. Inventario',
  PurchasingManager: 'Gte. Compras',
  ReadOnly: 'Solo lectura',
}

const roleTones: Record<string, string> = {
  Admin: 'bg-stone-100 text-stone-700 ring-stone-200',
  Supervisor: 'bg-sky-50 text-sky-700 ring-sky-200',
  Cashier: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  InventoryManager: 'bg-amber-50 text-amber-700 ring-amber-200',
  PurchasingManager: 'bg-rose-50 text-rose-700 ring-rose-200',
  ReadOnly: 'bg-stone-100 text-stone-700 ring-stone-200',
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('') || '?'
}

export function UserListItem({ user, selected, onClick }: Readonly<UserListItemProps>) {
  const roleLabel = roleLabels[user.role] ?? user.role
  const roleTone = roleTones[user.role] ?? 'bg-stone-100 text-stone-700 ring-stone-200'

  return (
    <li>
      <button
        aria-current={selected ? 'true' : undefined}
        className={cn(
          'group flex w-full items-center gap-3 px-3 py-3 text-left transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-stone-900/25',
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
            {initials(user.fullName)}
          </div>
          <span
            aria-hidden="true"
            className={cn(
              'absolute -bottom-0.5 -right-0.5 h-3 w-3 rounded-full ring-2',
              selected ? 'ring-stone-900' : 'ring-white',
              user.isActive ? 'bg-emerald-500' : 'bg-stone-300',
            )}
          />
        </div>

        <div className="min-w-0 flex-1">
          <div className="flex items-center justify-between gap-2">
            <p className={cn('truncate text-sm font-semibold', selected ? 'text-white' : 'text-stone-900')}>
              {user.fullName}
            </p>
            <ChevronRight
              aria-hidden="true"
              className={cn(
                'shrink-0 transition-transform',
                selected ? 'translate-x-0 text-white/80' : 'text-stone-300 group-hover:translate-x-0.5 group-hover:text-stone-500',
              )}
              size={14}
            />
          </div>
          <p className={cn('truncate text-xs font-medium', selected ? 'text-stone-200' : 'text-stone-500')}>
            {user.email}
          </p>
          <div className="mt-1.5 flex items-center gap-1.5">
            <span className={cn(
              'rounded-full px-1.5 py-0.5 text-[10px] font-semibold ring-1',
              selected ? 'bg-white/10 text-white ring-white/20' : roleTone,
            )}>
              {roleLabel}
            </span>
            {!user.isActive && (
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
