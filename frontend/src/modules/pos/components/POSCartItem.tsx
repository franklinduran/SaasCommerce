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
    <div className="grid min-w-0 grid-cols-[minmax(0,1fr)_auto] gap-3 border-b border-stone-200 py-3 last:border-b-0">
      <div className="min-w-0">
        <p className="truncate text-sm font-semibold text-stone-950">{item.name}</p>
        <p className="mt-1 font-mono text-xs font-semibold text-stone-500">{item.sku}</p>
        <p className="mt-2 text-sm font-semibold text-stone-700">
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
          <Minus aria-hidden="true" size={15} />
        </Button>
        <span className="flex h-9 min-w-9 items-center justify-center rounded-md bg-stone-100 px-2 text-sm font-semibold text-stone-900 ring-1 ring-stone-200">
          {item.quantity}
        </span>
        <Button
          aria-label={`Aumentar ${item.name}`}
          onClick={() => onIncrease(item.productId)}
          size="icon"
          type="button"
          variant="secondary"
        >
          <Plus aria-hidden="true" size={15} />
        </Button>
        <Button
          aria-label={`Quitar ${item.name}`}
          onClick={() => onRemove(item.productId)}
          size="icon"
          type="button"
          variant="ghost"
        >
          <Trash2 aria-hidden="true" size={15} />
        </Button>
      </div>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
