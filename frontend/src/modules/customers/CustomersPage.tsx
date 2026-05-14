import { Users } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function CustomersPage() {
  return (
    <ModulePage
      eyebrow="Cuentas"
      icon={Users}
      rows={[
        { label: 'Clientes', value: '0 registros', status: 'Base' },
        { label: 'Fiados', value: 'RD$ 0.00', status: 'Base' },
        { label: 'Limites', value: '0 definidos', status: 'Base' },
      ]}
      title="Clientes"
    />
  )
}
