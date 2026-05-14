export type ValidationError = {
  field: string
  message: string
}

export type ApiError = {
  code: string
  message: string
  target?: string | null
  validationErrors?: ValidationError[] | null
}

export type ApiResponse<T> = {
  succeeded: boolean
  data: T | null
  errors: ApiError[]
  correlationId?: string | null
}
