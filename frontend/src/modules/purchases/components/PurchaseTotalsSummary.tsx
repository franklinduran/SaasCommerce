export function PurchaseTotalsSummary({
  itemCount,
  total,
}: Readonly<{
  itemCount: number
  total: number
}>) {
  return (
    <div className="grid gap-3 rounded-md bg-stone-50 p-4 ring-1 ring-stone-200 sm:grid-cols-2">
      <div>
        <p className="text-xs font-semibold uppercase text-stone-500">Productos</p>
        <p className="mt-1 text-xl font-semibold text-stone-950">{itemCount}</p>
      </div>
      <div className="sm:text-right">
        <p className="text-xs font-semibold uppercase text-stone-500">Total</p>
        <p className="mt-1 text-xl font-semibold text-stone-950">{formatMoney(total)}</p>
      </div>
    </div>
  )
}

export function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`
}
