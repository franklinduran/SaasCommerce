import type { SaleDetailItem } from '@/modules/sales/types/salesTypes'
import {
  formatCurrency,
  formatQuantity,
} from '@/modules/sales/utils/formatSales'

export function SaleItemsTable({ items }: Readonly<{ items: SaleDetailItem[] }>) {
  return (
    <div className="overflow-x-auto rounded-md bg-white shadow-sm ring-1 ring-stone-200">
      <table className="w-full min-w-180 text-left text-sm">
        <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-5 py-3">Producto</th>
            <th className="px-5 py-3">SKU</th>
            <th className="px-5 py-3 text-right">Cantidad</th>
            <th className="px-5 py-3 text-right">Precio</th>
            <th className="px-5 py-3 text-right">Subtotal</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200">
          {items.map((item) => (
            <tr className="bg-white" key={`${item.productId}-${item.productName}`}>
              <td className="px-5 py-4 font-semibold text-stone-950">{item.productName}</td>
              <td className="px-5 py-4 font-mono text-xs font-semibold text-stone-600">
                {item.sku ?? 'Sin SKU'}
              </td>
              <td className="px-5 py-4 text-right font-semibold text-stone-800">
                {formatQuantity(item.quantity)}
              </td>
              <td className="px-5 py-4 text-right font-semibold text-stone-800">
                {formatCurrency(item.unitPrice)}
              </td>
              <td className="px-5 py-4 text-right font-semibold text-stone-950">
                {formatCurrency(item.subtotal)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
