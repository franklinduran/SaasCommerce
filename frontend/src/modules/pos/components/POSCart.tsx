import { ShoppingCart } from 'lucide-react'
import { POSCartItem } from '@/modules/pos/components/POSCartItem'
import type { POSCartItem as POSCartItemType } from '@/modules/pos/types/posTypes'

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
    <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
      <div className="flex items-center justify-between gap-3 border-b border-border px-5 py-4">
        <div className="flex items-center gap-3">
          <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-muted">
            <ShoppingCart aria-hidden="true" className="text-muted-foreground" size={15} strokeWidth={2} />
          </span>
          <div>
            <h2 className="text-[13.5px] font-semibold text-foreground">Carrito</h2>
            <p className="text-[12px] text-muted-foreground">{itemCount} articulos</p>
          </div>
        </div>
        <p className="text-[15px] font-bold tracking-tight text-foreground">{formatMoney(subtotal)}</p>
      </div>

      <div className="p-4">
        {items.length === 0 ? (
          <div className="flex min-h-36 flex-col items-center justify-center rounded-xl bg-muted p-6 text-center">
            <ShoppingCart aria-hidden="true" className="text-muted-foreground/50" size={24} />
            <p className="mt-3 text-[13px] font-semibold text-foreground">Carrito vacio</p>
            <p className="mt-1 text-[12px] text-muted-foreground">
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
      </div>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
