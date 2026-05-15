import { Boxes, History } from 'lucide-react'
import { InventoryAdjustmentForm } from '@/modules/inventory/components/InventoryAdjustmentForm'
import { useInventoryMovementsQuery, useStockQuery } from '@/modules/inventory/hooks/useInventory'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

export function InventoryPage() {
  const stock = useStockQuery()
  const movements = useInventoryMovementsQuery()
  const stockItems = stock.data?.items ?? []
  const movementItems = movements.data?.items ?? []

  return (
    <section className="space-y-6 p-6 lg:p-8">
      <div>
        <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">Stock</p>
        <h2 className="mt-1 text-2xl font-semibold text-stone-950">Inventario</h2>
        <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
          Controla existencias y movimientos sin mezclar reglas comerciales del catalogo.
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-md bg-stone-100 text-stone-900">
              <Boxes size={19} />
            </span>
            <div>
              <h3 className="text-base font-semibold text-stone-950">Ajuste de inventario</h3>
              <p className="text-sm font-medium text-stone-600">
                Cada ajuste crea un movimiento trazable con usuario y tenant.
              </p>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <InventoryAdjustmentForm />
        </CardContent>
      </Card>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_minmax(420px,0.8fr)]">
        <Card className="overflow-hidden">
          <CardHeader>
            <h3 className="text-base font-semibold text-stone-950">Existencias</h3>
            <p className="text-sm font-medium text-stone-600">{stock.data?.total ?? 0} productos con stock</p>
          </CardHeader>
          <div className="overflow-x-auto">
            <table className="w-full min-w-[620px] text-left text-sm">
              <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
                <tr>
                  <th className="px-5 py-3">Producto ID</th>
                  <th className="px-5 py-3 text-right">Cantidad</th>
                  <th className="px-5 py-3">Estado</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-stone-200">
                {stock.isLoading && <PlaceholderRows columns={3} />}
                {!stock.isLoading && stockItems.length === 0 && (
                  <EmptyRow columns={3} text="No hay existencias registradas." />
                )}
                {stock.isError && <ErrorRow columns={3} text="No se pudo cargar el stock." />}
                {stockItems.map((item) => (
                  <tr className="bg-white hover:bg-stone-50" key={item.id}>
                    <td className="px-5 py-4 font-mono text-xs font-semibold text-stone-700">{item.productId}</td>
                    <td className="px-5 py-4 text-right text-base font-semibold text-stone-950">{item.quantity}</td>
                    <td className="px-5 py-4">
                      <span className="rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200">
                        Disponible
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>

        <Card className="overflow-hidden">
          <CardHeader>
            <div className="flex items-center gap-2">
              <History className="text-stone-700" size={18} />
              <h3 className="text-base font-semibold text-stone-950">Movimientos recientes</h3>
            </div>
          </CardHeader>
          <div className="overflow-x-auto">
            <table className="w-full min-w-[520px] text-left text-sm">
              <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
                <tr>
                  <th className="px-5 py-3">Razon</th>
                  <th className="px-5 py-3 text-right">Cantidad</th>
                  <th className="px-5 py-3 text-right">Nuevo stock</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-stone-200">
                {movements.isLoading && <PlaceholderRows columns={3} />}
                {!movements.isLoading && movementItems.length === 0 && (
                  <EmptyRow columns={3} text="No hay movimientos todavia." />
                )}
                {movements.isError && <ErrorRow columns={3} text="No se pudo cargar movimientos." />}
                {movementItems.map((movement) => (
                  <tr className="bg-white hover:bg-stone-50" key={movement.id}>
                    <td className="px-5 py-4 font-semibold text-stone-950">{movement.reason}</td>
                    <td className="px-5 py-4 text-right font-semibold text-stone-700">{movement.quantity}</td>
                    <td className="px-5 py-4 text-right font-semibold text-stone-950">{movement.newStock}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      </div>
    </section>
  )
}

function PlaceholderRows({ columns }: { columns: number }) {
  return Array.from({ length: 4 }).map((_, index) => (
    <tr key={index}>
      <td className="px-5 py-4" colSpan={columns}>
        <div className="h-4 w-full rounded bg-stone-100" />
      </td>
    </tr>
  ))
}

function EmptyRow({ columns, text }: { columns: number; text: string }) {
  return (
    <tr>
      <td className="px-5 py-10 text-center text-sm font-medium text-stone-600" colSpan={columns}>
        {text}
      </td>
    </tr>
  )
}

function ErrorRow({ columns, text }: { columns: number; text: string }) {
  return (
    <tr>
      <td className="px-5 py-10 text-center text-sm font-medium text-red-700" colSpan={columns}>
        {text}
      </td>
    </tr>
  )
}
