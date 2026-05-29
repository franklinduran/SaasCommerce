import { ChevronRight, CreditCard, LogOut, Settings, Shield } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { logout as serverLogout } from '@/modules/auth/services/authService'
import { useAuthStore } from '@/modules/auth/authStore'
import { formatRoleLabel } from '@/app/navigation'
import { Permission } from '@/shared/types/permissions'
import { cn } from '@/shared/utils/cn'

type SidebarUserMenuProps = {
  collapsed: boolean
  userPermissions: string[]
}

/**
 * Sidebar footer: avatar button that triggers a dropdown with user options.
 * Shows name + role visible on the button in expanded mode.
 */
export function SidebarUserMenu({ collapsed, userPermissions }: Readonly<SidebarUserMenuProps>) {
  const session = useAuthStore((state) => state.session)
  const clearSession = useAuthStore((state) => state.clearSession)
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)

  const userName = session?.user.fullName ?? 'Admin'
  const userInitial = userName.trim().slice(0, 1).toUpperCase() || 'A'
  const userRoleLabel = formatRoleLabel(session?.user.roles?.[0])
  const canManageUsers = userPermissions.includes(Permission.UsersView)

  // Outside click / Escape closes the dropdown
  useEffect(() => {
    if (!open) return
    function handlePointerDown(event: MouseEvent) {
      if (!(event.target as Element | null)?.closest('[data-sidebar-user]')) {
        setOpen(false)
      }
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') setOpen(false)
    }
    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [open])

  async function handleLogout() {
    setOpen(false)
    if (session) {
      try {
        await serverLogout(session.accessToken, session.refreshToken)
      } catch {
        // Ignore server errors — still clear local session
      }
    }
    clearSession()
    navigate('/login', { replace: true })
  }

  function goTo(path: string) {
    setOpen(false)
    navigate(path)
  }

  return (
    <div className="hidden shrink-0 border-t border-sidebar-border/60 px-3 py-3 lg:block">
      <div className="relative" data-sidebar-user>
        {/* Trigger button */}
        <button
          aria-expanded={open}
          aria-haspopup="menu"
          aria-label="Abrir menu de usuario"
          className={cn(
            'flex w-full cursor-pointer items-center gap-2.5 rounded-md px-2.5 py-2 text-sidebar-fg transition-colors',
            'hover:bg-sidebar-hover-bg hover:text-gray-900',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-400/30',
            collapsed ? 'h-9 justify-center px-0' : 'h-auto min-h-[44px]',
            open && 'bg-sidebar-hover-bg text-gray-900',
          )}
          onClick={() => setOpen((o) => !o)}
          type="button"
        >
          {/* Avatar circle */}
          <span className={cn(
            'flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-[12px] font-semibold',
            open ? 'bg-gray-900 text-white' : 'bg-gray-200 text-gray-700',
          )}>
            {userInitial}
          </span>

          {!collapsed && (
            <>
              {/* Name + role stacked */}
              <div className="flex min-w-0 flex-1 flex-col text-left">
                <span className="truncate text-[13px] font-semibold leading-tight text-sidebar-fg">
                  {userName}
                </span>
                <span className="truncate text-[11px] font-normal leading-tight text-sidebar-muted">
                  {userRoleLabel}
                </span>
              </div>
              <ChevronRight
                aria-hidden="true"
                className={cn(
                  'shrink-0 text-gray-400 transition-transform duration-150',
                  open && '-rotate-90',
                )}
                size={13}
                strokeWidth={2}
              />
            </>
          )}
        </button>

        {/* Dropdown — opens upward */}
        {open && (
          <div
            aria-label="Menu de usuario"
            className="absolute bottom-full left-0 right-0 z-40 mb-1 overflow-hidden rounded-xl border border-gray-200 bg-white py-1 shadow-lg ring-1 ring-black/5 animate-in fade-in slide-in-from-bottom-1 duration-150"
            role="menu"
          >
            <div className="border-b border-gray-100 px-3 py-2.5">
              <p className="truncate text-[13px] font-semibold text-gray-900">{userName}</p>
              <p className="truncate text-[11px] text-gray-500">{userRoleLabel}</p>
            </div>
            <UserMenuItem icon={Settings} label="Ajustes de cuenta" onSelect={() => goTo('/settings')} />
            <UserMenuItem icon={CreditCard} label="Suscripción" onSelect={() => goTo('/subscription')} />
            {canManageUsers && (
              <UserMenuItem icon={Shield} label="Usuarios y roles" onSelect={() => goTo('/users')} />
            )}
            <div className="my-1 h-px bg-gray-100" />
            <UserMenuItem icon={LogOut} label="Cerrar sesión" onSelect={handleLogout} />
          </div>
        )}
      </div>
    </div>
  )
}

// ─── UserMenuItem ─────────────────────────────────────────────────────────────

type UserMenuItemProps = {
  icon: LucideIcon
  label: string
  onSelect: () => void | Promise<void>
}

function UserMenuItem({ icon: Icon, label, onSelect }: Readonly<UserMenuItemProps>) {
  return (
    <button
      className="flex w-full cursor-pointer items-center gap-2.5 px-3 py-2 text-left text-sm font-medium text-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring/20"
      onClick={onSelect}
      role="menuitem"
      type="button"
    >
      <Icon aria-hidden="true" className="shrink-0 text-muted-foreground" size={15} />
      <span className="truncate">{label}</span>
    </button>
  )
}
