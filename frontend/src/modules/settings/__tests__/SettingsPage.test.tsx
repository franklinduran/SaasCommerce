import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { SettingsPage } from '@/modules/settings/SettingsPage'

describe('SettingsPage', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders profile, business, branch and security sections', async () => {
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    expect(await screen.findByRole('heading', { name: 'Mi perfil' })).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Negocio' })).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Sucursal' })).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Seguridad' })).toBeTruthy()
  })

  it('shows validation errors for business identification and primary phone', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    const businessCard = await screen.findByRole('heading', { name: 'Negocio' })
    const card = businessCard.closest('div.rounded-xl') as HTMLElement
    const identificationInput = within(card).getByLabelText('Identificacion *')
    const primaryPhoneInput = within(card).getByLabelText('Telefono principal *')

    await user.clear(identificationInput)
    await user.type(identificationInput, '123')
    await user.clear(primaryPhoneInput)
    await user.click(within(card).getByRole('button', { name: 'Guardar cambios' }))

    expect(await within(card).findByText('El RNC debe tener 9 digitos.')).toBeTruthy()
    expect(await within(card).findByText('El telefono principal es requerido.')).toBeTruthy()
  })
})

function renderSettingsPage() {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-15T23:59:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: '22222222-2222-2222-2222-222222222222',
      businessId: '11111111-1111-1111-1111-111111111111',
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '44444444-4444-4444-4444-444444444444',
      roles: ['Admin'],
    },
  })

  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <SettingsPage />
    </QueryClientProvider>,
  )
}

function createSettingsFetchMock() {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/me')) {
      return createJsonResponse({
        branch: {
          branchId: '22222222-2222-2222-2222-222222222222',
          name: 'Sucursal principal',
        },
        business: {
          businessId: '11111111-1111-1111-1111-111111111111',
          identificationNumber: '123456789',
          identificationType: 'Rnc',
          name: 'Demo Business',
        },
        email: 'admin@test.com',
        fullName: 'Admin',
        phone: null,
        roles: ['Admin'],
        userId: '44444444-4444-4444-4444-444444444444',
      })
    }

    if (url.includes('/api/business/current')) {
      return createJsonResponse({
        businessId: '11111111-1111-1111-1111-111111111111',
        identificationNumber: '123456789',
        identificationType: 'Rnc',
        name: 'Demo Business',
        phones: [{ isPrimary: true, label: 'Principal', number: '8090000000' }],
      })
    }

    if (url.includes('/api/branches/current')) {
      return createJsonResponse({
        address: null,
        branchId: '22222222-2222-2222-2222-222222222222',
        businessId: '11111111-1111-1111-1111-111111111111',
        name: 'Sucursal principal',
        phone: null,
      })
    }

    return createJsonResponse(null, false, 404)
  })
}

function createJsonResponse(data: unknown, ok = true, status = 200) {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => ({
      correlationId: 'test',
      data,
      error: null,
      isSuccess: ok,
    }),
    ok,
    status,
  }
}
