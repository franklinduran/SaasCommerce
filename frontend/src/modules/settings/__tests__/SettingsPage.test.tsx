import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
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
    expect(screen.getByRole('button', { name: 'Negocio' })).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Sucursal' })).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Seguridad' })).toBeTruthy()
  })

  it('shows validation errors for business identification and primary phone', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await user.click(await screen.findByRole('button', { name: 'Negocio' }))
    await screen.findByRole('heading', { name: 'Negocio' })
    const identificationInput = screen.getByLabelText('Identificacion *')
    const primaryPhoneInput = screen.getByLabelText('Telefono principal *')

    await user.clear(identificationInput)
    await user.type(identificationInput, '123')
    await user.clear(primaryPhoneInput)
    await user.click(screen.getByRole('button', { name: 'Guardar cambios' }))

    expect(await screen.findByText('El RNC debe tener 9 digitos.')).toBeTruthy()
    expect(await screen.findByText('El telefono principal es requerido.')).toBeTruthy()
  })

  it('renders operational settings panel with tab navigation', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await openOperationalSettings(user)

    expect(await screen.findByText('Configuracion del sistema')).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Negocio' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Ventas' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Inventario' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Facturacion' })).toBeTruthy()
  })

  it('shows business settings form on default tab', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    // Wait for operational settings to load (default tab is Negocio → business settings)
    await openOperationalSettings(user)
    expect(await screen.findByRole('heading', { name: 'Informacion del negocio' })).toBeTruthy()
  })

  it('switches to sales settings tab', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await openOperationalSettings(user)
    await screen.findByRole('tab', { name: 'Ventas' })
    await user.click(screen.getByRole('tab', { name: 'Ventas' }))

    expect(await screen.findByRole('heading', { name: 'Configuracion de ventas' })).toBeTruthy()
  })

  it('switches to inventory settings tab', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await openOperationalSettings(user)
    await screen.findByRole('tab', { name: 'Inventario' })
    await user.click(screen.getByRole('tab', { name: 'Inventario' }))

    expect(await screen.findByRole('heading', { name: 'Configuracion de inventario' })).toBeTruthy()
  })

  it('switches to billing settings tab', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await openOperationalSettings(user)
    await screen.findByRole('tab', { name: 'Facturacion' })
    await user.click(screen.getByRole('tab', { name: 'Facturacion' }))

    expect(
      await screen.findByRole('heading', { name: 'Configuracion de facturacion' }),
    ).toBeTruthy()
  })

  it('loads operational business settings from API', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await openOperationalSettings(user)
    const commercialNameInput = await screen.findByDisplayValue('Demo Comercial')
    expect(commercialNameInput).toBeTruthy()
  })

  it('shows billing validation error when invoice prefix is empty', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await openOperationalSettings(user)
    await screen.findByRole('tab', { name: 'Facturacion' })
    await user.click(screen.getByRole('tab', { name: 'Facturacion' }))

    const tabPanel = await screen.findByRole('tabpanel')
    const prefixInput = await within(tabPanel).findByDisplayValue('RI')
    await user.clear(prefixInput)
    await user.click(within(tabPanel).getByRole('button', { name: 'Guardar cambios' }))

    expect(await within(tabPanel).findByText('El prefijo es requerido.')).toBeTruthy()
  })

  it('shows inventory validation error when threshold is negative', async () => {
    const user = userEvent.setup()
    vi.stubGlobal('fetch', createSettingsFetchMock())
    renderSettingsPage()

    await openOperationalSettings(user)
    await screen.findByRole('tab', { name: 'Inventario' })
    await user.click(screen.getByRole('tab', { name: 'Inventario' }))

    const tabPanel = await screen.findByRole('tabpanel')
    const thresholdInput = await within(tabPanel).findByDisplayValue('5')

    // Use fireEvent.change to bypass number input min constraint in jsdom
    fireEvent.change(thresholdInput, { target: { value: '-1' } })
    await user.click(within(tabPanel).getByRole('button', { name: 'Guardar cambios' }))

    expect(await within(tabPanel).findByText('El umbral debe ser >= 0.')).toBeTruthy()
  })
})

async function openOperationalSettings(user: ReturnType<typeof userEvent.setup>) {
  await user.click(await screen.findByRole('button', { name: 'Operativo' }))
}

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

    if (url.includes('/api/settings/business')) {
      return createJsonResponse({
        address: 'Av. 27 de Febrero 101',
        businessId: '11111111-1111-1111-1111-111111111111',
        commercialName: 'Demo Comercial',
        currency: 'DOP',
        email: 'info@demo.com',
        legalName: 'Demo S.R.L.',
        logoUrl: null,
        phone: '8091234567',
        receiptFooterText: null,
        rnc: '123456789',
        timezone: 'America/Santo_Domingo',
        updatedAt: '2026-05-01T10:00:00Z',
        updatedBy: '44444444-4444-4444-4444-444444444444',
      })
    }

    if (url.includes('/api/settings/sales')) {
      return createJsonResponse({
        allowDiscounts: true,
        allowNegativeStock: false,
        businessId: '11111111-1111-1111-1111-111111111111',
        defaultPaymentMethod: null,
        enableInvoiceAutoGeneration: true,
        enableReceiptPrintAfterSale: false,
        requireCustomerForCreditSale: true,
        updatedAt: '2026-05-01T10:00:00Z',
        updatedBy: '44444444-4444-4444-4444-444444444444',
      })
    }

    if (url.includes('/api/settings/inventory')) {
      return createJsonResponse({
        allowInventoryTransferBetweenBranches: false,
        businessId: '11111111-1111-1111-1111-111111111111',
        defaultLowStockThreshold: 5,
        enableLowStockAlerts: true,
        requireReasonForInventoryAdjustment: false,
        updatedAt: '2026-05-01T10:00:00Z',
        updatedBy: '44444444-4444-4444-4444-444444444444',
      })
    }

    if (url.includes('/api/settings/billing')) {
      return createJsonResponse({
        businessId: '11111111-1111-1111-1111-111111111111',
        enableInvoiceAutoGeneration: true,
        invoicePrefix: 'RI',
        invoiceSequenceStart: 1,
        receiptFooterText: null,
        receiptHeaderText: null,
        showLogoOnReceipt: false,
        showRncOnReceipt: false,
        updatedAt: '2026-05-01T10:00:00Z',
        updatedBy: '44444444-4444-4444-4444-444444444444',
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
