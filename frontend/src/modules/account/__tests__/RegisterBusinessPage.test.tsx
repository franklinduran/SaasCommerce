import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { RegisterBusinessPage } from '@/modules/account/RegisterBusinessPage'

describe('RegisterBusinessPage', () => {
  afterEach(() => {
    cleanup()
    vi.unstubAllGlobals()
  })

  it('keeps submit disabled when required fields are missing', () => {
    renderRegisterBusinessPage()

    expect(screen.getByRole('button', { name: 'Crear comercio' })).toBeDisabled()
  })

  it('shows identification and primary phone validation errors', async () => {
    const user = userEvent.setup()

    renderRegisterBusinessPage()

    await user.clear(screen.getByLabelText('Numero de identificacion *'))
    await user.type(screen.getByLabelText('Numero de identificacion *'), '123')
    await user.clear(screen.getByLabelText('Telefono principal *'))
    await user.type(screen.getByLabelText('Telefono principal *'), '1')
    await user.clear(screen.getByLabelText('Telefono principal *'))

    expect(await screen.findByText('La cedula debe tener 11 digitos')).toBeTruthy()
    expect(await screen.findByText('Telefono principal requerido')).toBeTruthy()
  })

  it('shows retry button when plans fail to load', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('Network error')))

    renderRegisterBusinessPage()

    expect(await screen.findByRole('button', { name: 'Reintentar' })).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Crear comercio' })).toBeDisabled()
  })

  it('submits valid registration data', async () => {
    const user = userEvent.setup()
    const fetchMock = vi.fn(async (url: RequestInfo | URL) => {
      if (String(url).includes('/api/subscription-plans')) {
        return createJsonResponse([
          {
            code: 'BASIC',
            createdAt: '2026-05-01T00:00:00Z',
            description: 'Plan inicial',
            features: ['Sales', 'Products'],
            id: '11111111-1111-1111-1111-111111111111',
            isActive: true,
            maxBranches: 1,
            maxProducts: 300,
            maxSalesPerMonth: 1000,
            maxUsers: 2,
            monthlyPrice: 29,
            name: 'Basic',
            updatedAt: '2026-05-01T00:00:00Z',
          },
        ])
      }

      return createJsonResponse({
        accessToken: 'jwt',
        branchId: '22222222-2222-2222-2222-222222222222',
        businessId: '11111111-1111-1111-1111-111111111111',
        expiresAt: '2026-05-15T23:59:00Z',
        refreshToken: 'refresh',
        user: {
          branchId: '22222222-2222-2222-2222-222222222222',
          businessId: '11111111-1111-1111-1111-111111111111',
          email: 'admin@lafe.com',
          fullName: 'Admin Principal',
          id: '33333333-3333-3333-3333-333333333333',
          roles: ['Admin'],
        },
        userId: '33333333-3333-3333-3333-333333333333',
      })
    })
    vi.stubGlobal('fetch', fetchMock)

    renderRegisterBusinessPage()

    await user.click(await screen.findByRole('combobox', { name: 'Plan *' }))
    await user.click(screen.getByRole('option', { name: 'Basic - RD$29.00 / mes' }))
    await user.type(screen.getByLabelText('Nombre del comercio *'), 'Colmado La Fe')
    await user.type(screen.getByLabelText('Administrador *'), 'Admin Principal')
    await user.type(screen.getByLabelText('Correo electronico *'), 'admin@lafe.com')
    await user.type(screen.getByLabelText('Contrasena *'), 'Password123!')
    await user.type(screen.getByLabelText('Numero de identificacion *'), '00112345678')
    await user.type(screen.getByLabelText('Telefono principal *'), '8090000000')
    await user.click(screen.getByRole('button', { name: 'Crear comercio' }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(2))
    const [, registerOptions] = fetchMock.mock.calls.find(([url]) =>
      String(url).includes('/api/account/register-business'),
    )!
    expect(JSON.parse(String(registerOptions?.body))).toEqual(expect.objectContaining({
      planId: '11111111-1111-1111-1111-111111111111',
    }))
  })
})

function createJsonResponse(data: unknown) {
  return {
    headers: new Headers({ 'content-type': 'application/json' }),
    json: async () => ({
      correlationId: 'test',
      data,
      error: null,
      isSuccess: true,
    }),
    ok: true,
    status: 200,
  }
}

function renderRegisterBusinessPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <RegisterBusinessPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}
