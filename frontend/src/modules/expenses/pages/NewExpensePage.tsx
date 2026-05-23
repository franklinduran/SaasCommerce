import { ArrowLeft, Loader2 } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useCreateExpense, useExpenseCategories } from '../hooks/useExpenses'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { Textarea } from '@/shared/components/ui/textarea'

type FormValues = {
  categoryId: string
  description: string
  amount: string
  paymentMethod: string
  status: string
  expenseDate: string
  notes: string
}

type FormErrors = Partial<Record<keyof FormValues, string>>

export function NewExpensePage() {
  const navigate = useNavigate()
  const { data: categories } = useExpenseCategories()
  const { mutate, isPending } = useCreateExpense()
  const [errorMsg, setErrorMsg] = useState<string | null>(null)

  const [values, setValues] = useState<FormValues>({
    categoryId: '',
    description: '',
    amount: '',
    paymentMethod: 'Cash',
    status: 'Pending',
    expenseDate: new Date().toISOString().split('T')[0],
    notes: '',
  })
  const [errors, setErrors] = useState<FormErrors>({})

  useEffect(() => {
    if ((categories ?? []).length > 0 && !values.categoryId) {
      setValues((v) => ({ ...v, categoryId: categories![0].id }))
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [categories])

  function validate(): boolean {
    const next: FormErrors = {}
    if (!values.categoryId) next.categoryId = 'Selecciona una categoría'
    if (!values.description || values.description.length < 3)
      next.description = 'La descripción es requerida (mínimo 3 caracteres)'
    if (!values.amount || isNaN(Number(values.amount)) || Number(values.amount) <= 0)
      next.amount = 'El monto debe ser mayor a 0'
    if (!values.expenseDate) next.expenseDate = 'La fecha es requerida'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return
    setErrorMsg(null)
    mutate(
      {
        branchId: '', // filled server-side from JWT BranchId claim
        categoryId: values.categoryId,
        description: values.description,
        amount: Number(values.amount),
        paymentMethod: values.paymentMethod as 'Cash' | 'Transfer' | 'Card',
        status: values.status as 'Pending' | 'Paid',
        expenseDate: new Date(values.expenseDate).toISOString(),
        notes: values.notes || null,
      },
      {
        onSuccess: () => navigate('/expenses'),
        onError: (err) =>
          setErrorMsg(err instanceof Error ? err.message : 'Error al registrar el gasto.'),
      },
    )
  }

  function setField(field: keyof FormValues) {
    return (value: string) => {
      setValues((v) => ({ ...v, [field]: value }))
      if (errors[field]) setErrors((e) => ({ ...e, [field]: undefined }))
    }
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/expenses')}>
          <ArrowLeft size={18} />
        </Button>
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Registrar gasto</h2>
          <p className="text-sm text-stone-500">Registra un gasto operativo del negocio.</p>
        </div>
      </div>

      <Card className="max-w-lg">
        <CardHeader>
          <CardTitle className="text-base">Datos del gasto</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Category */}
            <div className="space-y-1.5">
              <Label htmlFor="categoryId">Categoría</Label>
              <Select value={values.categoryId} onValueChange={setField('categoryId')}>
                <SelectTrigger id="categoryId">
                  <SelectValue placeholder="Selecciona una categoría" />
                </SelectTrigger>
                <SelectContent>
                  {(categories ?? []).map((cat) => (
                    <SelectItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.categoryId && (
                <p className="text-xs text-red-600">{errors.categoryId}</p>
              )}
            </div>

            {/* Description */}
            <div className="space-y-1.5">
              <Label htmlFor="description">Descripción</Label>
              <Input
                id="description"
                placeholder="Ej. Factura de electricidad"
                value={values.description}
                onChange={(e) => setField('description')(e.target.value)}
              />
              {errors.description && (
                <p className="text-xs text-red-600">{errors.description}</p>
              )}
            </div>

            {/* Amount */}
            <div className="space-y-1.5">
              <Label htmlFor="amount">Monto (DOP)</Label>
              <Input
                id="amount"
                type="number"
                min="0.01"
                step="0.01"
                placeholder="0.00"
                value={values.amount}
                onChange={(e) => setField('amount')(e.target.value)}
              />
              {errors.amount && (
                <p className="text-xs text-red-600">{errors.amount}</p>
              )}
            </div>

            {/* Payment Method */}
            <div className="space-y-1.5">
              <Label htmlFor="paymentMethod">Método de pago</Label>
              <Select value={values.paymentMethod} onValueChange={setField('paymentMethod')}>
                <SelectTrigger id="paymentMethod">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Cash">Efectivo</SelectItem>
                  <SelectItem value="Transfer">Transferencia</SelectItem>
                  <SelectItem value="Card">Tarjeta</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Status */}
            <div className="space-y-1.5">
              <Label htmlFor="status">Estado</Label>
              <Select value={values.status} onValueChange={setField('status')}>
                <SelectTrigger id="status">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Pending">Pendiente</SelectItem>
                  <SelectItem value="Paid">Pagado</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Expense Date */}
            <div className="space-y-1.5">
              <Label htmlFor="expenseDate">Fecha del gasto</Label>
              <Input
                id="expenseDate"
                type="date"
                value={values.expenseDate}
                onChange={(e) => setField('expenseDate')(e.target.value)}
              />
              {errors.expenseDate && (
                <p className="text-xs text-red-600">{errors.expenseDate}</p>
              )}
            </div>

            {/* Notes */}
            <div className="space-y-1.5">
              <Label htmlFor="notes">Notas (opcional)</Label>
              <Textarea
                id="notes"
                placeholder="Información adicional..."
                className="resize-none"
                rows={3}
                value={values.notes}
                onChange={(e) => setField('notes')(e.target.value)}
              />
            </div>

            {errorMsg && (
              <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
                {errorMsg}
              </p>
            )}

            <div className="flex gap-2 pt-2">
              <Button type="submit" disabled={isPending}>
                {isPending ? (
                  <>
                    <Loader2 size={16} className="mr-2 animate-spin" />
                    Guardando...
                  </>
                ) : (
                  'Registrar gasto'
                )}
              </Button>
              <Button type="button" variant="outline" onClick={() => navigate('/expenses')}>
                Cancelar
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
