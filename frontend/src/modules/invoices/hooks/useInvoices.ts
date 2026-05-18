import { useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/modules/auth/authStore'
import {
  cancelInvoice,
  getInvoice,
  getInvoiceBySale,
  getInvoices,
} from '@/modules/invoices/services/invoicesApi'
import type { InvoiceFilters, InvoiceRealtimeNotification } from '@/modules/invoices/types'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

export const invoiceKeys = {
  all: ['invoices'] as const,
  bySale: (saleId: string) => ['invoices', 'sale', saleId] as const,
  detail: (invoiceId: string) => ['invoices', 'detail', invoiceId] as const,
  list: (filters: InvoiceFilters) => ['invoices', 'list', filters] as const,
}

export function useInvoices(filters: InvoiceFilters) {
  return useQuery({
    queryFn: () => getInvoices(filters),
    queryKey: invoiceKeys.list(filters),
  })
}

export function useInvoice(invoiceId?: string) {
  return useQuery({
    enabled: Boolean(invoiceId),
    queryFn: () => getInvoice(invoiceId!),
    queryKey: invoiceId ? invoiceKeys.detail(invoiceId) : ['invoices', 'detail'],
  })
}

export function useInvoiceBySale(saleId?: string, enabled = true) {
  return useQuery({
    enabled: Boolean(saleId) && enabled,
    queryFn: () => getInvoiceBySale(saleId!),
    queryKey: saleId ? invoiceKeys.bySale(saleId) : ['invoices', 'sale'],
    retry: false,
  })
}

export function useCancelInvoice() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: cancelInvoice,
    onSuccess: async (invoice) => {
      await queryClient.invalidateQueries({ queryKey: invoiceKeys.all })
      await queryClient.invalidateQueries({ queryKey: invoiceKeys.detail(invoice.invoiceId) })
      await queryClient.invalidateQueries({ queryKey: invoiceKeys.bySale(invoice.saleId) })
    },
  })
}

export function useInvoiceRealtimeInvalidation() {
  const businessId = useAuthStore((state) => state.session?.user.businessId)
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!businessId) {
      return undefined
    }

    const handler = (payload: InvoiceRealtimeNotification) => {
      if (payload.businessId !== businessId) {
        return
      }

      void queryClient.invalidateQueries({ queryKey: invoiceKeys.all })
    }

    onRealtimeEvent('invoice.generated', handler)
    onRealtimeEvent('invoice.cancelled', handler)

    return () => {
      offRealtimeEvent('invoice.generated', handler)
      offRealtimeEvent('invoice.cancelled', handler)
    }
  }, [businessId, queryClient])
}
