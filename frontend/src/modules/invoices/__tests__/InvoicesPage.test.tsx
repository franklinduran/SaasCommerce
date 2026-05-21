import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { InvoicesPage } from '@/modules/invoices/InvoicesPage'

describe('Invoices module', () => {
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('renders invoice list data', async () => {
    vi.stubGlobal('fetch', createInvoicesFetchMock())
    renderInvoicesAt('/invoices')

    expect(await screen.findByText('RI-00000001')).toBeTruthy()
    expect(screen.getByText('Emitido')).toBeTruthy()
  })

  it('renders invoice detail panel from URL', async () => {
    vi.stubGlobal('fetch', createInvoicesFetchMock())
    renderInvoicesAt('/invoices/invoice-1')

    expect(await screen.findByRole('heading', { name: 'RI-00000001' })).toBeTruthy()
    expect(screen.getAllByText('RD$300.00').length).toBeGreaterThan(0)
  })

  it('shows cancelled status in invoice detail', async () => {
    vi.stubGlobal('fetch', createInvoicesFetchMock({ status: 'Cancelled' }))
    renderInvoicesAt('/invoices/invoice-1')

    expect((await screen.findAllByText('Cancelado')).length).toBeGreaterThan(0)
  })
})

function renderInvoicesAt(path: string) {
  useAuthStore.getState().setSession({
    accessToken: 'jwt',
    expiresAt: '2026-05-18T23:59:00Z',
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
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route element={<InvoicesPage />} path="/invoices" />
          <Route element={<InvoicesPage />} path="/invoices/:invoiceId" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

function createInvoicesFetchMock(options: { status?: 'Issued' | 'Cancelled' } = {}) {
  const invoice = {
    branchId: '22222222-2222-2222-2222-222222222222',
    businessId: '11111111-1111-1111-1111-111111111111',
    cancelledAt: options.status === 'Cancelled' ? '2026-05-18T12:00:00Z' : null,
    createdAt: '2026-05-18T10:00:00Z',
    customerId: null,
    discountTotal: 0,
    id: 'invoice-1',
    invoiceId: 'invoice-1',
    invoiceNumber: 'RI-00000001',
    saleId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    status: options.status ?? 'Issued',
    subtotal: 300,
    taxTotal: 0,
    total: 300,
    updatedAt: '2026-05-18T10:00:00Z',
  }

  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()

    if (url.includes('/api/business/current')) {
      return createJsonResponse({
        businessId: '11111111-1111-1111-1111-111111111111',
        identificationNumber: '123456789',
        identificationType: 'Rnc',
        name: 'Demo Business',
        phones: [{ isPrimary: true, label: 'Principal', number: '8090000000' }],
      })
    }

    if (url.includes('/api/invoices/invoice-1')) {
      return createJsonResponse(invoice)
    }

    if (url.includes('/api/invoices')) {
      return createJsonResponse({
        hasNextPage: false,
        hasPreviousPage: false,
        items: [invoice],
        page: 1,
        pageSize: 10,
        totalItems: 1,
        totalPages: 1,
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
      error: ok ? null : { code: 'NOT_FOUND', message: 'Not found' },
      isSuccess: ok,
    }),
    ok,
    status,
  }
}
