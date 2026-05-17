import { ReceiptText } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

type SaleSummaryProps = {
  disabled: boolean
  isSubmitting: boolean
  itemCount: number
  onProcessSale: () => void
  subtotal: number
  validationMessage: string | null
}

export function SaleSummary({
  disabled,
  isSubmitting,
  itemCount,
  onProcessSale,
  subtotal,
  validationMessage,
}: Readonly<SaleSummaryProps>) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <span className="flex h-10 w-10 items-center justify-center rounded-md bg-stone-100 text-stone-900">
          <ReceiptText aria-hidden="true" size={18} />
        </span>
        <div>
          <h2 className="text-base font-semibold text-stone-950">Resumen</h2>
          <p className="text-sm font-medium text-stone-600">{itemCount} articulos</p>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2 rounded-md bg-stone-50 p-4 ring-1 ring-stone-200">
          <div className="flex items-center justify-between gap-3 text-sm font-medium text-stone-600">
            <span>Subtotal visual</span>
            <span className="font-semibold text-stone-900">{formatMoney(subtotal)}</span>
          </div>
          <div className="flex items-center justify-between gap-3 text-base font-semibold text-stone-950">
            <span>Total estimado</span>
            <span>{formatMoney(subtotal)}</span>
          </div>
        </div>
        {validationMessage && (
          <p className="rounded-md bg-amber-50 px-3 py-2 text-sm font-semibold text-amber-800 ring-1 ring-amber-200">
            {validationMessage}
          </p>
        )}
        <Button
          className="h-12 w-full"
          disabled={disabled}
          onClick={onProcessSale}
          type="button"
        >
          {isSubmitting ? 'Procesando venta' : 'Procesar venta'}
        </Button>
      </CardContent>
    </Card>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
