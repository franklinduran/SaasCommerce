export function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`
}
