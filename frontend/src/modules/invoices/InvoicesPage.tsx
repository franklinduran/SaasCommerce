import { FileText, RefreshCw, Search } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { InvoiceDetailPanel } from '@/modules/invoices/components/InvoiceDetailPanel'
import { InvoiceListItem } from '@/modules/invoices/components/InvoiceListItem'
import { useInvoiceRealtimeInvalidation, useInvoices } from '@/modules/invoices/hooks/useInvoices'
import type { InvoiceFilters } from '@/modules/invoices/types'
import { formatInvoiceMoney } from '@/modules/invoices/utils/formatInvoiceMoney'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

const defaultFilters: InvoiceFilters = {
  dateFrom: '',
  dateTo: '',
  page: 1,
  pageSize: 50,
  query: '',
  status: '',
}

export function InvoicesPage() {
  const navigate = useNavigate()
  const { invoiceId } = useParams<{ invoiceId: string }>()
  const selectedId = invoiceId ?? null

  const [filters, setFilters] = useState<InvoiceFilters>(defaultFilters)
  const invoices = useInvoices(filters)
  const items = useMemo(() => invoices.data?.items ?? [], [invoices.data?.items])
  useInvoiceRealtimeInvalidation()

  const stats = useMemo(() => {
    const issued = items.filter((i) => i.status === 'Issued').length
    const cancelled = items.filter((i) => i.status === 'Cancelled').length
    const total = items.reduce((sum, i) => sum + i.total, 0)
    return { cancelled, issued, total, totalItems: items.length }
  }, [items])

  function updateFilters(values: Partial<InvoiceFilters>) {
    setFilters((current) => ({ ...current, ...values, page: 1 }))
  }

  function selectInvoice(id: string | null) {
    navigate(id ? `/invoices/${id}` : '/invoices')
  }

  const hasFilters = Boolean(filters.query || filters.status || filters.dateFrom || filters.dateTo)
  let listContent: React.ReactNode

  if (invoices.isLoading) {
    listContent = <ListSkeleton />
  } else if (invoices.isError) {
    listContent = <ListError onRetry={() => void invoices.refetch()} />
  } else if (items.length === 0) {
    listContent = <ListEmpty hasFilters={hasFilters} />
  } else {
    listContent = (
      <ul className="divide-y divide-stone-100">
        {items.map((inv) => (
          <InvoiceListItem
            invoice={inv}
            key={inv.invoiceId}
            onClick={() => selectInvoice(inv.invoiceId)}
            selected={selectedId === inv.invoiceId}
          />
        ))}
      </ul>
    )
  }

  return (
    <section className="flex min-h-full flex-col">
      <div className="shrink-0 border-b border-stone-200 bg-white px-4 py-5 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
              <FileText size={13} />
              Documentos
            </p>
            <h2 className="mt-1 text-2xl font-semibold text-stone-950">Recibos</h2>
            <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
              Consulta y administra los recibos generados a partir de las ventas.
            </p>
          </div>
          <Button
            disabled={invoices.isFetching}
            onClick={() => void invoices.refetch()}
            size="sm"
            type="button"
            variant="secondary"
          >
            <RefreshCw className={invoices.isFetching ? 'animate-spin' : undefined} size={14} />
            Refrescar
          </Button>
        </div>

        <div className="mt-5 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
          <StatChip label="Visibles" tone="stone" value={String(stats.totalItems)} />
          <StatChip label="Emitidos" tone="emerald" value={String(stats.issued)} />
          <StatChip label="Cancelados" tone="amber" value={String(stats.cancelled)} />
          <StatChip label="Monto total" tone="stone" value={formatInvoiceMoney(stats.total)} />
        </div>
      </div>

      <div className="grid flex-1 lg:grid-cols-[400px_minmax(0,1fr)]">
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
                placeholder="Buscar por numero o venta"
                value={filters.query}
              />
            </div>
            <div className="grid grid-cols-2 gap-2">
              <Input
                aria-label="Fecha desde"
                className="h-9 text-xs"
                onChange={(e) => updateFilters({ dateFrom: e.target.value })}
                type="date"
                value={filters.dateFrom}
              />
              <Input
                aria-label="Fecha hasta"
                className="h-9 text-xs"
                onChange={(e) => updateFilters({ dateTo: e.target.value })}
                type="date"
                value={filters.dateTo}
              />
            </div>
            <Select
              value={filters.status || '_'}
              onValueChange={(v) => updateFilters({ status: v === '_' ? '' : v })}
            >
              <SelectTrigger aria-label="Estado" className="h-9">
                <SelectValue placeholder="Estado" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="_">Todos</SelectItem>
                <SelectItem value="Issued">Emitidos</SelectItem>
                <SelectItem value="Cancelled">Cancelados</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex-1">{listContent}</div>

          <div className="shrink-0 border-t border-stone-200 px-3 py-2 text-xs font-medium text-stone-500">
            Mostrando {items.length} de {invoices.data?.totalItems ?? 0}
          </div>
        </div>

        <div className="bg-stone-50">
          {selectedId ? (
            <InvoiceDetailPanel
              invoiceId={selectedId}
              key={selectedId}
              onClose={() => selectInvoice(null)}
            />
          ) : (
            <DetailEmptyState />
          )}
        </div>
      </div>
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

const skeletonKeys = ['a', 'b', 'c', 'd', 'e']

function ListSkeleton() {
  return (
    <ul className="divide-y divide-stone-100">
      {skeletonKeys.map((k) => (
        <li className="space-y-1.5 px-3 py-3" key={k}>
          <div className="h-3.5 w-1/2 rounded bg-stone-100" />
          <div className="h-3 w-1/3 rounded bg-stone-100" />
        </li>
      ))}
    </ul>
  )
}

function ListError({ onRetry }: Readonly<{ onRetry: () => void }>) {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
      <div className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
        No se pudieron cargar los recibos.
      </div>
      <Button onClick={onRetry} size="sm" type="button" variant="secondary">
        <RefreshCw size={14} />
        Reintentar
      </Button>
    </div>
  )
}

function ListEmpty({ hasFilters }: Readonly<{ hasFilters: boolean }>) {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-2 p-8 text-center">
      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-stone-100">
        {hasFilters ? <Search className="text-stone-500" size={18} /> : <FileText className="text-stone-500" size={18} />}
      </div>
      <p className="text-sm font-semibold text-stone-900">
        {hasFilters ? 'Sin resultados' : 'Aun no hay recibos'}
      </p>
      <p className="max-w-xs text-xs font-medium text-stone-500">
        {hasFilters
          ? 'Ajusta los filtros para encontrar el recibo que buscas.'
          : 'Los recibos se generan automaticamente al completar ventas.'}
      </p>
    </div>
  )
}

function DetailEmptyState() {
  return (
    <div className="flex h-full min-h-[420px] flex-col items-center justify-center gap-4 p-8 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-white ring-1 ring-stone-200">
        <FileText className="text-stone-400" size={28} />
      </div>
      <div>
        <p className="text-base font-semibold text-stone-900">Selecciona un recibo</p>
        <p className="mt-1 max-w-sm text-sm font-medium text-stone-500">
          Elige un recibo de la lista para revisar sus totales y cancelarlo si es necesario.
        </p>
      </div>
    </div>
  )
}
