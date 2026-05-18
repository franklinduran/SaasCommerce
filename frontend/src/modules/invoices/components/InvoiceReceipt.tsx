import { InvoiceStatusBadge } from '@/modules/invoices/components/InvoiceStatusBadge'
import type { Invoice } from '@/modules/invoices/types'
import { formatInvoiceMoney } from '@/modules/invoices/utils/formatInvoiceMoney'

export function InvoiceReceipt({
  businessName,
  invoice,
}: Readonly<{
  businessName: string
  invoice: Invoice
}>) {
  return (
    <article className="rounded-md bg-white p-6 shadow-sm ring-1 ring-stone-200 print:shadow-none print:ring-0">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-sm font-semibold uppercase text-stone-500">Recibo interno no fiscal</p>
          <h3 className="mt-1 text-xl font-semibold text-stone-950">{businessName}</h3>
          <p className="mt-1 text-sm font-medium text-stone-600">{invoice.invoiceNumber}</p>
        </div>
        <InvoiceStatusBadge status={invoice.status} />
      </div>

      <dl className="mt-6 grid gap-3 border-y border-stone-200 py-4 text-sm sm:grid-cols-2">
        <div>
          <dt className="font-semibold text-stone-500">Venta</dt>
          <dd className="mt-1 font-semibold text-stone-900">{invoice.saleId.slice(0, 8).toUpperCase()}</dd>
        </div>
        <div>
          <dt className="font-semibold text-stone-500">Fecha</dt>
          <dd className="mt-1 font-semibold text-stone-900">
            {new Date(invoice.createdAt).toLocaleString('es-DO')}
          </dd>
        </div>
      </dl>

      <div className="mt-6 space-y-3 text-sm">
        <ReceiptRow label="Subtotal" value={formatInvoiceMoney(invoice.subtotal)} />
        <ReceiptRow label="Descuento" value={formatInvoiceMoney(invoice.discountTotal)} />
        <ReceiptRow label="Impuesto" value={formatInvoiceMoney(invoice.taxTotal)} />
        <ReceiptRow label="Total" strong value={formatInvoiceMoney(invoice.total)} />
      </div>
    </article>
  )
}

function ReceiptRow({
  label,
  strong = false,
  value,
}: Readonly<{
  label: string
  strong?: boolean
  value: string
}>) {
  return (
    <div className="flex items-center justify-between gap-4">
      <span className={strong ? 'text-base font-semibold text-stone-950' : 'font-medium text-stone-600'}>
        {label}
      </span>
      <span className={strong ? 'text-base font-semibold text-stone-950' : 'font-semibold text-stone-900'}>
        {value}
      </span>
    </div>
  )
}
