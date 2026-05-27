import { useAuthStore } from '@/modules/auth/authStore'
import { buildApiUrl } from '@/shared/services/apiConfig'

export type ImportRowError = {
  rowNumber: number
  name: string
  messages: string[]
}

export type ImportProductsResponse = {
  importedCount: number
  skippedCount: number
  totalRows: number
  errors: ImportRowError[]
}

function getAccessToken(): string | undefined {
  return useAuthStore.getState().session?.accessToken
}

export const productImportApi = {
  async downloadTemplate(): Promise<Blob> {
    const accessToken = getAccessToken()
    const res = await fetch(buildApiUrl('/api/products/import/template'), {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
    })

    if (!res.ok) throw new Error('No se pudo descargar la plantilla.')

    return res.blob()
  },

  async importCsv(file: File): Promise<ImportProductsResponse> {
    const accessToken = getAccessToken()
    const form = new FormData()
    form.append('file', file)

    const res = await fetch(buildApiUrl('/api/products/import'), {
      body: form,
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      method: 'POST',
    })

    const json = await res.json()

    if (!res.ok) {
      const message = json?.error?.message ?? 'Error al importar productos.'
      throw new Error(message)
    }

    return json.data as ImportProductsResponse
  },

  getTemplateCsvUrl(): string {
    const accessToken = getAccessToken()
    const tokenQuery = accessToken ? `?token=${accessToken}` : ''

    return buildApiUrl(`/api/products/import/template${tokenQuery}`)
  },
}

export async function getImportTemplate(): Promise<void> {
  const blob = await productImportApi.downloadTemplate()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'plantilla-productos.csv'
  a.click()
  URL.revokeObjectURL(url)
}
