import { useAuthStore } from '@/modules/auth/authStore'
import { customerSchema, registerCustomerPaymentSchema } from '@/modules/customers/schemas/customerSchemas'
import type {
  Customer,
  CustomerCreditMovementListResponse,
  CustomerCreditSummary,
  CustomerFilters,
  CustomerListResponse,
  CustomerUpsertRequest,
  RegisterCustomerPaymentRequest,
  RegisterCustomerPaymentResponse,
} from '@/modules/customers/types'
import { httpClient } from '@/shared/services/httpClient'

export async function getCustomers(filters: CustomerFilters): Promise<CustomerListResponse> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
    sortBy: filters.sortBy,
    sortDirection: filters.sortDirection,
  })

  if (filters.query.trim()) {
    params.set('query', filters.query.trim())
  }

  if (filters.isActive) {
    params.set('isActive', filters.isActive)
  }

  const response = await httpClient<CustomerListResponse>(`/api/customers?${params.toString()}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getCustomer(customerId: string): Promise<Customer> {
  const response = await httpClient<Customer>(`/api/customers/${customerId}`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function createCustomer(request: CustomerUpsertRequest): Promise<Customer> {
  const payload = customerSchema.parse(request)
  const response = await httpClient<Customer>('/api/customers', {
    accessToken: getAccessToken(),
    body: JSON.stringify(payload),
    method: 'POST',
  })

  return response.data!
}

export async function updateCustomer(
  customerId: string,
  request: CustomerUpsertRequest,
): Promise<Customer> {
  const payload = customerSchema.parse(request)
  const response = await httpClient<Customer>(`/api/customers/${customerId}`, {
    accessToken: getAccessToken(),
    body: JSON.stringify({ ...payload, isActive: request.isActive ?? true }),
    method: 'PUT',
  })

  return response.data!
}

export async function deactivateCustomer(customerId: string): Promise<Customer> {
  const response = await httpClient<Customer>(`/api/customers/${customerId}/deactivate`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })

  return response.data!
}

export async function getCustomerCredit(customerId: string): Promise<CustomerCreditSummary> {
  const response = await httpClient<CustomerCreditSummary>(`/api/customers/${customerId}/credit`, {
    accessToken: getAccessToken(),
  })

  return response.data!
}

export async function getCustomerCreditMovements(
  customerId: string,
): Promise<CustomerCreditMovementListResponse> {
  const response = await httpClient<CustomerCreditMovementListResponse>(
    `/api/customers/${customerId}/credit/movements?page=1&pageSize=50`,
    { accessToken: getAccessToken() },
  )

  return response.data!
}

export async function registerCustomerPayment(
  request: RegisterCustomerPaymentRequest,
): Promise<RegisterCustomerPaymentResponse> {
  const payload = registerCustomerPaymentSchema.parse({
    amount: request.amount,
    note: request.note,
  })
  const response = await httpClient<RegisterCustomerPaymentResponse>(
    `/api/customers/${request.customerId}/payments`,
    {
      accessToken: getAccessToken(),
      body: JSON.stringify(payload),
      method: 'POST',
    },
  )

  return response.data!
}

export async function blockCustomerCredit(customerId: string): Promise<CustomerCreditSummary> {
  const response = await httpClient<CustomerCreditSummary>(`/api/customers/${customerId}/credit/block`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })

  return response.data!
}

export async function unblockCustomerCredit(customerId: string): Promise<CustomerCreditSummary> {
  const response = await httpClient<CustomerCreditSummary>(`/api/customers/${customerId}/credit/unblock`, {
    accessToken: getAccessToken(),
    method: 'POST',
  })

  return response.data!
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}
