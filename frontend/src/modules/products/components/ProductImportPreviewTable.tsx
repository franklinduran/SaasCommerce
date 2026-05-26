import type { ImportProductsResponse } from '@/modules/products/services/productImportApi'

type Props = {
  result: ImportProductsResponse
}

export function ProductImportPreviewTable({ result }: Readonly<Props>) {
  return (
    <div className="space-y-4">
      {/* Summary cards */}
      <div className="grid grid-cols-3 gap-4">
        <div className="rounded-lg border border-green-200 bg-green-50 p-4 text-center">
          <p className="text-2xl font-bold text-green-700">{result.importedCount}</p>
          <p className="text-sm text-green-600">Importados</p>
        </div>
        <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-4 text-center">
          <p className="text-2xl font-bold text-yellow-700">{result.skippedCount}</p>
          <p className="text-sm text-yellow-600">Omitidos</p>
        </div>
        <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 text-center">
          <p className="text-2xl font-bold text-gray-700">{result.totalRows}</p>
          <p className="text-sm text-gray-600">Total filas</p>
        </div>
      </div>

      {/* Errors table */}
      {result.errors.length > 0 && (
        <div>
          <h3 className="mb-2 text-sm font-semibold text-gray-700">
            Filas con errores ({result.errors.length})
          </h3>
          <div className="overflow-hidden rounded-lg border border-red-200">
            <table className="min-w-full text-sm">
              <thead className="bg-red-50">
                <tr>
                  <th className="px-3 py-2 text-left font-medium text-red-700">Fila</th>
                  <th className="px-3 py-2 text-left font-medium text-red-700">Nombre</th>
                  <th className="px-3 py-2 text-left font-medium text-red-700">Errores</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-red-100 bg-white">
                {result.errors.map((err) => (
                  <tr key={err.rowNumber}>
                    <td className="px-3 py-2 text-gray-500">{err.rowNumber}</td>
                    <td className="px-3 py-2 text-gray-800">{err.name || '—'}</td>
                    <td className="px-3 py-2 text-red-600">
                      {err.messages.join(', ')}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}
