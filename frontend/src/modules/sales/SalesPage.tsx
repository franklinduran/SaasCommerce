import { ShoppingCart } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function SalesPage() {
  return (
    <ModulePage
      eyebrow="Caja"
      icon={ShoppingCart}
      rows={[
        { label: 'Ticket actual', value: 'RD$ 0.00', status: 'Listo' },
        { label: 'Pagos', value: '0 capturas', status: 'Base' },
        { label: 'Devoluciones', value: '0 pendientes', status: 'Base' },
      ]}
      title="POS"
    />
  )
}
