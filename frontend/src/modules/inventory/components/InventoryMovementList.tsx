import type { InventoryMovement } from '@/modules/inventory/types'

export function InventoryMovementList({ movements }: Readonly<{ movements: InventoryMovement[] }>) {
  if (movements.length === 0) {
    return <p className="rounded-md bg-white p-5 text-sm font-medium text-stone-600 ring-1 ring-stone-200">Sin movimientos recientes.</p>
  }

  return (
    <div className="overflow-x-auto rounded-md bg-white shadow-sm ring-1 ring-stone-200">
      <table className="w-full min-w-170 text-left text-sm">
        <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-5 py-3">Tipo</th>
            <th className="px-5 py-3">Sucursal</th>
            <th className="px-5 py-3 text-right">Cantidad</th>
            <th className="px-5 py-3 text-right">Anterior</th>
            <th className="px-5 py-3 text-right">Nuevo</th>
            <th className="px-5 py-3">Fecha</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200">
          {movements.map((movement) => (
            <tr key={movement.id}>
              <td className="px-5 py-4 font-semibold text-stone-950">{movement.reason}</td>
              <td className="px-5 py-4 font-medium text-stone-700">{movement.branchName ?? movement.branchId}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-700">{movement.quantity}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-700">{movement.previousStock}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-950">{movement.newStock}</td>
              <td className="px-5 py-4 font-medium text-stone-600">{formatDate(movement.createdAt)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value))
}
