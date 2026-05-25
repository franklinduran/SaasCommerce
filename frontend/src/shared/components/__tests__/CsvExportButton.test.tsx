import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { CsvExportButton } from '@/shared/components/CsvExportButton'

const CSV_CONTENT = 'SKU,Nombre\n001,Producto A\n'
const BUSINESS_ID = '11111111-1111-1111-1111-111111111111'

function setSession() {
  useAuthStore.getState().setSession({
    accessToken: 'test-jwt',
    expiresAt: '2027-01-01T00:00:00Z',
    refreshToken: 'refresh',
    user: {
      branchId: '22222222-2222-2222-2222-222222222222',
      businessId: BUSINESS_ID,
      email: 'admin@test.com',
      fullName: 'Admin',
      id: '33333333-3333-3333-3333-333333333333',
      roles: ['Admin'],
    },
  })
}

// Minimal DOM stubs for download flow
function stubDownloadGlobals() {
  const createObjectURL = vi.fn(() => 'blob:http://localhost/fake-url')
  const revokeObjectURL = vi.fn()
  Object.defineProperty(URL, 'createObjectURL', { value: createObjectURL, writable: true })
  Object.defineProperty(URL, 'revokeObjectURL', { value: revokeObjectURL, writable: true })

  // Stub anchor click so jsdom doesn't throw
  const originalCreate = document.createElement.bind(document)
  vi.spyOn(document, 'createElement').mockImplementation((tag: string) => {
    const el = originalCreate(tag)
    if (tag === 'a') {
      vi.spyOn(el as HTMLAnchorElement, 'click').mockImplementation(() => {})
    }
    return el
  })
}

function createCsvFetchMock(status = 200) {
  return vi.fn(async (_url: string, _init?: RequestInit) =>
    new Response(CSV_CONTENT, {
      status,
      headers: { 'Content-Type': 'text/csv; charset=utf-8' },
    }),
  )
}

describe('CsvExportButton', () => {
  beforeEach(() => {
    setSession()
    stubDownloadGlobals()
  })

  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.restoreAllMocks()
    vi.unstubAllGlobals()
  })

  it('renders with default label', () => {
    render(<CsvExportButton endpoint="/api/products/export" filename="productos.csv" />)
    expect(screen.getByRole('button', { name: /Exportar CSV/i })).toBeTruthy()
  })

  it('renders with custom label', () => {
    render(
      <CsvExportButton
        endpoint="/api/sales/export"
        filename="ventas.csv"
        label="Descargar ventas"
      />,
    )
    expect(screen.getByRole('button', { name: /Descargar ventas/i })).toBeTruthy()
  })

  it('shows "Exportando..." while fetching and restores label after', async () => {
    let resolveFetch!: (value: Response) => void
    vi.stubGlobal(
      'fetch',
      vi.fn(
        () =>
          new Promise<Response>((resolve) => {
            resolveFetch = resolve
          }),
      ),
    )

    const user = userEvent.setup()
    render(<CsvExportButton endpoint="/api/products/export" filename="productos.csv" />)

    const btn = screen.getByRole('button', { name: /Exportar CSV/i })
    await user.click(btn)

    expect(screen.getByText('Exportando...')).toBeTruthy()
    expect(btn).toHaveAttribute('disabled')

    // Resolve with a CSV response
    resolveFetch(
      new Response(CSV_CONTENT, {
        status: 200,
        headers: { 'Content-Type': 'text/csv' },
      }),
    )

    await waitFor(() => {
      expect(screen.getByText('Exportar CSV')).toBeTruthy()
    })
    expect(btn).not.toHaveAttribute('disabled')
  })

  it('calls fetch with Authorization header when authenticated', async () => {
    const mockFetch = createCsvFetchMock()
    vi.stubGlobal('fetch', mockFetch)

    const user = userEvent.setup()
    render(<CsvExportButton endpoint="/api/products/export" filename="productos.csv" />)
    await user.click(screen.getByRole('button', { name: /Exportar CSV/i }))

    await waitFor(() => {
      expect(screen.getByText('Exportar CSV')).toBeTruthy()
    })

    expect(mockFetch).toHaveBeenCalledOnce()
    const [, init] = mockFetch.mock.calls[0]
    expect((init as RequestInit).headers).toMatchObject({
      Authorization: 'Bearer test-jwt',
    })
  })

  it('appends query params when provided', async () => {
    const mockFetch = createCsvFetchMock()
    vi.stubGlobal('fetch', mockFetch)

    const user = userEvent.setup()
    render(
      <CsvExportButton
        endpoint="/api/sales/export"
        filename="ventas.csv"
        queryParams={{ dateFrom: '2026-01-01', dateTo: '2026-01-31' }}
      />,
    )
    await user.click(screen.getByRole('button', { name: /Exportar CSV/i }))

    await waitFor(() => {
      expect(screen.getByText('Exportar CSV')).toBeTruthy()
    })

    const [url] = mockFetch.mock.calls[0]
    expect(url).toContain('dateFrom=2026-01-01')
    expect(url).toContain('dateTo=2026-01-31')
  })

  it('omits undefined query params', async () => {
    const mockFetch = createCsvFetchMock()
    vi.stubGlobal('fetch', mockFetch)

    const user = userEvent.setup()
    render(
      <CsvExportButton
        endpoint="/api/sales/export"
        filename="ventas.csv"
        queryParams={{ dateFrom: '2026-01-01', dateTo: undefined }}
      />,
    )
    await user.click(screen.getByRole('button', { name: /Exportar CSV/i }))

    await waitFor(() => {
      expect(screen.getByText('Exportar CSV')).toBeTruthy()
    })

    const [url] = mockFetch.mock.calls[0]
    expect(url).toContain('dateFrom=2026-01-01')
    expect(url).not.toContain('dateTo')
  })

  it('silently resets state when API returns non-200', async () => {
    vi.stubGlobal('fetch', createCsvFetchMock(500))

    const user = userEvent.setup()
    render(<CsvExportButton endpoint="/api/products/export" filename="productos.csv" />)
    await user.click(screen.getByRole('button', { name: /Exportar CSV/i }))

    await waitFor(() => {
      expect(screen.getByText('Exportar CSV')).toBeTruthy()
    })
    // No crash, button re-enabled
    expect(screen.getByRole('button')).not.toHaveAttribute('disabled')
  })
})
