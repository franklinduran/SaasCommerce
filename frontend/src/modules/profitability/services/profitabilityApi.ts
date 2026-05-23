import { useAuthStore } from '@/modules/auth/authStore'
import { httpClient } from '@/shared/services/httpClient'
import type {
  BranchProfitability,
  ProfitabilityAlert,
  ProfitabilityFilters,
  ProfitabilitySummary,
  ProductProfitability,
} from '../types'

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

function buildQuery(filters: ProfitabilityFilters, extra?: Record<string, string | undefined>) {
  const query = new URLSearchParams()
  query.set('from', filters.from)
  query.set('to', filters.to)
  if (filters.branchId) query.set('branchId', filters.branchId)
  if (extra) {
    for (const [key, value] of Object.entries(extra)) {
      if (value) query.set(key, value)
    }
  }
  return query.toString()
}

export const profitabilityApi = {
  async getSummary(filters: ProfitabilityFilters): Promise<ProfitabilitySummary> {
    const response = await httpClient<ProfitabilitySummary>(
      `/api/profitability/summary?${buildQuery(filters)}`,
      { accessToken: getAccessToken() },
    )
    return response.data!
  },

  async getProducts(
    filters: ProfitabilityFilters,
    categoryId?: string,
  ): Promise<ProductProfitability[]> {
    const response = await httpClient<ProductProfitability[]>(
      `/api/profitability/products?${buildQuery(filters, { categoryId })}`,
      { accessToken: getAccessToken() },
    )
    return response.data ?? []
  },

  async getBranches(filters: Omit<ProfitabilityFilters, 'branchId'>): Promise<BranchProfitability[]> {
    const query = new URLSearchParams()
    query.set('from', filters.from)
    query.set('to', filters.to)
    const response = await httpClient<BranchProfitability[]>(
      `/api/profitability/branches?${query.toString()}`,
      { accessToken: getAccessToken() },
    )
    return response.data ?? []
  },

  async getAlerts(filters: ProfitabilityFilters): Promise<ProfitabilityAlert[]> {
    const response = await httpClient<ProfitabilityAlert[]>(
      `/api/profitability/alerts?${buildQuery(filters)}`,
      { accessToken: getAccessToken() },
    )
    return response.data ?? []
  },
}
