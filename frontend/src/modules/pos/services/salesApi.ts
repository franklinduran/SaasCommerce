import { z } from 'zod'
import { useAuthStore } from '@/modules/auth/authStore'
import type {
  CreateSaleRequest,
  CurrentBranchForPOS,
  CustomerListResponse,
  POSProductListResponse,
  SaleResponse,
} from '@/modules/pos/types/posTypes'
import { httpClient } from '@/shared/services/httpClient'

export const createSaleSchema = z.object({
  branchId: z.guid(),
  customerId: z.guid().nullable().optional(),
  paymentMethod: z.enum(['Cash', 'Transfer', 'Card', 'Credit']),
  items: z
    .array(
      z.object({
        productId: z.guid(),
        quantity: z.number().int().positive(),
      }),
    )
    .min(1),
})

export async function getProductsForPOS(query: string): Promise<POSProductListResponse> {
  const params = new URLSearchParams({
    isActive: 'true',
    page: '1',
    pageSize: '25',
    sortBy: 'name',
    sortDirection: 'asc',
  })

  if (query.trim().length > 0) {
    params.set('query', query.trim())
  }

  const response = await httpClient<POSProductListResponse>(
    `/api/catalog/products?${params.toString()}`,
    { accessToken: getAccessToken() },
  )

  return response.data!
}

export async function getCustomersForPOS(query: string): Promise<CustomerListResponse> {
  const params = new URLSearchParams({
    isActive: 'true',
    page: '1',
    pageSize: '25',
    sortBy: 'fullName',
    sortDirection: 'asc',
  })

  if (query.trim().length > 0) {
    params.set('query', query.trim())
  }

  const response = await httpClient<CustomerListResponse>(`/api/customers?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function createSale(request: CreateSaleRequest): Promise<SaleResponse> {
  const payload = createSaleSchema.parse(request)
  const response = await httpClient<SaleResponse>('/api/sales', {
    accessToken: getAccessToken(),
    body: JSON.stringify(payload),
    method: 'POST',
  })

  return response.data!
}

export async function getSaleByIdForPOS(saleId: string): Promise<SaleResponse> {
  const response = await httpClient<SaleResponse>(`/api/sales/${saleId}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getCurrentBranchForPOS(): Promise<CurrentBranchForPOS> {
  const response = await httpClient<CurrentBranchForPOS>('/api/branches/current', {
    accessToken: getAccessToken(),
  })

  return response.data!
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
