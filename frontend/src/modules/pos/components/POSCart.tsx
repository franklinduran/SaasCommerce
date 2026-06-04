import { ShoppingCart } from 'lucide-react'
import { POSCartItem } from '@/modules/pos/components/POSCartItem'
import type { POSCartItem as POSCartItemType } from '@/modules/pos/types/posTypes'

type POSCartProps = {
  itemCount: number
  items: POSCartItemType[]
  onDecrease: (productId: string) => void
  onIncrease: (productId: string) => void
  onRemove: (productId: string) => void
  onSetQuantity: (productId: string, quantity: number) => void
  subtotal: number
}

export function POSCart({
  itemCount,
  items,
  onDecrease,
  onIncrease,
  onRemove,
  onSetQuantity,
  subtotal,
}: Readonly<POSCartProps>) {
  return (
    <div>
      {/* Section header */}
      <div className="flex items-center gap-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          Carrito
        </p>
        <div className="h-px flex-1 bg-border" />
        <span className="text-[11px] font-semibold tabular-nums text-muted-foreground">
          {itemCount} art. · {formatMoney(subtotal)}
        </span>
      </div>

      {/* Content */}
      <div className="mt-4">
        {items.length === 0 ? (
          <div className="flex min-h-[100px] flex-col items-center justify-center gap-2 rounded-xl bg-muted py-6 text-center">
            <ShoppingCart aria-hidden="true" className="text-muted-foreground/40" size={22} />
            <p className="text-[13px] font-semibold text-foreground">Carrito vacio</p>
            <p className="text-[12px] text-muted-foreground">
              Agrega productos para iniciar la venta.
            </p>
          </div>
        ) : (
          <div className="max-h-[320px] overflow-y-auto">
            {items.map((item) => (
              <POSCartItem
                item={item}
                key={item.productId}
                onDecrease={onDecrease}
                onIncrease={onIncrease}
                onRemove={onRemove}
                onSetQuantity={onSetQuantity}
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
