import { ShoppingCart } from 'lucide-react'
import { POSCartItem } from '@/modules/pos/components/POSCartItem'
import type { POSCartItem as POSCartItemType } from '@/modules/pos/types/posTypes'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

type POSCartProps = {
  itemCount: number
  items: POSCartItemType[]
  onDecrease: (productId: string) => void
  onIncrease: (productId: string) => void
  onRemove: (productId: string) => void
  subtotal: number
}

export function POSCart({
  itemCount,
  items,
  onDecrease,
  onIncrease,
  onRemove,
  subtotal,
}: Readonly<POSCartProps>) {
  return (
    <Card className="overflow-hidden">
      <CardHeader className="flex flex-row items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <span className="flex h-10 w-10 items-center justify-center rounded-md bg-stone-100 text-stone-900">
            <ShoppingCart aria-hidden="true" size={18} />
          </span>
          <div>
            <h2 className="text-base font-semibold text-stone-950">Carrito</h2>
            <p className="text-sm font-medium text-stone-600">{itemCount} articulos</p>
          </div>
        </div>
        <p className="text-right text-base font-semibold text-stone-950">{formatMoney(subtotal)}</p>
      </CardHeader>
      <CardContent>
        {items.length === 0 ? (
          <div className="flex min-h-40 flex-col items-center justify-center rounded-md bg-stone-50 p-6 text-center ring-1 ring-stone-200">
            <ShoppingCart aria-hidden="true" className="text-stone-500" size={26} />
            <p className="mt-3 text-sm font-semibold text-stone-800">Carrito vacio</p>
            <p className="mt-1 text-sm font-medium text-stone-500">
              Agrega productos para iniciar la venta.
            </p>
          </div>
        ) : (
          <div className="max-h-[360px] overflow-y-auto pr-1">
            {items.map((item) => (
              <POSCartItem
                item={item}
                key={item.productId}
                onDecrease={onDecrease}
                onIncrease={onIncrease}
                onRemove={onRemove}
              />
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
