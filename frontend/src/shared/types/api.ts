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
  isSuccess: boolean
  data: T | null
  error: ApiError | null
  correlationId?: string | null
}
