import { Banknote, CreditCard, Landmark, WalletCards } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import type { PaymentMethod } from '@/modules/pos/types/posTypes'
import { cn } from '@/shared/utils/cn'

type PaymentMethodSelectorProps = {
  onChange: (paymentMethod: PaymentMethod) => void
  value: PaymentMethod
}

const methods: { icon: LucideIcon; label: string; value: PaymentMethod }[] = [
  { icon: Banknote, label: 'Efectivo', value: 'Cash' },
  { icon: Landmark, label: 'Transferencia', value: 'Transfer' },
  { icon: CreditCard, label: 'Tarjeta', value: 'Card' },
  { icon: WalletCards, label: 'Fiado', value: 'Credit' },
]

export function PaymentMethodSelector({
  onChange,
  value,
}: Readonly<PaymentMethodSelectorProps>) {
  return (
    <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
      <div className="flex items-center gap-3 border-b border-border px-5 py-4">
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-muted">
          <Banknote aria-hidden="true" className="text-muted-foreground" size={15} strokeWidth={2} />
        </span>
        <div>
          <h2 className="text-[13.5px] font-semibold text-foreground">Pago</h2>
          <p className="text-[12px] text-muted-foreground">Metodo de pago</p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-2 p-4">
        {methods.map((method) => {
          const Icon = method.icon
          const isSelected = value === method.value

          return (
            <button
              aria-pressed={isSelected}
              className={cn(
                'flex h-10 items-center justify-center gap-2 rounded-xl text-[13px] font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/25',
                isSelected
                  ? 'bg-primary text-primary-foreground shadow-sm'
                  : 'bg-muted text-muted-foreground hover:bg-muted/80 hover:text-foreground',
              )}
              key={method.value}
              onClick={() => onChange(method.value)}
              type="button"
            >
              <Icon aria-hidden="true" size={14} strokeWidth={2} />
              {method.label}
            </button>
          )
        })}
      </div>
    </div>
  )
}
