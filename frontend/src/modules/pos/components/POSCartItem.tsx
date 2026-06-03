import { Minus, Plus, Trash2 } from 'lucide-react'
import type { POSCartItem as POSCartItemType } from '@/modules/pos/types/posTypes'
import { Button } from '@/shared/components/ui/button'

type POSCartItemProps = {
  item: POSCartItemType
  onDecrease: (productId: string) => void
  onIncrease: (productId: string) => void
  onRemove: (productId: string) => void
}

export function POSCartItem({
  item,
  onDecrease,
  onIncrease,
  onRemove,
}: Readonly<POSCartItemProps>) {
  return (
    <div className="grid min-w-0 grid-cols-[minmax(0,1fr)_auto] gap-3 border-b border-border py-3 last:border-b-0">
      <div className="min-w-0">
        <p className="truncate text-[13px] font-semibold text-foreground">{item.name}</p>
        <p className="mt-0.5 font-mono text-[11px] text-muted-foreground">{item.sku}</p>
        <p className="mt-1.5 text-[13px] font-bold tracking-tight text-foreground">
          {formatMoney(item.unitPrice * item.quantity)}
        </p>
      </div>
      <div className="flex items-center gap-1">
        <Button
          aria-label={`Disminuir ${item.name}`}
          onClick={() => onDecrease(item.productId)}
          size="icon"
          type="button"
          variant="secondary"
        >
          <Minus aria-hidden="true" size={13} />
        </Button>
        <span className="flex h-8 min-w-8 items-center justify-center rounded-lg bg-muted px-2 text-[13px] font-semibold text-foreground ring-1 ring-border">
          {item.quantity}
        </span>
        <Button
          aria-label={`Aumentar ${item.name}`}
          onClick={() => onIncrease(item.productId)}
          size="icon"
          type="button"
          variant="secondary"
        >
          <Plus aria-hidden="true" size={13} />
        </Button>
        <Button
          aria-label={`Quitar ${item.name}`}
          onClick={() => onRemove(item.productId)}
          size="icon"
          type="button"
          variant="ghost"
        >
          <Trash2 aria-hidden="true" size={13} />
        </Button>
      </div>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
