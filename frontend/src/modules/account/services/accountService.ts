import type {
  RegisterBusinessRequest,
  RegisterBusinessResponse,
} from '@/modules/account/types'
import { httpClient } from '@/shared/services/httpClient'

export async function registerBusiness(
  request: RegisterBusinessRequest,
): Promise<RegisterBusinessResponse> {
  const response = await httpClient<RegisterBusinessResponse>(
    '/api/account/register-business',
    {
      body: JSON.stringify(request),
      method: 'POST',
    },
  )

  return response.data!
}
