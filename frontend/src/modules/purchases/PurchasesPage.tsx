import { Truck } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function PurchasesPage() {
  return (
    <ModulePage
      eyebrow="Abastecimiento"
      icon={Truck}
      rows={[
        { label: 'Ordenes', value: '0 abiertas', status: 'Base' },
        { label: 'Proveedores', value: '0 registros', status: 'Base' },
        { label: 'Recepciones', value: '0 pendientes', status: 'Base' },
      ]}
      title="Compras"
    />
  )
}
