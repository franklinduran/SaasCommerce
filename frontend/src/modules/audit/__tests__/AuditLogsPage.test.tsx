import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import AuditLogsPage from '@/modules/audit/AuditLogsPage'
import { useAuditLogs } from '@/modules/audit/useAuditLogs'

vi.mock('@/modules/audit/useAuditLogs', () => ({
  useAuditLogs: vi.fn(),
}))

vi.mock('@/modules/audit/components/AuditLogListItem', () => ({
  AuditLogListItem: ({ log, onClick, selected }: any) => (
    <button data-selected={selected} onClick={onClick} type="button">
      {log.action} - {log.entityName}
    </button>
  ),
}))

vi.mock('@/modules/audit/components/AuditLogDetailPanel', () => ({
  AuditLogDetailPanel: ({ auditLogId, onClose }: any) => (
    <div>
      Detail {auditLogId}
      <button onClick={onClose} type="button">Close detail</button>
    </div>
  ),
}))

describe('AuditLogsPage', () => {
  const refetch = vi.fn()

  beforeEach(() => {
    vi.mocked(useAuditLogs).mockReturnValue(result(auditData()) as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders stats, list, detail panel and pagination actions', async () => {
    const user = userEvent.setup()
    render(<AuditLogsPage />)

    expect(screen.getByText('Auditoria')).toBeTruthy()
    expect(screen.getByText('10')).toBeTruthy()
    expect(screen.getByText('auth.login_failed - Auth')).toBeTruthy()
    expect(screen.getByText('sale.completed - Sale')).toBeTruthy()

    await user.click(screen.getByText('sale.completed - Sale'))
    expect(screen.getByText('Detail audit-2')).toBeTruthy()

    await user.click(screen.getByLabelText('Pagina siguiente'))
    expect(vi.mocked(useAuditLogs).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({ page: 2 }))

    await user.click(screen.getByLabelText('Pagina anterior'))
    expect(vi.mocked(useAuditLogs).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({ page: 1 }))
  })

  it('updates filters, clears them and refetches', async () => {
    const user = userEvent.setup()
    render(<AuditLogsPage />)

    await user.type(screen.getByLabelText('Fecha desde'), '2026-05-01')
    expect(vi.mocked(useAuditLogs).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({ dateFrom: '2026-05-01' }))

    await user.click(screen.getByRole('button', { name: /Limpiar filtros/i }))
    expect(vi.mocked(useAuditLogs).mock.calls.at(-1)?.[0]).toEqual(expect.objectContaining({ dateFrom: '' }))

    await user.click(screen.getByRole('button', { name: /Refrescar/i }))
    expect(refetch).toHaveBeenCalled()
  })

  it('renders loading, error and empty states', () => {
    vi.mocked(useAuditLogs).mockReturnValueOnce(result(undefined, { isLoading: true }) as never)
    const { rerender } = render(<AuditLogsPage />)
    expect(document.querySelectorAll('.bg-stone-100').length).toBeGreaterThan(0)

    vi.mocked(useAuditLogs).mockReturnValueOnce(result(undefined, { isError: true }) as never)
    rerender(<AuditLogsPage />)
    expect(screen.getByText('Error al cargar los registros de auditoria.')).toBeTruthy()

    vi.mocked(useAuditLogs).mockReturnValueOnce(result({ ...auditData(), items: [], totalItems: 0, totalPages: 1 }) as never)
    rerender(<AuditLogsPage />)
    expect(screen.getByText('Sin registros')).toBeTruthy()
  })

  function result(data: unknown, overrides: Record<string, unknown> = {}) {
    return {
      data,
      isError: false,
      isFetching: false,
      isLoading: false,
      refetch,
      ...overrides,
    }
  }
})

function auditData() {
  return {
    hasNextPage: true,
    hasPreviousPage: true,
    items: [
      { action: 'auth.login_failed', auditLogId: 'audit-1', entityName: 'Auth' },
      { action: 'sale.completed', auditLogId: 'audit-2', entityName: 'Sale' },
    ],
    page: 1,
    pageSize: 50,
    totalItems: 10,
    totalPages: 2,
  }
}
