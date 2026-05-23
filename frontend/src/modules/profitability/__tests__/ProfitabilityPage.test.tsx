import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { ProfitabilityPage } from '../pages/ProfitabilityPage'
import { ProductProfitabilityPage } from '../pages/ProductProfitabilityPage'
import { BranchProfitabilityPage } from '../pages/BranchProfitabilityPage'
import { ProfitabilityAlertsPage } from '../pages/ProfitabilityAlertsPage'
import type { BranchProfitability, ProductProfitability, ProfitabilityAlert, ProfitabilitySummary } from '../types'

// ── Test data factories ───────────────────────────────────────────────────────

function createSummary(overrides?: Partial<ProfitabilitySummary>): ProfitabilitySummary {
  return {
    dateFrom: '2026-05-01T00:00:00Z',
    dateTo: '2026-05-31T23:59:59Z',
    totalSales: 50_000,
    totalCost: 30_000,
    grossProfit: 20_000,
    operatingExpenses: 5_000,
    estimatedNetProfit: 15_000,
    grossMarginPercent: 40,
    netMarginPercent: 30,
    salesCount: 100,
    warningCount: 0,
    ...overrides,
  }
}

function createProduct(overrides?: Partial<ProductProfitability>): ProductProfitability {
  return {
    productId: 'prod-1',
    productName: 'Producto de prueba',
    sku: 'SKU-001',
    totalQuantity: 20,
    totalSales: 10_000,
    totalCost: 6_000,
    grossProfit: 4_000,
    marginPercent: 40,
    hasMissingCost: false,
    categoryId: null,
    categoryName: null,
    ...overrides,
  }
}

function createBranch(overrides?: Partial<BranchProfitability>): BranchProfitability {
  return {
    branchId: 'branch-1',
    branchName: 'Sucursal Central',
    totalSales: 30_000,
    totalCost: 18_000,
    grossProfit: 12_000,
    operatingExpenses: 3_000,
    estimatedNetProfit: 9_000,
    netMarginPercent: 30,
    salesCount: 60,
    ...overrides,
  }
}

function createAlert(overrides?: Partial<ProfitabilityAlert>): ProfitabilityAlert {
  return {
    alertType: 'MissingCost',
    message: "El producto 'Producto X' no tiene costo registrado.",
    branchId: null,
    branchName: null,
    productId: 'prod-1',
    productName: 'Producto X',
    estimatedImpact: 5_000,
    ...overrides,
  }
}

// ── API mock helpers ──────────────────────────────────────────────────────────

function createJsonResponse<T>(data: T, ok = true, status = 200) {
  return {
    ok: ok && status >= 200 && status < 300,
    status,
    json: async () => ({
      isSuccess: ok,
      data,
      error: ok ? null : { code: 'ERROR', message: 'failed' },
      correlationId: 'test-correlation-id',
    }),
    headers: new Headers({ 'content-type': 'application/json' }),
  }
}

function createFetchMock({
  summary = createSummary(),
  products = [createProduct()],
  branches = [createBranch()],
  alerts = [] as ProfitabilityAlert[],
}: {
  summary?: ProfitabilitySummary
  products?: ProductProfitability[]
  branches?: BranchProfitability[]
  alerts?: ProfitabilityAlert[]
} = {}) {
  return vi.fn(async (input: RequestInfo | URL) => {
    const url = input.toString()
    if (url.includes('/api/profitability/summary')) return createJsonResponse(summary)
    if (url.includes('/api/profitability/products')) return createJsonResponse(products)
    if (url.includes('/api/profitability/branches')) return createJsonResponse(branches)
    if (url.includes('/api/profitability/alerts')) return createJsonResponse(alerts)
    return createJsonResponse(null, false, 404)
  })
}

// ── Render helpers ────────────────────────────────────────────────────────────

function setSession() {
  useAuthStore.getState().setSession({
    accessToken: 'test-token',
    refreshToken: 'refresh-token',
    expiresAt: '2099-01-01T00:00:00.000Z',
    user: {
      id: '33333333-3333-3333-3333-333333333333',
      businessId: '11111111-1111-1111-1111-111111111111',
      branchId: '22222222-2222-2222-2222-222222222222',
      fullName: 'Test User',
      email: 'test@test.com',
      roles: ['Owner'],
    },
  })
}

function createQueryClient() {
  return new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
}

function renderPage(element: React.ReactNode) {
  const qc = createQueryClient()
  return render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>{element}</MemoryRouter>
    </QueryClientProvider>,
  )
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('ProfitabilityPage (summary)', () => {
  beforeEach(() => { setSession() })
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows loading state initially', () => {
    vi.stubGlobal('fetch', createFetchMock())

    renderPage(<ProfitabilityPage />)

    expect(screen.queryByRole('heading', { name: /rentabilidad/i }) ?? screen.queryByText('Rentabilidad')).toBeTruthy()
  })

  it('renders summary cards when data is loaded', async () => {
    vi.stubGlobal('fetch', createFetchMock({ summary: createSummary() }))

    renderPage(<ProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText('Ventas totales')).toBeTruthy()
    })
    expect(screen.getByText('Ganancia bruta')).toBeTruthy()
    expect(screen.getByText('Ganancia neta estimada')).toBeTruthy()
    expect(screen.getByText('Gastos operativos')).toBeTruthy()
  })

  it('shows warning banner when warningCount > 0', async () => {
    vi.stubGlobal('fetch', createFetchMock({ summary: createSummary({ warningCount: 3 }) }))

    renderPage(<ProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText(/sin costo registrado/i)).toBeTruthy()
    })
  })

  it('shows navigation buttons to sub-pages', async () => {
    vi.stubGlobal('fetch', createFetchMock())

    renderPage(<ProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /por producto/i })).toBeTruthy()
      expect(screen.getByRole('button', { name: /por sucursal/i })).toBeTruthy()
      expect(screen.getByRole('button', { name: /alertas/i })).toBeTruthy()
    })
  })

  it('shows error state when fetch fails', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => createJsonResponse(null, false, 500)))

    renderPage(<ProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText(/error al cargar/i)).toBeTruthy()
    })
  })
})

describe('ProductProfitabilityPage', () => {
  beforeEach(() => { setSession() })
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows product table when data is loaded', async () => {
    vi.stubGlobal('fetch', createFetchMock({ products: [createProduct()] }))

    renderPage(<ProductProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText('Producto de prueba')).toBeTruthy()
    })
    expect(screen.getByText('SKU-001')).toBeTruthy()
  })

  it('shows empty state when no products', async () => {
    vi.stubGlobal('fetch', createFetchMock({ products: [] }))

    renderPage(<ProductProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText(/no hay ventas completadas/i)).toBeTruthy()
    })
  })

  it('shows "Sin costo" when product has missing cost', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({ products: [createProduct({ hasMissingCost: true })] }),
    )

    renderPage(<ProductProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText('Sin costo')).toBeTruthy()
    })
  })
})

describe('BranchProfitabilityPage', () => {
  beforeEach(() => { setSession() })
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows branch cards when data is loaded', async () => {
    vi.stubGlobal('fetch', createFetchMock({ branches: [createBranch()] }))

    renderPage(<BranchProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText('Sucursal Central')).toBeTruthy()
    })
    expect(screen.getByText(/60 venta/i)).toBeTruthy()
  })

  it('shows empty state when no branches', async () => {
    vi.stubGlobal('fetch', createFetchMock({ branches: [] }))

    renderPage(<BranchProfitabilityPage />)

    await waitFor(() => {
      expect(screen.getByText(/no hay ventas completadas/i)).toBeTruthy()
    })
  })
})

describe('ProfitabilityAlertsPage', () => {
  beforeEach(() => { setSession() })
  afterEach(() => {
    cleanup()
    useAuthStore.getState().clearSession()
    vi.unstubAllGlobals()
  })

  it('shows no-alerts state when list is empty', async () => {
    vi.stubGlobal('fetch', createFetchMock({ alerts: [] }))

    renderPage(<ProfitabilityAlertsPage />)

    await waitFor(() => {
      expect(screen.getByText(/sin alertas/i)).toBeTruthy()
    })
  })

  it('shows alert cards when alerts exist', async () => {
    vi.stubGlobal(
      'fetch',
      createFetchMock({ alerts: [createAlert()] }),
    )

    renderPage(<ProfitabilityAlertsPage />)

    await waitFor(() => {
      expect(screen.getByText('Producto X')).toBeTruthy()
    })
    expect(screen.getByText(/no tiene costo registrado/i)).toBeTruthy()
  })

  it('shows NegativeMargin alert type correctly', async () => {
    const alert = createAlert({
      alertType: 'NegativeMargin',
      message: "El producto 'X' tiene margen negativo.",
      productName: 'Producto Negativo',
    })
    vi.stubGlobal('fetch', createFetchMock({ alerts: [alert] }))

    renderPage(<ProfitabilityAlertsPage />)

    await waitFor(() => {
      expect(screen.getByText('Margen negativo')).toBeTruthy()
    })
  })

  it('shows HighExpenses alert type with branch name', async () => {
    const alert = createAlert({
      alertType: 'HighExpenses',
      message: "La sucursal 'Norte' tiene gastos altos.",
      branchName: 'Sucursal Norte',
      branchId: 'branch-2',
      productId: null,
      productName: null,
    })
    vi.stubGlobal('fetch', createFetchMock({ alerts: [alert] }))

    renderPage(<ProfitabilityAlertsPage />)

    await waitFor(() => {
      expect(screen.getByText('Sucursal Norte')).toBeTruthy()
      expect(screen.getByText('Gastos altos')).toBeTruthy()
    })
  })
})
