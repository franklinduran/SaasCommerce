import { zodResolver } from '@hookform/resolvers/zod'
import { Check, Plus } from 'lucide-react'
import type { InputHTMLAttributes, ReactNode } from 'react'
import { useEffect } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import { Button } from '@/shared/components/ui/button'
import { HttpClientError } from '@/shared/services/httpClient'
import { useCreateProductMutation, useUpdateProductMutation } from '@/modules/products/hooks/useProducts'
import type { Product, CreateProductRequest } from '@/modules/products/types'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const productSchema = z.object({
  allowsDiscount: z.boolean(),
  allowNegativeStock: z.boolean(),
  attributesJson: z.string().optional(),
  barcode: z.string().optional(),
  costPrice: z.coerce.number().min(0, 'El costo no puede ser negativo'),
  description: z.string().optional(),
  internalCode: z.string().optional(),
  isTaxIncluded: z.boolean(),
  maximumStock: z.coerce.number().nullable().optional(),
  minimumStock: z.coerce.number().nullable().optional(),
  minSalePrice: z.coerce.number().nullable().optional(),
  name: z.string().min(2, 'Nombre requerido'),
  parentProductId: z.string().optional(),
  productType: z.string().min(1, 'Tipo requerido'),
  reorderPoint: z.coerce.number().nullable().optional(),
  salePrice: z.coerce.number().min(0, 'El precio no puede ser negativo'),
  sku: z.string().min(2, 'SKU requerido'),
  supplierCode: z.string().optional(),
  taxCategory: z.string().min(1, 'Categoria fiscal requerida'),
  taxRate: z.coerce.number().min(0, 'Impuesto invalido'),
  trackInventory: z.boolean(),
  unitOfMeasure: z.string().min(1, 'Unidad requerida'),
  variantName: z.string().optional(),
  wholesalePrice: z.coerce.number().nullable().optional(),
})

type ProductFormValues = z.infer<typeof productSchema>
type ProductFormInput = z.input<typeof productSchema>

const inputClass =
  'h-11 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15'

const emptyProductFormDefaults: ProductFormInput = {
  allowsDiscount: true,
  allowNegativeStock: false,
  attributesJson: '',
  barcode: '',
  costPrice: 0,
  description: '',
  internalCode: '',
  isTaxIncluded: true,
  maximumStock: null,
  minimumStock: null,
  minSalePrice: null,
  name: '',
  parentProductId: '',
  productType: 'Simple',
  reorderPoint: null,
  salePrice: 0,
  sku: '',
  supplierCode: '',
  taxCategory: 'Itbis18',
  taxRate: 18,
  trackInventory: true,
  unitOfMeasure: 'Unit',
  variantName: '',
  wholesalePrice: null,
}

type ProductFormProps = {
  product?: Product | null
  onSaved?: () => void
}

export function ProductForm({ product, onSaved }: Readonly<ProductFormProps>) {
  const createProduct = useCreateProductMutation()
  const updateProduct = useUpdateProductMutation()
  const isEditing = Boolean(product)
  const {
    formState: { errors },
    handleSubmit,
    control,
    register,
    reset,
  } = useForm<ProductFormInput, undefined, ProductFormValues>({
    resolver: zodResolver(productSchema),
    defaultValues: toFormDefaults(product),
  })

  useEffect(() => {
    reset(toFormDefaults(product))
  }, [product, reset])

  const productType = useWatch({ control, name: 'productType' })
  const errorMessage =
    createProduct.error instanceof HttpClientError || updateProduct.error instanceof HttpClientError
      ? (createProduct.error ?? updateProduct.error)?.message
      : null
  const isPending = createProduct.isPending || updateProduct.isPending

  async function onSubmit(values: ProductFormValues) {
    const request: CreateProductRequest = {
      allowNegativeStock: values.allowNegativeStock,
      allowsDiscount: values.allowsDiscount,
      attributesJson: emptyToNull(values.attributesJson),
      barcode: emptyToNull(values.barcode),
      brandId: null,
      categoryId: null,
      costPrice: values.costPrice,
      description: emptyToNull(values.description),
      internalCode: emptyToNull(values.internalCode),
      isTaxIncluded: values.isTaxIncluded,
      maximumStock: values.maximumStock ?? null,
      minimumStock: values.minimumStock ?? null,
      minSalePrice: values.minSalePrice ?? null,
      name: values.name,
      parentProductId: emptyToNull(values.parentProductId),
      productType: values.productType,
      reorderPoint: values.reorderPoint ?? null,
      salePrice: values.salePrice,
      sku: values.sku,
      supplierCode: emptyToNull(values.supplierCode),
      taxCategory: values.taxCategory,
      taxRate: values.taxRate,
      trackInventory: values.productType === 'Service' ? false : values.trackInventory,
      unitOfMeasure: values.unitOfMeasure,
      variantName: emptyToNull(values.variantName),
      wholesalePrice: values.wholesalePrice ?? null,
    }

    if (product) {
      await updateProduct.mutateAsync({
        productId: product.id,
        request: {
          ...request,
          isActive: product.isActive,
        },
      })
    } else {
      await createProduct.mutateAsync(request)
      reset(toFormDefaults(null))
    }

    onSaved?.()
  }

  return (
    <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
      <FormSection title="Datos generales">
        <Field error={errors.name?.message} label="Nombre">
          <input className={inputClass} placeholder="Cafe molido" {...register('name')} />
        </Field>
        <Field error={errors.productType?.message} label="Tipo">
          <Controller
            control={control}
            name="productType"
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="Simple">Simple</SelectItem>
                  <SelectItem value="Service">Servicio</SelectItem>
                  <SelectItem value="Weighed">Pesado</SelectItem>
                  <SelectItem value="Composite">Combo</SelectItem>
                  <SelectItem value="VariantParent">Producto padre</SelectItem>
                  <SelectItem value="VariantChild">Variante</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
        </Field>
        <Field label="Descripcion">
          <input className={inputClass} placeholder="Descripcion corta" {...register('description')} />
        </Field>
      </FormSection>

      <FormSection title="Precio y costos">
        <Field error={errors.salePrice?.message} label="Precio venta">
          <input className={inputClass} min="0" step="0.01" type="number" {...register('salePrice')} />
        </Field>
        <Field error={errors.costPrice?.message} label="Costo">
          <input className={inputClass} min="0" step="0.01" type="number" {...register('costPrice')} />
        </Field>
        <Field label="Precio mayorista">
          <input className={inputClass} min="0" step="0.01" type="number" {...register('wholesalePrice')} />
        </Field>
        <Field label="Precio minimo">
          <input className={inputClass} min="0" step="0.01" type="number" {...register('minSalePrice')} />
        </Field>
      </FormSection>

      <FormSection title="Inventario">
        <Field error={errors.unitOfMeasure?.message} label="Unidad">
          <Controller
            control={control}
            name="unitOfMeasure"
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="Unit">Unidad</SelectItem>
                  <SelectItem value="Pound">Libra</SelectItem>
                  <SelectItem value="Kilogram">Kilogramo</SelectItem>
                  <SelectItem value="Gram">Gramo</SelectItem>
                  <SelectItem value="Liter">Litro</SelectItem>
                  <SelectItem value="Milliliter">Mililitro</SelectItem>
                  <SelectItem value="Box">Caja</SelectItem>
                  <SelectItem value="Pack">Paquete</SelectItem>
                  <SelectItem value="Dozen">Docena</SelectItem>
                  <SelectItem value="Meter">Metro</SelectItem>
                  <SelectItem value="Service">Servicio</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
        </Field>
        <Toggle disabled={productType === 'Service'} label="Controla inventario" {...register('trackInventory')} />
        <Field label="Stock minimo">
          <input className={inputClass} step="0.001" type="number" {...register('minimumStock')} />
        </Field>
        <Field label="Punto reorden">
          <input className={inputClass} step="0.001" type="number" {...register('reorderPoint')} />
        </Field>
        <Field label="Stock maximo">
          <input className={inputClass} step="0.001" type="number" {...register('maximumStock')} />
        </Field>
        <Toggle label="Permitir stock negativo" {...register('allowNegativeStock')} />
      </FormSection>

      <FormSection title="Codigos">
        <Field error={errors.sku?.message} label="SKU">
          <input className={inputClass} placeholder="SKU-001" {...register('sku')} />
        </Field>
        <Field label="Codigo de barras">
          <input className={inputClass} placeholder="7460000000000" {...register('barcode')} />
        </Field>
        <Field label="Codigo interno">
          <input className={inputClass} {...register('internalCode')} />
        </Field>
        <Field label="Codigo proveedor">
          <input className={inputClass} {...register('supplierCode')} />
        </Field>
      </FormSection>

      <FormSection title="Impuestos y opciones">
        <Field error={errors.taxCategory?.message} label="Categoria fiscal">
          <Controller
            control={control}
            name="taxCategory"
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="Itbis18">ITBIS 18%</SelectItem>
                  <SelectItem value="Exempt">Exento</SelectItem>
                  <SelectItem value="Reduced">Reducido</SelectItem>
                  <SelectItem value="Other">Otro</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
        </Field>
        <Field error={errors.taxRate?.message} label="Tasa impuesto">
          <input className={inputClass} min="0" step="0.01" type="number" {...register('taxRate')} />
        </Field>
        <Toggle label="Impuesto incluido" {...register('isTaxIncluded')} />
        <Toggle label="Permite descuentos" {...register('allowsDiscount')} />
      </FormSection>

      <FormSection title="Opciones avanzadas">
        <Field label="Producto padre">
          <input className={inputClass} placeholder="guid opcional" {...register('parentProductId')} />
        </Field>
        <Field label="Nombre variante">
          <input className={inputClass} placeholder="Rojo / M" {...register('variantName')} />
        </Field>
        <Field label="Atributos JSON">
          <input className={inputClass} placeholder='{"color":"rojo"}' {...register('attributesJson')} />
        </Field>
      </FormSection>

      {errorMessage && (
        <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-700 ring-1 ring-red-200">
          {errorMessage}
        </p>
      )}

      <div className="flex justify-end">
        <Button disabled={isPending} type="submit">
          {isEditing ? <Check size={16} /> : <Plus size={16} />}
          {getSubmitLabel(isPending, isEditing)}
        </Button>
      </div>
    </form>
  )
}

type FieldProps = {
  children: ReactNode
  error?: string
  label: string
}

function Field({ children, error, label }: Readonly<FieldProps>) {
  return (
    <label className="block min-w-0">
      <span className="mb-2 block text-sm font-semibold text-stone-900">{label}</span>
      {children}
      {error && <span className="mt-2 block text-sm font-medium text-red-700">{error}</span>}
    </label>
  )
}

function FormSection({ children, title }: Readonly<{ children: ReactNode; title: string }>) {
  return (
    <section className="rounded-md bg-stone-50 p-4 ring-1 ring-stone-200">
      <h4 className="mb-4 text-sm font-semibold text-stone-950">{title}</h4>
      <div className="grid gap-4 lg:grid-cols-3">{children}</div>
    </section>
  )
}

function Toggle({
  disabled,
  label,
  ...props
}: {
  disabled?: boolean
  label: string
} & Readonly<InputHTMLAttributes<HTMLInputElement>>) {
  return (
    <label className="flex h-11 items-center gap-3 self-end rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)]">
      <input
        className="h-4 w-4 accent-stone-900 disabled:cursor-not-allowed disabled:accent-stone-400"
        disabled={disabled}
        type="checkbox"
        {...props}
      />
      {label}
    </label>
  )
}

function emptyToNull(value?: string | null): string | null {
  return value && value.trim().length > 0 ? value.trim() : null
}

function getSubmitLabel(isPending: boolean, isEditing: boolean): string {
  if (isPending) {
    return 'Guardando'
  }

  return isEditing ? 'Guardar cambios' : 'Crear producto'
}

function toFormDefaults(product?: Product | null): ProductFormInput {
  if (!product) {
    return { ...emptyProductFormDefaults }
  }

  return {
    allowsDiscount: product.allowsDiscount,
    allowNegativeStock: product.allowNegativeStock,
    attributesJson: nullableTextToInput(product.attributesJson),
    barcode: nullableTextToInput(product.barcode),
    costPrice: product.costPrice,
    description: nullableTextToInput(product.description),
    internalCode: nullableTextToInput(product.internalCode),
    isTaxIncluded: product.isTaxIncluded,
    maximumStock: product.maximumStock,
    minimumStock: product.minimumStock,
    minSalePrice: product.minSalePrice,
    name: product.name,
    parentProductId: nullableTextToInput(product.parentProductId),
    productType: product.productType,
    reorderPoint: product.reorderPoint,
    salePrice: product.salePrice,
    sku: product.sku,
    supplierCode: nullableTextToInput(product.supplierCode),
    taxCategory: product.taxCategory,
    taxRate: product.taxRate,
    trackInventory: product.trackInventory,
    unitOfMeasure: product.unitOfMeasure,
    variantName: nullableTextToInput(product.variantName),
    wholesalePrice: product.wholesalePrice,
  }
}

function nullableTextToInput(value: string | null): string {
  return value ?? ''
}
