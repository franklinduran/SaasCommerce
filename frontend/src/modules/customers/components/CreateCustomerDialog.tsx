import { UserPlus } from 'lucide-react'
import { useState } from 'react'
import { customerSchema } from '@/modules/customers/schemas/customerSchemas'
import type { CustomerUpsertRequest } from '@/modules/customers/types'
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

type CreateCustomerDialogProps = {
  open: boolean
  isSubmitting: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: (request: CustomerUpsertRequest) => void
}

export function CreateCustomerDialog({
  isSubmitting,
  onOpenChange,
  onSubmit,
  open,
}: Readonly<CreateCustomerDialogProps>) {
  const [fullName, setFullName] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [error, setError] = useState<string | null>(null)

  function resetState() {
    setFullName('')
    setPhone('')
    setEmail('')
    setError(null)
  }

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) {
      resetState()
    }
    onOpenChange(nextOpen)
  }

  function handleSubmit() {
    const payload = {
      email: email.trim() || null,
      fullName: fullName.trim(),
      isActive: true,
      phone: phone.trim() || null,
    }
    const validation = customerSchema.safeParse(payload)
    if (!validation.success) {
      setError(validation.error.issues[0]?.message ?? 'Revisa los datos del cliente.')
      return
    }
    setError(null)
    onSubmit(payload)
  }

  return (
    <Dialog onOpenChange={handleOpenChange} open={open}>
      <DialogContent>
        <DialogHeader>
          <div className="flex items-start gap-3">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
              <UserPlus size={18} />
            </span>
            <div>
              <DialogTitle>Crear cliente</DialogTitle>
              <DialogDescription>
                Captura los datos basicos para empezar a vender o registrar fiados.
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
            <Label htmlFor="fullName">Nombre completo</Label>
            <Input
              disabled={isSubmitting}
              id="fullName"
              onChange={(e) => setFullName(e.target.value)}
              placeholder="Maria Sanchez"
              value={fullName}
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="phone">Telefono</Label>
              <Input
                disabled={isSubmitting}
                id="phone"
                onChange={(e) => setPhone(e.target.value)}
                placeholder="8090000000"
                value={phone}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="email">Correo</Label>
              <Input
                disabled={isSubmitting}
                id="email"
                onChange={(e) => setEmail(e.target.value)}
                placeholder="cliente@correo.com"
                type="email"
                value={email}
              />
            </div>
          </div>
        </form>

        <DialogFooter>
          <Button
            disabled={isSubmitting}
            onClick={() => handleOpenChange(false)}
            type="button"
            variant="secondary"
          >
            Cancelar
          </Button>
          <Button disabled={isSubmitting} onClick={handleSubmit} type="button">
            <UserPlus size={15} />
            {isSubmitting ? 'Creando...' : 'Crear cliente'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
