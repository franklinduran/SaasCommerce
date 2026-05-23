import { Loader2, Plus, ReceiptText } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useExpenseCategories, useExpenses } from '../hooks/useExpenses'
import type { ExpenseFilters, ExpenseStatus } from '../types'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'
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

function formatDate(dateString: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
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

export function ExpensesPage() {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<ExpenseFilters>({ page: 1, pageSize: 20 })
  const { data: categoriesData } = useExpenseCategories()
  const { data, isLoading, isError } = useExpenses(filters)

  const items = data?.items ?? []
  const total = data?.totalCount ?? 0

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Gastos operativos</h2>
          <p className="text-sm text-stone-500">
            {total > 0 ? `${total} gasto${total !== 1 ? 's' : ''} registrado${total !== 1 ? 's' : ''}` : 'Registra los gastos del negocio.'}
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => navigate('/expenses/categories')}
          >
            Categorías
          </Button>
          <Button size="sm" onClick={() => navigate('/expenses/new')}>
            <Plus size={16} className="mr-1" />
            Nuevo gasto
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3">
        <Select
          value={filters.status ?? ''}
          onValueChange={(v) => setFilters((f) => ({ ...f, status: v || undefined, page: 1 }))}
        >
          <SelectTrigger className="w-36">
            <SelectValue placeholder="Estado" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos</SelectItem>
            <SelectItem value="Pending">Pendientes</SelectItem>
            <SelectItem value="Paid">Pagados</SelectItem>
            <SelectItem value="Cancelled">Cancelados</SelectItem>
          </SelectContent>
        </Select>

        <Select
          value={filters.categoryId ?? ''}
          onValueChange={(v) => setFilters((f) => ({ ...f, categoryId: v || undefined, page: 1 }))}
        >
          <SelectTrigger className="w-44">
            <SelectValue placeholder="Categoría" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todas</SelectItem>
            {(categoriesData ?? []).map((cat) => (
              <SelectItem key={cat.id} value={cat.id}>
                {cat.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          value={filters.paymentMethod ?? ''}
          onValueChange={(v) => setFilters((f) => ({ ...f, paymentMethod: v || undefined, page: 1 }))}
        >
          <SelectTrigger className="w-40">
            <SelectValue placeholder="Método de pago" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos</SelectItem>
            <SelectItem value="Cash">Efectivo</SelectItem>
            <SelectItem value="Transfer">Transferencia</SelectItem>
            <SelectItem value="Card">Tarjeta</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      )}

      {/* Error */}
      {isError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Error al cargar los gastos. Intenta nuevamente.
        </p>
      )}

      {/* Empty state */}
      {!isLoading && !isError && items.length === 0 && (
        <Card>
          <CardContent className="flex h-40 flex-col items-center justify-center gap-2">
            <ReceiptText className="text-stone-300" size={32} />
            <p className="text-sm text-stone-500">No hay gastos registrados.</p>
            <Button size="sm" onClick={() => navigate('/expenses/new')}>
              Registrar gasto
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Expense list */}
      {items.length > 0 && (
        <Card>
          <CardContent className="p-0">
            <div className="divide-y divide-stone-100">
              {items.map((expense) => (
                <button
                  key={expense.id}
                  type="button"
                  className="flex w-full items-center justify-between px-4 py-3 text-left transition-colors hover:bg-stone-50"
                  onClick={() => navigate(`/expenses/${expense.id}`)}
                >
                  <div className="space-y-0.5">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium text-stone-900">{expense.description}</span>
                      <Badge variant={statusVariant(expense.status)}>
                        {statusLabel(expense.status)}
                      </Badge>
                    </div>
                    <p className="text-xs text-stone-500">
                      {expense.categoryName} · {paymentMethodLabel(expense.paymentMethod)} ·{' '}
                      {formatDate(expense.expenseDate)}
                    </p>
                  </div>
                  <p className="text-sm font-semibold text-stone-900">
                    {formatCurrency(expense.amount)}
                  </p>
                </button>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Pagination */}
      {total > (filters.pageSize ?? 20) && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-stone-500">
            Página {filters.page ?? 1} · {total} resultados
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={(filters.page ?? 1) <= 1}
              onClick={() => setFilters((f) => ({ ...f, page: (f.page ?? 1) - 1 }))}
            >
              Anterior
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={items.length < (filters.pageSize ?? 20)}
              onClick={() => setFilters((f) => ({ ...f, page: (f.page ?? 1) + 1 }))}
            >
              Siguiente
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
