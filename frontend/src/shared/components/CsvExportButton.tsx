import { Download } from 'lucide-react'
import { useState } from 'react'
import { useAuthStore } from '@/modules/auth/authStore'
import { buildApiUrl } from '@/shared/services/apiConfig'

interface CsvExportButtonProps {
  endpoint: string
  filename: string
  label?: string
  className?: string
  queryParams?: Record<string, string | undefined>
}

export function CsvExportButton({
  endpoint,
  filename,
  label = 'Exportar CSV',
  className = '',
  queryParams,
}: Readonly<CsvExportButtonProps>) {
  const [isExporting, setIsExporting] = useState(false)
  const accessToken = useAuthStore.getState().session?.accessToken

  const handleExport = async () => {
    setIsExporting(true)
    try {
      let url = buildApiUrl(endpoint)

      if (queryParams) {
        const params = new URLSearchParams()
        for (const [key, value] of Object.entries(queryParams)) {
          if (value !== undefined) {
            params.set(key, value)
          }
        }
        const qs = params.toString()
        if (qs) url += `?${qs}`
      }

      const response = await fetch(url, {
        headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      })

      if (!response.ok) return

      const blob = await response.blob()
      const objectUrl = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = objectUrl
      a.download = filename
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(objectUrl)
    } finally {
      setIsExporting(false)
    }
  }

  return (
    <button
      type="button"
      onClick={handleExport}
      disabled={isExporting}
      className={`inline-flex items-center gap-1.5 rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 disabled:opacity-50 ${className}`}
    >
      <Download size={14} />
      {isExporting ? 'Exportando...' : label}
    </button>
  )
}
