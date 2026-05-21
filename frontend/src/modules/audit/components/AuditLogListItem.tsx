import { ChevronRight, User } from 'lucide-react'
import type { AuditLogItem } from '../auditLogTypes'
import { AuditLogActionBadge } from './AuditLogActionBadge'
import { cn } from '@/shared/utils/cn'

type AuditLogListItemProps = {
  log: AuditLogItem
  selected: boolean
  onClick: () => void
}

function formatDate(iso: string) {
  try {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      month: 'short',
    }).format(new Date(iso))
  } catch {
    return iso
  }
}

function initials(name: string | null) {
  if (!name) return null
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('') || null
}

export function AuditLogListItem({ log, selected, onClick }: Readonly<AuditLogListItemProps>) {
  const userInitials = initials(log.userFullName)

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
            'flex h-10 w-10 shrink-0 items-center justify-center rounded-full text-xs font-semibold ring-1',
            selected
              ? 'bg-white text-stone-900 ring-white/30'
              : 'bg-white text-stone-700 ring-stone-200 group-hover:ring-stone-300',
          )}
        >
          {userInitials ?? <User size={15} />}
        </div>

        <div className="min-w-0 flex-1">
          <div className="flex items-center justify-between gap-2">
            <p className={cn('truncate text-sm font-semibold', selected ? 'text-white' : 'text-stone-900')}>
              {log.userFullName ?? 'Sistema'}
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
          <p className={cn('mt-1 line-clamp-1 text-xs font-medium', selected ? 'text-stone-200' : 'text-stone-500')}>
            {log.description ?? log.entityName}
          </p>
          <div className="mt-1.5 flex items-center justify-between gap-2">
            <AuditLogActionBadge action={log.action} />
            <span className={cn('text-[11px] font-medium tabular-nums', selected ? 'text-stone-200' : 'text-stone-500')}>
              {formatDate(log.createdAt)}
            </span>
          </div>
        </div>
      </button>
    </li>
  )
}
