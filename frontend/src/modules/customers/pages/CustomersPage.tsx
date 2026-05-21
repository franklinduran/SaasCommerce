import {
  RefreshCw,
  Search,
  UserPlus,
  Users,
} from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  useCreateCustomer,
  useCustomerCreditInvalidation,
  useCustomers,
} from '@/modules/customers/hooks/useCustomers'
import type { CustomerFilters } from '@/modules/customers/types'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { CreateCustomerDialog } from '@/modules/customers/components/CreateCustomerDialog'
import { CustomerDetailPanel } from '@/modules/customers/components/CustomerDetailPanel'
import { CustomerListItem } from '@/modules/customers/components/CustomerListItem'

const defaultFilters: CustomerFilters = {
  isActive: '',
  page: 1,
  pageSize: 50,
  query: '',
  sortBy: 'fullName',
  sortDirection: 'asc',
}

export function CustomersPage() {
  const navigate = useNavigate()
  const { customerId } = useParams<{ customerId: string }>()
  const selectedId = customerId ?? null

  const [filters, setFilters] = useState<CustomerFilters>(defaultFilters)
  const [createOpen, setCreateOpen] = useState(false)

  const customers = useCustomers(filters)
  const createCustomer = useCreateCustomer()
  const items = useMemo(() => customers.data?.items ?? [], [customers.data?.items])
  useCustomerCreditInvalidation()

  // Auto-select first customer if none selected and list is loaded
  useEffect(() => {
    if (!selectedId && items.length > 0 && !customers.isLoading) {
      // optional: auto-select first - skipped to avoid jarring jumps
    }
  }, [selectedId, items.length, customers.isLoading])

  const stats = useMemo(() => {
    const active = items.filter((c) => c.isActive).length
    const withBalance = items.filter((c) => c.currentBalance > 0).length
    const blocked = items.filter((c) => c.creditStatus === 'Blocked').length
    const totalBalance = items.reduce((sum, c) => sum + c.currentBalance, 0)
    return { active, blocked, total: items.length, totalBalance, withBalance }
  }, [items])

  function selectCustomer(id: string | null) {
    navigate(id ? `/customers/${id}` : '/customers')
  }

  function updateFilters(values: Partial<CustomerFilters>) {
    setFilters((current) => ({ ...current, ...values, page: 1 }))
  }

  const hasFilters = Boolean(filters.query || filters.isActive)
  let listContent: React.ReactNode

  if (customers.isLoading) {
    listContent = <ListSkeleton />
  } else if (customers.isError) {
    listContent = <ListError onRetry={() => void customers.refetch()} />
  } else if (items.length === 0) {
    listContent = <ListEmpty hasFilters={hasFilters} onCreate={() => setCreateOpen(true)} />
  } else {
    listContent = (
      <ul className="divide-y divide-stone-100">
        {items.map((c) => (
          <CustomerListItem
            customer={c}
            key={c.id}
            onClick={() => selectCustomer(c.id)}
            selected={selectedId === c.id}
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
              <Users size={13} />
              Cuentas
            </p>
            <h2 className="mt-1 text-2xl font-semibold text-stone-950">Clientes</h2>
            <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
              Administra clientes, fiados, abonos y balances pendientes del negocio.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button
              disabled={customers.isFetching}
              onClick={() => void customers.refetch()}
              size="sm"
              type="button"
              variant="secondary"
            >
              <RefreshCw className={customers.isFetching ? 'animate-spin' : undefined} size={14} />
              Refrescar
            </Button>
            <Button onClick={() => setCreateOpen(true)} size="sm" type="button">
              <UserPlus size={14} />
              Crear cliente
            </Button>
          </div>
        </div>

        <div className="mt-5 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
          <StatChip label="Total" tone="stone" value={String(stats.total)} />
          <StatChip label="Activos" tone="emerald" value={String(stats.active)} />
          <StatChip label="Con balance" tone="amber" value={String(stats.withBalance)} />
          <StatChip
            label="Por cobrar"
            tone="stone"
            value={formatMoney(stats.totalBalance)}
          />
        </div>
      </div>

      {/* Split view */}
      <div className="grid flex-1 lg:grid-cols-[400px_minmax(0,1fr)]">
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
                onChange={(e) => updateFilters({ query: e.target.value })}
                placeholder="Buscar por nombre o telefono"
                value={filters.query}
              />
            </div>
            <Select
              value={filters.isActive || '_'}
              onValueChange={(v) => updateFilters({ isActive: v === '_' ? '' : v })}
            >
              <SelectTrigger aria-label="Filtrar por estado" className="h-9">
                <SelectValue placeholder="Estado" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="_">Todos los clientes</SelectItem>
                <SelectItem value="true">Activos</SelectItem>
                <SelectItem value="false">Inactivos</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex-1">{listContent}</div>

          <div className="shrink-0 border-t border-stone-200 px-3 py-2 text-xs font-medium text-stone-500">
            Mostrando {items.length} de {customers.data?.totalItems ?? 0}
          </div>
        </div>

        {/* Right column */}
        <div className="bg-stone-50">
          {selectedId ? (
            <CustomerDetailPanel
              customerId={selectedId}
              key={selectedId}
              onClose={() => selectCustomer(null)}
            />
          ) : (
            <DetailEmptyState onCreate={() => setCreateOpen(true)} />
          )}
        </div>
      </div>

      <CreateCustomerDialog
        isSubmitting={createCustomer.isPending}
        onOpenChange={setCreateOpen}
        onSubmit={(request) =>
          createCustomer.mutate(request, {
            onSuccess: (created) => {
              setCreateOpen(false)
              if (created?.id) selectCustomer(created.id)
            },
          })
        }
        open={createOpen}
      />
    </section>
  )
}

// ── Stat chip ────────────────────────────────────────────────────────────────

type ChipTone = 'stone' | 'emerald' | 'amber'

const toneClasses: Record<ChipTone, string> = {
  amber: 'bg-amber-50 ring-amber-200 text-amber-800',
  emerald: 'bg-emerald-50 ring-emerald-200 text-emerald-800',
  stone: 'bg-stone-50 ring-stone-200 text-stone-700',
}

function StatChip({ label, tone, value }: Readonly<{ label: string; tone: ChipTone; value: string }>) {
  return (
    <div className={`flex items-center justify-between rounded-md px-3 py-2.5 ring-1 ${toneClasses[tone]}`}>
      <span className="text-xs font-semibold uppercase tracking-wide">{label}</span>
      <span className="text-lg font-semibold tabular-nums">{value}</span>
    </div>
  )
}

// ── List states ──────────────────────────────────────────────────────────────

const skeletonKeys = ['a', 'b', 'c', 'd', 'e', 'f']

function ListSkeleton() {
  return (
    <ul className="divide-y divide-stone-100">
      {skeletonKeys.map((k) => (
        <li className="flex items-center gap-3 px-3 py-3" key={k}>
          <div className="h-10 w-10 shrink-0 rounded-full bg-stone-100" />
          <div className="min-w-0 flex-1 space-y-1.5">
            <div className="h-3.5 w-3/4 rounded bg-stone-100" />
            <div className="h-3 w-1/2 rounded bg-stone-100" />
          </div>
        </li>
      ))}
    </ul>
  )
}

function ListError({ onRetry }: Readonly<{ onRetry: () => void }>) {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
        No se pudieron cargar los clientes.
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
          Ajusta la busqueda o los filtros para encontrar clientes.
        </p>
      </div>
    )
  }

  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-stone-100">
        <Users className="text-stone-500" size={20} />
      </div>
      <p className="text-sm font-semibold text-stone-900">Aun no hay clientes</p>
      <p className="max-w-xs text-xs font-medium text-stone-500">
        Crea el primer cliente para empezar a gestionar fiados y abonos.
      </p>
      <Button onClick={onCreate} size="sm" type="button">
        <UserPlus size={14} />
        Crear cliente
      </Button>
    </div>
  )
}

// ── Detail empty state ───────────────────────────────────────────────────────

function DetailEmptyState({ onCreate }: Readonly<{ onCreate: () => void }>) {
  return (
    <div className="flex h-full min-h-[420px] flex-col items-center justify-center gap-4 p-8 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-white ring-1 ring-stone-200">
        <Users className="text-stone-400" size={28} />
      </div>
      <div>
        <p className="text-base font-semibold text-stone-900">Selecciona un cliente</p>
        <p className="mt-1 max-w-sm text-sm font-medium text-stone-500">
          Elige un cliente para ver su informacion de credito, balance y movimientos. O crea uno nuevo.
        </p>
      </div>
      <Button onClick={onCreate} type="button" variant="secondary">
        <UserPlus size={15} />
        Crear cliente
      </Button>
    </div>
  )
}

// ── Helpers ──────────────────────────────────────────────────────────────────

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { maximumFractionDigits: 0 })}`
}
