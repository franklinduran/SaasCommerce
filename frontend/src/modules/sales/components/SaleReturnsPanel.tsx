import { useMemo, useState } from 'react'
import { FileText, RotateCcw } from 'lucide-react'
import { SaleReturnDialog } from '@/modules/sales/components/SaleReturnDialog'
import type { SaleDetail, SaleReturn } from '@/modules/sales/types/salesTypes'
import { formatCurrency, formatDateTime, formatQuantity } from '@/modules/sales/utils/formatSales'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'

type SaleReturnsPanelProps = {
  sale: SaleDetail
  returns: SaleReturn[]
  isLoading: boolean
}

export function SaleReturnsPanel({
  isLoading,
  returns,
  sale,
}: Readonly<SaleReturnsPanelProps>) {
  const [dialogOpen, setDialogOpen] = useState(false)
  const returnedBySaleItem = useMemo(() => {
    const totals = new Map<string, number>()

    for (const saleReturn of returns) {
      if (saleReturn.status === 'Failed') {
        continue
      }

      for (const item of saleReturn.items) {
        totals.set(item.saleItemId, (totals.get(item.saleItemId) ?? 0) + item.quantity)
      }
    }

    return totals
  }, [returns])

  const hasReturnableItems = sale.items.some((item) => {
    const returnedQuantity = returnedBySaleItem.get(item.saleItemId) ?? 0

    return item.quantity > returnedQuantity
  })
  const canCreateReturn = sale.status === 'Completed' && hasReturnableItems
  const content = buildPanelContent(isLoading, returns)

  return (
    <div className="space-y-4">
      <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
        <div>
          <h3 className="text-base font-semibold text-stone-950">Devoluciones</h3>
          <p className="mt-1 text-sm font-medium text-stone-600">
            {returns.length} registros asociados a esta venta
          </p>
        </div>
        <Button
          disabled={!canCreateReturn || isLoading}
          onClick={() => setDialogOpen(true)}
          variant="secondary"
        >
          <RotateCcw size={16} />
          Registrar devolución
        </Button>
      </div>

      <div className="rounded-md bg-white shadow-sm ring-1 ring-stone-200">
        {content}
      </div>

      <SaleReturnDialog
        onOpenChange={setDialogOpen}
        open={dialogOpen}
        returns={returns}
        sale={sale}
      />
    </div>
  )
}

function buildPanelContent(isLoading: boolean, returns: SaleReturn[]) {
  if (isLoading) {
    return (
      <div className="space-y-3 p-5">
        <div className="h-4 w-40 rounded bg-stone-100" />
        <div className="h-16 rounded bg-stone-100" />
      </div>
    )
  }

  if (returns.length === 0) {
    return (
      <div className="flex items-center gap-3 p-5 text-sm font-semibold text-stone-600">
        <FileText className="size-5 text-stone-500" />
        Sin devoluciones registradas
      </div>
    )
  }

  return (
    <div className="divide-y divide-stone-200">
      {returns.map((saleReturn) => (
        <article className="p-5" key={saleReturn.id}>
          <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-start">
            <div>
              <div className="flex flex-wrap items-center gap-2">
                <p className="font-mono text-xs font-semibold uppercase text-stone-500">
                  {saleReturn.id.slice(0, 8)}
                </p>
                <ReturnStatusBadge status={saleReturn.status} />
                {saleReturn.creditNote && (
                  <Badge variant="secondary">{saleReturn.creditNote.code}</Badge>
                )}
              </div>
              <p className="mt-2 text-sm font-semibold text-stone-950">
                {saleReturn.reason}
              </p>
              <p className="mt-1 text-xs font-semibold text-stone-500">
                {formatDateTime(saleReturn.requestedAt)}
              </p>
            </div>
            <p className="text-right text-base font-bold text-stone-950">
              {formatCurrency(saleReturn.total)}
            </p>
          </div>

          <div className="mt-4 grid gap-2 sm:grid-cols-2">
            {saleReturn.items.map((item) => (
              <div className="rounded-md bg-stone-50 px-3 py-2 text-sm" key={item.id}>
                <p className="font-semibold text-stone-950">{item.productName}</p>
                <p className="mt-1 font-medium text-stone-600">
                  {formatQuantity(item.quantity)} x {formatCurrency(item.unitPrice)}
                </p>
              </div>
            ))}
          </div>

          {saleReturn.failureReason && (
            <p className="mt-3 rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">
              {saleReturn.failureReason}
            </p>
          )}
        </article>
      ))}
    </div>
  )
}

function ReturnStatusBadge({ status }: Readonly<{ status: SaleReturn['status'] }>) {
  const label = {
    Approved: 'Aprobada',
    Failed: 'Fallida',
    Requested: 'Solicitada',
  }[status]

  let variant: 'destructive' | 'secondary' | 'success' = 'secondary'
  if (status === 'Approved') {
    variant = 'success'
  }

  if (status === 'Failed') {
    variant = 'destructive'
  }

  return <Badge variant={variant}>{label}</Badge>
}
