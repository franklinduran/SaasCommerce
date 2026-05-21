import { ArrowLeft, ReceiptText, RefreshCw } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { useInvoiceBySale } from '@/modules/invoices/hooks/useInvoices'
import { SaleDetailHeader } from '@/modules/sales/components/SaleDetailHeader'
import { SaleItemsTable } from '@/modules/sales/components/SaleItemsTable'
import { SaleReceipt } from '@/modules/sales/components/SaleReceipt'
import { PrintReceiptButton } from '@/modules/sales/components/PrintReceiptButton'
import { useSaleDetail } from '@/modules/sales/hooks/useSaleDetail'
import { useSaleStatusInvalidation } from '@/modules/sales/hooks/useSales'
import { useCurrentBusinessQuery } from '@/modules/settings/hooks/useSettings'
import { Button } from '@/shared/components/ui/button'

export function SaleDetailPage() {
  const { saleId } = useParams()
  const sale = useSaleDetail(saleId)
  const business = useCurrentBusinessQuery()
  const invoice = useInvoiceBySale(saleId, sale.data?.status === 'Completed')

  useSaleStatusInvalidation(saleId ? [saleId] : [])

  if (sale.isLoading) {
    return (
      <section className="space-y-6 p-4 sm:p-6 lg:p-8">
        <PageBackLink />
        <div className="rounded-md bg-white p-5 shadow-sm ring-1 ring-stone-200 sm:p-8">
          <div className="h-5 w-64 rounded bg-stone-100" />
          <div className="mt-6 grid gap-3 md:grid-cols-4">
            <div className="h-20 rounded bg-stone-100" />
            <div className="h-20 rounded bg-stone-100" />
            <div className="h-20 rounded bg-stone-100" />
            <div className="h-20 rounded bg-stone-100" />
          </div>
        </div>
      </section>
    )
  }

  if (sale.isError || !sale.data) {
    return (
      <section className="space-y-6 p-4 sm:p-6 lg:p-8">
        <PageBackLink />
        <div className="rounded-md bg-white p-5 text-center shadow-sm ring-1 ring-stone-200 sm:p-8">
          <p className="text-sm font-semibold text-red-700">No se pudo cargar la venta.</p>
          <Button className="mt-4" onClick={() => sale.refetch()} variant="secondary">
            <RefreshCw size={16} />
            Reintentar
          </Button>
        </div>
      </section>
    )
  }

  const businessName = business.data?.name ?? 'Negocio'

  return (
    <section className="space-y-6 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col justify-between gap-4 print:hidden lg:flex-row lg:items-center">
        <PageBackLink />
        <div className="flex flex-wrap gap-2">
          {invoice.data && (
            <Button asChild variant="secondary">
              <Link to={`/invoices/${invoice.data.invoiceId}`}>
                <ReceiptText size={16} />
                Ver recibo
              </Link>
            </Button>
          )}
          <PrintReceiptButton />
        </div>
      </div>

      <SaleDetailHeader sale={sale.data} />

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
        <div className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-stone-950">Productos vendidos</h3>
            <p className="mt-1 text-sm font-medium text-stone-600">
              {sale.data.items.length} lineas de venta
            </p>
          </div>
          <SaleItemsTable items={sale.data.items} />
        </div>

        <div className="xl:sticky xl:top-20 xl:self-start">
          <SaleReceipt businessName={businessName} sale={sale.data} />
        </div>
      </div>
    </section>
  )
}

function PageBackLink() {
  return (
    <Button asChild variant="ghost">
      <Link to="/sales">
        <ArrowLeft size={16} />
        Volver a ventas
      </Link>
    </Button>
  )
}
