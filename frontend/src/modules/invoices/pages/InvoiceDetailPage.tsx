import { ArrowLeft, Ban, RefreshCw } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { InvoiceReceipt } from '@/modules/invoices/components/InvoiceReceipt'
import { InvoiceStatusBadge } from '@/modules/invoices/components/InvoiceStatusBadge'
import { useCancelInvoice, useInvoice, useInvoiceRealtimeInvalidation } from '@/modules/invoices/hooks/useInvoices'
import { formatInvoiceMoney } from '@/modules/invoices/utils/formatInvoiceMoney'
import { PrintReceiptButton } from '@/modules/sales/components/PrintReceiptButton'
import { useCurrentBusinessQuery } from '@/modules/settings/hooks/useSettings'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'

export function InvoiceDetailPage() {
  const { invoiceId } = useParams()
  const invoice = useInvoice(invoiceId)
  const business = useCurrentBusinessQuery()
  const cancelInvoice = useCancelInvoice()

  useInvoiceRealtimeInvalidation()

  if (invoice.isLoading) {
    return (
      <section className="space-y-6 p-6 lg:p-8">
        <PageBackLink />
        <Card>
          <CardContent className="space-y-4 p-6">
            <div className="h-6 w-56 rounded bg-stone-100" />
            <div className="h-40 rounded bg-stone-100" />
          </CardContent>
        </Card>
      </section>
    )
  }

  if (invoice.isError || !invoice.data) {
    return (
      <section className="space-y-6 p-6 lg:p-8">
        <PageBackLink />
        <Card>
          <CardContent className="p-8 text-center">
            <p className="text-sm font-semibold text-red-700">No se pudo cargar el recibo.</p>
            <Button className="mt-4" onClick={() => invoice.refetch()} type="button" variant="secondary">
              <RefreshCw size={16} />
              Reintentar
            </Button>
          </CardContent>
        </Card>
      </section>
    )
  }

  const businessName = business.data?.name ?? 'Negocio'

  return (
    <section className="space-y-6 p-6 lg:p-8">
      <div className="flex flex-col justify-between gap-4 print:hidden lg:flex-row lg:items-center">
        <PageBackLink />
        <div className="flex flex-wrap gap-2">
          <PrintReceiptButton />
          <Button
            disabled={invoice.data.status !== 'Issued' || cancelInvoice.isPending}
            onClick={() => cancelInvoice.mutate(invoice.data.invoiceId)}
            type="button"
            variant="secondary"
          >
            <Ban size={16} />
            Cancelar
          </Button>
        </div>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_420px]">
        <Card>
          <CardContent className="p-6">
            <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start">
              <div>
                <p className="text-sm font-semibold uppercase text-stone-500">Recibo</p>
                <h2 className="mt-1 text-2xl font-semibold text-stone-950">{invoice.data.invoiceNumber}</h2>
                <p className="mt-2 text-sm font-medium text-stone-600">
                  Venta {invoice.data.saleId.slice(0, 8).toUpperCase()}
                </p>
              </div>
              <InvoiceStatusBadge status={invoice.data.status} />
            </div>

            <dl className="mt-6 grid gap-4 md:grid-cols-4">
              <Fact label="Subtotal" value={formatInvoiceMoney(invoice.data.subtotal)} />
              <Fact label="Descuento" value={formatInvoiceMoney(invoice.data.discountTotal)} />
              <Fact label="Impuesto" value={formatInvoiceMoney(invoice.data.taxTotal)} />
              <Fact label="Total" value={formatInvoiceMoney(invoice.data.total)} />
            </dl>
          </CardContent>
        </Card>

        <InvoiceReceipt businessName={businessName} invoice={invoice.data} />
      </div>
    </section>
  )
}

function Fact({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="rounded-md bg-stone-50 p-4 ring-1 ring-stone-200">
      <dt className="text-xs font-semibold uppercase text-stone-500">{label}</dt>
      <dd className="mt-1 text-base font-semibold text-stone-950">{value}</dd>
    </div>
  )
}

function PageBackLink() {
  return (
    <Link
      className="inline-flex h-10 items-center gap-2 rounded-md px-3 text-sm font-semibold text-stone-700 transition-colors hover:bg-stone-100 hover:text-stone-950 print:hidden"
      to="/invoices"
    >
      <ArrowLeft size={16} />
      Volver a recibos
    </Link>
  )
}
