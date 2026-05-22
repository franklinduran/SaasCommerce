import type { InventoryTransferStatus } from '@/modules/inventory-transfers/types'

const config: Record<InventoryTransferStatus, { label: string; className: string }> = {
  Cancelled: {
    className: 'rounded-full bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-500 ring-1 ring-stone-200',
    label: 'Cancelada',
  },
  Completed: {
    className: 'rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200',
    label: 'Completada',
  },
  Failed: {
    className: 'rounded-full bg-red-50 px-2.5 py-1 text-xs font-semibold text-red-700 ring-1 ring-red-200',
    label: 'Fallida',
  },
  Pending: {
    className: 'rounded-full bg-amber-50 px-2.5 py-1 text-xs font-semibold text-amber-700 ring-1 ring-amber-200',
    label: 'Pendiente',
  },
}

export function InventoryTransferStatusBadge({
  status,
}: Readonly<{ status: InventoryTransferStatus }>) {
  const { className, label } = config[status] ?? config.Pending

  return <span className={className}>{label}</span>
}
