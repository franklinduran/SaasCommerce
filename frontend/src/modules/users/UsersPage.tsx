import {
  RefreshCw,
  Search,
  Shield,
  UserPlus,
  Users as UsersIcon,
} from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { CreateUserDialog } from './components/CreateUserDialog'
import { UserDetailPanel } from './components/UserDetailPanel'
import { UserListItem } from './components/UserListItem'
import { usersApi } from './services/usersApi'
import type { UserSummary } from './types'

export default function UsersPage() {
  const [users, setUsers] = useState<UserSummary[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [roleFilter, setRoleFilter] = useState<string>('')
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)

  const applyUsers = useCallback((items: UserSummary[]) => {
    setUsers(items)
    setError(null)
  }, [])

  const loadUsers = useCallback(async () => {
    setIsLoading(true)
    try {
      const response = await usersApi.getUsers()
      applyUsers(response.items)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cargar usuarios')
    } finally {
      setIsLoading(false)
    }
  }, [applyUsers])

  useEffect(() => {
    let cancelled = false

    async function fetchInitialUsers() {
      try {
        const response = await usersApi.getUsers()
        if (!cancelled) {
          applyUsers(response.items)
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Error al cargar usuarios')
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    }

    void fetchInitialUsers()

    return () => {
      cancelled = true
    }
  }, [applyUsers])

  const filteredUsers = useMemo(() => {
    const term = search.trim().toLowerCase()
    return users.filter((u) => {
      if (term && !`${u.fullName} ${u.email}`.toLowerCase().includes(term)) return false
      if (roleFilter && u.role !== roleFilter) return false
      if (statusFilter === 'active' && !u.isActive) return false
      if (statusFilter === 'inactive' && u.isActive) return false
      return true
    })
  }, [users, search, roleFilter, statusFilter])

  const stats = useMemo(
    () => ({
      total: users.length,
      active: users.filter((u) => u.isActive).length,
      inactive: users.filter((u) => !u.isActive).length,
      admins: users.filter((u) => u.role === 'Admin').length,
    }),
    [users],
  )

  const hasFilters = Boolean(search || roleFilter || statusFilter)
  let listContent: React.ReactNode

  if (isLoading) {
    listContent = <ListSkeleton />
  } else if (error) {
    listContent = <ListError message={error} onRetry={() => void loadUsers()} />
  } else if (filteredUsers.length === 0) {
    listContent = <ListEmpty hasFilters={hasFilters} onCreate={() => setCreateOpen(true)} />
  } else {
    listContent = (
      <ul className="divide-y divide-stone-100">
        {filteredUsers.map((u) => (
          <UserListItem
            key={u.userId}
            onClick={() => setSelectedUserId(u.userId)}
            selected={selectedUserId === u.userId}
            user={u}
          />
        ))}
      </ul>
    )
  }

  return (
    <section className="flex min-h-full flex-col">
      {/* Header */}
      <div className="shrink-0 border-b border-stone-200 bg-white px-4 py-5 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
              <Shield size={13} />
              Seguridad
            </p>
            <h2 className="mt-1 text-2xl font-semibold text-stone-950">Usuarios</h2>
            <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
              Gestiona cuentas, roles y accesos del personal del negocio.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button
              disabled={isLoading}
              onClick={() => void loadUsers()}
              size="sm"
              type="button"
              variant="secondary"
            >
              <RefreshCw className={isLoading ? 'animate-spin' : undefined} size={14} />
              Refrescar
            </Button>
            <Button onClick={() => setCreateOpen(true)} size="sm" type="button">
              <UserPlus size={14} />
              Crear usuario
            </Button>
          </div>
        </div>

        <div className="mt-5 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
          <StatChip label="Total de usuarios" tone="stone" value={stats.total} />
          <StatChip label="Activos" tone="emerald" value={stats.active} />
          <StatChip label="Inactivos" tone="amber" value={stats.inactive} />
          <StatChip label="Administradores" tone="stone" value={stats.admins} />
        </div>
      </div>

      {/* Split view */}
      <div className="grid flex-1 lg:grid-cols-[380px_minmax(0,1fr)]">
        {/* Left column */}
        <div className="flex min-h-0 flex-col bg-white lg:border-r lg:border-stone-200">
          <div className="shrink-0 space-y-2 border-b border-stone-200 p-3">
            <div className="relative">
              <Search
                aria-hidden="true"
                className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400"
                size={14}
              />
              <Input
                className="h-9 pl-9 text-sm"
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Buscar por nombre o correo"
                value={search}
              />
            </div>
            <div className="grid grid-cols-2 gap-2">
              <Select
                value={roleFilter || '_'}
                onValueChange={(v) => setRoleFilter(v === '_' ? '' : v)}
              >
                <SelectTrigger aria-label="Filtrar por rol" className="h-9">
                  <SelectValue placeholder="Rol" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Todos los roles</SelectItem>
                  <SelectItem value="Admin">Administrador</SelectItem>
                  <SelectItem value="Supervisor">Supervisor</SelectItem>
                  <SelectItem value="Cashier">Cajero</SelectItem>
                  <SelectItem value="InventoryManager">Gte. Inventario</SelectItem>
                  <SelectItem value="PurchasingManager">Gte. Compras</SelectItem>
                  <SelectItem value="ReadOnly">Solo lectura</SelectItem>
                </SelectContent>
              </Select>
              <Select
                value={statusFilter || '_'}
                onValueChange={(v) => setStatusFilter(v === '_' ? '' : v)}
              >
                <SelectTrigger aria-label="Filtrar por estado" className="h-9">
                  <SelectValue placeholder="Estado" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">Todos</SelectItem>
                  <SelectItem value="active">Activos</SelectItem>
                  <SelectItem value="inactive">Inactivos</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex-1">{listContent}</div>

          <div className="shrink-0 border-t border-stone-200 px-3 py-2 text-xs font-medium text-stone-500">
            Mostrando {filteredUsers.length} de {users.length} usuarios
          </div>
        </div>

        {/* Right column */}
        <div className="bg-stone-50">
          {selectedUserId ? (
            <UserDetailPanel
              key={selectedUserId}
              onClose={() => setSelectedUserId(null)}
              onUpdated={() => void loadUsers()}
              userId={selectedUserId}
            />
          ) : (
            <DetailEmptyState onCreate={() => setCreateOpen(true)} />
          )}
        </div>
      </div>

      <CreateUserDialog
        onOpenChange={setCreateOpen}
        onSuccess={(newUserId) => {
          setCreateOpen(false)
          void loadUsers()
          if (newUserId) setSelectedUserId(newUserId)
        }}
        open={createOpen}
      />
    </section>
  )
}

// ── Stat chip ────────────────────────────────────────────────────────────────

type ChipTone = 'stone' | 'emerald' | 'amber'

const toneClasses: Record<ChipTone, string> = {
  stone: 'bg-stone-50 ring-stone-200 text-stone-700',
  emerald: 'bg-emerald-50 ring-emerald-200 text-emerald-800',
  amber: 'bg-amber-50 ring-amber-200 text-amber-800',
}

function StatChip({ label, tone, value }: Readonly<{ label: string; tone: ChipTone; value: number }>) {
  return (
    <div className={`flex items-center justify-between rounded-md px-3 py-2.5 ring-1 ${toneClasses[tone]}`}>
      <span className="text-xs font-semibold uppercase tracking-wide">{label}</span>
      <span className="text-lg font-semibold tabular-nums">{value}</span>
    </div>
  )
}

// ── List states ──────────────────────────────────────────────────────────────

const skeletonRows = ['a', 'b', 'c', 'd', 'e', 'f']

function ListSkeleton() {
  return (
    <ul className="divide-y divide-stone-100">
      {skeletonRows.map((k) => (
        <li className="flex items-center gap-3 px-3 py-3" key={k}>
          <div className="h-9 w-9 shrink-0 rounded-full bg-stone-100" />
          <div className="min-w-0 flex-1 space-y-1.5">
            <div className="h-3.5 w-3/4 rounded bg-stone-100" />
            <div className="h-3 w-1/2 rounded bg-stone-100" />
          </div>
        </li>
      ))}
    </ul>
  )
}

function ListError({ message, onRetry }: Readonly<{ message: string; onRetry: () => void }>) {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
        {message}
      </div>
      <Button onClick={onRetry} size="sm" type="button" variant="secondary">
        <RefreshCw size={14} />
        Reintentar
      </Button>
    </div>
  )
}

function ListEmpty({ hasFilters, onCreate }: Readonly<{ hasFilters: boolean; onCreate: () => void }>) {
  if (hasFilters) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-2 p-8 text-center">
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-stone-100">
          <Search className="text-stone-500" size={18} />
        </div>
        <p className="text-sm font-semibold text-stone-900">Sin resultados</p>
        <p className="max-w-xs text-xs font-medium text-stone-500">
          Ajusta la busqueda o los filtros para encontrar usuarios.
        </p>
      </div>
    )
  }

  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-stone-100">
        <UsersIcon className="text-stone-500" size={20} />
      </div>
      <p className="text-sm font-semibold text-stone-900">Aun no hay usuarios</p>
      <p className="max-w-xs text-xs font-medium text-stone-500">
        Crea la primera cuenta para empezar a delegar accesos.
      </p>
      <Button onClick={onCreate} size="sm" type="button">
        <UserPlus size={14} />
        Crear usuario
      </Button>
    </div>
  )
}

// ── Detail empty state ───────────────────────────────────────────────────────

function DetailEmptyState({ onCreate }: Readonly<{ onCreate: () => void }>) {
  return (
    <div className="flex h-full min-h-[420px] flex-col items-center justify-center gap-4 p-8 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-white ring-1 ring-stone-200">
        <UsersIcon className="text-stone-400" size={28} />
      </div>
      <div>
        <p className="text-base font-semibold text-stone-900">Selecciona un usuario</p>
        <p className="mt-1 max-w-sm text-sm font-medium text-stone-500">
          Elige una cuenta de la lista para ver sus datos, cambiar su rol o gestionar
          su acceso. O crea un nuevo usuario.
        </p>
      </div>
      <Button onClick={onCreate} type="button" variant="secondary">
        <UserPlus size={15} />
        Crear usuario
      </Button>
    </div>
  )
}
