import { z } from 'zod'

export const betaFeedbackCategories = [
  'Bug',
  'Improvement',
  'Question',
  'DataError',
  'SaleIssue',
  'InventoryIssue',
  'PurchaseIssue',
  'CashIssue',
] as const

export const betaFeedbackStatuses = [
  'New',
  'InReview',
  'Accepted',
  'Rejected',
  'Resolved',
] as const

export const betaFeedbackSchema = z.object({
  category: z.enum(betaFeedbackCategories, { message: 'Selecciona una categoria.' }),
  contextUrl: z.string().max(500, 'Maximo 500 caracteres.').optional(),
  description: z
    .string()
    .trim()
    .min(10, 'Describe el caso con al menos 10 caracteres.')
    .max(2000, 'Maximo 2000 caracteres.'),
  title: z
    .string()
    .trim()
    .min(5, 'El titulo debe tener al menos 5 caracteres.')
    .max(160, 'Maximo 160 caracteres.'),
})

export type BetaFeedbackFormValues = z.infer<typeof betaFeedbackSchema>
