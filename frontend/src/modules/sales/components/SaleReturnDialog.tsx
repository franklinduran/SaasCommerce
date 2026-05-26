import { useMemo, useState } from 'react'
import { RotateCcw } from 'lucide-react'
import { useCreateSaleReturn } from '@/modules/sales/hooks/useSales'
import type { SaleDetail, SaleReturn } from '@/modules/sales/types/salesTypes'
import { formatCurrency, formatQuantity } from '@/modules/sales/utils/formatSales'
import { Button } from '@/shared/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { Textarea } from '@/shared/components/ui/textarea'

type SaleReturnDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  sale: SaleDetail
  returns: SaleReturn[]
}

export function SaleReturnDialog({
  onOpenChange,
  open,
  returns,
  sale,
}: Readonly<SaleReturnDialogProps>) {
  const [reason, setReason] = useState('')
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const mutation = useCreateSaleReturn(sale.id)

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

  const returnableItems = sale.items.map((item) => ({
    ...item,
    returnedQuantity: returnedBySaleItem.get(item.saleItemId) ?? 0,
    remainingQuantity: Math.max(item.quantity - (returnedBySaleItem.get(item.saleItemId) ?? 0), 0),
  }))

  const selectedItems = returnableItems
    .map((item) => ({
      saleItemId: item.saleItemId,
      quantity: Number(quantities[item.saleItemId] ?? 0),
      remainingQuantity: item.remainingQuantity,
    }))
    .filter((item) => item.quantity > 0)

  const hasInvalidQuantity = selectedItems.some(
    (item) => item.quantity > item.remainingQuantity,
  )
  const canSubmit = reason.trim().length > 0 && selectedItems.length > 0 && !hasInvalidQuantity

  async function handleSubmit(event: { preventDefault: () => void }) {
    event.preventDefault()

    if (!canSubmit) {
      return
    }

    await mutation.mutateAsync({
      reason: reason.trim(),
      items: selectedItems.map((item) => ({
        saleItemId: item.saleItemId,
        quantity: item.quantity,
      })),
    })
    setReason('')
    setQuantities({})
    onOpenChange(false)
  }

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen && !mutation.isPending) {
      setReason('')
      setQuantities({})
    }

    onOpenChange(nextOpen)
  }

  return (
    <Dialog onOpenChange={handleOpenChange} open={open}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-2xl">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Registrar devolución</DialogTitle>
            <DialogDescription>
              Selecciona las cantidades a devolver para la venta {sale.code}.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-5">
            <div className="space-y-2">
              <Label htmlFor="sale-return-reason">Motivo</Label>
              <Textarea
                id="sale-return-reason"
                maxLength={500}
                onChange={(event) => setReason(event.target.value)}
                placeholder="Producto devuelto, cambio solicitado..."
                value={reason}
              />
            </div>

            <div className="overflow-x-auto rounded-md ring-1 ring-stone-200">
              <table className="w-full min-w-150 text-left text-sm">
                <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
                  <tr>
                    <th className="px-4 py-3">Producto</th>
                    <th className="px-4 py-3 text-right">Disponible</th>
                    <th className="px-4 py-3 text-right">Precio</th>
                    <th className="px-4 py-3 text-right">Devolver</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-stone-200">
                  {returnableItems.map((item) => (
                    <tr key={item.saleItemId}>
                      <td className="px-4 py-3">
                        <p className="font-semibold text-stone-950">{item.productName}</p>
                        <p className="mt-1 font-mono text-xs font-semibold text-stone-500">
                          {item.sku ?? 'Sin SKU'}
                        </p>
                      </td>
                      <td className="px-4 py-3 text-right font-semibold text-stone-800">
                        {formatQuantity(item.remainingQuantity)}
                      </td>
                      <td className="px-4 py-3 text-right font-semibold text-stone-800">
                        {formatCurrency(item.unitPrice)}
                      </td>
                      <td className="px-4 py-3">
                        <Input
                          aria-label={`Cantidad a devolver de ${item.productName}`}
                          className="ml-auto w-28 text-right"
                          disabled={item.remainingQuantity <= 0 || mutation.isPending}
                          max={item.remainingQuantity}
                          min={0}
                          onChange={(event) =>
                            setQuantities((current) => ({
                              ...current,
                              [item.saleItemId]: event.target.value,
                            }))
                          }
                          step="0.001"
                          type="number"
                          value={quantities[item.saleItemId] ?? ''}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {mutation.isError && (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">
                {mutation.error.message}
              </p>
            )}
          </div>

          <DialogFooter>
            <Button
              disabled={mutation.isPending}
              onClick={() => handleOpenChange(false)}
              type="button"
              variant="secondary"
            >
              Cancelar
            </Button>
            <Button disabled={!canSubmit || mutation.isPending} type="submit">
              <RotateCcw size={16} />
              Registrar
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
