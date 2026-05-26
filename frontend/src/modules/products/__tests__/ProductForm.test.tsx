import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ProductForm } from '@/modules/products/components/ProductForm'
import {
  useCreateProductMutation,
  useUpdateProductMutation,
} from '@/modules/products/hooks/useProducts'
import type { Product } from '@/modules/products/types'
import { HttpClientError } from '@/shared/services/httpClient'

vi.mock('@/modules/products/hooks/useProducts', () => ({
  useCreateProductMutation: vi.fn(),
  useUpdateProductMutation: vi.fn(),
}))

describe('ProductForm', () => {
  const createProduct = vi.fn()
  const updateProduct = vi.fn()
  const onSaved = vi.fn()

  beforeEach(() => {
    vi.mocked(useCreateProductMutation).mockReturnValue({
      error: null,
      isPending: false,
      mutateAsync: createProduct,
    } as never)
    vi.mocked(useUpdateProductMutation).mockReturnValue({
      error: null,
      isPending: false,
      mutateAsync: updateProduct,
    } as never)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('creates products with default and normalized values', async () => {
    const user = userEvent.setup()
    createProduct.mockResolvedValueOnce(undefined)
    render(<ProductForm onSaved={onSaved} />)

    await user.type(screen.getByLabelText('Nombre'), 'Cafe molido')
    await user.type(screen.getByLabelText('SKU'), 'SKU-001')
    await user.type(screen.getByLabelText('Descripcion'), '  Premium  ')
    await user.type(screen.getByLabelText('Codigo de barras'), '   ')
    await user.type(screen.getByLabelText('Codigo interno'), ' INT-1 ')
    await user.click(screen.getByLabelText('Atributos JSON'))
    await user.paste('{"origen":"DO"}')
    await user.click(screen.getByRole('button', { name: /Crear producto/i }))

    await waitFor(() => {
      expect(createProduct).toHaveBeenCalledWith(
        expect.objectContaining({
          attributesJson: '{"origen":"DO"}',
          barcode: null,
          brandId: null,
          categoryId: null,
          costPrice: 0,
          description: 'Premium',
          internalCode: 'INT-1',
          name: 'Cafe molido',
          productType: 'Simple',
          salePrice: 0,
          sku: 'SKU-001',
          taxCategory: 'Itbis18',
          taxRate: 18,
          trackInventory: true,
          unitOfMeasure: 'Unit',
        }),
      )
    })
    expect(onSaved).toHaveBeenCalled()
  })

  it('updates an existing product and preserves its active status', async () => {
    const user = userEvent.setup()
    updateProduct.mockResolvedValueOnce(undefined)
    render(<ProductForm onSaved={onSaved} product={productFixture()} />)

    const name = screen.getByLabelText('Nombre')
    await user.clear(name)
    await user.type(name, 'Cafe editado')
    await user.click(screen.getByRole('button', { name: /Guardar cambios/i }))

    await waitFor(() => {
      expect(updateProduct).toHaveBeenCalledWith({
        productId: 'product-1',
        request: expect.objectContaining({
          attributesJson: null,
          barcode: '7460000000000',
          description: null,
          isActive: false,
          name: 'Cafe editado',
          parentProductId: null,
          productType: 'Service',
          sku: 'CAF-001',
          trackInventory: false,
          variantName: null,
        }),
      })
    })
    expect(onSaved).toHaveBeenCalled()
  })

  it('shows validation, api error and pending states', async () => {
    const user = userEvent.setup()
    const apiError = new HttpClientError('Producto duplicado', 400, {
      code: 'ProductExists',
      message: 'Producto duplicado',
    })
    vi.mocked(useCreateProductMutation).mockReturnValue({
      error: apiError,
      isPending: true,
      mutateAsync: createProduct,
    } as never)

    render(<ProductForm />)

    expect(screen.getByText('Producto duplicado')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Guardando/i })).toBeDisabled()

    cleanup()
    vi.mocked(useCreateProductMutation).mockReturnValue({
      error: null,
      isPending: false,
      mutateAsync: createProduct,
    } as never)
    render(<ProductForm />)
    await user.click(screen.getByRole('button', { name: /Crear producto/i }))
    expect(await screen.findByText('Nombre requerido')).toBeTruthy()
    expect(await screen.findByText('SKU requerido')).toBeTruthy()
  })
})

function productFixture(): Product {
  return {
    allowNegativeStock: true,
    allowsDiscount: false,
    attributesJson: null,
    barcode: '7460000000000',
    brandId: null,
    businessId: 'business-1',
    categoryId: null,
    costPrice: 40,
    description: null,
    id: 'product-1',
    internalCode: null,
    isActive: false,
    isTaxIncluded: false,
    maximumStock: 100,
    minimumStock: 5,
    minSalePrice: null,
    name: 'Cafe original',
    parentProductId: null,
    productType: 'Service',
    profitMargin: null,
    reorderPoint: 10,
    salePrice: 100,
    sku: 'CAF-001',
    supplierCode: null,
    taxCategory: 'Exempt',
    taxRate: 0,
    trackInventory: true,
    unitOfMeasure: 'Service',
    variantName: null,
    wholesalePrice: null,
  }
}
