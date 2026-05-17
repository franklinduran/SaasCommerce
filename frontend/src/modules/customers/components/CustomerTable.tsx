import { Eye } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { Customer } from '@/modules/customers/types'

type CustomerTableProps = {
  customers: Customer[]
  isError: boolean
  isLoading: boolean
}

export function CustomerTable({ customers, isError, isLoading }: Readonly<CustomerTableProps>) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-200 text-left text-sm">
        <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-5 py-3">Cliente</th>
            <th className="px-5 py-3">Telefono</th>
            <th className="px-5 py-3 text-right">Balance</th>
            <th className="px-5 py-3">Credito</th>
            <th className="px-5 py-3 text-right">Acciones</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200 bg-white">
          {isLoading && (
            <tr><td className="px-5 py-8 text-stone-500" colSpan={5}>Cargando clientes...</td></tr>
          )}
          {isError && (
            <tr><td className="px-5 py-8 text-red-700" colSpan={5}>No se pudieron cargar clientes.</td></tr>
          )}
          {!isLoading && !isError && customers.length === 0 && (
            <tr><td className="px-5 py-10 text-center text-stone-600" colSpan={5}>No hay clientes registrados.</td></tr>
          )}
          {customers.map((customer) => (
            <tr className="hover:bg-stone-50" key={customer.id}>
              <td className="px-5 py-4">
                <p className="font-semibold text-stone-950">{customer.fullName}</p>
                <p className="mt-1 text-xs font-medium text-stone-500">{customer.email ?? 'Sin email'}</p>
              </td>
              <td className="px-5 py-4 font-medium text-stone-700">{customer.phone ?? 'Sin telefono'}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-950">{formatMoney(customer.currentBalance)}</td>
              <td className="px-5 py-4"><CreditStatusBadge status={customer.creditStatus} /></td>
              <td className="px-5 py-4 text-right">
                <Link className={smallLinkClass} to={`/customers/${customer.id}`}>
                  <Eye size={14} />
                  Ver
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function CreditStatusBadge({ status }: Readonly<{ status: Customer['creditStatus'] }>) {
  const classes = {
    Active: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
    Blocked: 'bg-red-50 text-red-700 ring-red-200',
    Closed: 'bg-stone-100 text-stone-700 ring-stone-200',
  }[status]

  return (
    <span className={`inline-flex rounded-md px-2 py-1 text-xs font-semibold ring-1 ${classes}`}>
      {status === 'Active' ? 'Activo' : status === 'Blocked' ? 'Bloqueado' : 'Cerrado'}
    </span>
  )
}

const smallLinkClass =
  'inline-flex h-8 items-center justify-center gap-2 rounded-md bg-white px-3 text-xs font-semibold text-stone-900 shadow-sm ring-1 ring-stone-300 hover:bg-stone-50'

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
