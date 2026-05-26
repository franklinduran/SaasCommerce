import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { DashboardPage } from '@/modules/dashboard/DashboardPage'
import { useDashboardSummary } from '@/modules/dashboard/hooks/useDashboard'

const refetch = vi.fn()
let state: {
  data?: ReturnType<typeof summaryData>
  isError?: boolean
  isLoading?: boolean
}

vi.mock('@/modules/dashboard/hooks/useDashboard', () => ({
  useDashboardRealtimeInvalidation: vi.fn(),
  useDashboardSummary: vi.fn(),
}))

describe('DashboardPage', () => {
  beforeEach(() => {
    state = { data: summaryData(), isError: false, isLoading: false }
    vi.mocked(useDashboardSummary).mockImplementation(() => ({
      data: state.data,
      isError: Boolean(state.isError),
      isLoading: Boolean(state.isLoading),
      refetch,
    }) as unknown as ReturnType<typeof useDashboardSummary>)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders summary metrics and recent activity', async () => {
    const user = userEvent.setup()
    renderPage()

    expect(screen.getByText('Tu tienda de un vistazo')).toBeTruthy()
    expect(screen.getByText('Ventas hoy')).toBeTruthy()
    expect(screen.getByText('SALE-001')).toBeTruthy()
    expect(screen.getByText('INV-001')).toBeTruthy()
    expect(screen.getByText('PUR-001')).toBeTruthy()
    expect(screen.getByText('2 clientes con saldo pendiente')).toBeTruthy()

    await user.click(screen.getByRole('button', { name: 'Actualizar' }))
    expect(refetch).toHaveBeenCalled()
  })

  it('renders loading, empty and error states', () => {
    state = { isError: true, isLoading: true }
    const { rerender } = renderPage()
    expect(document.querySelectorAll('.animate-pulse').length).toBeGreaterThan(0)
    expect(screen.getByText('Error al cargar el resumen. Intenta actualizar.')).toBeTruthy()

    state = {
      data: {
        ...summaryData(),
        receivables: { customerCount: 0, totalPending: 0 },
        recentInvoices: [],
        recentPurchases: [],
        recentSales: [],
      },
      isLoading: false,
    }
    rerender(<MemoryRouter><DashboardPage /></MemoryRouter>)

    expect(screen.getByText('Sin ventas hoy')).toBeTruthy()
    expect(screen.getByText('Sin facturas hoy')).toBeTruthy()
    expect(screen.queryByText('Compras recientes')).toBeNull()
  })
})

function renderPage() {
  return render(<MemoryRouter><DashboardPage /></MemoryRouter>)
}

function summaryData() {
  return {
    invoicesToday: { count: 1, totalAmount: 800 },
    lowStock: { productCount: 4 },
    receivables: { customerCount: 2, totalPending: 3500 },
    recentInvoices: [
      { invoiceId: 'invoice-1', invoiceNumber: 'INV-001', status: 'Issued', total: 800 },
    ],
    recentPurchases: [
      { purchaseId: 'purchase-1', purchaseNumber: 'PUR-001', status: 'Received', supplierName: null, total: 500 },
    ],
    recentSales: [
      { paymentMethod: 'Cash', saleId: 'sale-1', saleNumber: 'SALE-001', status: 'Completed', total: 1250 },
      { paymentMethod: 'Card', saleId: 'sale-2', saleNumber: 'SALE-002', status: 'Cancelled', total: 50 },
      { paymentMethod: 'Transfer', saleId: 'sale-3', saleNumber: 'SALE-003', status: 'Pending', total: 75 },
      { paymentMethod: 'Cash', saleId: 'sale-4', saleNumber: 'SALE-004', status: 'Other', total: 20 },
    ],
    salesToday: { count: 4, totalAmount: 1250 },
  }
}
