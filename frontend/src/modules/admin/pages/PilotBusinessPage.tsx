import { useState } from 'react'
import { useCreatePilotBusiness } from '@/modules/admin/hooks/usePilotBusiness'
import { PilotBusinessForm } from '@/modules/admin/components/PilotBusinessForm'
import type { CreatePilotBusinessResponse } from '@/modules/admin/types'

export function PilotBusinessPage() {
  const { mutateAsync, isPending } = useCreatePilotBusiness()
  const [created, setCreated] = useState<CreatePilotBusinessResponse | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | undefined>()

  async function handleSubmit(data: Parameters<typeof mutateAsync>[0]) {
    setErrorMessage(undefined)
    setCreated(null)

    try {
      const result = await mutateAsync(data)
      setCreated(result)
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Error al crear el negocio piloto.'
      setErrorMessage(message)
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6 py-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Nuevo negocio piloto</h1>
        <p className="mt-1 text-gray-600">
          Registra un nuevo negocio en período de prueba (14 días gratis).
        </p>
      </div>

      {created ? (
        <div className="rounded-lg border border-green-200 bg-green-50 p-6 space-y-4">
          <h2 className="text-lg font-semibold text-green-800">
            ✅ Negocio creado exitosamente
          </h2>
          <dl className="grid grid-cols-2 gap-x-4 gap-y-2 text-sm">
            <dt className="font-medium text-gray-600">Negocio</dt>
            <dd className="text-gray-900">{created.businessName}</dd>
            <dt className="font-medium text-gray-600">Email del admin</dt>
            <dd className="text-gray-900">{created.adminEmail}</dd>
            <dt className="font-medium text-gray-600">Sucursal</dt>
            <dd className="text-gray-900">{created.branchName}</dd>
            {created.trialEndsAt && (
              <>
                <dt className="font-medium text-gray-600">Trial hasta</dt>
                <dd className="text-gray-900">
                  {new Date(created.trialEndsAt).toLocaleDateString('es-DO', {
                    day: '2-digit',
                    month: 'long',
                    year: 'numeric',
                  })}
                </dd>
              </>
            )}
          </dl>
          <button
            className="rounded-md bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700"
            onClick={() => setCreated(null)}
            type="button"
          >
            Registrar otro negocio
          </button>
        </div>
      ) : (
        <div className="rounded-lg border border-gray-200 bg-white p-6">
          <PilotBusinessForm
            errorMessage={errorMessage}
            isSubmitting={isPending}
            onSubmit={handleSubmit}
          />
        </div>
      )}
    </div>
  )
}
