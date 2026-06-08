import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  blockCustomerCredit,
  createCustomer,
  deactivateCustomer,
  getCustomer,
  getCustomerCredit,
  getCustomerCreditMovements,
  getCustomers,
  registerCustomerPayment,
  unblockCustomerCredit,
  updateCustomer,
} from '@/modules/customers/services/customersApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'token-1' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('customersApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'result-1' } })
  })

  it('builds customer list query params and authenticated GET requests', async () => {
    await getCustomers({
      isActive: 'true',
      page: 2,
      pageSize: 25,
      query: ' maria ',
      sortBy: 'fullName',
      sortDirection: 'desc',
    })
    await getCustomer('customer-1')
    await getCustomerCredit('customer-1')
    await getCustomerCreditMovements('customer-1')

    expect(httpClient).toHaveBeenNthCalledWith(
      1,
      '/api/customers?page=2&pageSize=25&sortBy=fullName&sortDirection=desc&query=maria&isActive=true',
      { accessToken: 'token-1' },
    )
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/customers/customer-1', { accessToken: 'token-1' })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/customers/customer-1/credit', { accessToken: 'token-1' })
    expect(httpClient).toHaveBeenNthCalledWith(
      4,
      '/api/customers/customer-1/credit/movements?page=1&pageSize=50',
      { accessToken: 'token-1' },
    )
  })

  it('sends create, update, deactivate and credit mutation payloads', async () => {
    await createCustomer({ email: 'ana@test.com', firstName: 'Ana', lastName: 'Cliente', phone: '8090000000' })
    await updateCustomer('customer-1', { firstName: 'Ana', lastName: 'Editada', isActive: false, phone: null })
    await deactivateCustomer('customer-1')
    await registerCustomerPayment({ amount: 125, customerId: 'customer-1', note: ' pago ' })
    await blockCustomerCredit('customer-1')
    await unblockCustomerCredit('customer-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/customers', expect.objectContaining({
      accessToken: 'token-1',
      method: 'POST',
    }))
    expect(JSON.parse(vi.mocked(httpClient).mock.calls[0][1]!.body as string)).toEqual({
      email: 'ana@test.com',
      firstName: 'Ana',
      lastName: 'Cliente',
      phone: '8090000000',
    })
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/customers/customer-1', expect.objectContaining({
      method: 'PUT',
    }))
    expect(JSON.parse(vi.mocked(httpClient).mock.calls[1][1]!.body as string)).toEqual({
      firstName: 'Ana',
      lastName: 'Editada',
      isActive: false,
      phone: null,
    })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/customers/customer-1/deactivate', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/customers/customer-1/payments', expect.objectContaining({
      method: 'POST',
    }))
    expect(JSON.parse(vi.mocked(httpClient).mock.calls[3][1]!.body as string)).toEqual({ amount: 125, note: 'pago' })
    expect(httpClient).toHaveBeenNthCalledWith(5, '/api/customers/customer-1/credit/block', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(6, '/api/customers/customer-1/credit/unblock', expect.objectContaining({ method: 'POST' }))
  })
})
