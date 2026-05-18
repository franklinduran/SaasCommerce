import { ClipboardList } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

const placeholderRows = [
  { label: 'Registro de auditoria', value: 'Acciones del sistema', status: 'Activo' },
]

export default function AuditLogsPage() {
  return (
    <ModulePage
      eyebrow="Seguridad"
      icon={ClipboardList}
      rows={placeholderRows}
      title="Auditoria"
    />
  )
}
