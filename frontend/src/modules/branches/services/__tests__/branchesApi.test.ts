import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  activateBranch,
  createBranch,
  deactivateBranch,
  getBranchById,
  getBranches,
  updateBranch,
} from '@/modules/branches/services/branchesApi'
import { httpClient } from '@/shared/services/httpClient'

vi.mock('@/modules/auth/authStore', () => ({
  useAuthStore: { getState: () => ({ session: { accessToken: 'branch-token' } }) },
}))

vi.mock('@/shared/services/httpClient', () => ({ httpClient: vi.fn() }))

describe('branchesApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(httpClient).mockResolvedValue({ data: { id: 'branch-result' } })
  })

  it('builds branch filters and loads details', async () => {
    await getBranches({ isActive: true, page: 1, pageSize: 10, query: ' north ' } as any)
    await getBranches({ isActive: false, page: 1, pageSize: 10, query: '' } as any)
    await getBranchById('branch-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/branches?isActive=true&page=1&pageSize=10&query=+north+', { accessToken: 'branch-token' })
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/branches?page=1&pageSize=10', { accessToken: 'branch-token' })
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/branches/branch-1', { accessToken: 'branch-token' })
  })

  it('sends branch mutations', async () => {
    await createBranch({ address: 'Main', name: 'North' } as any)
    await updateBranch('branch-1', { address: 'Second', name: 'North 2' } as any)
    await activateBranch('branch-1')
    await deactivateBranch('branch-1')

    expect(httpClient).toHaveBeenNthCalledWith(1, '/api/branches', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(2, '/api/branches/branch-1', expect.objectContaining({ method: 'PUT' }))
    expect(httpClient).toHaveBeenNthCalledWith(3, '/api/branches/branch-1/activate', expect.objectContaining({ method: 'POST' }))
    expect(httpClient).toHaveBeenNthCalledWith(4, '/api/branches/branch-1/deactivate', expect.objectContaining({ method: 'POST' }))
  })
})
