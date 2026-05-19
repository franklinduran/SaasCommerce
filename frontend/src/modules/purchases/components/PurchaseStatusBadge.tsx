import type { PurchaseStatus } from '@/modules/purchases/types'

export function PurchaseStatusBadge({ status }: Readonly<{ status: PurchaseStatus }>) {
  const className = getClassName(status)

  let label: string
  if (status === 'Received') {
    label = 'Recibida'
  } else if (status === 'Cancelled') {
    label = 'Cancelada'
  } else {
    label = 'Borrador'
  }

  return <span className={className}>{label}</span>
}

function getClassName(status: PurchaseStatus) {
  if (status === 'Received') {
    return 'rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200'
  }

  if (status === 'Cancelled') {
    return 'rounded-full bg-red-50 px-2.5 py-1 text-xs font-semibold text-red-700 ring-1 ring-red-200'
  }

  return 'rounded-full bg-amber-50 px-2.5 py-1 text-xs font-semibold text-amber-700 ring-1 ring-amber-200'
}
