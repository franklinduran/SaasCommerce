export type Supplier = {
  id: string
  businessId: string
  name: string
  rnc: string | null
  phone: string | null
  email: string | null
  address: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}

export type SupplierListResponse = {
  items: Supplier[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type SupplierFilters = {
  query: string
  isActive: string
  page: number
  pageSize: number
  sortBy: string
  sortDirection: string
}

export type SupplierRequest = {
  name: string
  rnc: string | null
  phone: string | null
  email: string | null
  address: string | null
}

export type UpdateSupplierRequest = SupplierRequest & {
  isActive: boolean
}
