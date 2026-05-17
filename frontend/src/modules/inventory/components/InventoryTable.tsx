import { Link } from 'react-router-dom'
import { StockStatusBadge } from '@/modules/inventory/components/StockStatusBadge'
import type { StockItem } from '@/modules/inventory/types'
import { Button } from '@/shared/components/ui/button'

export function InventoryTable({
  error,
  isLoading,
  items,
  onRetry,
}: Readonly<{
  error: boolean
  isLoading: boolean
  items: StockItem[]
  onRetry: () => void
}>) {
  return (
    <div className="overflow-x-auto rounded-md bg-white shadow-sm ring-1 ring-stone-200">
      <table className="w-full min-w-210 text-left text-sm">
        <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-5 py-3">Producto</th>
            <th className="px-5 py-3">Sucursal</th>
            <th className="px-5 py-3 text-right">Stock actual</th>
            <th className="px-5 py-3 text-right">Stock minimo</th>
            <th className="px-5 py-3">Estado</th>
            <th className="px-5 py-3">Ultima actualizacion</th>
            <th className="px-5 py-3" />
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200">
          {isLoading && <Row colSpan={7} text="Cargando inventario..." />}
          {error && (
            <tr>
              <td className="px-5 py-10 text-center" colSpan={7}>
                <p className="text-sm font-medium text-red-700">No se pudo cargar el inventario.</p>
                <Button className="mt-3" onClick={onRetry} type="button" variant="secondary">
                  Reintentar
                </Button>
              </td>
            </tr>
          )}
          {!isLoading && !error && items.length === 0 && (
            <Row colSpan={7} text="No hay inventario con los filtros actuales." />
          )}
          {!isLoading && !error && items.map((item) => (
            <tr className="bg-white hover:bg-stone-50" key={item.id}>
              <td className="px-5 py-4">
                <p className="font-semibold text-stone-950">{item.productName}</p>
                <p className="mt-1 text-xs font-medium text-stone-500">{item.sku || 'Sin SKU'}</p>
              </td>
              <td className="px-5 py-4 font-semibold text-stone-700">{item.branchName ?? item.branchId}</td>
              <td className="px-5 py-4 text-right text-base font-semibold text-stone-950">{item.quantity}</td>
              <td className="px-5 py-4 text-right font-semibold text-stone-700">{item.minimumStock ?? '-'}</td>
              <td className="px-5 py-4">
                <StockStatusBadge status={item.status} />
              </td>
              <td className="px-5 py-4 font-medium text-stone-600">{formatDate(item.lastUpdatedAt)}</td>
              <td className="px-5 py-4 text-right">
                <Link className="text-sm font-semibold text-stone-950 underline-offset-4 hover:underline" to={`/inventory/products/${item.productId}`}>
                  Ver detalle
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function Row({ colSpan, text }: Readonly<{ colSpan: number; text: string }>) {
  return (
    <tr>
      <td className="px-5 py-10 text-center text-sm font-medium text-stone-600" colSpan={colSpan}>
        {text}
      </td>
    </tr>
  )
}

function formatDate(value: string | null) {
  return value ? new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value)) : '-'
}
