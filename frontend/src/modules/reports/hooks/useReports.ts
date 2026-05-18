import { useQuery } from '@tanstack/react-query'
import {
  getAccountsReceivableReport,
  getInvoiceReport,
  getLowStockReport,
  getPurchaseReport,
  getSalesReport,
} from '@/modules/reports/services/reportsApi'
import type {
  AccountsReceivableFilters,
  InvoiceReportFilters,
  LowStockFilters,
  PurchaseReportFilters,
  SalesReportFilters,
} from '@/modules/reports/types'

export const reportKeys = {
  sales: (filters: SalesReportFilters) => ['reports', 'sales', filters] as const,
  invoices: (filters: InvoiceReportFilters) => ['reports', 'invoices', filters] as const,
  ar: (filters: AccountsReceivableFilters) => ['reports', 'ar', filters] as const,
  lowStock: (filters: LowStockFilters) => ['reports', 'lowStock', filters] as const,
  purchases: (filters: PurchaseReportFilters) => ['reports', 'purchases', filters] as const,
}

export function useSalesReport(filters: SalesReportFilters) {
  return useQuery({
    queryFn: () => getSalesReport(filters),
    queryKey: reportKeys.sales(filters),
    staleTime: 60 * 1000,
  })
}

export function useInvoiceReport(filters: InvoiceReportFilters) {
  return useQuery({
    queryFn: () => getInvoiceReport(filters),
    queryKey: reportKeys.invoices(filters),
    staleTime: 60 * 1000,
  })
}

export function useAccountsReceivableReport(filters: AccountsReceivableFilters) {
  return useQuery({
    queryFn: () => getAccountsReceivableReport(filters),
    queryKey: reportKeys.ar(filters),
    staleTime: 60 * 1000,
  })
}

export function useLowStockReport(filters: LowStockFilters) {
  return useQuery({
    queryFn: () => getLowStockReport(filters),
    queryKey: reportKeys.lowStock(filters),
    staleTime: 60 * 1000,
  })
}

export function usePurchaseReport(filters: PurchaseReportFilters) {
  return useQuery({
    queryFn: () => getPurchaseReport(filters),
    queryKey: reportKeys.purchases(filters),
    staleTime: 60 * 1000,
  })
}
