import { useState, type FormEvent } from 'react'
import { X } from 'lucide-react'
import { registerCustomerPaymentSchema } from '@/modules/customers/schemas/customerSchemas'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

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

  if (!isOpen) {
    return null
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
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
    <div className="fixed inset-0 z-50 grid place-items-center bg-stone-950/35 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="flex flex-row items-center justify-between gap-3 border-b border-stone-200">
          <div>
            <h3 className="text-lg font-semibold text-stone-950">Registrar abono</h3>
            <p className="mt-1 text-sm font-medium text-stone-600">Aplica al balance pendiente.</p>
          </div>
          <Button aria-label="Cerrar" onClick={onClose} size="icon" type="button" variant="ghost">
            <X size={18} />
          </Button>
        </CardHeader>
        <form className="space-y-4 p-5" onSubmit={handleSubmit}>
          <label className="grid gap-1 text-sm font-semibold text-stone-700">
            Monto
            <input
              className={inputClass}
              min="0"
              onChange={(event) => setAmount(event.target.value)}
              step="0.01"
              type="number"
              value={amount}
            />
          </label>
          <label className="grid gap-1 text-sm font-semibold text-stone-700">
            Nota
            <textarea
              className="min-h-24 rounded-md bg-white px-3 py-2 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15"
              onChange={(event) => setNote(event.target.value)}
              value={note}
            />
          </label>
          {error && <p className="text-sm font-semibold text-red-700">{error}</p>}
          <div className="flex justify-end gap-2">
            <Button onClick={onClose} type="button" variant="secondary">Cancelar</Button>
            <Button disabled={isSubmitting} type="submit">Registrar</Button>
          </div>
        </form>
      </Card>
    </div>
  )
}

const inputClass =
  'h-10 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'
