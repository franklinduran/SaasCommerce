import { CheckCircle2, Loader2 } from 'lucide-react'
import { useOnboardingStatus } from '@/modules/onboarding/hooks/useOnboardingStatus'
import { OnboardingStepCard } from '@/modules/onboarding/components/OnboardingStepCard'
import type { OnboardingStep } from '@/modules/onboarding/types'

export function OnboardingPage() {
  const { data: status, isLoading, error } = useOnboardingStatus()

  const steps: OnboardingStep[] = status
    ? [
        {
          key: 'BusinessInfo',
          label: 'Información del negocio',
          description: 'Configura el nombre, RNC y datos básicos de tu negocio.',
          completed: status.businessInfoCompleted,
          href: '/settings',
          icon: 'building',
        },
        {
          key: 'Products',
          label: 'Catálogo de productos',
          description: 'Agrega tus primeros productos al catálogo o impórtalos desde un CSV.',
          completed: status.productsCompleted,
          href: '/products',
          icon: 'package',
        },
        {
          key: 'Inventory',
          label: 'Inventario inicial',
          description: 'Registra el stock inicial de tus productos para empezar a vender.',
          completed: status.inventoryCompleted,
          href: '/inventory',
          icon: 'layers',
        },
        {
          key: 'CashSession',
          label: 'Primera sesión de caja',
          description: 'Abre tu primera caja para comenzar a registrar ventas.',
          completed: status.cashSessionCompleted,
          href: '/cash',
          icon: 'banknote',
        },
      ]
    : []

  if (isLoading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-blue-500" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">
        No se pudo cargar el estado del onboarding. Intenta de nuevo.
      </div>
    )
  }

  const progressPercent = status
    ? Math.round((status.completedCount / status.totalSteps) * 100)
    : 0

  return (
    <div className="mx-auto max-w-2xl space-y-6 py-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Configuración inicial</h1>
        <p className="mt-1 text-gray-600">
          Completa estos pasos para empezar a usar BimmoFlow RD.
        </p>
      </div>

      {/* Progress bar */}
      <div className="rounded-lg border border-gray-200 bg-white p-4">
        <div className="flex items-center justify-between">
          <span className="text-sm font-medium text-gray-700">
            Progreso: {status?.completedCount ?? 0} de {status?.totalSteps ?? 4} pasos
          </span>
          <span className="text-sm font-semibold text-blue-600">{progressPercent}%</span>
        </div>
        <div className="mt-2 h-2 w-full overflow-hidden rounded-full bg-gray-100">
          <div
            className="h-full rounded-full bg-blue-500 transition-all duration-500"
            style={{ width: `${progressPercent}%` }}
          />
        </div>
        {status?.isComplete && (
          <div className="mt-3 flex items-center gap-2 text-green-600">
            <CheckCircle2 className="h-5 w-5" />
            <span className="text-sm font-medium">
              ¡Configuración completa! Tu negocio está listo para operar.
            </span>
          </div>
        )}
      </div>

      {/* Steps */}
      <div className="space-y-3">
        {steps.map((step, index) => (
          <OnboardingStepCard key={step.key} step={step} stepNumber={index + 1} />
        ))}
      </div>

      {/* Import shortcut */}
      {status && !status.productsCompleted && (
        <div className="rounded-lg border border-blue-200 bg-blue-50 p-4">
          <h3 className="font-semibold text-blue-800">¿Ya tienes una lista de productos?</h3>
          <p className="mt-1 text-sm text-blue-700">
            Importa tus productos en segundos desde un archivo CSV.
          </p>
          <a
            className="mt-2 inline-flex items-center rounded-md bg-blue-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-blue-700"
            href="/products/import"
          >
            Importar desde CSV
          </a>
        </div>
      )}
    </div>
  )
}
