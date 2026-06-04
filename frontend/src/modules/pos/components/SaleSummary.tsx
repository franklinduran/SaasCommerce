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
    <div>
      {/* Section header */}
      <div className="flex items-center gap-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          Resumen
        </p>
        <div className="h-px flex-1 bg-border" />
        <span className="text-[11px] font-semibold text-muted-foreground">
          {itemCount} artículos
        </span>
      </div>

      {/* Totals */}
      <div className="mt-3 space-y-1.5">
        <div className="flex items-center justify-between gap-3 text-[13px] text-muted-foreground">
          <span>Subtotal</span>
          <span className="tabular-nums font-medium text-foreground">{formatMoney(subtotal)}</span>
        </div>
        <div className="flex items-center justify-between gap-3 text-[15px] font-bold tracking-tight text-foreground">
          <span>Total</span>
          <span className="tabular-nums">{formatMoney(subtotal)}</span>
        </div>
      </div>

      {/* Validation */}
      {validationMessage && (
        <p className="mt-3 rounded-lg bg-amber-50 px-3 py-2 text-[12.5px] font-semibold text-amber-800 ring-1 ring-amber-200">
          {validationMessage}
        </p>
      )}

      {/* CTA */}
      <Button
        className="mt-4 h-11 w-full"
        disabled={disabled}
        onClick={onProcessSale}
        type="button"
      >
        {isSubmitting ? 'Procesando…' : 'Procesar venta'}
      </Button>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
