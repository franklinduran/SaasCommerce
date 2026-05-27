import { zodResolver } from '@hookform/resolvers/zod'
import { Filter, Loader2, MessageSquareWarning, RefreshCw, Send } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import {
  betaFeedbackCategories,
  betaFeedbackSchema,
  betaFeedbackStatuses,
  type BetaFeedbackFormValues,
} from '@/modules/beta-feedback/schemas/betaFeedbackSchema'
import {
  useBetaFeedback,
  useBetaFeedbackRealtimeInvalidation,
  useCreateBetaFeedback,
  useUpdateBetaFeedbackStatus,
} from '@/modules/beta-feedback/hooks/useBetaFeedback'
import type { BetaFeedback, BetaFeedbackFilters, BetaFeedbackStatus } from '@/modules/beta-feedback/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { useCurrentUserPermissions } from '@/shared/hooks/usePermissions'
import { Permission } from '@/shared/types/permissions'

const initialFilters: BetaFeedbackFilters = {
  category: '',
  page: 1,
  pageSize: 20,
  status: '',
}

const categoryLabels: Record<string, string> = {
  Bug: 'Bug',
  CashIssue: 'Problema de caja',
  DataError: 'Error de datos',
  Improvement: 'Mejora',
  InventoryIssue: 'Problema de inventario',
  PurchaseIssue: 'Problema de compra',
  Question: 'Duda',
  SaleIssue: 'Problema de venta',
}

const statusLabels: Record<BetaFeedbackStatus, string> = {
  Accepted: 'Aceptado',
  InReview: 'En revision',
  New: 'Nuevo',
  Rejected: 'Rechazado',
  Resolved: 'Resuelto',
}

export function BetaFeedbackPage() {
  const [filters, setFilters] = useState<BetaFeedbackFilters>(initialFilters)
  const feedback = useBetaFeedback(filters)
  const createFeedback = useCreateBetaFeedback()
  const updateStatus = useUpdateBetaFeedbackStatus()
  const permissions = useCurrentUserPermissions()
  const canManage = Boolean(permissions.data?.permissions.includes(Permission.BetaFeedbackManage))

  useBetaFeedbackRealtimeInvalidation()

  const items = useMemo(() => feedback.data?.items ?? [], [feedback.data?.items])
  const hasFilters = Boolean(filters.status || filters.category)

  const form = useForm<BetaFeedbackFormValues>({
    defaultValues: {
      category: 'Bug',
      contextUrl: '',
      description: '',
      title: '',
    },
    resolver: zodResolver(betaFeedbackSchema),
  })

  async function onSubmit(values: BetaFeedbackFormValues) {
    await createFeedback.mutateAsync({
      category: values.category,
      contextUrl: optionalText(values.contextUrl),
      description: values.description.trim(),
      title: values.title.trim(),
    })
    form.reset({ category: 'Bug', contextUrl: '', description: '', title: '' })
  }

  function updateFilters(values: Partial<BetaFeedbackFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
        <div>
          <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
            <MessageSquareWarning size={13} />
            Beta interna
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Feedback de pilotos</h2>
          <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
            Registra fricciones operativas y revisa reportes por negocio sin mezclar tenants.
          </p>
        </div>
        <Button disabled={feedback.isFetching} onClick={() => void feedback.refetch()} type="button" variant="secondary">
          <RefreshCw className={feedback.isFetching ? 'animate-spin' : undefined} size={16} />
          Actualizar
        </Button>
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(320px,420px)_minmax(0,1fr)]">
        <Card>
          <CardHeader className="border-b border-stone-200">
            <h3 className="text-base font-semibold text-stone-950">Reportar caso</h3>
            <p className="mt-1 text-sm font-medium text-stone-600">
              Usa este formulario para bugs, dudas y problemas de datos durante pilotos.
            </p>
          </CardHeader>
          <CardContent className="pt-5">
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <Field label="Categoria" message={form.formState.errors.category?.message}>
                <select
                  className="h-10 w-full rounded-md border border-stone-300 bg-white px-3 text-sm font-medium text-stone-900 outline-none focus:border-stone-500 focus:ring-2 focus:ring-stone-900/10"
                  {...form.register('category')}
                >
                  {betaFeedbackCategories.map((category) => (
                    <option key={category} value={category}>
                      {categoryLabels[category]}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Titulo" message={form.formState.errors.title?.message}>
                <Input placeholder="Ej. La venta queda procesando" {...form.register('title')} />
              </Field>

              <Field label="Descripcion" message={form.formState.errors.description?.message}>
                <textarea
                  className="min-h-32 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-900 outline-none focus:border-stone-500 focus:ring-2 focus:ring-stone-900/10"
                  placeholder="Cuenta que paso, modulo, pasos y resultado esperado."
                  {...form.register('description')}
                />
              </Field>

              <Field label="URL o contexto" message={form.formState.errors.contextUrl?.message}>
                <Input placeholder="/sales/..." {...form.register('contextUrl')} />
              </Field>

              {createFeedback.isError && (
                <p className="rounded-md bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 ring-1 ring-red-200">
                  No se pudo registrar el feedback. Intenta nuevamente.
                </p>
              )}

              {createFeedback.isSuccess && (
                <p className="rounded-md bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-700 ring-1 ring-emerald-200">
                  Feedback registrado para revision.
                </p>
              )}

              <Button className="w-full" disabled={createFeedback.isPending} type="submit">
                {createFeedback.isPending ? <Loader2 className="animate-spin" size={16} /> : <Send size={16} />}
                Enviar feedback
              </Button>
            </form>
          </CardContent>
        </Card>

        <Card className="overflow-hidden">
          <CardHeader className="border-b border-stone-200">
            <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
              <div>
                <h3 className="text-base font-semibold text-stone-950">Casos reportados</h3>
                <p className="mt-1 text-sm font-medium text-stone-600">
                  {feedback.data?.totalItems ?? 0} reportes encontrados
                </p>
              </div>
              <div className="grid gap-2 sm:grid-cols-2">
                <FilterSelect
                  label="Estado"
                  onChange={(value) => updateFilters({ status: value as BetaFeedbackFilters['status'] })}
                  value={filters.status}
                >
                  <option value="">Todos</option>
                  {betaFeedbackStatuses.map((status) => (
                    <option key={status} value={status}>
                      {statusLabels[status]}
                    </option>
                  ))}
                </FilterSelect>
                <FilterSelect
                  label="Categoria"
                  onChange={(value) => updateFilters({ category: value as BetaFeedbackFilters['category'] })}
                  value={filters.category}
                >
                  <option value="">Todas</option>
                  {betaFeedbackCategories.map((category) => (
                    <option key={category} value={category}>
                      {categoryLabels[category]}
                    </option>
                  ))}
                </FilterSelect>
              </div>
            </div>
          </CardHeader>

          <CardContent className="p-0">
            {feedback.isLoading && (
              <div className="flex h-56 items-center justify-center">
                <Loader2 className="animate-spin text-stone-400" size={28} />
              </div>
            )}

            {feedback.isError && (
              <div className="p-6 text-center">
                <p className="text-sm font-semibold text-red-700">No se pudo cargar el feedback.</p>
                <Button className="mt-3" onClick={() => void feedback.refetch()} size="sm" type="button" variant="secondary">
                  Reintentar
                </Button>
              </div>
            )}

            {!feedback.isLoading && !feedback.isError && items.length === 0 && (
              <div className="p-10 text-center">
                <p className="text-sm font-semibold text-stone-900">
                  {hasFilters ? 'No hay casos con esos filtros' : 'Aun no hay feedback registrado'}
                </p>
                <p className="mt-1 text-sm font-medium text-stone-500">
                  {hasFilters ? 'Ajusta estado o categoria para revisar otros casos.' : 'Los reportes de pilotos apareceran aqui.'}
                </p>
              </div>
            )}

            {items.length > 0 && (
              <div className="divide-y divide-stone-200">
                {items.map((item) => (
                  <FeedbackRow
                    canManage={canManage}
                    feedback={item}
                    isUpdating={updateStatus.isPending}
                    key={item.id}
                    onStatusChange={(status) =>
                      updateStatus.mutate({
                        feedbackId: item.id,
                        request: { status },
                      })}
                  />
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </section>
  )
}

function Field({
  children,
  label,
  message,
}: Readonly<{
  children: React.ReactNode
  label: string
  message?: string
}>) {
  return (
    <label className="block space-y-1.5">
      <span className="block text-sm font-semibold text-stone-800">{label}</span>
      {children}
      {message && <span className="block text-xs font-semibold text-red-600">{message}</span>}
    </label>
  )
}

function FilterSelect({
  children,
  label,
  onChange,
  value,
}: Readonly<{
  children: React.ReactNode
  label: string
  onChange: (value: string) => void
  value: string
}>) {
  return (
    <label className="block min-w-0">
      <span className="mb-1 flex items-center gap-1 text-xs font-semibold uppercase tracking-wide text-stone-500">
        <Filter size={12} />
        {label}
      </span>
      <select
        className="h-9 w-full rounded-md border border-stone-300 bg-white px-3 text-sm font-medium text-stone-900"
        onChange={(event) => onChange(event.target.value)}
        value={value}
      >
        {children}
      </select>
    </label>
  )
}

function FeedbackRow({
  canManage,
  feedback,
  isUpdating,
  onStatusChange,
}: Readonly<{
  canManage: boolean
  feedback: BetaFeedback
  isUpdating: boolean
  onStatusChange: (status: BetaFeedbackStatus) => void
}>) {
  return (
    <article className="space-y-3 bg-white p-5">
      <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <StatusBadge status={feedback.status} />
            <span className="rounded-full bg-stone-100 px-2.5 py-0.5 text-xs font-semibold text-stone-700 ring-1 ring-stone-200">
              {categoryLabels[feedback.category]}
            </span>
          </div>
          <h4 className="mt-2 text-base font-semibold text-stone-950">{feedback.title}</h4>
          <p className="mt-1 max-w-3xl whitespace-pre-wrap text-sm font-medium text-stone-600">
            {feedback.description}
          </p>
          {feedback.contextUrl && (
            <p className="mt-2 break-all text-xs font-semibold text-stone-500">{feedback.contextUrl}</p>
          )}
        </div>
        {canManage && (
          <select
            aria-label={`Cambiar estado de ${feedback.title}`}
            className="h-9 rounded-md border border-stone-300 bg-white px-3 text-sm font-semibold text-stone-900"
            disabled={isUpdating}
            onChange={(event) => onStatusChange(event.target.value as BetaFeedbackStatus)}
            value={feedback.status}
          >
            {betaFeedbackStatuses.map((status) => (
              <option key={status} value={status}>
                {statusLabels[status]}
              </option>
            ))}
          </select>
        )}
      </div>
      <p className="text-xs font-medium text-stone-500">
        Creado {formatDate(feedback.createdAt)}
        {feedback.reviewNote ? ` - Nota: ${feedback.reviewNote}` : ''}
      </p>
    </article>
  )
}

function StatusBadge({ status }: Readonly<{ status: BetaFeedbackStatus }>) {
  const tone = {
    Accepted: 'bg-sky-50 text-sky-700 ring-sky-200',
    InReview: 'bg-amber-50 text-amber-700 ring-amber-200',
    New: 'bg-stone-100 text-stone-700 ring-stone-200',
    Rejected: 'bg-red-50 text-red-700 ring-red-200',
    Resolved: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  }[status]

  return (
    <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ${tone}`}>
      {statusLabels[status]}
    </span>
  )
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('es-DO', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

function optionalText(value?: string) {
  const trimmed = value?.trim()
  return trimmed && trimmed.length > 0 ? trimmed : undefined
}
