import { ArrowLeft, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useCancelExpense, useExpense, usePayExpense } from '../hooks/useExpenses'
import type { ExpenseStatus } from '../types'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function formatDate(dateString: string | null | undefined) {
  if (!dateString) return '—'
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(dateString))
}

function statusLabel(status: ExpenseStatus) {
  switch (status) {
    case 'Paid':
      return 'Pagado'
    case 'Cancelled':
      return 'Cancelado'
    default:
      return 'Pendiente'
  }
}

function statusVariant(status: ExpenseStatus): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (status) {
    case 'Paid':
      return 'default'
    case 'Cancelled':
      return 'destructive'
    default:
      return 'secondary'
  }
}

function paymentMethodLabel(method: string) {
  switch (method) {
    case 'Cash':
      return 'Efectivo'
    case 'Transfer':
      return 'Transferencia'
    case 'Card':
      return 'Tarjeta'
    default:
      return method
  }
}

export function ExpenseDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: expense, isLoading, isError } = useExpense(id!)
  const { mutate: payExpense, isPending: isPaying } = usePayExpense()
  const { mutate: cancelExpense, isPending: isCancelling } = useCancelExpense()
  const [paymentMethod, setPaymentMethod] = useState('Transfer')
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)

  function handlePay() {
    if (!expense) return
    setActionError(null)
    setActionSuccess(null)
    payExpense(
      { id: expense.id, request: { paymentMethod } },
      {
        onSuccess: () => setActionSuccess('Gasto pagado correctamente.'),
        onError: (err) =>
          setActionError(err instanceof Error ? err.message : 'Error al pagar. Intenta nuevamente.'),
      },
    )
  }

  function handleCancel() {
    if (!expense) return
    setActionError(null)
    setActionSuccess(null)
    cancelExpense(expense.id, {
      onSuccess: () => setActionSuccess('Gasto cancelado correctamente.'),
      onError: (err) =>
        setActionError(err instanceof Error ? err.message : 'Error al cancelar. Intenta nuevamente.'),
    })
  }

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="animate-spin text-stone-400" size={24} />
      </div>
    )
  }

  if (isError || !expense) {
    return (
      <div className="p-6">
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          No se pudo cargar el gasto.
        </p>
      </div>
    )
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/expenses')}>
          <ArrowLeft size={18} />
        </Button>
        <div className="flex items-center gap-3">
          <h2 className="text-lg font-semibold text-stone-900">{expense.description}</h2>
          <Badge variant={statusVariant(expense.status)}>{statusLabel(expense.status)}</Badge>
        </div>
      </div>

      {/* Detail card */}
      <Card className="max-w-lg">
        <CardHeader>
          <CardTitle className="text-base">Detalle del gasto</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <Row label="Categoría" value={expense.categoryName} />
          <Row label="Monto" value={formatCurrency(expense.amount)} bold />
          <Row label="Método de pago" value={paymentMethodLabel(expense.paymentMethod)} />
          <Row label="Fecha del gasto" value={formatDate(expense.expenseDate)} />
          <Row label="Registrado" value={formatDate(expense.createdAt)} />
          {expense.paidAt && <Row label="Pagado en" value={formatDate(expense.paidAt)} />}
          {expense.cancelledAt && (
            <Row label="Cancelado en" value={formatDate(expense.cancelledAt)} />
          )}
          {expense.notes && <Row label="Notas" value={expense.notes} />}
          {expense.cashSessionId && (
            <Row label="Sesión de caja" value={expense.cashSessionId.slice(0, 8) + '...'} />
          )}
        </CardContent>
      </Card>

      {/* Actions for Pending expenses */}
      {expense.status === 'Pending' && (
        <Card className="max-w-lg">
          <CardHeader>
            <CardTitle className="text-base">Acciones</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {actionSuccess && (
              <p className="rounded-md bg-green-50 px-4 py-3 text-sm font-medium text-green-700">
                {actionSuccess}
              </p>
            )}
            {actionError && (
              <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
                {actionError}
              </p>
            )}
            {/* Pay */}
            <div className="space-y-2">
              <p className="text-sm font-medium text-stone-700">Registrar pago</p>
              <div className="flex gap-2">
                <Select value={paymentMethod} onValueChange={setPaymentMethod}>
                  <SelectTrigger className="w-40">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Cash">Efectivo</SelectItem>
                    <SelectItem value="Transfer">Transferencia</SelectItem>
                    <SelectItem value="Card">Tarjeta</SelectItem>
                  </SelectContent>
                </Select>
                <Button onClick={handlePay} disabled={isPaying}>
                  {isPaying ? (
                    <Loader2 size={16} className="animate-spin" />
                  ) : (
                    'Pagar'
                  )}
                </Button>
              </div>
            </div>

            {/* Cancel */}
            <div>
              <p className="text-sm font-medium text-stone-700 mb-2">Cancelar gasto</p>
              <Button
                variant="destructive"
                size="sm"
                onClick={handleCancel}
                disabled={isCancelling}
              >
                {isCancelling ? <Loader2 size={16} className="animate-spin" /> : 'Cancelar gasto'}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

function Row({ label, value, bold }: { label: string; value: string; bold?: boolean }) {
  return (
    <div className="flex justify-between text-sm">
      <span className="text-stone-500">{label}</span>
      <span className={bold ? 'font-semibold text-stone-900' : 'text-stone-900'}>{value}</span>
    </div>
  )
}
