import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { PilotBusinessPage } from '@/modules/admin/pages/PilotBusinessPage'

const businessId = '11111111-1111-1111-1111-111111111111'

describe('PilotBusinessPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders the form with all fields', async () => {
    renderPilotBusinessPage()

    expect(await screen.findByText('Nuevo negocio piloto')).toBeTruthy()
    expect(screen.getByLabelText('Nombre del negocio *')).toBeTruthy()
    expect(screen.getByLabelText('Tipo de ID *')).toBeTruthy()
    expect(screen.getByLabelText('Número de ID *')).toBeTruthy()
    expect(screen.getByLabelText('Teléfono *')).toBeTruthy()
    expect(screen.getByLabelText('Email *')).toBeTruthy()
    expect(screen.getByLabelText('Contraseña temporal *')).toBeTruthy()
  })

  it('shows validation errors when submitting empty form', async () => {
    const user = userEvent.setup()
    renderPilotBusinessPage()

    await user.click(await screen.findByRole('button', { name: 'Crear negocio piloto' }))

    expect(await screen.findByText('El nombre es obligatorio')).toBeTruthy()
    expect(screen.getByText('El email es obligatorio')).toBeTruthy()
  })

  it('shows success message after creating a pilot business', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSuccessFetchMock())
    renderPilotBusinessPage()

    await user.type(
      await screen.findByLabelText('Nombre del negocio *'),
      'Colmado El Piloto SRL',
    )
    await user.type(screen.getByLabelText('Número de ID *'), '132001234')
    await user.type(screen.getByLabelText('Teléfono *'), '8091234567')
    await user.type(screen.getByLabelText('Nombre completo *'), 'Ana Belkis')
    await user.type(screen.getByLabelText('Email *'), 'piloto@colmado.com')
    await user.type(screen.getByLabelText('Contraseña temporal *'), 'Admin123!')

    await user.click(screen.getByRole('button', { name: 'Crear negocio piloto' }))

    expect(await screen.findByText('✅ Negocio creado exitosamente')).toBeTruthy()
    expect(screen.getByText('Colmado El Piloto SRL')).toBeTruthy()
    expect(screen.getByText('piloto@colmado.com')).toBeTruthy()
  })

  it('shows error message when API returns duplicate email error', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createDuplicateEmailFetchMock())
    renderPilotBusinessPage()

    await user.type(
      await screen.findByLabelText('Nombre del negocio *'),
      'Colmado El Piloto SRL',
    )
    await user.type(screen.getByLabelText('Número de ID *'), '132001234')
    await user.type(screen.getByLabelText('Teléfono *'), '8091234567')
    await user.type(screen.getByLabelText('Nombre completo *'), 'Ana Belkis')
    await user.type(screen.getByLabelText('Email *'), 'dup@test.com')
    await user.type(screen.getByLabelText('Contraseña temporal *'), 'Admin123!')

    await user.click(screen.getByRole('button', { name: 'Crear negocio piloto' }))

    expect(await screen.findByText(/Email already registered/)).toBeTruthy()
  })

  it('allows creating another business after success', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSuccessFetchMock())
    renderPilotBusinessPage()

    await fillAndSubmitForm(user)

    await screen.findByText('✅ Negocio creado exitosamente')
    await user.click(screen.getByRole('button', { name: 'Registrar otro negocio' }))

    expect(await screen.findByLabelText('Nombre del negocio *')).toBeTruthy()
  })
})

// ── Helpers ──────────────────────────────────────────────────────────────────

async function fillAndSubmitForm(user: ReturnType<typeof userEvent.setup>) {
  await user.type(
    await screen.findByLabelText('Nombre del negocio *'),
    'Colmado El Piloto SRL',
  )
  await user.type(screen.getByLabelText('Número de ID *'), '132001234')
  await user.type(screen.getByLabelText('Teléfono *'), '8091234567')
  await user.type(screen.getByLabelText('Nombre completo *'), 'Ana Belkis')
  await user.type(screen.getByLabelText('Email *'), 'piloto@colmado.com')
  await user.type(screen.getByLabelText('Contraseña temporal *'), 'Admin123!')
  await user.click(screen.getByRole('button', { name: 'Crear negocio piloto' }))
}

function createSuccessFetchMock() {
  return vi.fn(async (_url: string) =>
    new Response(
      JSON.stringify({
        isSuccess: true,
        data: {
          businessId: 'aabbccdd-0000-0000-0000-000000000001',
          branchId: 'aabbccdd-0000-0000-0000-000000000002',
          adminUserId: 'aabbccdd-0000-0000-0000-000000000003',
          businessName: 'Colmado El Piloto SRL',
          branchName: 'Sucursal Principal',
          adminEmail: 'piloto@colmado.com',
          trialEndsAt: '2026-06-07T12:00:00Z',
        },
        error: null,
      }),
      { status: 201, headers: { 'Content-Type': 'application/json' } },
    ),
  )
}

function createDuplicateEmailFetchMock() {
  return vi.fn(async (_url: string) =>
    new Response(
      JSON.stringify({
        isSuccess: false,
        data: null,
        error: { code: 'PILOT_BUSINESS_DUPLICATE_EMAIL', message: 'Email already registered.' },
      }),
      { status: 400, headers: { 'Content-Type': 'application/json' } },
    ),
  )
}

function renderPilotBusinessPage() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-24T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: '22222222-2222-2222-2222-222222222222',
      businessId,
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '44444444-4444-4444-4444-444444444444',
      roles: ['Admin'],
    },
  })

  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createMemoryRouter([{ path: '/', element: <PilotBusinessPage /> }])

  render(
    <QueryClientProvider client={qc}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}
