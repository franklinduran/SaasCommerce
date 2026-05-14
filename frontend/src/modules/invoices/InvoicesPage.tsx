import { ReceiptText } from 'lucide-react'
import { ModulePage } from '@/shared/components/ModulePage'

export function InvoicesPage() {
  return (
    <ModulePage
      eyebrow="Documentos"
      icon={ReceiptText}
      rows={[
        { label: 'Facturas', value: '0 emitidas', status: 'Base' },
        { label: 'Recibos', value: '0 emitidos', status: 'Base' },
        { label: 'NCF', value: 'Pendiente', status: 'Base' },
      ]}
      title="Facturas"
    />
  )
}
