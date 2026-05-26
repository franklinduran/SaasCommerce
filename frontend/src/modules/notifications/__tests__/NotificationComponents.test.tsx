import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  NotificationEmptyState,
  NotificationItem,
} from '@/modules/notifications/components/NotificationItem'
import { NotificationBell } from '@/modules/notifications/components/NotificationBell'
import { NotificationDropdown } from '@/modules/notifications/components/NotificationDropdown'
import type { NotificationItem as NotificationItemType } from '@/modules/notifications/types'

// ── Hook mocks ────────────────────────────────────────────────────────────────

const mockMarkRead = vi.fn()
const mockMarkAll = vi.fn()
const mockNavigate = vi.fn()

vi.mock('@/modules/notifications/hooks/useNotifications', () => ({
  useNotifications: vi.fn(),
  useUnreadNotificationCount: vi.fn(),
  useMarkNotificationRead: vi.fn(),
  useMarkAllNotificationsRead: vi.fn(),
}))

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
  useUnreadNotificationCount,
} from '@/modules/notifications/hooks/useNotifications'

// ── Helpers ───────────────────────────────────────────────────────────────────

function createNotif(overrides: Partial<NotificationItemType> = {}): NotificationItemType {
  return {
    id: 'notif-1',
    type: 'LowStock',
    severity: 'Warning',
    status: 'Unread',
    title: 'Stock bajo',
    message: 'Producto tiene 2 unidades.',
    branchId: null,
    relatedEntityId: null,
    relatedEntityType: null,
    createdAt: new Date(Date.now() - 5 * 60_000).toISOString(), // 5 min ago
    readAt: null,
    ...overrides,
  }
}

function renderWithRouter(ui: React.ReactElement) {
  const router = createMemoryRouter([{ path: '/*', element: ui }], { initialEntries: ['/'] })
  return render(<RouterProvider router={router} />)
}

function notificationsResult(
  data: unknown = { items: [], unreadCount: 0, totalCount: 0, page: 1, pageSize: 10 },
  isLoading = false,
) {
  return { data, isLoading } as unknown as ReturnType<typeof useNotifications>
}

function unreadCountResult(unreadCount: number) {
  return { data: { unreadCount } } as unknown as ReturnType<typeof useUnreadNotificationCount>
}

function markReadResult() {
  return { mutate: mockMarkRead, isPending: false } as unknown as ReturnType<
    typeof useMarkNotificationRead
  >
}

function markAllResult() {
  return { mutate: mockMarkAll, isPending: false } as unknown as ReturnType<
    typeof useMarkAllNotificationsRead
  >
}

// ── NotificationItem ──────────────────────────────────────────────────────────

describe('NotificationItem', () => {
  afterEach(() => cleanup())

  it('renders title and message', () => {
    render(<NotificationItem notification={createNotif()} />)
    expect(screen.getByText('Stock bajo')).toBeTruthy()
    expect(screen.getByText('Producto tiene 2 unidades.')).toBeTruthy()
  })

  it('shows Marcar como leída button for Unread notification with onRead', () => {
    render(<NotificationItem notification={createNotif({ status: 'Unread' })} onRead={vi.fn()} />)
    expect(screen.getByRole('button', { name: 'Marcar como leída' })).toBeTruthy()
  })

  it('does not show mark-read button when status is Read', () => {
    render(<NotificationItem notification={createNotif({ status: 'Read' })} onRead={vi.fn()} />)
    expect(screen.queryByRole('button', { name: 'Marcar como leída' })).toBeNull()
  })

  it('does not show mark-read button when onRead is not provided', () => {
    render(<NotificationItem notification={createNotif({ status: 'Unread' })} />)
    expect(screen.queryByRole('button', { name: 'Marcar como leída' })).toBeNull()
  })

  it('calls onRead with notification id when mark-read button is clicked', async () => {
    const onRead = vi.fn()
    const user = userEvent.setup()
    render(<NotificationItem notification={createNotif({ id: 'n-99' })} onRead={onRead} />)
    await user.click(screen.getByRole('button', { name: 'Marcar como leída' }))
    expect(onRead).toHaveBeenCalledWith('n-99')
  })

  it('shows relative time "Hace X min" for recent notifications', () => {
    const recentNotif = createNotif({
      createdAt: new Date(Date.now() - 3 * 60_000).toISOString(),
    })
    render(<NotificationItem notification={recentNotif} />)
    expect(screen.getByText(/Hace \d+ min/)).toBeTruthy()
  })

  it('shows "Ahora mismo" for very recent notifications (< 1 min)', () => {
    const nowNotif = createNotif({
      createdAt: new Date(Date.now() - 30_000).toISOString(), // 30 sec ago
    })
    render(<NotificationItem notification={nowNotif} />)
    expect(screen.getByText('Ahora mismo')).toBeTruthy()
  })

  it('shows relative time in hours for older notifications', () => {
    const oldNotif = createNotif({
      createdAt: new Date(Date.now() - 2 * 60 * 60_000).toISOString(), // 2h ago
    })
    render(<NotificationItem notification={oldNotif} />)
    expect(screen.getByText(/Hace \d+h/)).toBeTruthy()
  })

  it('shows relative time in days for notifications older than 24h', () => {
    const veryOldNotif = createNotif({
      createdAt: new Date(Date.now() - 2 * 24 * 60 * 60_000).toISOString(), // 2 days ago
    })
    render(<NotificationItem notification={veryOldNotif} />)
    expect(screen.getByText(/Hace \d+d/)).toBeTruthy()
  })

  it('shows formatted date for notifications older than 7 days', () => {
    const ancientNotif = createNotif({
      createdAt: new Date(Date.now() - 10 * 24 * 60 * 60_000).toISOString(), // 10 days ago
    })
    render(<NotificationItem notification={ancientNotif} />)
    const expectedDate = new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      month: 'short',
    }).format(new Date(ancientNotif.createdAt))
    const timeEl = screen.getByText(expectedDate)
    expect(timeEl).toBeTruthy()
  })
})

// ── NotificationEmptyState ────────────────────────────────────────────────────

describe('NotificationEmptyState', () => {
  afterEach(() => cleanup())

  it('renders "Todo al día" message', () => {
    render(<NotificationEmptyState />)
    expect(screen.getByText('Todo al día')).toBeTruthy()
    expect(screen.getByText('No tienes notificaciones pendientes.')).toBeTruthy()
  })
})

// ── NotificationBell ──────────────────────────────────────────────────────────

describe('NotificationBell', () => {
  beforeEach(() => {
    vi.mocked(useUnreadNotificationCount).mockReturnValue(unreadCountResult(0))
    vi.mocked(useNotifications).mockReturnValue(notificationsResult())
    vi.mocked(useMarkNotificationRead).mockReturnValue(markReadResult())
    vi.mocked(useMarkAllNotificationsRead).mockReturnValue(markAllResult())
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders bell button with default aria-label when no unread', () => {
    renderWithRouter(<NotificationBell />)
    expect(screen.getByRole('button', { name: 'Notificaciones' })).toBeTruthy()
  })

  it('shows unread count badge when unreadCount > 0', () => {
    vi.mocked(useUnreadNotificationCount).mockReturnValue(unreadCountResult(5))
    renderWithRouter(<NotificationBell />)
    expect(screen.getByText('5')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Notificaciones (5 sin leer)' })).toBeTruthy()
  })

  it('shows "99+" when unreadCount > 99', () => {
    vi.mocked(useUnreadNotificationCount).mockReturnValue(unreadCountResult(150))
    renderWithRouter(<NotificationBell />)
    expect(screen.getByText('99+')).toBeTruthy()
  })

  it('opens dropdown when bell button is clicked', async () => {
    const user = userEvent.setup()
    renderWithRouter(<NotificationBell />)
    await user.click(screen.getByRole('button', { name: 'Notificaciones' }))
    expect(screen.getByText('Notificaciones')).toBeTruthy()
    // Dropdown renders with header text "Notificaciones"
    expect(screen.getByText('Ver todas las notificaciones')).toBeTruthy()
  })
})

// ── NotificationDropdown ──────────────────────────────────────────────────────

describe('NotificationDropdown', () => {
  beforeEach(() => {
    vi.mocked(useNotifications).mockReturnValue(notificationsResult())
    vi.mocked(useMarkNotificationRead).mockReturnValue(markReadResult())
    vi.mocked(useMarkAllNotificationsRead).mockReturnValue(markAllResult())
    vi.mocked(useUnreadNotificationCount).mockReturnValue(unreadCountResult(0))
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows loading spinner when isLoading is true', () => {
    vi.mocked(useNotifications).mockReturnValue(notificationsResult(undefined, true))
    renderWithRouter(<NotificationDropdown />)
    // Loader2 icon renders when loading
    expect(screen.queryByText('Todo al día')).toBeNull()
    expect(screen.getByText('Ver todas las notificaciones')).toBeTruthy()
  })

  it('shows empty state when no notifications', () => {
    renderWithRouter(<NotificationDropdown />)
    expect(screen.getByText('Todo al día')).toBeTruthy()
  })

  it('renders notification items in the list', () => {
    vi.mocked(useNotifications).mockReturnValue(
      notificationsResult({
        items: [createNotif({ title: 'Alerta stock', message: 'Bajo inventario.' })],
        unreadCount: 1,
        totalCount: 1,
        page: 1,
        pageSize: 10,
      }),
    )
    renderWithRouter(<NotificationDropdown />)
    expect(screen.getByText('Bajo inventario.')).toBeTruthy()
  })

  it('shows "X sin leer" and "Marcar todas" button when unreadCount > 0', () => {
    vi.mocked(useNotifications).mockReturnValue(
      notificationsResult({ items: [createNotif()], unreadCount: 3, totalCount: 1, page: 1, pageSize: 10 }),
    )
    renderWithRouter(<NotificationDropdown />)
    expect(screen.getByText('3 sin leer')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Marcar todas' })).toBeTruthy()
  })

  it('does not show "Marcar todas" when unreadCount is 0', () => {
    renderWithRouter(<NotificationDropdown />)
    expect(screen.queryByRole('button', { name: 'Marcar todas' })).toBeNull()
  })

  it('calls markAll.mutate when "Marcar todas" is clicked', async () => {
    const user = userEvent.setup()
    vi.mocked(useNotifications).mockReturnValue(
      notificationsResult({ items: [createNotif()], unreadCount: 1, totalCount: 1, page: 1, pageSize: 10 }),
    )
    renderWithRouter(<NotificationDropdown />)
    await user.click(screen.getByRole('button', { name: 'Marcar todas' }))
    expect(mockMarkAll).toHaveBeenCalledTimes(1)
  })

  it('navigates to /notifications and calls onClose when "Ver todas" is clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    renderWithRouter(<NotificationDropdown onClose={onClose} />)
    await user.click(screen.getByRole('button', { name: 'Ver todas las notificaciones' }))
    expect(mockNavigate).toHaveBeenCalledWith('/notifications')
    expect(onClose).toHaveBeenCalledTimes(1)
  })
})
