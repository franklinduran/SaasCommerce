import { Package } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function ProductsPage() {
  return (
    <ModulePage
      eyebrow="Catalogo"
      icon={Package}
      rows={[
        { label: 'Productos activos', value: '0 registros', status: 'Base' },
        { label: 'Categorias', value: '0 registros', status: 'Base' },
        { label: 'Codigos de barra', value: '0 registros', status: 'Base' },
      ]}
      title="Productos"
    />
  )
}
