import { Printer } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'

export function PrintReceiptButton() {
  return (
    <Button className="print:hidden" onClick={() => window.print()} type="button">
      <Printer size={16} />
      Imprimir recibo
    </Button>
  )
}
