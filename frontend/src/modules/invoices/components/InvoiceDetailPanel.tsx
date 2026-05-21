import {
  AlertTriangle,
  Ban,
  CheckCircle2,
  Printer,
  Receipt,
  RefreshCw,
  X,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { InvoiceReceipt } from '@/modules/invoices/components/InvoiceReceipt'
import { InvoiceStatusBadge } from '@/modules/invoices/components/InvoiceStatusBadge'
import { useCancelInvoice, useInvoice } from '@/modules/invoices/hooks/useInvoices'
import { formatInvoiceMoney } from '@/modules/invoices/utils/formatInvoiceMoney'
import { useCurrentBusinessQuery } from '@/modules/settings/hooks/useSettings'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog'
import { Button } from '@/shared/components/ui/button'

type InvoiceDetailPanelProps = {
  invoiceId: string
  onClose: () => void
}

function formatDateTime(value: string) {
  try {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(new Date(value))
  } catch {
    return value
  }
}

export function InvoiceDetailPanel({ invoiceId, onClose }: Readonly<InvoiceDetailPanelProps>) {
  const invoice = useInvoice(invoiceId)
  const business = useCurrentBusinessQuery()
  const cancelInvoice = useCancelInvoice()
  const [confirmCancel, setConfirmCancel] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)

  useEffect(() => {
    if (!notice) return
    const t = window.setTimeout(() => setNotice(null), 3500)
    return () => window.clearTimeout(t)
  }, [notice])

  if (invoice.isLoading || !invoice.data) {
    if (invoice.isError) {
      return (
        <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
          <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
            No se pudo cargar el recibo.
          </div>
          <Button onClick={() => void invoice.refetch()} size="sm" type="button" variant="secondary">
            <RefreshCw size={14} />
            Reintentar
          </Button>
        </div>
      )
    }
    return <DetailSkeleton />
  }

  const data = invoice.data
  const businessName = business.data?.name ?? 'Negocio'

  function handleCancelConfirm() {
    cancelInvoice.mutate(data.invoiceId, {
      onSuccess: () => {
        setConfirmCancel(false)
        setNotice('Recibo cancelado.')
      },
    })
  }

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="sticky top-0 z-10 border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-center gap-3">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white shadow-sm">
              <Receipt size={20} />
            </div>
            <div className="min-w-0">
              <h3 className="truncate font-mono text-lg font-semibold text-stone-950">
                {data.invoiceNumber}
              </h3>
              <p className="mt-0.5 text-xs font-medium text-stone-500">
                Venta{' '}
                <Link
                  className="font-mono font-semibold text-stone-700 hover:text-stone-950"
                  to={`/sales/${data.saleId}`}
                >
                  {data.saleId.slice(0, 8).toUpperCase()}
                </Link>{' '}
                · {formatDateTime(data.createdAt)}
              </p>
              <div className="mt-2 flex flex-wrap items-center gap-1.5">
                <InvoiceStatusBadge status={data.status} />
                {data.cancelledAt && (
                  <span className="rounded-full bg-stone-100 px-2 py-0.5 text-[11px] font-semibold text-stone-600 ring-1 ring-stone-200">
                    Cancelado {formatDateTime(data.cancelledAt)}
                  </span>
                )}
              </div>
            </div>
          </div>
          <div className="flex shrink-0 items-start gap-1">
            <Button
              onClick={() => window.print()}
              size="sm"
              type="button"
              variant="secondary"
            >
              <Printer size={14} />
              Imprimir
            </Button>
            <Button
              aria-label="Cerrar panel"
              className="lg:hidden"
              onClick={onClose}
              size="icon"
              type="button"
              variant="ghost"
            >
              <X size={16} />
            </Button>
          </div>
        </div>

        {notice && (
          <div className="mt-3 flex items-center gap-2 rounded-md bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-800 ring-1 ring-emerald-200">
            <CheckCircle2 size={15} />
            {notice}
          </div>
        )}
      </div>

      <div className="min-h-0 flex-1 space-y-4 p-4 print:p-0 sm:p-6 lg:p-8">
        {/* Totales */}
        <Section description="Resumen financiero del recibo." title="Totales">
          <dl className="grid gap-3 sm:grid-cols-4">
            <Fact label="Subtotal" value={formatInvoiceMoney(data.subtotal)} />
            <Fact label="Descuento" tone="red" value={formatInvoiceMoney(data.discountTotal)} />
            <Fact label="Impuesto" value={formatInvoiceMoney(data.taxTotal)} />
            <Fact bold label="Total" tone="dark" value={formatInvoiceMoney(data.total)} />
          </dl>
        </Section>

        {/* Receipt preview */}
        <Section description="Vista del recibo impreso." title="Vista del recibo">
          <div className="rounded-md bg-stone-100 p-4">
            <InvoiceReceipt businessName={businessName} invoice={data} />
          </div>
        </Section>

        {/* Danger zone */}
        {data.status === 'Issued' && (
          <Section
            description="Una vez cancelado, el recibo no podra reutilizarse."
            icon={<AlertTriangle className="text-amber-600" size={16} />}
            title="Acciones"
          >
            <div className="flex flex-col gap-3 rounded-md border border-red-200 bg-red-50/40 p-3 sm:flex-row sm:items-start sm:justify-between">
              <div className="flex items-start gap-2.5">
                <span className="mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-red-100 text-red-700 ring-1 ring-red-200">
                  <Ban size={16} />
                </span>
                <div>
                  <p className="text-sm font-semibold text-stone-900">Cancelar recibo</p>
                  <p className="mt-0.5 text-xs font-medium text-stone-500">
                    El estado pasara a Cancelado y no podra revertirse.
                  </p>
                </div>
              </div>
              <Button
                disabled={cancelInvoice.isPending}
                onClick={() => setConfirmCancel(true)}
                size="sm"
                type="button"
                variant="destructive"
              >
                Cancelar recibo
              </Button>
            </div>
          </Section>
        )}
      </div>

      <AlertDialog onOpenChange={setConfirmCancel} open={confirmCancel}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cancelar recibo</AlertDialogTitle>
            <AlertDialogDescription>
              El recibo <strong>{data.invoiceNumber}</strong> quedara en estado Cancelado. Esta accion no se puede revertir.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Volver</AlertDialogCancel>
            <AlertDialogAction
              disabled={cancelInvoice.isPending}
              onClick={handleCancelConfirm}
              variant="destructive"
            >
              {cancelInvoice.isPending ? 'Cancelando...' : 'Si, cancelar'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

function Section({
  children,
  description,
  icon,
  title,
}: Readonly<{
  children: React.ReactNode
  description?: string
  icon?: React.ReactNode
  title: string
}>) {
  return (
    <section className="rounded-md bg-white p-4 ring-1 ring-stone-200 sm:p-5">
      <header className="mb-4 flex items-start gap-2.5">
        {icon && <span className="mt-0.5 shrink-0">{icon}</span>}
        <div className="min-w-0">
          <h4 className="text-sm font-semibold text-stone-950">{title}</h4>
          {description && (
            <p className="mt-0.5 text-xs font-medium text-stone-500">{description}</p>
          )}
        </div>
      </header>
      {children}
    </section>
  )
}

type FactTone = 'default' | 'dark' | 'red'

function Fact({
  bold,
  label,
  tone = 'default',
  value,
}: Readonly<{ bold?: boolean; label: string; tone?: FactTone; value: string }>) {
  const toneClass =
    tone === 'dark'
      ? 'bg-stone-900 text-white ring-stone-900'
      : tone === 'red'
        ? 'bg-red-50 text-red-700 ring-red-200'
        : 'bg-stone-50 text-stone-900 ring-stone-200'

  return (
    <div className={`rounded-md p-3 ring-1 ${toneClass}`}>
      <dt className={`text-xs font-semibold uppercase tracking-wide ${tone === 'dark' ? 'text-stone-300' : 'text-stone-500'}`}>
        {label}
      </dt>
      <dd className={`mt-1 ${bold ? 'text-lg' : 'text-base'} font-semibold tabular-nums`}>{value}</dd>
    </div>
  )
}

function DetailSkeleton() {
  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-center gap-3">
          <div className="h-12 w-12 shrink-0 rounded-md bg-stone-100" />
          <div className="min-w-0 flex-1 space-y-2">
            <div className="h-4 w-1/3 rounded bg-stone-100" />
            <div className="h-3 w-1/2 rounded bg-stone-100" />
          </div>
        </div>
      </div>
      <div className="space-y-4 p-4 sm:p-6 lg:p-8">
        <div className="h-24 rounded-md bg-white ring-1 ring-stone-200" />
        <div className="h-72 rounded-md bg-white ring-1 ring-stone-200" />
      </div>
    </div>
  )
}
