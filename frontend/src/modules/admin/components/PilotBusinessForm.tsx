import { useState } from 'react'
import type { FormEvent } from 'react'
import type { CreatePilotBusinessRequest } from '@/modules/admin/types'

type Props = {
  onSubmit: (data: CreatePilotBusinessRequest) => void
  isSubmitting: boolean
  successMessage?: string
  errorMessage?: string
}

const IDENTIFICATION_TYPES = [
  { value: 'Rnc', label: 'RNC' },
  { value: 'Cedula', label: 'Cédula' },
  { value: 'Passport', label: 'Pasaporte' },
]

export function PilotBusinessForm({
  onSubmit,
  isSubmitting,
  successMessage,
  errorMessage,
}: Props) {
  const [form, setForm] = useState<CreatePilotBusinessRequest>({
    businessName: '',
    identificationType: 'Rnc',
    identificationNumber: '',
    phone: '',
    branchName: 'Sucursal Principal',
    adminFullName: '',
    adminEmail: '',
    adminPassword: '',
  })

  const [errors, setErrors] = useState<Partial<Record<keyof CreatePilotBusinessRequest, string>>>(
    {},
  )

  function validate(): boolean {
    const newErrors: typeof errors = {}

    if (!form.businessName.trim()) newErrors.businessName = 'El nombre es obligatorio'
    if (!form.identificationNumber.trim())
      newErrors.identificationNumber = 'El RNC/Cédula es obligatorio'
    if (!form.phone.trim()) newErrors.phone = 'El teléfono es obligatorio'
    if (!form.branchName.trim()) newErrors.branchName = 'El nombre de sucursal es obligatorio'
    if (!form.adminFullName.trim()) newErrors.adminFullName = 'El nombre del admin es obligatorio'
    if (!form.adminEmail.trim()) newErrors.adminEmail = 'El email es obligatorio'
    if (form.adminPassword.length < 8)
      newErrors.adminPassword = 'La contraseña debe tener al menos 8 caracteres'

    setErrors(newErrors)

    return Object.keys(newErrors).length === 0
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (validate()) {
      onSubmit(form)
    }
  }

  function update(field: keyof CreatePilotBusinessRequest, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }))
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }))
    }
  }

  return (
    <form className="space-y-4" onSubmit={handleSubmit}>
      <div className="grid grid-cols-2 gap-4">
        <div className="col-span-2">
          <label className="mb-1 block text-sm font-medium text-gray-700" htmlFor="businessName">
            Nombre del negocio *
          </label>
          <input
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="businessName"
            onChange={(e) => update('businessName', e.target.value)}
            placeholder="Colmado El Buen Precio SRL"
            type="text"
            value={form.businessName}
          />
          {errors.businessName && (
            <p className="mt-1 text-xs text-red-600">{errors.businessName}</p>
          )}
        </div>

        <div>
          <label
            className="mb-1 block text-sm font-medium text-gray-700"
            htmlFor="identificationType"
          >
            Tipo de ID *
          </label>
          <select
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="identificationType"
            onChange={(e) => update('identificationType', e.target.value)}
            value={form.identificationType}
          >
            {IDENTIFICATION_TYPES.map((t) => (
              <option key={t.value} value={t.value}>
                {t.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label
            className="mb-1 block text-sm font-medium text-gray-700"
            htmlFor="identificationNumber"
          >
            Número de ID *
          </label>
          <input
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="identificationNumber"
            onChange={(e) => update('identificationNumber', e.target.value)}
            placeholder="132001234"
            type="text"
            value={form.identificationNumber}
          />
          {errors.identificationNumber && (
            <p className="mt-1 text-xs text-red-600">{errors.identificationNumber}</p>
          )}
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700" htmlFor="phone">
            Teléfono *
          </label>
          <input
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="phone"
            onChange={(e) => update('phone', e.target.value)}
            placeholder="8091234567"
            type="tel"
            value={form.phone}
          />
          {errors.phone && <p className="mt-1 text-xs text-red-600">{errors.phone}</p>}
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700" htmlFor="branchName">
            Nombre de sucursal *
          </label>
          <input
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="branchName"
            onChange={(e) => update('branchName', e.target.value)}
            type="text"
            value={form.branchName}
          />
          {errors.branchName && (
            <p className="mt-1 text-xs text-red-600">{errors.branchName}</p>
          )}
        </div>

        <div className="col-span-2 border-t pt-4">
          <h3 className="mb-3 text-sm font-semibold text-gray-700">
            Credenciales del administrador
          </h3>
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700" htmlFor="adminFullName">
            Nombre completo *
          </label>
          <input
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="adminFullName"
            onChange={(e) => update('adminFullName', e.target.value)}
            placeholder="Ana Belkis García"
            type="text"
            value={form.adminFullName}
          />
          {errors.adminFullName && (
            <p className="mt-1 text-xs text-red-600">{errors.adminFullName}</p>
          )}
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700" htmlFor="adminEmail">
            Email *
          </label>
          <input
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="adminEmail"
            onChange={(e) => update('adminEmail', e.target.value)}
            placeholder="admin@negocio.com"
            type="email"
            value={form.adminEmail}
          />
          {errors.adminEmail && (
            <p className="mt-1 text-xs text-red-600">{errors.adminEmail}</p>
          )}
        </div>

        <div className="col-span-2">
          <label
            className="mb-1 block text-sm font-medium text-gray-700"
            htmlFor="adminPassword"
          >
            Contraseña temporal *
          </label>
          <input
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            id="adminPassword"
            onChange={(e) => update('adminPassword', e.target.value)}
            placeholder="Mín. 8 caracteres"
            type="password"
            value={form.adminPassword}
          />
          {errors.adminPassword && (
            <p className="mt-1 text-xs text-red-600">{errors.adminPassword}</p>
          )}
        </div>
      </div>

      {errorMessage && (
        <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {errorMessage}
        </div>
      )}

      {successMessage && (
        <div className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-700">
          {successMessage}
        </div>
      )}

      <button
        className="w-full rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-60"
        disabled={isSubmitting}
        type="submit"
      >
        {isSubmitting ? 'Creando negocio…' : 'Crear negocio piloto'}
      </button>
    </form>
  )
}
