import { Trash2 } from 'lucide-react'
import type { Product } from '@/modules/products/types'
import { formatMoney } from '@/modules/purchases/components/PurchaseTotalsSummary'
import { Button } from '@/shared/components/ui/button'

export type DraftPurchaseItem = {
  productId: string
  quantity: number
  unitCost: number
}

type PurchaseItemsTableProps = {
  items: DraftPurchaseItem[]
  products: Product[]
  onChange: (index: number, item: DraftPurchaseItem) => void
  onRemove: (index: number) => void
}

const inputClass =
  'h-10 w-full rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'

export function PurchaseItemsTable({
  items,
  products,
  onChange,
  onRemove,
}: Readonly<PurchaseItemsTableProps>) {
  return (
    <div className="overflow-x-auto rounded-md ring-1 ring-stone-200">
      <table className="w-full min-w-180 text-left text-sm">
        <thead className="bg-stone-50 text-xs font-semibold uppercase text-stone-600">
          <tr>
            <th className="px-4 py-3">Producto</th>
            <th className="px-4 py-3 text-right">Cantidad</th>
            <th className="px-4 py-3 text-right">Costo</th>
            <th className="px-4 py-3 text-right">Subtotal</th>
            <th className="px-4 py-3 text-right">Acciones</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200 bg-white">
          {items.map((item, index) => {
            const product = products.find((candidate) => candidate.id === item.productId)
            const subtotal = item.quantity * item.unitCost

            return (
              <tr key={`${index}-${item.productId || 'empty'}`}>
                <td className="px-4 py-3">
                  <select
                    className={inputClass}
                    onChange={(event) => {
                      const nextProduct = products.find((candidate) => candidate.id === event.target.value)
                      onChange(index, {
                        ...item,
                        productId: event.target.value,
                        unitCost: nextProduct?.costPrice ?? item.unitCost,
                      })
                    }}
                    value={item.productId}
                  >
                    <option value="">Seleccionar producto</option>
                    {products.map((candidate) => (
                      <option key={candidate.id} value={candidate.id}>
                        {candidate.name} / {candidate.sku}
                      </option>
                    ))}
                  </select>
                  {product && <p className="mt-1 text-xs font-medium text-stone-500">{product.sku}</p>}
                </td>
                <td className="px-4 py-3">
                  <input
                    className={`${inputClass} text-right`}
                    min="0"
                    onChange={(event) => onChange(index, { ...item, quantity: Number(event.target.value) })}
                    step="0.001"
                    type="number"
                    value={item.quantity}
                  />
                </td>
                <td className="px-4 py-3">
                  <input
                    className={`${inputClass} text-right`}
                    min="0"
                    onChange={(event) => onChange(index, { ...item, unitCost: Number(event.target.value) })}
                    step="0.01"
                    type="number"
                    value={item.unitCost}
                  />
                </td>
                <td className="px-4 py-3 text-right font-semibold text-stone-950">
                  {formatMoney(subtotal)}
                </td>
                <td className="px-4 py-3 text-right">
                  <Button aria-label="Eliminar producto" onClick={() => onRemove(index)} size="icon" type="button" variant="ghost">
                    <Trash2 size={16} />
                  </Button>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
