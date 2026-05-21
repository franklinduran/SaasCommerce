import { WalletCards } from 'lucide-react'
import { useState } from 'react'
import { registerCustomerPaymentSchema } from '@/modules/customers/schemas/customerSchemas'
import { Button } from '@/shared/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'

type RegisterCustomerPaymentDialogProps = {
  isOpen: boolean
  isSubmitting: boolean
  onClose: () => void
  onSubmit: (amount: number, note: string | null) => void
}

export function RegisterCustomerPaymentDialog({
  isOpen,
  isSubmitting,
  onClose,
  onSubmit,
}: Readonly<RegisterCustomerPaymentDialogProps>) {
  const [amount, setAmount] = useState('')
  const [note, setNote] = useState('')
  const [error, setError] = useState<string | null>(null)

  function resetState() {
    setAmount('')
    setNote('')
    setError(null)
  }

  function handleOpenChange(open: boolean) {
    if (!open) {
      resetState()
      onClose()
    }
  }

  function handleSubmit() {
    const parsedAmount = Number(amount)
    const validation = registerCustomerPaymentSchema.safeParse({
      amount: parsedAmount,
      note,
    })
    if (!validation.success) {
      setError(validation.error.issues[0]?.message ?? 'Revisa el monto.')
      return
    }
    setError(null)
    onSubmit(parsedAmount, note.trim() || null)
  }

  return (
    <Dialog onOpenChange={handleOpenChange} open={isOpen}>
      <DialogContent>
        <DialogHeader>
          <div className="flex items-start gap-3">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
              <WalletCards size={18} />
            </span>
            <div>
              <DialogTitle>Registrar abono</DialogTitle>
              <DialogDescription>
                Aplica un pago al balance pendiente del cliente.
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <form
          className="space-y-4"
          onSubmit={(event) => {
            event.preventDefault()
            handleSubmit()
          }}
        >
          {error && (
            <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
              {error}
            </div>
          )}

          <div className="space-y-1.5">
            <Label htmlFor="amount">Monto del abono</Label>
            <Input
              disabled={isSubmitting}
              id="amount"
              min="0"
              onChange={(e) => setAmount(e.target.value)}
              placeholder="0.00"
              step="0.01"
              type="number"
              value={amount}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="note">Nota (opcional)</Label>
            <textarea
              className="min-h-24 w-full rounded-md bg-white px-3 py-2 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
              disabled={isSubmitting}
              id="note"
              onChange={(e) => setNote(e.target.value)}
              placeholder="Detalles del abono"
              value={note}
            />
          </div>
        </form>

        <DialogFooter>
          <Button disabled={isSubmitting} onClick={() => handleOpenChange(false)} type="button" variant="secondary">
            Cancelar
          </Button>
          <Button disabled={isSubmitting} onClick={handleSubmit} type="button">
            {isSubmitting ? 'Registrando...' : 'Registrar abono'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
