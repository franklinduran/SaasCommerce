import { ArrowLeft, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useProductProfitability } from '../hooks/useProfitability'
import type { ProfitabilityFilters } from '../types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function defaultFilters(): ProfitabilityFilters {
  const now = new Date()
  const from = new Date(now.getFullYear(), now.getMonth(), 1)
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  return {
    from: from.toISOString().slice(0, 10) + 'T00:00:00.000Z',
    to: to.toISOString().slice(0, 10) + 'T23:59:59.999Z',
  }
}

function marginClass(marginPercent: number, hasMissingCost: boolean) {
  if (hasMissingCost) return 'text-stone-400'
  if (marginPercent < 0) return 'text-red-600 font-semibold'
  if (marginPercent < 10) return 'text-amber-600 font-semibold'
  return 'text-green-700 font-semibold'
}

function productSummaryText(count: number) {
  if (count === 0) return 'Márgenes y ganancias por producto.'

  const suffix = count === 1 ? '' : 's'
  return `${count} producto${suffix} con ventas en el período`
}

export function ProductProfitabilityPage() {
  const navigate = useNavigate()
  const [filters] = useState<ProfitabilityFilters>(defaultFilters)

  const { data: products = [], isLoading, isError } = useProductProfitability(filters)

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/profitability')}>
          <ArrowLeft size={18} />
        </Button>
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Rentabilidad por producto</h2>
          <p className="text-sm text-stone-500">{productSummaryText(products.length)}</p>
        </div>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      )}

      {/* Error */}
      {isError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Error al cargar rentabilidad de productos. Intenta nuevamente.
        </p>
      )}

      {/* Empty state */}
      {!isLoading && !isError && products.length === 0 && (
        <Card>
          <CardContent className="flex h-40 flex-col items-center justify-center gap-2">
            <p className="text-sm text-stone-500">No hay ventas completadas en el período seleccionado.</p>
          </CardContent>
        </Card>
      )}

      {/* Products table */}
      {products.length > 0 && (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-stone-100 bg-stone-50 text-left text-xs font-semibold uppercase tracking-wider text-stone-500">
                    <th className="px-4 py-3">Producto</th>
                    <th className="px-4 py-3 text-right">Cantidad</th>
                    <th className="px-4 py-3 text-right">Ventas</th>
                    <th className="px-4 py-3 text-right">Costo</th>
                    <th className="px-4 py-3 text-right">Ganancia</th>
                    <th className="px-4 py-3 text-right">Margen</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-stone-100">
                  {products.map((product) => (
                    <tr key={product.productId} className="hover:bg-stone-50">
                      <td className="px-4 py-3">
                        <div>
                          <p className="font-medium text-stone-900">{product.productName}</p>
                          <p className="text-xs text-stone-400">{product.sku}</p>
                          {product.categoryName && (
                            <p className="text-xs text-stone-400">{product.categoryName}</p>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-right text-stone-700">{product.totalQuantity}</td>
                      <td className="px-4 py-3 text-right font-medium text-stone-900">
                        {formatCurrency(product.totalSales)}
                      </td>
                      <td className="px-4 py-3 text-right text-stone-600">
                        {product.hasMissingCost ? (
                          <span className="text-amber-500">Sin costo</span>
                        ) : (
                          formatCurrency(product.totalCost)
                        )}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <span className={marginClass(product.marginPercent, product.hasMissingCost)}>
                          {product.hasMissingCost ? '—' : formatCurrency(product.grossProfit)}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right">
                        <span className={marginClass(product.marginPercent, product.hasMissingCost)}>
                          {product.hasMissingCost ? '—' : `${product.marginPercent.toFixed(1)}%`}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
