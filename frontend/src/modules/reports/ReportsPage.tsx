import { BarChart3 } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function ReportsPage() {
  return (
    <ModulePage
      eyebrow="Analitica"
      icon={BarChart3}
      rows={[
        { label: 'Ventas', value: '0 periodos', status: 'Base' },
        { label: 'Margenes', value: '0 calculos', status: 'Base' },
        { label: 'Inventario', value: '0 cortes', status: 'Base' },
      ]}
      title="Reportes"
    />
  )
}
