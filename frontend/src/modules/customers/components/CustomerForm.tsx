import { useState, type FormEvent } from 'react'
import { Save } from 'lucide-react'
import { customerSchema } from '@/modules/customers/schemas/customerSchemas'
import type { Customer, CustomerUpsertRequest } from '@/modules/customers/types'
import { Button } from '@/shared/components/ui/button'

type CustomerFormProps = {
  customer?: Customer | null
  isSubmitting: boolean
  onSubmit: (request: CustomerUpsertRequest) => void
}

const inputClass =
  'h-10 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'

export function CustomerForm({
  customer,
  isSubmitting,
  onSubmit,
}: Readonly<CustomerFormProps>) {
  const [fullName, setFullName] = useState(customer?.fullName ?? '')
  const [phone, setPhone] = useState(customer?.phone ?? '')
  const [email, setEmail] = useState(customer?.email ?? '')
  const [error, setError] = useState<string | null>(null)

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const payload = {
      email: email.trim() || null,
      fullName,
      isActive: customer?.isActive ?? true,
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
    <form className="grid gap-3 md:grid-cols-[minmax(0,1.2fr)_180px_minmax(0,1fr)_auto]" onSubmit={handleSubmit}>
      <label className="grid gap-1 text-sm font-semibold text-stone-700">
        Nombre
        <input
          className={inputClass}
          onChange={(event) => setFullName(event.target.value)}
          placeholder="Cliente"
          value={fullName}
        />
      </label>
      <label className="grid gap-1 text-sm font-semibold text-stone-700">
        Telefono
        <input
          className={inputClass}
          onChange={(event) => setPhone(event.target.value)}
          placeholder="8095550000"
          value={phone}
        />
      </label>
      <label className="grid gap-1 text-sm font-semibold text-stone-700">
        Email
        <input
          className={inputClass}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="cliente@correo.com"
          value={email}
        />
      </label>
      <div className="flex flex-col justify-end gap-1">
        <Button disabled={isSubmitting} type="submit">
          <Save size={16} />
          Guardar
        </Button>
      </div>
      {error && <p className="text-sm font-semibold text-red-700 md:col-span-4">{error}</p>}
    </form>
  )
}
