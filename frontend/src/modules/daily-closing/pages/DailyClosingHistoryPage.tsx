import { Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useDailyClosingList } from '@/modules/daily-closing/hooks/useDailyClosing'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function formatDate(dateString: string) {
  // dateString is "yyyy-MM-dd" — append T00:00:00 to parse in local time
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(`${dateString}T00:00:00`))
}

const PAGE_SIZE = 20

export function DailyClosingHistoryPage() {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)

  const { data, isLoading, isError } = useDailyClosingList({ page, pageSize: PAGE_SIZE })
  const items = data?.items ?? []
  const total = data?.totalCount ?? 0

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Historial de cierres</h2>
          <p className="text-sm text-stone-500">
            {total > 0
              ? `${total} cierre${total !== 1 ? 's' : ''} registrado${total !== 1 ? 's' : ''}`
              : 'Sin cierres registrados aún.'}
          </p>
        </div>
        <Button onClick={() => navigate('/daily-closing')}>Nuevo cierre</Button>
      </div>

      {isLoading && (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      )}

      {isError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Error al cargar el historial. Intenta nuevamente.
        </p>
      )}

      {!isLoading && !isError && items.length === 0 && (
        <Card>
          <CardContent className="flex h-40 flex-col items-center justify-center gap-3">
            <p className="text-sm text-stone-500">No hay cierres registrados.</p>
            <Button size="sm" onClick={() => navigate('/daily-closing')}>
              Crear primer cierre
            </Button>
          </CardContent>
        </Card>
      )}

      {items.length > 0 && (
        <Card>
          <CardContent className="p-0">
            <div className="divide-y divide-stone-100">
              {items.map((item) => (
                <button
                  key={item.id}
                  type="button"
                  className="flex w-full items-center justify-between px-4 py-3 text-left transition-colors hover:bg-stone-50"
                  onClick={() => navigate(`/daily-closing/${item.id}`)}
                >
                  <div className="space-y-0.5">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="text-sm font-medium text-stone-900">
                        {formatDate(item.closingDate)}
                      </span>
                      <Badge variant={item.status === 'Closed' ? 'default' : 'secondary'}>
                        {item.status === 'Closed' ? 'Cerrado' : 'Borrador'}
                      </Badge>
                      {item.alertCount > 0 && (
                        <Badge
                          variant="outline"
                          className="border-amber-200 bg-amber-50 text-amber-700"
                        >
                          {item.alertCount} alerta{item.alertCount !== 1 ? 's' : ''}
                        </Badge>
                      )}
                    </div>
                    <p className="text-xs text-stone-500">{item.branchName}</p>
                  </div>
                  <div className="ml-4 shrink-0 text-right">
                    <p className="text-sm font-semibold text-stone-900 tabular-nums">
                      {formatCurrency(item.totalSales)}
                    </p>
                    <p
                      className={`text-xs font-medium tabular-nums ${item.estimatedNetProfit >= 0 ? 'text-green-700' : 'text-red-700'}`}
                    >
                      {formatCurrency(item.estimatedNetProfit)} neto
                    </p>
                  </div>
                </button>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {total > PAGE_SIZE && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-stone-500">
            Página {page} · {total} resultados
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
            >
              Anterior
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={items.length < PAGE_SIZE}
              onClick={() => setPage((p) => p + 1)}
            >
              Siguiente
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
