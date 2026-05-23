import { useQuery } from '@tanstack/react-query'
import { profitabilityApi } from '../services/profitabilityApi'
import type { ProfitabilityFilters } from '../types'

export const profitabilityQueryKeys = {
  summary: (filters: ProfitabilityFilters) => ['profitability', 'summary', filters] as const,
  products: (filters: ProfitabilityFilters, categoryId?: string) =>
    ['profitability', 'products', filters, categoryId] as const,
  branches: (filters: Omit<ProfitabilityFilters, 'branchId'>) =>
    ['profitability', 'branches', filters] as const,
  alerts: (filters: ProfitabilityFilters) => ['profitability', 'alerts', filters] as const,
}

function isValidFilter(filters: ProfitabilityFilters) {
  return Boolean(filters.from && filters.to)
}

export function useProfitabilitySummary(filters: ProfitabilityFilters) {
  return useQuery({
    queryKey: profitabilityQueryKeys.summary(filters),
    queryFn: () => profitabilityApi.getSummary(filters),
    enabled: isValidFilter(filters),
    staleTime: 1000 * 60,
  })
}

export function useProductProfitability(filters: ProfitabilityFilters, categoryId?: string) {
  return useQuery({
    queryKey: profitabilityQueryKeys.products(filters, categoryId),
    queryFn: () => profitabilityApi.getProducts(filters, categoryId),
    enabled: isValidFilter(filters),
    staleTime: 1000 * 60,
  })
}

export function useBranchProfitability(filters: Omit<ProfitabilityFilters, 'branchId'>) {
  return useQuery({
    queryKey: profitabilityQueryKeys.branches(filters),
    queryFn: () => profitabilityApi.getBranches(filters),
    enabled: isValidFilter(filters),
    staleTime: 1000 * 60,
  })
}

export function useProfitabilityAlerts(filters: ProfitabilityFilters) {
  return useQuery({
    queryKey: profitabilityQueryKeys.alerts(filters),
    queryFn: () => profitabilityApi.getAlerts(filters),
    enabled: isValidFilter(filters),
    staleTime: 1000 * 60,
  })
}
