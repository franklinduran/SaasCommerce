import { Minus, Plus, Trash2 } from 'lucide-react'
import { useState } from 'react'
import type { POSCartItem as POSCartItemType } from '@/modules/pos/types/posTypes'
import { Button } from '@/shared/components/ui/button'
import { cn } from '@/shared/utils/cn'

type POSCartItemProps = {
  item: POSCartItemType
  onDecrease: (productId: string) => void
  onIncrease: (productId: string) => void
  onRemove: (productId: string) => void
  onSetQuantity: (productId: string, quantity: number) => void
}

export function POSCartItem({
  item,
  onDecrease,
  onIncrease,
  onRemove,
  onSetQuantity,
}: Readonly<POSCartItemProps>) {
  const [inputValue, setInputValue] = useState(String(item.quantity))
  const [focused, setFocused] = useState(false)

  // Sync input when store quantity changes externally (e.g. +/- buttons)
  // Only sync when not actively editing
  const displayValue = focused ? inputValue : String(item.quantity)

  function handleFocus(e: React.FocusEvent<HTMLInputElement>) {
    setInputValue(String(item.quantity))
    setFocused(true)
    // Select all on focus so the user can just type the new value
    e.currentTarget.select()
  }

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    // Allow only digits
    const raw = e.target.value.replace(/\D/g, '')
    setInputValue(raw)
  }

  function commit() {
    setFocused(false)
    const parsed = parseInt(inputValue, 10)
    if (!Number.isFinite(parsed) || parsed <= 0) {
      // Remove item if 0 or empty
      onSetQuantity(item.productId, 0)
    } else {
      onSetQuantity(item.productId, parsed)
    }
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Enter') {
      e.currentTarget.blur()
    }
    if (e.key === 'Escape') {
      setInputValue(String(item.quantity))
      setFocused(false)
      e.currentTarget.blur()
    }
  }

  return (
    <div className="grid min-w-0 grid-cols-[minmax(0,1fr)_auto] gap-3 border-b border-border py-3 last:border-b-0">
      <div className="min-w-0">
        <p className="truncate text-[13px] font-semibold text-foreground">{item.name}</p>
        <p className="mt-0.5 font-mono text-[11px] text-muted-foreground">{item.sku}</p>
        <p className="mt-1.5 text-[13px] font-bold tabular-nums tracking-tight text-foreground">
          {formatMoney(item.unitPrice * item.quantity)}
        </p>
      </div>

      <div className="flex items-center gap-1">
        <Button
          aria-label={`Disminuir ${item.name}`}
          onClick={() => onDecrease(item.productId)}
          size="icon"
          type="button"
          variant="ghost"
        >
          <Minus aria-hidden="true" size={13} />
        </Button>

        <input
          aria-label={`Cantidad de ${item.name}`}
          className={cn(
            'h-7 w-10 rounded-md border bg-white text-center text-[13px] font-semibold tabular-nums text-foreground outline-none transition',
            focused
              ? 'border-primary ring-2 ring-primary/15'
              : 'border-gray-200 hover:border-gray-300',
          )}
          inputMode="numeric"
          onBlur={commit}
          onChange={handleChange}
          onFocus={handleFocus}
          onKeyDown={handleKeyDown}
          type="text"
          value={displayValue}
        />

        <Button
          aria-label={`Aumentar ${item.name}`}
          onClick={() => onIncrease(item.productId)}
          size="icon"
          type="button"
          variant="ghost"
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
          <Trash2 aria-hidden="true" className="text-muted-foreground" size={13} />
        </Button>
      </div>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
