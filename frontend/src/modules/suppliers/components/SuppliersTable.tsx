import { Pencil, RefreshCw, ToggleLeft, ToggleRight } from 'lucide-react'
import { SupplierStatusBadge } from '@/modules/suppliers/components/SupplierStatusBadge'
import type { Supplier } from '@/modules/suppliers/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

type SuppliersTableProps = {
  error: boolean
  isLoading: boolean
  items: Supplier[]
  onEdit: (supplier: Supplier) => void
  onRetry: () => void
  onToggle: (supplier: Supplier) => void
}

export function SuppliersTable({
  error,
  isLoading,
  items,
  onEdit,
  onRetry,
  onToggle,
}: Readonly<SuppliersTableProps>) {
  return (
    <Card className="overflow-hidden">
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <h3 className="text-base font-semibold text-stone-950">Proveedores</h3>
          <p className="text-sm font-medium text-stone-600">{items.length} visibles</p>
        </div>
        <Button onClick={onRetry} type="button" variant="ghost">
          <RefreshCw size={16} />
          Reintentar
        </Button>
      </CardHeader>
      <div className="overflow-x-auto">
        <table className="w-full min-w-220 text-left text-sm">
          <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
            <tr>
              <th className="px-5 py-3">Proveedor</th>
              <th className="px-5 py-3">Contacto</th>
              <th className="px-5 py-3">RNC</th>
              <th className="px-5 py-3">Estado</th>
              <th className="px-5 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-stone-200">
            {isLoading && (
              <tr>
                <td className="px-5 py-8 text-stone-500" colSpan={5}>Cargando proveedores...</td>
              </tr>
            )}
            {error && (
              <tr>
                <td className="px-5 py-8 text-red-700" colSpan={5}>No se pudo cargar proveedores.</td>
              </tr>
            )}
            {!isLoading && !error && items.length === 0 && (
              <tr>
                <td className="px-5 py-10 text-center text-stone-600" colSpan={5}>
                  No hay proveedores registrados.
                </td>
              </tr>
            )}
            {items.map((supplier) => (
              <tr className="bg-white hover:bg-stone-50" key={supplier.id}>
                <td className="px-5 py-4">
                  <p className="font-semibold text-stone-950">{supplier.name}</p>
                  <p className="mt-1 text-xs font-medium text-stone-500">{supplier.address ?? 'Sin direccion'}</p>
                </td>
                <td className="px-5 py-4">
                  <p className="font-medium text-stone-800">{supplier.phone ?? 'Sin telefono'}</p>
                  <p className="mt-1 text-xs font-medium text-stone-500">{supplier.email ?? 'Sin correo'}</p>
                </td>
                <td className="px-5 py-4 font-mono text-xs font-semibold text-stone-700">
                  {supplier.rnc ?? 'N/A'}
                </td>
                <td className="px-5 py-4"><SupplierStatusBadge isActive={supplier.isActive} /></td>
                <td className="px-5 py-4">
                  <div className="flex justify-end gap-2">
                    <Button onClick={() => onEdit(supplier)} size="sm" type="button" variant="secondary">
                      <Pencil size={14} />
                      Editar
                    </Button>
                    <Button onClick={() => onToggle(supplier)} size="sm" type="button" variant="ghost">
                      {supplier.isActive ? <ToggleLeft size={14} /> : <ToggleRight size={14} />}
                      {supplier.isActive ? 'Desactivar' : 'Activar'}
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  )
}
