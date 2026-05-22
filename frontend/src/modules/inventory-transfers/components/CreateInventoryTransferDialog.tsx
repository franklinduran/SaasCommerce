import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight, Plus, Trash2 } from 'lucide-react'
import { useEffect } from 'react'
import { useFieldArray, useForm } from 'react-hook-form'
import { useBranches } from '@/modules/branches/hooks/useBranches'
import { useCreateInventoryTransferMutation } from '@/modules/inventory-transfers/hooks/useInventoryTransfers'
import {
  createInventoryTransferSchema,
} from '@/modules/inventory-transfers/schemas/inventoryTransferSchemas'
import type { CreateInventoryTransferFormValues } from '@/modules/inventory-transfers/schemas/inventoryTransferSchemas'
import { useProductsQuery } from '@/modules/products/hooks/useProducts'
import { Button } from '@/shared/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { HttpClientError } from '@/shared/services/httpClient'

type CreateInventoryTransferDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSaved?: () => void
}

const defaultValues: CreateInventoryTransferFormValues = {
  items: [{ productId: '', productName: '', quantity: 1 }],
  note: '',
  sourceBranchId: '',
  targetBranchId: '',
}

export function CreateInventoryTransferDialog({
  onOpenChange,
  onSaved,
  open,
}: Readonly<CreateInventoryTransferDialogProps>) {
  const createTransfer = useCreateInventoryTransferMutation()

  const branches = useBranches({ isActive: true })
  const branchItems = branches.data?.items ?? []

  const products = useProductsQuery({
    categoryId: '',
    isActive: 'true',
    page: 1,
    pageSize: 200,
    productType: '',
    query: '',
    sortBy: 'name',
    sortDirection: 'asc',
  })
  const productItems = products.data?.items ?? []

  const {
    control,
    formState: { errors },
    handleSubmit,
    register,
    reset,
    setError,
    setValue,
    watch,
  } = useForm<CreateInventoryTransferFormValues>({
    defaultValues,
    resolver: zodResolver(createInventoryTransferSchema),
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'items' })

  const sourceBranchId = watch('sourceBranchId')
  const targetBranchId = watch('targetBranchId')

  useEffect(() => {
    if (open) {
      reset(defaultValues)
    }
  }, [open, reset])

  async function onSubmit(values: CreateInventoryTransferFormValues) {
    try {
      await createTransfer.mutateAsync({
        items: values.items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
        note: values.note?.trim() || null,
        sourceBranchId: values.sourceBranchId,
        targetBranchId: values.targetBranchId,
      })
      onSaved?.()
      onOpenChange(false)
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo crear la transferencia.'
          : 'No se pudo crear la transferencia.'
      setError('root', { message })
    }
  }

  const isSubmitting = createTransfer.isPending

  return (
    <Dialog onOpenChange={onOpenChange} open={open}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <div className="flex items-start gap-3">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
              <ArrowRight size={18} />
            </span>
            <div>
              <DialogTitle>Nueva transferencia de inventario</DialogTitle>
              <DialogDescription>
                Mueve productos entre sucursales. La transferencia se procesará automáticamente.
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <form className="space-y-5" id="transfer-form" onSubmit={handleSubmit(onSubmit)}>
          {/* Source / Target branches */}
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="source-branch">Sucursal origen *</Label>
              <Select
                value={sourceBranchId || '_'}
                onValueChange={(v) => setValue('sourceBranchId', v === '_' ? '' : v)}
              >
                <SelectTrigger id="source-branch">
                  <SelectValue placeholder="Selecciona..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Selecciona...</SelectItem>
                  {branchItems.map((branch) => (
                    <SelectItem key={branch.id} value={branch.id}>
                      {branch.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.sourceBranchId && (
                <p className="text-sm font-semibold text-red-700">{errors.sourceBranchId.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="target-branch">Sucursal destino *</Label>
              <Select
                value={targetBranchId || '_'}
                onValueChange={(v) => setValue('targetBranchId', v === '_' ? '' : v)}
              >
                <SelectTrigger id="target-branch">
                  <SelectValue placeholder="Selecciona..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Selecciona...</SelectItem>
                  {branchItems
                    .filter((branch) => branch.id !== sourceBranchId)
                    .map((branch) => (
                      <SelectItem key={branch.id} value={branch.id}>
                        {branch.name}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
              {errors.targetBranchId && (
                <p className="text-sm font-semibold text-red-700">{errors.targetBranchId.message}</p>
              )}
            </div>
          </div>

          {/* Items */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label>Productos *</Label>
              <Button
                onClick={() => append({ productId: '', productName: '', quantity: 1 })}
                size="sm"
                type="button"
                variant="secondary"
              >
                <Plus size={14} />
                Agregar producto
              </Button>
            </div>

            {errors.items && !Array.isArray(errors.items) && (
              <p className="text-sm font-semibold text-red-700">{errors.items.message}</p>
            )}

            <div className="space-y-2">
              {fields.map((field, index) => (
                <div className="grid grid-cols-[1fr_120px_40px] items-start gap-2" key={field.id}>
                  <div>
                    <Select
                      value={watch(`items.${index}.productId`) || '_'}
                      onValueChange={(v) => {
                        const product = productItems.find((p) => p.id === v)
                        setValue(`items.${index}.productId`, v === '_' ? '' : v)
                        setValue(`items.${index}.productName`, product?.name ?? '')
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Selecciona producto..." />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="_">Selecciona producto...</SelectItem>
                        {productItems.map((product) => (
                          <SelectItem key={product.id} value={product.id}>
                            {product.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    {errors.items?.[index]?.productId && (
                      <p className="mt-1 text-xs font-semibold text-red-700">
                        {errors.items[index].productId?.message}
                      </p>
                    )}
                  </div>

                  <div>
                    <Input
                      min="0.001"
                      placeholder="Cantidad"
                      step="0.001"
                      type="number"
                      {...register(`items.${index}.quantity`, { valueAsNumber: true })}
                    />
                    {errors.items?.[index]?.quantity && (
                      <p className="mt-1 text-xs font-semibold text-red-700">
                        {errors.items[index].quantity?.message}
                      </p>
                    )}
                  </div>

                  <Button
                    className="mt-0.5"
                    disabled={fields.length === 1}
                    onClick={() => remove(index)}
                    size="sm"
                    type="button"
                    variant="ghost"
                  >
                    <Trash2 size={14} />
                  </Button>
                </div>
              ))}
            </div>
          </div>

          {/* Note */}
          <div className="space-y-1.5">
            <Label htmlFor="transfer-note">Nota (opcional)</Label>
            <Input
              id="transfer-note"
              maxLength={500}
              placeholder="Motivo u observaciones..."
              {...register('note')}
            />
          </div>

          {errors.root && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
              {errors.root.message}
            </p>
          )}
        </form>

        <DialogFooter>
          <Button
            disabled={isSubmitting}
            onClick={() => onOpenChange(false)}
            type="button"
            variant="secondary"
          >
            Cancelar
          </Button>
          <Button disabled={isSubmitting} form="transfer-form" type="submit">
            {isSubmitting ? 'Creando...' : 'Crear transferencia'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
