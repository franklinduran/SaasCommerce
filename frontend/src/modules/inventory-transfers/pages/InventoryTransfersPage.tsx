import {
  ArrowRight,
  ChevronLeft,
  ChevronRight,
  Eye,
  Plus,
  XCircle,
} from 'lucide-react'
import { useState } from 'react'
import { useBranches } from '@/modules/branches/hooks/useBranches'
import { CreateInventoryTransferDialog } from '@/modules/inventory-transfers/components/CreateInventoryTransferDialog'
import { InventoryTransferStatusBadge } from '@/modules/inventory-transfers/components/InventoryTransferStatusBadge'
import {
  useCancelInventoryTransferMutation,
  useInventoryTransferRealtimeInvalidation,
  useInventoryTransfers,
} from '@/modules/inventory-transfers/hooks/useInventoryTransfers'
import type { InventoryTransfer, InventoryTransferFilters } from '@/modules/inventory-transfers/types'
import { Button } from '@/shared/components/ui/button'
import { Card } from '@/shared/components/ui/card'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { HttpClientError } from '@/shared/services/httpClient'

const initialFilters: InventoryTransferFilters = {
  page: 1,
  pageSize: 20,
  sourceBranchId: undefined,
  status: undefined,
  targetBranchId: undefined,
}

export function InventoryTransfersPage() {
  const [filters, setFilters] = useState<InventoryTransferFilters>(initialFilters)
  const [createOpen, setCreateOpen] = useState(false)
  const [detailTransfer, setDetailTransfer] = useState<InventoryTransfer | null>(null)
  const [cancelError, setCancelError] = useState<string | null>(null)

  const { data, isError, isLoading, refetch } = useInventoryTransfers(filters)
  const items = data?.items ?? []
  const cancelTransfer = useCancelInventoryTransferMutation()

  useInventoryTransferRealtimeInvalidation()

  const branches = useBranches({ isActive: null })
  const branchItems = branches.data?.items ?? []

  function updateFilters(patch: Partial<InventoryTransferFilters>) {
    setFilters((current) => ({ ...current, ...patch, page: patch.page ?? 1 }))
  }

  async function handleCancel(transferId: string) {
    setCancelError(null)
    try {
      await cancelTransfer.mutateAsync(transferId)
      if (detailTransfer?.id === transferId) {
        setDetailTransfer(null)
      }
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo cancelar la transferencia.'
          : 'No se pudo cancelar la transferencia.'
      setCancelError(message)
    }
  }

  const selectedTransfer = detailTransfer
    ? (items.find((t) => t.id === detailTransfer.id) ?? detailTransfer)
    : null

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
        <div>
          <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
            <ArrowRight size={13} />
            Inventario
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Transferencias de inventario</h2>
          <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
            Mueve productos entre sucursales. Las transferencias se procesan automáticamente.
          </p>
        </div>
        <Button className="w-full xl:w-auto" onClick={() => setCreateOpen(true)} type="button">
          <Plus size={16} />
          Nueva transferencia
        </Button>
      </div>

      {cancelError && (
        <div className="rounded-md bg-red-50 px-4 py-3 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          {cancelError}
        </div>
      )}

      {/* Filters */}
      <div className="flex flex-wrap items-end gap-3">
        <div className="min-w-[160px] space-y-1.5">
          <Label htmlFor="filter-source">Sucursal origen</Label>
          <Select
            value={filters.sourceBranchId ?? '_'}
            onValueChange={(v) => updateFilters({ sourceBranchId: v === '_' ? undefined : v })}
          >
            <SelectTrigger id="filter-source">
              <SelectValue placeholder="Todas" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="_">Todas</SelectItem>
              {branchItems.map((branch) => (
                <SelectItem key={branch.id} value={branch.id}>
                  {branch.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="min-w-[160px] space-y-1.5">
          <Label htmlFor="filter-target">Sucursal destino</Label>
          <Select
            value={filters.targetBranchId ?? '_'}
            onValueChange={(v) => updateFilters({ targetBranchId: v === '_' ? undefined : v })}
          >
            <SelectTrigger id="filter-target">
              <SelectValue placeholder="Todas" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="_">Todas</SelectItem>
              {branchItems.map((branch) => (
                <SelectItem key={branch.id} value={branch.id}>
                  {branch.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="min-w-[160px] space-y-1.5">
          <Label htmlFor="filter-status">Estado</Label>
          <Select
            value={filters.status ?? '_'}
            onValueChange={(v) => updateFilters({ status: v === '_' ? undefined : v })}
          >
            <SelectTrigger id="filter-status">
              <SelectValue placeholder="Todos" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="_">Todos</SelectItem>
              <SelectItem value="Pending">Pendiente</SelectItem>
              <SelectItem value="Completed">Completada</SelectItem>
              <SelectItem value="Failed">Fallida</SelectItem>
              <SelectItem value="Cancelled">Cancelada</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className={`grid gap-5 ${selectedTransfer ? 'lg:grid-cols-[1fr_420px]' : ''}`}>
        {/* Table */}
        <Card className="overflow-hidden">
          <div className="border-b border-stone-200 px-4 py-3 sm:px-5">
            <h3 className="text-base font-semibold text-stone-950">
              {data?.total ?? 0} transferencias encontradas
            </h3>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full min-w-[700px] text-left text-sm">
              <thead className="bg-stone-50 text-xs font-semibold uppercase tracking-wide text-stone-500">
                <tr>
                  <th className="px-5 py-3">Origen → Destino</th>
                  <th className="px-5 py-3">Productos</th>
                  <th className="px-5 py-3">Estado</th>
                  <th className="px-5 py-3">Fecha</th>
                  <th className="px-5 py-3 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-stone-200 bg-white">
                {isLoading && (
                  <tr>
                    <td className="px-5 py-10 text-center text-sm font-medium text-stone-600" colSpan={5}>
                      Cargando transferencias...
                    </td>
                  </tr>
                )}
                {isError && (
                  <tr>
                    <td className="px-5 py-10 text-center" colSpan={5}>
                      <p className="text-sm font-medium text-red-700">No se pudieron cargar las transferencias.</p>
                      <Button className="mt-3" onClick={() => void refetch()} size="sm" type="button" variant="secondary">
                        Reintentar
                      </Button>
                    </td>
                  </tr>
                )}
                {!isLoading && !isError && items.length === 0 && (
                  <tr>
                    <td className="px-5 py-12 text-center" colSpan={5}>
                      <p className="text-sm font-semibold text-stone-900">Sin transferencias</p>
                      <p className="mt-1 text-sm font-medium text-stone-500">
                        Crea una transferencia para mover productos entre sucursales.
                      </p>
                      <Button className="mt-4" onClick={() => setCreateOpen(true)} type="button">
                        <Plus size={16} />
                        Nueva transferencia
                      </Button>
                    </td>
                  </tr>
                )}
                {!isLoading && !isError && items.map((transfer) => (
                  <tr
                    className={`hover:bg-stone-50 ${selectedTransfer?.id === transfer.id ? 'bg-stone-50' : ''}`}
                    key={transfer.id}
                  >
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-1.5 font-semibold text-stone-950">
                        <span>{transfer.sourceBranchName ?? transfer.sourceBranchId.slice(0, 8)}</span>
                        <ArrowRight className="shrink-0 text-stone-400" size={14} />
                        <span>{transfer.targetBranchName ?? transfer.targetBranchId.slice(0, 8)}</span>
                      </div>
                      {transfer.note && (
                        <p className="mt-1 text-xs font-medium text-stone-500 line-clamp-1">{transfer.note}</p>
                      )}
                    </td>
                    <td className="px-5 py-4 font-medium text-stone-700">
                      {transfer.items.length} {transfer.items.length === 1 ? 'producto' : 'productos'}
                    </td>
                    <td className="px-5 py-4">
                      <InventoryTransferStatusBadge status={transfer.status} />
                    </td>
                    <td className="px-5 py-4 text-stone-600">{formatDate(transfer.createdAt)}</td>
                    <td className="px-5 py-4">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          onClick={() => setDetailTransfer(transfer)}
                          size="sm"
                          type="button"
                          variant="secondary"
                        >
                          <Eye size={14} />
                          Detalle
                        </Button>
                        {transfer.status === 'Pending' && (
                          <Button
                            disabled={cancelTransfer.isPending}
                            onClick={() => void handleCancel(transfer.id)}
                            size="sm"
                            type="button"
                            variant="secondary"
                          >
                            <XCircle size={14} />
                            Cancelar
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {data && (
            <div className="flex flex-col gap-3 border-t border-stone-200 px-5 py-3 sm:flex-row sm:items-center sm:justify-between">
              <p className="text-sm font-medium text-stone-500">
                Página {filters.page} de {data.totalPages || 1}
              </p>
              <div className="flex gap-2">
                <Button
                  disabled={!data.hasPreviousPage}
                  onClick={() => updateFilters({ page: filters.page - 1 })}
                  size="sm"
                  type="button"
                  variant="secondary"
                >
                  <ChevronLeft size={14} />
                  Anterior
                </Button>
                <Button
                  disabled={!data.hasNextPage}
                  onClick={() => updateFilters({ page: filters.page + 1 })}
                  size="sm"
                  type="button"
                  variant="secondary"
                >
                  Siguiente
                  <ChevronRight size={14} />
                </Button>
              </div>
            </div>
          )}
        </Card>

        {/* Detail panel */}
        {selectedTransfer && (
          <Card className="h-fit">
            <div className="flex items-center justify-between border-b border-stone-200 px-4 py-3">
              <h3 className="text-sm font-semibold text-stone-950">Detalle de transferencia</h3>
              <Button
                onClick={() => setDetailTransfer(null)}
                size="sm"
                type="button"
                variant="ghost"
              >
                ✕
              </Button>
            </div>
            <div className="space-y-4 px-4 py-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Estado</p>
                <div className="mt-1">
                  <InventoryTransferStatusBadge status={selectedTransfer.status} />
                </div>
              </div>

              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Origen</p>
                <p className="mt-1 font-semibold text-stone-900">
                  {selectedTransfer.sourceBranchName ?? selectedTransfer.sourceBranchId}
                </p>
              </div>

              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Destino</p>
                <p className="mt-1 font-semibold text-stone-900">
                  {selectedTransfer.targetBranchName ?? selectedTransfer.targetBranchId}
                </p>
              </div>

              {selectedTransfer.note && (
                <div>
                  <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Nota</p>
                  <p className="mt-1 text-sm font-medium text-stone-700">{selectedTransfer.note}</p>
                </div>
              )}

              {selectedTransfer.failureReason && (
                <div className="rounded-md bg-red-50 px-3 py-2.5">
                  <p className="text-xs font-semibold uppercase tracking-wide text-red-600">Motivo de fallo</p>
                  <p className="mt-1 text-sm font-medium text-red-800">{selectedTransfer.failureReason}</p>
                </div>
              )}

              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Productos</p>
                <div className="mt-2 space-y-1">
                  {selectedTransfer.items.map((item) => (
                    <div
                      className="flex items-center justify-between rounded bg-stone-50 px-3 py-2"
                      key={item.productId}
                    >
                      <span className="text-sm font-medium text-stone-800">
                        {item.productName ?? item.productId}
                      </span>
                      <span className="font-mono text-sm font-semibold text-stone-950">
                        {item.quantity}
                      </span>
                    </div>
                  ))}
                </div>
              </div>

              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Fechas</p>
                <p className="mt-1 text-sm font-medium text-stone-600">
                  Creada: {formatDate(selectedTransfer.createdAt)}
                </p>
                <p className="text-sm font-medium text-stone-600">
                  Actualizada: {formatDate(selectedTransfer.updatedAt)}
                </p>
              </div>

              {selectedTransfer.status === 'Pending' && (
                <Button
                  className="w-full"
                  disabled={cancelTransfer.isPending}
                  onClick={() => void handleCancel(selectedTransfer.id)}
                  type="button"
                  variant="secondary"
                >
                  <XCircle size={14} />
                  Cancelar transferencia
                </Button>
              )}
            </div>
          </Card>
        )}
      </div>

      <CreateInventoryTransferDialog
        open={createOpen}
        onOpenChange={setCreateOpen}
        onSaved={() => void refetch()}
      />
    </section>
  )
}

function formatDate(value: string) {
  try {
    return new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium', timeStyle: 'short' }).format(
      new Date(value),
    )
  } catch {
    return value
  }
}
