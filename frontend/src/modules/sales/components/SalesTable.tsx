import { Eye, RefreshCw } from 'lucide-react'
import { Link } from 'react-router-dom'
import { SaleStatusBadge } from '@/modules/sales/components/SaleStatusBadge'
import type { SaleListItem } from '@/modules/sales/types/salesTypes'
import { formatCurrency, formatDateTime, formatPaymentMethod } from '@/modules/sales/utils/formatSales'

type SalesTableProps = {
  emptyMessage: string
  isError: boolean
  isLoading: boolean
  onRetry: () => void
  sales: SaleListItem[]
}

const columns = ['Fecha', 'Código', 'Cliente', 'Estado', 'Método', 'Total', '']

// Staggered skeleton widths for a more natural loading feel
const skeletons = [
  ['w-32', 'w-20', 'w-40', 'w-20', 'w-16', 'w-20', 'w-20'],
  ['w-28', 'w-20', 'w-56', 'w-20', 'w-14', 'w-16', 'w-20'],
  ['w-32', 'w-20', 'w-36', 'w-20', 'w-16', 'w-24', 'w-20'],
  ['w-28', 'w-20', 'w-48', 'w-20', 'w-16', 'w-16', 'w-20'],
  ['w-32', 'w-20', 'w-32', 'w-20', 'w-14', 'w-20', 'w-20'],
]

export function SalesTable({
  emptyMessage,
  isError,
  isLoading,
  onRetry,
  sales,
}: Readonly<SalesTableProps>) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[680px] text-left">

        <thead>
          <tr className="border-b border-gray-100 bg-gray-50/70">
            {columns.map((col, i) => (
              <th
                key={`${col}-${i}`}
                className="px-5 py-2.5 text-[11px] font-semibold uppercase tracking-[0.1em] text-gray-400 last:text-right"
              >
                {col}
              </th>
            ))}
          </tr>
        </thead>

        <tbody className="divide-y divide-gray-100">

          {/* Loading skeleton */}
          {isLoading && skeletons.map((widths, i) => (
            <tr key={`skeleton-${i}`} className="animate-pulse">
              {widths.map((w, j) => (
                <td key={j} className="px-5 py-3.5">
                  <div className={`h-3 rounded-md bg-gray-100 ${j === widths.length - 1 ? 'ml-auto' : ''} ${w}`} />
                </td>
              ))}
            </tr>
          ))}

          {/* Error */}
          {isError && (
            <tr>
              <td className="px-5 py-14 text-center" colSpan={7}>
                <p className="text-[13px] font-semibold text-red-500">
                  No se pudo cargar el historial de ventas.
                </p>
                <button
                  className="mx-auto mt-3 flex items-center gap-1.5 rounded-lg border border-gray-200 px-3 py-1.5 text-[12px] font-medium text-gray-600 transition hover:border-gray-300 hover:text-gray-900"
                  onClick={onRetry}
                  type="button"
                >
                  <RefreshCw size={12} />
                  Reintentar
                </button>
              </td>
            </tr>
          )}

          {/* Empty */}
          {!isLoading && !isError && sales.length === 0 && (
            <tr>
              <td className="px-5 py-16 text-center" colSpan={7}>
                <p className="text-[13px] font-medium text-gray-400">{emptyMessage}</p>
              </td>
            </tr>
          )}

          {/* Rows */}
          {sales.map((sale) => (
            <tr className="bg-white transition-colors hover:bg-gray-50/60" key={sale.id}>
              <td className="whitespace-nowrap px-5 py-3 text-[12.5px] font-medium text-gray-500">
                {formatDateTime(sale.createdAt)}
              </td>
              <td className="px-5 py-3 font-mono text-[11px] font-semibold tracking-widest text-gray-400">
                {sale.code}
              </td>
              <td className="px-5 py-3 text-[13px] font-semibold text-gray-900">
                {sale.customerName ?? (
                  <span className="font-normal italic text-gray-400">Consumidor final</span>
                )}
              </td>
              <td className="px-5 py-3">
                <SaleStatusBadge status={sale.status} />
              </td>
              <td className="px-5 py-3 text-[12.5px] font-medium text-gray-500">
                {formatPaymentMethod(sale.paymentMethod)}
              </td>
              <td className="px-5 py-3 text-right text-[13px] font-bold tabular-nums text-gray-900">
                {formatCurrency(sale.total)}
              </td>
              <td className="px-5 py-3 text-right">
                <Link
                  className="inline-flex h-7 items-center gap-1.5 rounded-md border border-gray-200 px-2.5 text-[11.5px] font-medium text-gray-600 transition hover:border-gray-300 hover:text-gray-900"
                  to={`/sales/${sale.id}`}
                >
                  <Eye size={12} />
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
