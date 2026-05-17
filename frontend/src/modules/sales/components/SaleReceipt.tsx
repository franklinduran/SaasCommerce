import { SaleStatusBadge } from '@/modules/sales/components/SaleStatusBadge'
import type { SaleDetail } from '@/modules/sales/types/salesTypes'
import {
  formatCurrency,
  formatDateTime,
  formatQuantity,
} from '@/modules/sales/utils/formatSales'

type SaleReceiptProps = {
  businessName: string
  sale: SaleDetail
}

export function SaleReceipt({ businessName, sale }: Readonly<SaleReceiptProps>) {
  return (
    <section
      aria-label="Recibo no fiscal"
      className="rounded-md bg-white p-5 shadow-sm ring-1 ring-stone-200 print:shadow-none print:ring-0"
    >
      <div className="border-b border-dashed border-stone-300 pb-4 text-center">
        <p className="text-lg font-semibold text-stone-950">{businessName}</p>
        <p className="mt-1 text-sm font-medium text-stone-600">
          {sale.branchName ?? 'Sucursal no disponible'}
        </p>
        <p className="mt-3 text-xs font-bold uppercase text-stone-500">Recibo no fiscal</p>
      </div>

      <div className="space-y-2 border-b border-dashed border-stone-300 py-4 text-sm">
        <ReceiptRow label="Venta" value={sale.code} />
        <ReceiptRow label="Fecha" value={formatDateTime(sale.createdAt)} />
        <ReceiptRow label="Cliente" value={sale.customerName ?? 'Consumidor final'} />
        <ReceiptRow label="Metodo" value={sale.paymentMethod} />
        <div className="flex items-center justify-between gap-4">
          <span className="font-medium text-stone-600">Estado</span>
          <SaleStatusBadge status={sale.status} />
        </div>
      </div>

      <div className="space-y-3 border-b border-dashed border-stone-300 py-4">
        {sale.items.map((item) => (
          <div key={`${item.productId}-${item.productName}`} className="text-sm">
            <div className="flex justify-between gap-4">
              <span className="font-semibold text-stone-900">{item.productName}</span>
              <span className="font-semibold text-stone-950">
                {formatCurrency(item.subtotal)}
              </span>
            </div>
            <p className="mt-1 text-xs font-medium text-stone-500">
              {formatQuantity(item.quantity)} x {formatCurrency(item.unitPrice)}
            </p>
          </div>
        ))}
      </div>

      <div className="space-y-2 pt-4 text-sm">
        <ReceiptRow label="Subtotal" value={formatCurrency(sale.total)} strong />
        <ReceiptRow label="Total" value={formatCurrency(sale.total)} strong />
      </div>
    </section>
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
    <div className="flex justify-between gap-4">
      <span className="font-medium text-stone-600">{label}</span>
      <span className={strong ? 'font-bold text-stone-950' : 'font-semibold text-stone-900'}>
        {value}
      </span>
    </div>
  )
}
