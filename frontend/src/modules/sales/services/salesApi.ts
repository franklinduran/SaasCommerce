import { useAuthStore } from '@/modules/auth/authStore'
import type {
  CreateSaleReturnInput,
  SaleDetail,
  SaleDetailItem,
  SaleListItem,
  SaleListResponse,
  SaleReturn,
  SalesFilters,
  SaleStatus,
} from '@/modules/sales/types/salesTypes'
import { httpClient } from '@/shared/services/httpClient'

type ApiSaleListResponse = Omit<SaleListResponse, 'items'> & {
  items: ApiSale[]
}

type ApiSale = {
  id?: string
  saleId: string
  code?: string
  customerName?: string | null
  branchName?: string | null
  status: SaleStatus
  paymentMethod: string
  total: number
  failureReason?: string | null
  cancellationReason?: string | null
  createdAt: string
  items?: ApiSaleItem[]
}

type ApiSaleItem = {
  saleItemId?: string
  id?: string
  productId: string
  productName?: string | null
  sku?: string | null
  quantity: number
  unitPrice: number
  subtotal?: number
  lineTotal?: number
}

type ApiSaleReturn = SaleReturn

export async function getSales(filters: SalesFilters): Promise<SaleListResponse> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
    sortBy: 'createdAt',
    sortDirection: 'desc',
  })

  if (filters.status) {
    params.set('status', filters.status)
  }

  if (filters.paymentMethod) {
    params.set('paymentMethod', filters.paymentMethod)
  }

  if (filters.query.trim().length > 0) {
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

  const response = await httpClient<ApiSaleListResponse>(`/api/sales?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return {
    ...response.data!,
    items: response.data!.items.map(mapSaleListItem),
  }
}

export async function getSaleDetail(saleId: string): Promise<SaleDetail> {
  const response = await httpClient<ApiSale>(`/api/sales/${saleId}`, {
    accessToken: getAccessToken(),
  })

  return mapSaleDetail(response.data!)
}

export async function getSaleReturns(saleId: string): Promise<SaleReturn[]> {
  const response = await httpClient<ApiSaleReturn[]>(`/api/sales/${saleId}/returns`, {
    accessToken: getAccessToken(),
  })

  return response.data ?? []
}

export async function getSaleReturn(saleReturnId: string): Promise<SaleReturn> {
  const response = await httpClient<ApiSaleReturn>(`/api/sale-returns/${saleReturnId}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function createSaleReturn(
  saleId: string,
  input: CreateSaleReturnInput,
): Promise<SaleReturn> {
  const response = await httpClient<ApiSaleReturn>(`/api/sales/${saleId}/returns`, {
    accessToken: getAccessToken(),
    body: JSON.stringify(input),
    method: 'POST',
  })

  return response.data!
}

function mapSaleListItem(sale: ApiSale): SaleListItem {
  const id = sale.id ?? sale.saleId

  return {
    id,
    code: sale.code ?? toShortSaleCode(id),
    customerName: sale.customerName ?? null,
    status: sale.status,
    paymentMethod: sale.paymentMethod,
    total: sale.total,
    createdAt: sale.createdAt,
  }
}

function mapSaleDetail(sale: ApiSale): SaleDetail {
  return {
    ...mapSaleListItem(sale),
    branchName: sale.branchName ?? null,
    failureReason: sale.failureReason ?? null,
    cancellationReason: sale.cancellationReason ?? null,
    items: (sale.items ?? []).map(mapSaleItem),
  }
}

function mapSaleItem(item: ApiSaleItem): SaleDetailItem {
  return {
    saleItemId: item.saleItemId ?? item.id ?? item.productId,
    productId: item.productId,
    productName: item.productName ?? 'Producto no disponible',
    sku: item.sku ?? null,
    quantity: item.quantity,
    unitPrice: item.unitPrice,
    subtotal: item.subtotal ?? item.lineTotal ?? 0,
  }
}

function toDateFrom(value: string): string | null {
  return value ? new Date(`${value}T00:00:00`).toISOString() : null
}

function toDateTo(value: string): string | null {
  return value ? new Date(`${value}T23:59:59.999`).toISOString() : null
}

function toShortSaleCode(saleId: string): string {
  return saleId.replaceAll('-', '').slice(0, 8).toUpperCase()
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
