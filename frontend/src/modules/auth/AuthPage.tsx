import { ShieldCheck } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function AuthPage() {
  return (
    <ModulePage
      eyebrow="Acceso"
      icon={ShieldCheck}
      rows={[
        { label: 'Usuarios', value: 'Pendiente', status: 'Base' },
        { label: 'Roles', value: 'Pendiente', status: 'Base' },
        { label: 'Sesiones', value: 'Pendiente', status: 'Base' },
      ]}
      title="Autenticacion"
    />
  )
}
