import { Loader2 } from 'lucide-react'
import { useState } from 'react'
import {
  useCashRegisterHistory,
  useCashRegisterRealtimeInvalidation,
} from '@/modules/cash-register/hooks/useCashRegister'
import { Badge } from '@/shared/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

function formatCurrency(amount: number | null) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount ?? 0)
}

function formatDate(value: string | null) {
  return value ? new Date(value).toLocaleString('es-DO') : '-'
}

function statusLabel(status: string) {
  if (status === 'Open') return 'Abierta'
  if (status === 'Closed') return 'Cerrada'
  return 'Cancelada'
}

function differenceLabel(type: string | null) {
  if (type === 'Balanced') return 'Cuadrado'
  if (type === 'Surplus') return 'Sobrante'
  if (type === 'Shortage') return 'Faltante'
  return '-'
}

export function CashRegisterHistoryPage() {
  useCashRegisterRealtimeInvalidation()
  const [status, setStatus] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const { data, isError, isLoading } = useCashRegisterHistory({
    dateFrom: dateFrom || undefined,
    dateTo: dateTo || undefined,
    status: status || undefined,
    page: 1,
    pageSize: 50,
  })

  const items = data?.items ?? []

  return (
    <div className="space-y-6 p-6">
      <div>
        <h2 className="text-lg font-semibold text-stone-900">Historial de arqueos</h2>
        <p className="text-sm text-stone-500">Consulta cajas abiertas y cerradas por fecha y estado.</p>
      </div>

      <Card>
        <CardContent className="grid gap-4 pt-6 sm:grid-cols-3">
          <div className="space-y-1.5">
            <Label htmlFor="cash-register-from">Desde</Label>
            <input
              id="cash-register-from"
              type="datetime-local"
              value={dateFrom}
              onChange={(event) => setDateFrom(event.target.value)}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="cash-register-to">Hasta</Label>
            <input
              id="cash-register-to"
              type="datetime-local"
              value={dateTo}
              onChange={(event) => setDateTo(event.target.value)}
              className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="cash-register-status">Estado</Label>
            <Select value={status || 'all'} onValueChange={(value) => setStatus(value === 'all' ? '' : value)}>
              <SelectTrigger id="cash-register-status">
                <SelectValue placeholder="Todos" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="Open">Abiertas</SelectItem>
                <SelectItem value="Closed">Cerradas</SelectItem>
                <SelectItem value="Cancelled">Canceladas</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Cajas registradas</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading && (
            <div className="flex h-32 items-center justify-center">
              <Loader2 className="animate-spin text-stone-400" size={24} />
            </div>
          )}
          {isError && <p className="text-sm text-red-700">No se pudo cargar el historial.</p>}
          {!isLoading && !isError && items.length === 0 && (
            <p className="text-sm text-stone-500">No hay cajas para los filtros seleccionados.</p>
          )}
          {!isLoading && !isError && items.length > 0 && (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-stone-200 text-sm">
                <thead className="bg-stone-50 text-left text-xs uppercase tracking-wide text-stone-500">
                  <tr>
                    <th className="px-4 py-3">Apertura</th>
                    <th className="px-4 py-3">Cierre</th>
                    <th className="px-4 py-3">Estado</th>
                    <th className="px-4 py-3 text-right">Inicial</th>
                    <th className="px-4 py-3 text-right">Esperado</th>
                    <th className="px-4 py-3 text-right">Contado</th>
                    <th className="px-4 py-3 text-right">Diferencia</th>
                    <th className="px-4 py-3">Resultado</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-stone-100">
                  {items.map((item) => (
                    <tr key={item.cashRegisterId}>
                      <td className="px-4 py-3">{formatDate(item.openedAt)}</td>
                      <td className="px-4 py-3">{formatDate(item.closedAt)}</td>
                      <td className="px-4 py-3">{statusLabel(item.status)}</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(item.openingAmount)}</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(item.expectedCashAmount)}</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(item.countedAmount)}</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(item.difference)}</td>
                      <td className="px-4 py-3">
                        <Badge variant="outline">{differenceLabel(item.differenceType)}</Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
