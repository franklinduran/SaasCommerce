import { CheckCircle2 } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { OnboardingStep } from '@/modules/onboarding/types'

type Props = {
  step: OnboardingStep
  stepNumber: number
}

export function OnboardingStepCard({ step, stepNumber }: Readonly<Props>) {
  return (
    <div
      className={`flex items-start gap-4 rounded-lg border p-4 transition-colors ${
        step.completed
          ? 'border-green-200 bg-green-50'
          : 'border-gray-200 bg-white hover:border-blue-300'
      }`}
    >
      <div className="flex-shrink-0 pt-0.5">
        {step.completed ? (
          <CheckCircle2 className="h-6 w-6 text-green-500" />
        ) : (
          <div className="flex h-6 w-6 items-center justify-center rounded-full border-2 border-gray-300 text-xs font-semibold text-gray-500">
            {stepNumber}
          </div>
        )}
      </div>

      <div className="min-w-0 flex-1">
        <div className="flex items-center justify-between gap-2">
          <h3
            className={`font-semibold ${step.completed ? 'text-green-800' : 'text-gray-900'}`}
          >
            {step.label}
          </h3>
          {step.completed && (
            <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-700">
              Completado
            </span>
          )}
        </div>
        <p className="mt-1 text-sm text-gray-600">{step.description}</p>
        {!step.completed && (
          <Link
            className="mt-2 inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
            to={step.href}
          >
            Ir a configurar →
          </Link>
        )}
      </div>
    </div>
  )
}
