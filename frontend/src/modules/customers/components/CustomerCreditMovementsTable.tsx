import type { CustomerCreditMovement } from '@/modules/customers/types'

type CustomerCreditMovementsTableProps = {
  movements: CustomerCreditMovement[]
  isError: boolean
  isLoading: boolean
}

export function CustomerCreditMovementsTable({
  movements,
  isError,
  isLoading,
}: Readonly<CustomerCreditMovementsTableProps>) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-200 text-left text-sm">
        <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-5 py-3">Fecha</th>
            <th className="px-5 py-3">Tipo</th>
            <th className="px-5 py-3 text-right">Monto</th>
            <th className="px-5 py-3 text-right">Balance</th>
            <th className="px-5 py-3">Nota</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200 bg-white">
          {isLoading && (
            <tr><td className="px-5 py-8 text-stone-500" colSpan={5}>Cargando movimientos...</td></tr>
          )}
          {isError && (
            <tr><td className="px-5 py-8 text-red-700" colSpan={5}>No se pudo cargar el historial.</td></tr>
          )}
          {!isLoading && !isError && movements.length === 0 && (
            <tr><td className="px-5 py-10 text-center text-stone-600" colSpan={5}>Sin movimientos de credito.</td></tr>
          )}
          {movements.map((movement) => (
            <tr className="hover:bg-stone-50" key={movement.id}>
              <td className="px-5 py-4 font-medium text-stone-700">{formatDate(movement.createdAt)}</td>
              <td className="px-5 py-4 font-semibold text-stone-950">{movementLabel(movement.type)}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-950">{formatMoney(movement.amount)}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-950">{formatMoney(movement.newBalance)}</td>
              <td className="px-5 py-4 text-stone-600">{movement.note ?? 'Sin nota'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function movementLabel(type: CustomerCreditMovement['type']) {
  return {
    Adjustment: 'Ajuste',
    Cancellation: 'Reverso',
    Debit: 'Crédito',
    Payment: 'Abono',
  }[type]
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('es-DO', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}
