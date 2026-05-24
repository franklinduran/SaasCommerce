import { Bell } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { useUnreadNotificationCount } from '@/modules/notifications/hooks/useNotifications'
import { cn } from '@/shared/utils/cn'
import { NotificationDropdown } from './NotificationDropdown'

export function NotificationBell() {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const { data } = useUnreadNotificationCount()
  const unreadCount = data?.unreadCount ?? 0

  // Close when clicking outside
  useEffect(() => {
    if (!open) return

    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [open])

  return (
    <div className="relative" ref={containerRef}>
      <button
        aria-label={unreadCount > 0 ? `Notificaciones (${unreadCount} sin leer)` : 'Notificaciones'}
        className={cn(
          'relative flex h-9 w-9 items-center justify-center rounded-md text-stone-500 transition-colors hover:bg-stone-100 hover:text-stone-900',
          open && 'bg-stone-100 text-stone-900',
        )}
        onClick={() => setOpen((v) => !v)}
      >
        <Bell size={18} strokeWidth={2} />
        {unreadCount > 0 && (
          <span
            aria-hidden="true"
            className="absolute right-1 top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[9px] font-bold text-white"
          >
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-2">
          <NotificationDropdown onClose={() => setOpen(false)} />
        </div>
      )}
    </div>
  )
}
