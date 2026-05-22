export type Branch = {
  id: string
  businessId: string
  name: string
  code: string
  address: string | null
  phone: string | null
  isMain: boolean
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export type BranchListResponse = {
  items: Branch[]
  total: number
}

export type CreateBranchRequest = {
  name: string
  code: string
  address?: string | null
  phone?: string | null
  isMain: boolean
}

export type UpdateBranchRequest = {
  name: string
  address?: string | null
  phone?: string | null
}

export type BranchFilters = {
  isActive?: boolean | null
}
