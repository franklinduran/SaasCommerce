const paymentMethodLabels: Record<string, string> = {
  Cash: 'Efectivo',
  Transfer: 'Transferencia',
  Card: 'Tarjeta',
  Credit: 'Crédito',
}

export function formatPaymentMethod(value: string): string {
  return paymentMethodLabels[value] ?? value
}

export function formatCurrency(value: number): string {
  return value.toLocaleString('es-DO', {
    currency: 'DOP',
    minimumFractionDigits: 2,
    style: 'currency',
  })
}

export function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat('es-DO', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function formatQuantity(value: number): string {
  return value.toLocaleString('es-DO', {
    maximumFractionDigits: 3,
    minimumFractionDigits: Number.isInteger(value) ? 0 : 3,
  })
}
