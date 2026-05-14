import { Boxes } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function InventoryPage() {
  return (
    <ModulePage
      eyebrow="Stock"
      icon={Boxes}
      rows={[
        { label: 'Existencias', value: '0 movimientos', status: 'Base' },
        { label: 'Ajustes', value: '0 pendientes', status: 'Base' },
        { label: 'Alertas', value: '0 activas', status: 'Base' },
      ]}
      title="Inventario"
    />
  )
}
