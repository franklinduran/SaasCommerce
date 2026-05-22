import { GitBranch, Pencil, Plus, PowerOff, Zap } from 'lucide-react'
import { useState } from 'react'
import { BranchFormDialog } from '@/modules/branches/components/BranchFormDialog'
import { BranchStatusBadge } from '@/modules/branches/components/BranchStatusBadge'
import {
  useActivateBranchMutation,
  useBranches,
  useDeactivateBranchMutation,
} from '@/modules/branches/hooks/useBranches'
import type { Branch } from '@/modules/branches/types'
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

export function BranchesPage() {
  const [activeFilter, setActiveFilter] = useState<string>('_')
  const [formOpen, setFormOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<Branch | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  const filters = {
    isActive: activeFilter === '_' ? null : activeFilter === 'true',
  }

  const { data, isError, isLoading, refetch } = useBranches(filters)
  const items = data?.items ?? []

  const activateBranch = useActivateBranchMutation()
  const deactivateBranch = useDeactivateBranchMutation()

  function openCreate() {
    setEditTarget(null)
    setFormOpen(true)
  }

  function openEdit(branch: Branch) {
    setEditTarget(branch)
    setFormOpen(true)
  }

  async function handleActivate(branchId: string) {
    setActionError(null)
    try {
      await activateBranch.mutateAsync(branchId)
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo activar la sucursal.'
          : 'No se pudo activar la sucursal.'
      setActionError(message)
    }
  }

  async function handleDeactivate(branchId: string) {
    setActionError(null)
    try {
      await deactivateBranch.mutateAsync(branchId)
    } catch (error) {
      const message =
        error instanceof HttpClientError
          ? error.error?.message ?? 'No se pudo desactivar la sucursal.'
          : 'No se pudo desactivar la sucursal.'
      setActionError(message)
    }
  }

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
        <div>
          <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
            <GitBranch size={13} />
            Organización
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Sucursales</h2>
          <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
            Gestiona las sucursales de tu negocio, actívalas o desactívalas según sea necesario.
          </p>
        </div>

        <Button className="w-full xl:w-auto" onClick={openCreate} type="button">
          <Plus size={16} />
          Nueva sucursal
        </Button>
      </div>

      {actionError && (
        <div className="rounded-md bg-red-50 px-4 py-3 text-sm font-semibold text-red-700 ring-1 ring-red-200">
          {actionError}
        </div>
      )}

      <Card className="overflow-hidden">
        <div className="border-b border-stone-200 px-4 py-3 sm:px-5">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h3 className="text-base font-semibold text-stone-950">Listado de sucursales</h3>
              <p className="mt-1 text-sm font-medium text-stone-600">
                {data?.total ?? 0} sucursales registradas
              </p>
            </div>

            <div className="flex items-center gap-2">
              <Label className="shrink-0 text-sm" htmlFor="branch-status-filter">Estado</Label>
              <Select
                value={activeFilter}
                onValueChange={(v) => setActiveFilter(v)}
              >
                <SelectTrigger className="h-9 w-[160px]" id="branch-status-filter">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Todas</SelectItem>
                  <SelectItem value="true">Activas</SelectItem>
                  <SelectItem value="false">Inactivas</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full min-w-[700px] text-left text-sm">
            <thead className="bg-stone-50 text-xs font-semibold uppercase tracking-wide text-stone-500">
              <tr>
                <th className="px-5 py-3">Nombre</th>
                <th className="px-5 py-3">Código</th>
                <th className="px-5 py-3">Dirección</th>
                <th className="px-5 py-3">Teléfono</th>
                <th className="px-5 py-3">Estado</th>
                <th className="px-5 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200 bg-white">
              {isLoading && (
                <tr>
                  <td className="px-5 py-10 text-center text-sm font-medium text-stone-600" colSpan={6}>
                    Cargando sucursales...
                  </td>
                </tr>
              )}
              {isError && (
                <tr>
                  <td className="px-5 py-10 text-center" colSpan={6}>
                    <p className="text-sm font-medium text-red-700">No se pudieron cargar las sucursales.</p>
                    <Button className="mt-3" onClick={() => void refetch()} size="sm" type="button" variant="secondary">
                      Reintentar
                    </Button>
                  </td>
                </tr>
              )}
              {!isLoading && !isError && items.length === 0 && (
                <tr>
                  <td className="px-5 py-12 text-center" colSpan={6}>
                    <p className="text-sm font-semibold text-stone-900">No hay sucursales</p>
                    <p className="mt-1 text-sm font-medium text-stone-500">
                      Crea una sucursal para comenzar.
                    </p>
                    <Button className="mt-4" onClick={openCreate} type="button">
                      <Plus size={16} />
                      Nueva sucursal
                    </Button>
                  </td>
                </tr>
              )}
              {!isLoading && !isError && items.map((branch) => (
                <tr className="hover:bg-stone-50" key={branch.id}>
                  <td className="px-5 py-4">
                    <p className="font-semibold text-stone-950">{branch.name}</p>
                  </td>
                  <td className="px-5 py-4">
                    <span className="rounded bg-stone-100 px-1.5 py-0.5 font-mono text-xs font-semibold text-stone-700">
                      {branch.code}
                    </span>
                  </td>
                  <td className="px-5 py-4 text-stone-600">{branch.address ?? '-'}</td>
                  <td className="px-5 py-4 text-stone-600">{branch.phone ?? '-'}</td>
                  <td className="px-5 py-4">
                    <BranchStatusBadge isActive={branch.isActive} isMain={branch.isMain} />
                  </td>
                  <td className="px-5 py-4">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        onClick={() => openEdit(branch)}
                        size="sm"
                        type="button"
                        variant="secondary"
                      >
                        <Pencil size={14} />
                        Editar
                      </Button>
                      {branch.isActive ? (
                        <Button
                          disabled={branch.isMain || deactivateBranch.isPending}
                          onClick={() => void handleDeactivate(branch.id)}
                          size="sm"
                          title={branch.isMain ? 'No se puede desactivar la sucursal principal' : undefined}
                          type="button"
                          variant="secondary"
                        >
                          <PowerOff size={14} />
                          Desactivar
                        </Button>
                      ) : (
                        <Button
                          disabled={activateBranch.isPending}
                          onClick={() => void handleActivate(branch.id)}
                          size="sm"
                          type="button"
                          variant="secondary"
                        >
                          <Zap size={14} />
                          Activar
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      <BranchFormDialog
        branch={editTarget}
        open={formOpen}
        onOpenChange={setFormOpen}
        onSaved={() => void refetch()}
      />
    </section>
  )
}
