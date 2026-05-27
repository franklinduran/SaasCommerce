import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { BetaFeedbackPage } from '@/modules/beta-feedback/pages/BetaFeedbackPage'
import {
  useBetaFeedback,
  useCreateBetaFeedback,
  useUpdateBetaFeedbackStatus,
} from '@/modules/beta-feedback/hooks/useBetaFeedback'
import type { BetaFeedback, BetaFeedbackListResponse } from '@/modules/beta-feedback/types'
import { Permission } from '@/shared/types/permissions'
import { useCurrentUserPermissions } from '@/shared/hooks/usePermissions'

vi.mock('@/modules/beta-feedback/hooks/useBetaFeedback', () => ({
  useBetaFeedback: vi.fn(),
  useBetaFeedbackRealtimeInvalidation: vi.fn(),
  useCreateBetaFeedback: vi.fn(),
  useUpdateBetaFeedbackStatus: vi.fn(),
}))

vi.mock('@/shared/hooks/usePermissions', () => ({
  useCurrentUserPermissions: vi.fn(),
}))

const refetch = vi.fn()

describe('BetaFeedbackPage', () => {
  const create = vi.fn()
  const update = vi.fn()

  beforeEach(() => {
    vi.mocked(useBetaFeedback).mockReturnValue(queryResult(listResponse()) as never)
    vi.mocked(useCreateBetaFeedback).mockReturnValue({
      isError: false,
      isPending: false,
      isSuccess: false,
      mutateAsync: create,
    } as never)
    vi.mocked(useUpdateBetaFeedbackStatus).mockReturnValue({
      isPending: false,
      mutate: update,
    } as never)
    vi.mocked(useCurrentUserPermissions).mockReturnValue({
      data: { permissions: [Permission.BetaFeedbackManage] },
    } as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders feedback rows and status filters', async () => {
    render(<BetaFeedbackPage />)

    expect(screen.getByText('Feedback de pilotos')).toBeTruthy()
    expect(screen.getByText('Venta queda procesando')).toBeTruthy()
    expect(screen.getAllByText('Problema de venta').length).toBeGreaterThan(0)
    expect(screen.getAllByRole('combobox', { name: /Estado/i }).length).toBeGreaterThan(0)
  })

  it('shows empty, loading and error states', () => {
    vi.mocked(useBetaFeedback).mockReturnValue(queryResult(listResponse([])) as never)
    const { rerender } = render(<BetaFeedbackPage />)
    expect(screen.getByText('Aun no hay feedback registrado')).toBeTruthy()

    vi.mocked(useBetaFeedback).mockReturnValue({ ...queryResult(), isLoading: true } as never)
    rerender(<BetaFeedbackPage />)
    expect(screen.getByText('Casos reportados')).toBeTruthy()

    vi.mocked(useBetaFeedback).mockReturnValue({ ...queryResult(), isError: true } as never)
    rerender(<BetaFeedbackPage />)
    expect(screen.getByText('No se pudo cargar el feedback.')).toBeTruthy()
  })

  it('validates the feedback form before submitting', async () => {
    const user = userEvent.setup()
    render(<BetaFeedbackPage />)

    await user.click(screen.getByRole('button', { name: /Enviar feedback/i }))

    expect(await screen.findByText('El titulo debe tener al menos 5 caracteres.')).toBeTruthy()
    expect(create).not.toHaveBeenCalled()
  })

  it('submits valid feedback with category and context', async () => {
    const user = userEvent.setup()
    create.mockResolvedValueOnce(feedbackItem())
    render(<BetaFeedbackPage />)

    await user.selectOptions(screen.getAllByLabelText('Categoria')[0], 'InventoryIssue')
    await user.type(screen.getByLabelText('Titulo'), 'Stock incorrecto')
    await user.type(screen.getByLabelText('Descripcion'), 'El inventario no subio luego de registrar compra.')
    await user.type(screen.getByLabelText('URL o contexto'), '/inventory')
    await user.click(screen.getByRole('button', { name: /Enviar feedback/i }))

    await waitFor(() => {
      expect(create).toHaveBeenCalledWith({
        category: 'InventoryIssue',
        contextUrl: '/inventory',
        description: 'El inventario no subio luego de registrar compra.',
        title: 'Stock incorrecto',
      })
    })
  })

  it('updates status when the user has manage permission', async () => {
    const user = userEvent.setup()
    render(<BetaFeedbackPage />)

    await user.selectOptions(screen.getByLabelText('Cambiar estado de Venta queda procesando'), 'Resolved')

    expect(update).toHaveBeenCalledWith({
      feedbackId: 'feedback-1',
      request: { status: 'Resolved' },
    })
  })

  it('hides status controls when user cannot manage feedback', () => {
    vi.mocked(useCurrentUserPermissions).mockReturnValue({
      data: { permissions: [Permission.BetaFeedbackView] },
    } as never)

    render(<BetaFeedbackPage />)

    expect(screen.queryByLabelText('Cambiar estado de Venta queda procesando')).toBeNull()
  })
})

function queryResult(data: BetaFeedbackListResponse = listResponse()) {
  return {
    data,
    isError: false,
    isFetching: false,
    isLoading: false,
    refetch,
  }
}

function listResponse(items: BetaFeedback[] = [feedbackItem()]): BetaFeedbackListResponse {
  return {
    items,
    page: 1,
    pageSize: 20,
    totalItems: items.length,
    totalPages: items.length > 0 ? 1 : 0,
  }
}

function feedbackItem(): BetaFeedback {
  return {
    businessId: 'business-1',
    category: 'SaleIssue',
    contextUrl: '/sales/1',
    createdAt: '2026-05-27T12:00:00Z',
    description: 'La venta queda en estado Processing.',
    id: 'feedback-1',
    reviewNote: null,
    reviewedAt: null,
    reviewedByUserId: null,
    status: 'New',
    title: 'Venta queda procesando',
    updatedAt: '2026-05-27T12:00:00Z',
    userId: 'user-1',
  }
}
