import { Banknote, CreditCard, Landmark, WalletCards } from 'lucide-react'
import type { PaymentMethod } from '@/modules/pos/types/posTypes'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

type PaymentMethodSelectorProps = {
  onChange: (paymentMethod: PaymentMethod) => void
  value: PaymentMethod
}

export function PaymentMethodSelector({
  onChange,
  value,
}: Readonly<PaymentMethodSelectorProps>) {
  const methods = [
    { icon: Banknote, label: 'Efectivo', value: 'Cash' as const },
    { icon: Landmark, label: 'Transferencia', value: 'Transfer' as const },
    { icon: CreditCard, label: 'Tarjeta', value: 'Card' as const },
    { icon: WalletCards, label: 'Fiado', value: 'Credit' as const },
  ]

  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <span className="flex h-10 w-10 items-center justify-center rounded-md bg-stone-100 text-stone-900">
          <Banknote aria-hidden="true" size={18} />
        </span>
        <div>
          <h2 className="text-base font-semibold text-stone-950">Pago</h2>
          <p className="text-sm font-medium text-stone-600">Metodo</p>
        </div>
      </CardHeader>
      <CardContent className="grid grid-cols-1 gap-2 sm:grid-cols-2">
        {methods.map((method) => {
          const Icon = method.icon

          return (
            <Button
              aria-pressed={value === method.value}
              className="w-full"
              key={method.value}
              onClick={() => onChange(method.value)}
              type="button"
              variant={value === method.value ? 'default' : 'secondary'}
            >
              <Icon aria-hidden="true" size={16} />
              {method.label}
            </Button>
          )
        })}
      </CardContent>
    </Card>
  )
}
