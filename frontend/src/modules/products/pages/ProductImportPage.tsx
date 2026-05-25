import { useRef } from 'react'
import { Download, Upload } from 'lucide-react'
import { useProductImport } from '@/modules/products/hooks/useProductImport'
import { ProductImportPreviewTable } from '@/modules/products/components/ProductImportPreviewTable'

export function ProductImportPage() {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const { importCsv, downloadTemplate, isImporting, isDownloadingTemplate, result, errorMessage, reset } =
    useProductImport()

  async function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]

    if (!file) return

    reset()

    try {
      await importCsv(file)
    } catch {
      // error captured in errorMessage
    }

    // Reset input so the same file can be re-uploaded
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6 py-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Importar productos</h1>
        <p className="mt-1 text-gray-600">
          Carga masiva de productos desde un archivo CSV. Máximo 500 productos por archivo.
        </p>
      </div>

      {/* Template download */}
      <div className="rounded-lg border border-blue-200 bg-blue-50 p-4">
        <h2 className="mb-2 font-semibold text-blue-800">1. Descarga la plantilla</h2>
        <p className="mb-3 text-sm text-blue-700">
          Completa la plantilla CSV con tus productos y luego súbela aquí.
        </p>
        <button
          className="inline-flex items-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-60"
          disabled={isDownloadingTemplate}
          onClick={() => downloadTemplate()}
          type="button"
        >
          <Download className="h-4 w-4" />
          {isDownloadingTemplate ? 'Descargando…' : 'Descargar plantilla CSV'}
        </button>
      </div>

      {/* File upload */}
      <div className="rounded-lg border-2 border-dashed border-gray-300 bg-white p-6 text-center">
        <h2 className="mb-2 font-semibold text-gray-800">2. Sube tu archivo CSV</h2>
        <Upload className="mx-auto mb-3 h-10 w-10 text-gray-400" />
        <p className="mb-4 text-sm text-gray-500">
          Formato: CSV con columnas Name, Sku, CategoryName, SalePrice, CostPrice, StockQuantity
        </p>
        <input
          accept=".csv,text/csv"
          className="hidden"
          id="csvFile"
          onChange={handleFileChange}
          ref={fileInputRef}
          type="file"
        />
        <label
          className={`inline-flex cursor-pointer items-center gap-2 rounded-md px-4 py-2 text-sm font-semibold text-white ${
            isImporting
              ? 'bg-gray-400 cursor-not-allowed'
              : 'bg-gray-800 hover:bg-gray-900'
          }`}
          htmlFor={isImporting ? undefined : 'csvFile'}
        >
          <Upload className="h-4 w-4" />
          {isImporting ? 'Importando…' : 'Seleccionar archivo'}
        </label>
      </div>

      {/* Error */}
      {errorMessage && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">
          <strong>Error:</strong> {errorMessage}
        </div>
      )}

      {/* Results */}
      {result && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h2 className="font-semibold text-gray-800">Resultados de la importación</h2>
            <button
              className="text-sm text-blue-600 hover:underline"
              onClick={reset}
              type="button"
            >
              Limpiar
            </button>
          </div>
          <ProductImportPreviewTable result={result} />
          {result.importedCount > 0 && (
            <p className="text-sm text-green-700">
              ✅ {result.importedCount} producto(s) importado(s) exitosamente.{' '}
              <a className="text-blue-600 hover:underline" href="/products">
                Ver catálogo
              </a>
            </p>
          )}
        </div>
      )}
    </div>
  )
}
