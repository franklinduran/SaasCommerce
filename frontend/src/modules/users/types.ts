export interface UserSummary {
  userId: string
  fullName: string
  email: string
  phone?: string | null
  role: string
  isActive: boolean
  createdAt: string
}

export interface UserDetail {
  id: string
  fullName: string
  email: string
  phone?: string
  role: string
  defaultBranchId?: string
  isActive: boolean
  mustChangePassword: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateUserRequest {
  fullName: string
  email: string
  password: string
  role: string
  defaultBranchId?: string
}

export interface UpdateUserRequest {
  fullName: string
  phone?: string
  defaultBranchId?: string
}

export interface ResetPasswordResponse {
  temporaryPassword: string
}

export interface UserListResponse {
  items: UserSummary[]
  totalItems: number
}
