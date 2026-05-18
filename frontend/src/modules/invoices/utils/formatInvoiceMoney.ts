export function formatInvoiceMoney(value: number): string {
  return new Intl.NumberFormat('es-DO', {
    currency: 'DOP',
    style: 'currency',
  }).format(value)
}
