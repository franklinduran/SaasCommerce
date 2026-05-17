import { Banknote } from 'lucide-react'
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
      <CardContent>
        <Button
          aria-pressed={value === 'Cash'}
          className="w-full"
          onClick={() => onChange('Cash')}
          type="button"
          variant={value === 'Cash' ? 'default' : 'secondary'}
        >
          <Banknote aria-hidden="true" size={16} />
          Efectivo
        </Button>
      </CardContent>
    </Card>
  )
}
