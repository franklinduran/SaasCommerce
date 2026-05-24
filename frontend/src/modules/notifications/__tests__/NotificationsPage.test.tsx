import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { NotificationListPage } from '@/modules/notifications/pages/NotificationListPage'
import type { NotificationItem } from '@/modules/notifications/types'

// ── Constants ─────────────────────────────────────────────────────────────────

const BUSINESS_ID = '11111111-1111-1111-1111-111111111111'
const BRANCH_ID = '22222222-2222-2222-2222-222222222222'
const NOTIFICATION_ID = 'aaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'

// ── Describe ──────────────────────────────────────────────────────────────────

describe('NotificationListPage', () => {
  beforeEach(() => {
    setSession()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows empty state when no notifications exist', async () => {
    vi.stubGlobal('fetch', createFetchMock({ list: emptyList() }))

    renderPage()

    // Wait for the empty state card (not the header subtitle which shows immediately)
    expect(await screen.findByText('No hay notificaciones en este momento.')).toBeTruthy()
  })

  it('shows notification list with title and message', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        list: listWith([
          createNotification({
            title: 'Stock bajo',
            message: 'Producto A tiene 2 unidades.',
            severity: 'Warning',
            type: 'LowStock',
          }),
        ]),
      }),
    )

    renderPage()

    // "Stock bajo" appears both as title and as typeLabel badge — use the unique message
    expect(await screen.findByText('Producto A tiene 2 unidades.')).toBeTruthy()
    // Title appears at least once
    expect(screen.getAllByText('Stock bajo').length).toBeGreaterThan(0)
  })

  it('shows Critical severity badge for critical notifications', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        list: listWith([createNotification({ severity: 'Critical', status: 'Unread' })]),
      }),
    )

    renderPage()

    expect(await screen.findByText('Crítico')).toBeTruthy()
  })

  it('shows "mark all" button when there are unread notifications', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        list: listWith([createNotification({ status: 'Unread' })], { unreadCount: 1 }),
      }),
    )

    renderPage()

    expect(await screen.findByRole('button', { name: /Marcar todas como leídas/i })).toBeTruthy()
  })

  it('does not show "mark all" button when no unread notifications', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({
        list: listWith([createNotification({ status: 'Read' })], { unreadCount: 0 }),
      }),
    )

    renderPage()

    // Wait for data to load using the unique message
    expect(await screen.findByText('El producto tiene stock bajo.')).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Marcar todas/i })).toBeNull()
  })

  it('shows error state when API fails', async () => {
    vi.stubGlobal('fetch', createFetchMock({ failList: true }))

    renderPage()

    expect(
      await screen.findByText(/Error al cargar las notificaciones/i),
    ).toBeTruthy()
  })

  it('calls mark-read API when mark-all is clicked', async () => {
    const user = userEvent.setup()
    const fetchMock = createFetchMock({
      list: listWith([createNotification({ status: 'Unread' })], { unreadCount: 1 }),
    })
    vi.stubGlobal('fetch', fetchMock)

    renderPage()

    const btn = await screen.findByRole('button', { name: /Marcar todas como leídas/i })
    await user.click(btn)

    const calls = (fetchMock as ReturnType<typeof vi.fn>).mock.calls
    const markAllCall = calls.find((args: unknown[]) =>
      String(args[0]).includes('/api/notifications/read-all') &&
      (args[1] as RequestInit | undefined)?.method === 'PUT',
    )
    expect(markAllCall).toBeTruthy()
  })
})

// ── Render helper ──────────────────────────────────────────────────────────────

function renderPage() {
  const router = createMemoryRouter(
    [{ element: <NotificationListPage />, path: '/notifications' }],
    { initialEntries: ['/notifications'] },
  )
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

function setSession() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-12-31T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: BRANCH_ID,
      businessId: BUSINESS_ID,
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '44444444-4444-4444-4444-444444444444',
      roles: ['Admin'],
    },
  })
}

// ── Factory functions ──────────────────────────────────────────────────────────

function createNotification(overrides: Partial<NotificationItem> = {}): NotificationItem {
  return {
    id: NOTIFICATION_ID,
    type: 'LowStock',
    severity: 'Warning',
    status: 'Unread',
    title: 'Stock bajo',
    message: "El producto tiene stock bajo.",
    branchId: BRANCH_ID,
    relatedEntityId: '55555555-5555-5555-5555-555555555555',
    relatedEntityType: 'Product',
    createdAt: '2026-05-23T10:00:00Z',
    readAt: null,
    ...overrides,
  }
}

function emptyList() {
  return { items: [] as NotificationItem[], totalCount: 0, page: 1, pageSize: 20, unreadCount: 0 }
}

function listWith(
  items: NotificationItem[],
  overrides: { unreadCount?: number } = {},
) {
  return {
    items,
    totalCount: items.length,
    page: 1,
    pageSize: 20,
    unreadCount: overrides.unreadCount ?? items.filter((i) => i.status === 'Unread').length,
  }
}

// ── Fetch mock ─────────────────────────────────────────────────────────────────

function createFetchMock({
  list,
  failList = false,
}: {
  list?: ReturnType<typeof emptyList>
  failList?: boolean
} = {}) {
  return vi.fn(async (input: RequestInfo | URL, opts?: RequestInit) => {
    const url = input.toString()

    // List / paginated notifications
    if (url.includes('/api/notifications/unread-count')) {
      return createJsonResponse({ unreadCount: list?.unreadCount ?? 0 })
    }

    if (url.includes('/api/notifications/read-all') && opts?.method === 'PUT') {
      return createJsonResponse(null)
    }

    if (url.match(/\/api\/notifications\/[^/]+\/read/) && opts?.method === 'PUT') {
      return createJsonResponse(null)
    }

    if (url.includes('/api/notifications')) {
      if (failList) {
        return createJsonResponse(null, false, 500, 'SERVER_ERROR', 'List failed.')
      }
      return createJsonResponse(list ?? emptyList())
    }

    return createJsonResponse(null, false, 404, 'NOT_FOUND', 'Route not found.')
  })
}

function createJsonResponse(
  data: unknown,
  ok = true,
  status = 200,
  code = 'ERROR',
  message = 'Request failed',
) {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => ({
      correlationId: 'test',
      data,
      error: ok ? null : { code, message },
      isSuccess: ok,
    }),
    ok,
    status,
  }
}
