import { Loader2 } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useCashSessions } from '@/modules/cash/hooks/useCash'
import { CsvExportButton } from '@/shared/components/CsvExportButton'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent } from '@/shared/components/ui/card'
import { useHasPermission } from '@/shared/hooks/usePermissions'
import { Permission } from '@/shared/types/permissions'

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

export function CashHistoryPage() {
  const navigate = useNavigate()
  const { data, isLoading, isError } = useCashSessions()
  const items = data?.items ?? []
  const canExportCash = useHasPermission(Permission.CashExport)

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-stone-900">Historial de cajas</h2>
          <p className="text-sm text-stone-500">Todas las sesiones de caja de esta sucursal.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {canExportCash && (
            <CsvExportButton
              endpoint="/api/cash-registers/export"
              filename={`cajas_${new Date().toISOString().slice(0, 10)}.csv`}
            />
          )}
          <Button onClick={() => navigate('/cash')} variant="outline">
            Caja actual
          </Button>
        </div>
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
          <CardContent className="flex h-40 items-center justify-center">
            <p className="text-sm text-stone-500">No hay sesiones de caja registradas.</p>
          </CardContent>
        </Card>
      )}

      {items.length > 0 && (
        <Card>
          <CardContent className="p-0">
            <div className="divide-y divide-stone-100">
              {items.map((session) => (
                <button
                  key={session.id}
                  className="flex w-full items-center justify-between px-4 py-3 text-left hover:bg-stone-50 transition-colors"
                  type="button"
                  onClick={() => navigate(`/cash/${session.id}`)}
                >
                  <div className="space-y-0.5">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium text-stone-900">
                        {formatDate(session.openedAt)}
                      </span>
                      <Badge variant={session.status === 'Open' ? 'default' : 'secondary'}>
                        {session.status === 'Open' ? 'Abierta' : 'Cerrada'}
                      </Badge>
                    </div>
                    {session.closedAt && (
                      <p className="text-xs text-stone-500">Cierre: {formatDate(session.closedAt)}</p>
                    )}
                  </div>
                  <div className="text-right space-y-0.5">
                    <p className="text-sm font-semibold text-stone-900">
                      {formatCurrency(session.systemBalance)}
                    </p>
                    <p className="text-xs text-stone-500">
                      Inicial: {formatCurrency(session.openingBalance)}
                    </p>
                  </div>
                </button>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
