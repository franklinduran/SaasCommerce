import { ArrowDownLeft, ArrowUpRight, Loader2 } from 'lucide-react'
import { useNavigate, useParams } from 'react-router-dom'
import { useCashSession } from '@/modules/cash/hooks/useCash'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { cn } from '@/shared/utils/cn'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function formatDate(dateString: string) {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(dateString))
}

function differenceClassName(difference: number) {
  if (difference === 0) return 'text-green-700'
  return difference > 0 ? 'text-amber-700' : 'text-red-700'
}

export function CashSessionDetailPage() {
  const { cashSessionId } = useParams<{ cashSessionId: string }>()
  const navigate = useNavigate()
  const { data: session, isLoading, isError } = useCashSession(cashSessionId ?? '')

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="animate-spin text-stone-400" size={28} />
      </div>
    )
  }

  if (isError || !session) {
    return (
      <div className="p-6">
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          No se encontro la sesion de caja.
        </p>
        <Button className="mt-4" onClick={() => navigate('/cash/history')} variant="outline">
          Volver al historial
        </Button>
      </div>
    )
  }

  const difference =
    session.closingBalance !== null ? session.closingBalance - session.systemBalance : null

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Detalle de caja</h2>
          <p className="text-sm text-stone-500">{formatDate(session.openedAt)}</p>
        </div>
        <Button onClick={() => navigate('/cash/history')} variant="outline">
          Volver
        </Button>
      </div>

      {/* Summary cards */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Balance inicial
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-xl font-semibold text-stone-900">
              {formatCurrency(session.openingBalance)}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
              Balance sistema
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-xl font-semibold text-stone-900">
              {formatCurrency(session.systemBalance)}
            </p>
          </CardContent>
        </Card>

        {session.closingBalance !== null && (
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-xs font-medium uppercase tracking-wide text-stone-500">
                Balance cierre
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-xl font-semibold text-stone-900">
                {formatCurrency(session.closingBalance)}
              </p>
              {difference !== null && (
                <p
                  className={cn(
                    'mt-1 text-sm font-medium',
                    differenceClassName(difference),
                  )}
                >
                  Diferencia: {difference > 0 ? '+' : ''}{formatCurrency(difference)}
                </p>
              )}
            </CardContent>
          </Card>
        )}
      </div>

      {/* Status and dates */}
      <Card>
        <CardContent className="grid gap-3 pt-6 sm:grid-cols-2">
          <div>
            <p className="text-xs font-medium text-stone-500">Estado</p>
            <Badge className="mt-1" variant={session.status === 'Open' ? 'default' : 'secondary'}>
              {session.status === 'Open' ? 'Abierta' : 'Cerrada'}
            </Badge>
          </div>
          <div>
            <p className="text-xs font-medium text-stone-500">Apertura</p>
            <p className="mt-1 text-sm font-medium text-stone-900">{formatDate(session.openedAt)}</p>
          </div>
          {session.closedAt && (
            <div>
              <p className="text-xs font-medium text-stone-500">Cierre</p>
              <p className="mt-1 text-sm font-medium text-stone-900">{formatDate(session.closedAt)}</p>
            </div>
          )}
          {session.notes && (
            <div>
              <p className="text-xs font-medium text-stone-500">Notas</p>
              <p className="mt-1 text-sm text-stone-700">{session.notes}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Movements */}
      {session.movements.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-semibold">
              Movimientos ({session.movements.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y divide-stone-100">
              {[...session.movements].reverse().map((movement) => (
                <div key={movement.id} className="flex items-center justify-between px-4 py-3">
                  <div className="flex items-center gap-3">
                    <span
                      className={cn(
                        'flex h-8 w-8 items-center justify-center rounded-full',
                        movement.type === 'CashIn'
                          ? 'bg-green-50 text-green-600'
                          : 'bg-red-50 text-red-600',
                      )}
                    >
                      {movement.type === 'CashIn' ? (
                        <ArrowDownLeft size={16} />
                      ) : (
                        <ArrowUpRight size={16} />
                      )}
                    </span>
                    <div>
                      <p className="text-sm font-medium text-stone-900">{movement.description}</p>
                      <p className="text-xs text-stone-500">{formatDate(movement.createdAt)}</p>
                    </div>
                  </div>
                  <p
                    className={cn(
                      'text-sm font-semibold tabular-nums',
                      movement.type === 'CashIn' ? 'text-green-700' : 'text-red-700',
                    )}
                  >
                    {movement.type === 'CashIn' ? '+' : '-'}
                    {formatCurrency(movement.amount)}
                  </p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {session.movements.length === 0 && (
        <Card>
          <CardContent className="flex h-28 items-center justify-center">
            <p className="text-sm text-stone-500">Sin movimientos registrados.</p>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
