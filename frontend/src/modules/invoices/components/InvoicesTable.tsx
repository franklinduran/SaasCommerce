import { Link } from 'react-router-dom'
import { InvoiceStatusBadge } from '@/modules/invoices/components/InvoiceStatusBadge'
import type { Invoice } from '@/modules/invoices/types'
import { formatInvoiceMoney } from '@/modules/invoices/utils/formatInvoiceMoney'

type InvoicesTableProps = {
  invoices: Invoice[]
  isError: boolean
  isLoading: boolean
}

export function InvoicesTable({ invoices, isError, isLoading }: Readonly<InvoicesTableProps>) {
  if (isLoading) {
    return (
      <div className="space-y-3 p-5">
        {['invoice-skeleton-1', 'invoice-skeleton-2', 'invoice-skeleton-3'].map((id) => (
          <div className="h-14 rounded-md bg-stone-100" key={id} />
        ))}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="p-8 text-center">
        <p className="text-sm font-semibold text-red-700">No se pudieron cargar los recibos.</p>
      </div>
    )
  }

  if (invoices.length === 0) {
    return (
      <div className="p-8 text-center">
        <p className="text-sm font-semibold text-stone-800">No hay recibos emitidos.</p>
      </div>
    )
  }

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-stone-200 text-sm">
        <thead className="bg-stone-50 text-left text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-5 py-3">Recibo</th>
            <th className="px-5 py-3">Venta</th>
            <th className="px-5 py-3">Estado</th>
            <th className="px-5 py-3 text-right">Total</th>
            <th className="px-5 py-3">Fecha</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-100 bg-white">
          {invoices.map((invoice) => (
            <tr className="hover:bg-stone-50" key={invoice.invoiceId}>
              <td className="px-5 py-4">
                <Link
                  className="font-semibold text-stone-950 hover:text-stone-700"
                  to={`/invoices/${invoice.invoiceId}`}
                >
                  {invoice.invoiceNumber}
                </Link>
              </td>
              <td className="px-5 py-4 font-medium text-stone-600">
                {invoice.saleId.slice(0, 8).toUpperCase()}
              </td>
              <td className="px-5 py-4">
                <InvoiceStatusBadge status={invoice.status} />
              </td>
              <td className="px-5 py-4 text-right font-semibold text-stone-950">
                {formatInvoiceMoney(invoice.total)}
              </td>
              <td className="px-5 py-4 font-medium text-stone-600">
                {new Date(invoice.createdAt).toLocaleString('es-DO')}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
