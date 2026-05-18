import { useAuthStore } from '@/modules/auth/authStore'
import type { Invoice, InvoiceFilters, InvoiceListResponse } from '@/modules/invoices/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getInvoices(filters: InvoiceFilters): Promise<InvoiceListResponse> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  })

  if (filters.status) {
    params.set('status', filters.status)
  }

  if (filters.query.trim()) {
    params.set('query', filters.query.trim())
  }

  const dateFrom = toDateFrom(filters.dateFrom)
  const dateTo = toDateTo(filters.dateTo)

  if (dateFrom) {
    params.set('dateFrom', dateFrom)
  }

  if (dateTo) {
    params.set('dateTo', dateTo)
  }

  const response = await httpClient<InvoiceListResponse>(`/api/invoices?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getInvoice(invoiceId: string): Promise<Invoice> {
  const response = await httpClient<Invoice>(`/api/invoices/${invoiceId}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getInvoiceBySale(saleId: string): Promise<Invoice> {
  const response = await httpClient<Invoice>(`/api/sales/${saleId}/invoice`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function cancelInvoice(invoiceId: string): Promise<Invoice> {
  const response = await httpClient<Invoice>(`/api/invoices/${invoiceId}/cancel`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })

  return response.data!
}

function toDateFrom(value: string): string | null {
  return value ? new Date(`${value}T00:00:00`).toISOString() : null
}

function toDateTo(value: string): string | null {
  return value ? new Date(`${value}T23:59:59.999`).toISOString() : null
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
