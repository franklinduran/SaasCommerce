import { Shield } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

const placeholderRows = [
  { label: 'Gestion de usuarios', value: 'Roles y permisos', status: 'Activo' },
]

export default function UsersPage() {
  return (
    <ModulePage
      eyebrow="Seguridad"
      icon={Shield}
      rows={placeholderRows}
      title="Usuarios"
    />
  )
}
