import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  changeMyPassword,
  getBillingSettings,
  getBusinessSettings,
  getCurrentBranch,
  getCurrentBusiness,
  getInventorySettings,
  getMe,
  getSalesSettings,
  updateBillingSettings,
  updateBusinessSettings,
  updateCurrentBranch,
  updateCurrentBusiness,
  updateInventorySettings,
  updateMyProfile,
  updateSalesSettings,
} from '@/modules/settings/services/settingsService'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: {
    getState: () => ({ session: { accessToken: 'settings-token' } }),
  },
}))

vi.mock('@/shared/services/httpClient', () => ({
  httpClient: vi.fn(),
}))

describe('settingsService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'settings-result' } })
  })

  it('loads every authenticated settings resource', async () => {
    await getMe()
    await getCurrentBusiness()
    await getCurrentBranch()
    await getBusinessSettings()
    await getSalesSettings()
    await getInventorySettings()
    await getBillingSettings()

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/me', { accessToken: 'settings-token' })
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/business/current', { accessToken: 'settings-token' })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/branches/current', { accessToken: 'settings-token' })
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/settings/business', { accessToken: 'settings-token' })
    expect(httpClient).toHaveBeenNthCalledWith(5, '/api/settings/sales', { accessToken: 'settings-token' })
    expect(httpClient).toHaveBeenNthCalledWith(6, '/api/settings/inventory', { accessToken: 'settings-token' })
    expect(httpClient).toHaveBeenNthCalledWith(7, '/api/settings/billing', { accessToken: 'settings-token' })
  })

  it('sends update payloads with PUT and the current access token', async () => {
    await updateMyProfile({ fullName: 'Ada Lovelace' })
    await changeMyPassword({ currentPassword: 'old', newPassword: 'new-Password1!' })
    await updateCurrentBusiness({ name: 'Commerce Flow' })
    await updateCurrentBranch({ address: 'Main St', name: 'Principal' })
    await updateBusinessSettings({ defaultCurrency: 'DOP', timezone: 'America/Santo_Domingo' })
    await updateSalesSettings({ allowNegativeStock: false, defaultPaymentMethod: 'Cash' })
    await updateInventorySettings({ lowStockThreshold: 5 })
    await updateBillingSettings({ invoicePrefix: 'INV', taxRate: 18 })

    const expectedUrls = [
      '/api/me/profile',
      '/api/me/password',
      '/api/business/current',
      '/api/branches/current',
      '/api/settings/business',
      '/api/settings/sales',
      '/api/settings/inventory',
      '/api/settings/billing',
    ]

    expectedUrls.forEach((url, index) => {
      expect(httpClient).toHaveBeenNthCalledWith(index + 1, url, expect.objectContaining({
        accessToken: 'settings-token',
        method: 'PUT',
      }))
      expect(vi.mocked(httpClient).mock.calls[index][1]?.body).toEqual(expect.any(String))
    })
  })
})
