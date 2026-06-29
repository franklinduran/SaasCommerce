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
  { icon: WalletCards, label: 'Crédito', value: 'Credit' },
]

export function PaymentMethodSelector({
  onChange,
  value,
}: Readonly<PaymentMethodSelectorProps>) {
  return (
    <div>
      {/* Section header */}
      <div className="flex items-center gap-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          Método de pago
        </p>
        <div className="h-px flex-1 bg-border" />
      </div>

      {/* Content */}
      <div className="mt-3 grid grid-cols-2 gap-2">
        {methods.map((method) => {
          const Icon = method.icon
          const active = value === method.value

          return (
            <button
              aria-pressed={active}
              className={cn(
                'flex h-9 items-center justify-center gap-2 rounded-lg text-[13px] font-medium transition-colors',
                'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/25',
                active
                  ? 'bg-primary text-primary-foreground shadow-sm'
                  : 'bg-muted text-muted-foreground hover:bg-muted/70 hover:text-foreground',
              )}
              key={method.value}
              onClick={() => onChange(method.value)}
              type="button"
            >
              <Icon aria-hidden="true" size={13} strokeWidth={2} />
              {method.label}
            </button>
          )
        })}
      </div>
    </div>
  )
}
