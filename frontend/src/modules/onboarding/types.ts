export type OnboardingStatusResponse = {
  businessInfoCompleted: boolean
  productsCompleted: boolean
  inventoryCompleted: boolean
  cashSessionCompleted: boolean
  isComplete: boolean
  completedCount: number
  totalSteps: number
}

export type OnboardingStepKey =
  | 'BusinessInfo'
  | 'Products'
  | 'Inventory'
  | 'CashSession'

export type OnboardingStep = {
  key: OnboardingStepKey
  label: string
  description: string
  completed: boolean
  href: string
  icon: string
}
