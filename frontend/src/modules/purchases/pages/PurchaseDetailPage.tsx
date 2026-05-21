import { ArrowLeft, Ban, CheckCircle2, RefreshCw } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { PurchaseStatusBadge } from '@/modules/purchases/components/PurchaseStatusBadge'
import { PurchaseTotalsSummary } from '@/modules/purchases/components/PurchaseTotalsSummary'
import {
  useCancelPurchase,
  usePurchaseDetail,
  usePurchaseRealtimeInvalidation,
  useReceivePurchase,
} from '@/modules/purchases/hooks/usePurchases'
import { formatMoney } from '@/modules/purchases/utils/formatMoney'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

export function PurchaseDetailPage() {
  const { purchaseId } = useParams()
  const purchase = usePurchaseDetail(purchaseId)
  const receivePurchase = useReceivePurchase()
  const cancelPurchase = useCancelPurchase()
  const item = purchase.data
  usePurchaseRealtimeInvalidation(purchaseId)

  if (purchase.isLoading) {
    return <section className="p-4 text-sm font-semibold text-stone-600 sm:p-6 lg:p-8">Cargando compra...</section>
  }

  if (purchase.isError || !item) {
    return (
      <section className="space-y-4 p-4 sm:p-6 lg:p-8">
        <p className="text-sm font-semibold text-red-700">No se pudo cargar la compra.</p>
        <Button onClick={() => purchase.refetch()} type="button" variant="secondary">
          <RefreshCw size={16} />
          Reintentar
        </Button>
      </section>
    )
  }

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <Button asChild variant="ghost">
            <Link to="/purchases">
              <ArrowLeft size={16} />
              Compras
            </Link>
          </Button>
          <div className="mt-3 flex flex-wrap items-center gap-3">
            <h2 className="text-2xl font-semibold text-stone-950">Compra {item.code}</h2>
            <PurchaseStatusBadge status={item.status} />
          </div>
        </div>
        <div className="flex gap-2">
          {item.status === 'Draft' && (
            <>
              <Button disabled={receivePurchase.isPending} onClick={() => receivePurchase.mutateAsync(item.purchaseId)} type="button">
                <CheckCircle2 size={16} />
                Recibir
              </Button>
              <Button disabled={cancelPurchase.isPending} onClick={() => cancelPurchase.mutateAsync(item.purchaseId)} type="button" variant="secondary">
                <Ban size={16} />
                Cancelar
              </Button>
            </>
          )}
        </div>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_320px]">
        <Card>
          <CardHeader>
            <h3 className="text-base font-semibold text-stone-950">Proveedor</h3>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            <Detail label="Nombre" value={item.supplierName} />
            <Detail label="Factura" value={item.supplierInvoiceNumber ?? 'Sin factura'} />
            <Detail label="Fecha" value={formatDate(item.purchaseDate)} />
            <Detail label="Notas" value={item.notes ?? 'Sin notas'} />
          </CardContent>
        </Card>
        <PurchaseTotalsSummary itemCount={item.items.length} total={item.total} />
      </div>

      <Card className="overflow-hidden">
        <CardHeader>
          <h3 className="text-base font-semibold text-stone-950">Productos</h3>
        </CardHeader>
        <div className="overflow-x-auto">
          <table className="w-full min-w-180 text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
              <tr>
                <th className="px-5 py-3">Producto</th>
                <th className="px-5 py-3 text-right">Cantidad</th>
                <th className="px-5 py-3 text-right">Costo</th>
                <th className="px-5 py-3 text-right">Subtotal</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200">
              {item.items.map((line) => (
                <tr className="bg-white" key={line.id}>
                  <td className="px-5 py-4">
                    <p className="font-semibold text-stone-950">{line.productName}</p>
                    <p className="mt-1 text-xs font-medium text-stone-500">{line.sku ?? 'Sin SKU'}</p>
                  </td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-800">{line.quantity}</td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-800">{formatMoney(line.unitCost)}</td>
                  <td className="px-5 py-4 text-right font-semibold text-stone-950">{formatMoney(line.subtotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      <Card className="overflow-hidden">
        <CardHeader>
          <h3 className="text-base font-semibold text-stone-950">Movimientos generados</h3>
        </CardHeader>
        <div className="overflow-x-auto">
          <table className="w-full min-w-160 text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
              <tr>
                <th className="px-5 py-3">Producto</th>
                <th className="px-5 py-3 text-right">Antes</th>
                <th className="px-5 py-3 text-right">Despues</th>
                <th className="px-5 py-3 text-right">Cantidad</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200">
              {item.movements.length === 0 && (
                <tr><td className="px-5 py-8 text-center text-stone-600" colSpan={4}>No hay movimientos registrados.</td></tr>
              )}
              {item.movements.map((movement) => (
                <tr className="bg-white" key={movement.id}>
                  <td className="px-5 py-4 font-mono text-xs font-semibold text-stone-700">{movement.productId}</td>
                  <td className="px-5 py-4 text-right">{movement.previousStock}</td>
                  <td className="px-5 py-4 text-right">{movement.newStock}</td>
                  <td className="px-5 py-4 text-right font-semibold text-emerald-700">+{movement.quantity}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </section>
  )
}

function Detail({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div>
      <p className="text-xs font-semibold uppercase text-stone-500">{label}</p>
      <p className="mt-1 text-sm font-semibold text-stone-950">{value}</p>
    </div>
  )
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium' }).format(new Date(value))
}
