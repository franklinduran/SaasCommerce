import { Eye, RefreshCw } from 'lucide-react'
import { Link } from 'react-router-dom'
import { SaleStatusBadge } from '@/modules/sales/components/SaleStatusBadge'
import type { SaleListItem } from '@/modules/sales/types/salesTypes'
import { formatCurrency, formatDateTime } from '@/modules/sales/utils/formatSales'
import { Button } from '@/shared/components/ui/button'

type SalesTableProps = {
  emptyMessage: string
  isError: boolean
  isLoading: boolean
  onRetry: () => void
  sales: SaleListItem[]
}

const skeletonRowIds = ['sale-row-1', 'sale-row-2', 'sale-row-3', 'sale-row-4', 'sale-row-5']

export function SalesTable({
  emptyMessage,
  isError,
  isLoading,
  onRetry,
  sales,
}: Readonly<SalesTableProps>) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-220 text-left text-sm">
        <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-5 py-3">Fecha</th>
            <th className="px-5 py-3">Codigo</th>
            <th className="px-5 py-3">Cliente</th>
            <th className="px-5 py-3">Estado</th>
            <th className="px-5 py-3">Metodo</th>
            <th className="px-5 py-3 text-right">Total</th>
            <th className="px-5 py-3 text-right">Acciones</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200">
          {isLoading && <SkeletonRows />}
          {isError && (
            <tr>
              <td className="px-5 py-10 text-center" colSpan={7}>
                <div className="mx-auto max-w-sm space-y-3">
                  <p className="text-sm font-semibold text-red-700">
                    No se pudo cargar el historial de ventas.
                  </p>
                  <Button onClick={onRetry} variant="secondary">
                    <RefreshCw size={16} />
                    Reintentar
                  </Button>
                </div>
              </td>
            </tr>
          )}
          {!isLoading && !isError && sales.length === 0 && (
            <tr>
              <td className="px-5 py-12 text-center" colSpan={7}>
                <p className="text-sm font-semibold text-stone-700">{emptyMessage}</p>
              </td>
            </tr>
          )}
          {sales.map((sale) => (
            <tr className="bg-white hover:bg-stone-50" key={sale.id}>
              <td className="px-5 py-4 font-semibold text-stone-800">
                {formatDateTime(sale.createdAt)}
              </td>
              <td className="px-5 py-4 font-mono text-xs font-semibold text-stone-700">
                {sale.code}
              </td>
              <td className="px-5 py-4 font-semibold text-stone-900">
                {sale.customerName ?? 'Consumidor final'}
              </td>
              <td className="px-5 py-4">
                <SaleStatusBadge status={sale.status} />
              </td>
              <td className="px-5 py-4 font-semibold text-stone-700">{sale.paymentMethod}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-950">
                {formatCurrency(sale.total)}
              </td>
              <td className="px-5 py-4 text-right">
                <Link
                  className="inline-flex h-8 items-center justify-center gap-2 rounded-md bg-white px-3 text-xs font-semibold text-stone-900 shadow-sm ring-1 ring-stone-300 transition-colors hover:bg-stone-50"
                  to={`/sales/${sale.id}`}
                >
                  <Eye size={14} />
                  Ver detalle
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function SkeletonRows() {
  return skeletonRowIds.map((id) => (
    <tr key={id}>
      <td className="px-5 py-4" colSpan={7}>
        <div className="h-4 w-full rounded bg-stone-100" />
      </td>
    </tr>
  ))
}
