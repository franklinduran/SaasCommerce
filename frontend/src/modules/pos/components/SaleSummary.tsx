import { ReceiptText } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'

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
    <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
      <div className="flex items-center gap-3 border-b border-border px-5 py-4">
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-muted">
          <ReceiptText aria-hidden="true" className="text-muted-foreground" size={15} strokeWidth={2} />
        </span>
        <div>
          <h2 className="text-[13.5px] font-semibold text-foreground">Resumen</h2>
          <p className="text-[12px] text-muted-foreground">{itemCount} articulos</p>
        </div>
      </div>

      <div className="space-y-4 p-4">
        <div className="space-y-2 rounded-xl bg-muted p-4">
          <div className="flex items-center justify-between gap-3 text-[12.5px] text-muted-foreground">
            <span>Subtotal visual</span>
            <span className="font-semibold text-foreground">{formatMoney(subtotal)}</span>
          </div>
          <div className="flex items-center justify-between gap-3 text-[14px] font-bold tracking-tight text-foreground">
            <span>Total estimado</span>
            <span>{formatMoney(subtotal)}</span>
          </div>
        </div>

        {validationMessage && (
          <p className="rounded-xl bg-amber-50 px-3 py-2 text-[12.5px] font-semibold text-amber-800 ring-1 ring-amber-200">
            {validationMessage}
          </p>
        )}

        <Button
          className="h-11 w-full"
          disabled={disabled}
          onClick={onProcessSale}
          type="button"
        >
          {isSubmitting ? 'Procesando venta…' : 'Procesar venta'}
        </Button>
      </div>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
