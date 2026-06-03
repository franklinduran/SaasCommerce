export type Product = {
  id: string
  businessId: string
  productType: string
  categoryId: string | null
  brandId: string | null
  parentProductId: string | null
  name: string
  description: string | null
  sku: string
  barcode: string | null
  internalCode: string | null
  supplierCode: string | null
  unitOfMeasure: string
  salePrice: number
  costPrice: number
  wholesalePrice: number | null
  minSalePrice: number | null
  taxCategory: string
  taxRate: number
  profitMargin: number | null
  isTaxIncluded: boolean
  allowsDiscount: boolean
  trackInventory: boolean
  minimumStock: number | null
  maximumStock: number | null
  reorderPoint: number | null
  allowNegativeStock: boolean
  variantName: string | null
  attributesJson: string | null
  imageUrl: string | null
  isActive: boolean
}

export type ProductListResponse = {
  items: Product[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type ProductFilters = {
  query: string
  productType: string
  isActive: string
  categoryId: string
  page: number
  pageSize: number
  sortBy: string
  sortDirection: string
}

export type Category = {
  id: string
  businessId: string
  name: string
  description: string | null
  isActive: boolean
}

export type CreateProductRequest = {
  productType: string
  name: string
  description: string | null
  sku: string
  barcode: string | null
  categoryId: string | null
  brandId: string | null
  unitOfMeasure: string
  salePrice: number
  costPrice: number
  wholesalePrice: number | null
  minSalePrice: number | null
  taxCategory: string
  taxRate: number
  isTaxIncluded: boolean
  allowsDiscount: boolean
  trackInventory: boolean
  minimumStock: number | null
  maximumStock: number | null
  reorderPoint: number | null
  allowNegativeStock: boolean
  internalCode: string | null
  supplierCode: string | null
  parentProductId: string | null
  variantName: string | null
  attributesJson: string | null
}

export type UpdateProductRequest = CreateProductRequest & {
  isActive: boolean
}
