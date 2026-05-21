import {
  AlertTriangle,
  Ban,
  CheckCircle2,
  CreditCard,
  Mail,
  Phone,
  RotateCcw,
  ShieldOff,
  Wallet,
  WalletCards,
  X,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { CustomerCreditMovementsTable } from '@/modules/customers/components/CustomerCreditMovementsTable'
import { RegisterCustomerPaymentDialog } from '@/modules/customers/components/RegisterCustomerPaymentDialog'
import {
  useBlockCustomerCredit,
  useCustomerCredit,
  useCustomerCreditMovements,
  useUnblockCustomerCredit,
} from '@/modules/customers/hooks/useCustomerCredit'
import { useCustomerDetail } from '@/modules/customers/hooks/useCustomerDetail'
import {
  useDeactivateCustomer,
  useUpdateCustomer,
} from '@/modules/customers/hooks/useCustomers'
import { useRegisterCustomerPayment } from '@/modules/customers/hooks/useRegisterCustomerPayment'
import { customerSchema } from '@/modules/customers/schemas/customerSchemas'
import type { CustomerCreditStatus } from '@/modules/customers/types'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import { cn } from '@/shared/utils/cn'

type CustomerDetailPanelProps = {
  customerId: string
  onClose: () => void
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('') || '?'
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}

const creditStatusLabels: Record<CustomerCreditStatus, string> = {
  Active: 'Activo',
  Blocked: 'Bloqueado',
  Closed: 'Cerrado',
}

const creditStatusTones: Record<CustomerCreditStatus, string> = {
  Active: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Blocked: 'bg-red-50 text-red-700 ring-red-200',
  Closed: 'bg-stone-100 text-stone-700 ring-stone-200',
}

export function CustomerDetailPanel({ customerId, onClose }: Readonly<CustomerDetailPanelProps>) {
  const customer = useCustomerDetail(customerId)
  const credit = useCustomerCredit(customerId)
  const movements = useCustomerCreditMovements(customerId)
  const updateCustomer = useUpdateCustomer(customerId)
  const deactivateCustomer = useDeactivateCustomer()
  const registerPayment = useRegisterCustomerPayment(customerId)
  const blockCredit = useBlockCustomerCredit(customerId)
  const unblockCredit = useUnblockCustomerCredit(customerId)

  const [paymentOpen, setPaymentOpen] = useState(false)
  const [confirmBlock, setConfirmBlock] = useState(false)
  const [confirmDeactivate, setConfirmDeactivate] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [fullName, setFullName] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')

  useEffect(() => {
    if (customer.data) {
      setFullName(customer.data.fullName)
      setPhone(customer.data.phone ?? '')
      setEmail(customer.data.email ?? '')
    }
  }, [customer.data])

  useEffect(() => {
    if (!notice) return
    const t = window.setTimeout(() => setNotice(null), 3500)
    return () => window.clearTimeout(t)
  }, [notice])

  if (customer.isLoading || !customer.data) {
    return <DetailSkeleton />
  }

  const data = customer.data
  const summary = credit.data
  const isBlocked = summary?.status === 'Blocked'
  const isDirty =
    fullName !== data.fullName ||
    (phone || '') !== (data.phone ?? '') ||
    (email || '') !== (data.email ?? '')

  function handleSaveProfile() {
    const payload = {
      email: email.trim() || null,
      fullName: fullName.trim(),
      isActive: data.isActive,
      phone: phone.trim() || null,
    }
    const validation = customerSchema.safeParse(payload)
    if (!validation.success) {
      setFormError(validation.error.issues[0]?.message ?? 'Revisa los datos del cliente.')
      return
    }
    setFormError(null)
    updateCustomer.mutate(payload, {
      onSuccess: () => setNotice('Datos del cliente guardados.'),
    })
  }

  function handleToggleBlock() {
    if (isBlocked) {
      unblockCredit.mutate(undefined, {
        onSuccess: () => setNotice('Credito desbloqueado.'),
      })
    } else {
      setConfirmBlock(true)
    }
  }

  function confirmBlockCredit() {
    blockCredit.mutate(undefined, {
      onSuccess: () => {
        setConfirmBlock(false)
        setNotice('Credito bloqueado.')
      },
    })
  }

  function handleConfirmDeactivate() {
    deactivateCustomer.mutate(customerId, {
      onSuccess: () => {
        setConfirmDeactivate(false)
        setNotice('Cliente desactivado.')
      },
    })
  }

  const status = summary?.status ?? 'Active'

  return (
    <div className="flex h-full min-h-0 flex-col">
      {/* Sticky header */}
      <div className="sticky top-0 z-10 border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-center gap-3">
            <div className="relative shrink-0">
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-stone-900 text-base font-semibold text-white shadow-sm">
                {initials(data.fullName)}
              </div>
              <span
                aria-hidden="true"
                className={cn(
                  'absolute -bottom-0.5 -right-0.5 h-3.5 w-3.5 rounded-full ring-2 ring-white',
                  data.isActive ? 'bg-emerald-500' : 'bg-stone-300',
                )}
              />
            </div>
            <div className="min-w-0">
              <h3 className="truncate text-lg font-semibold text-stone-950">{data.fullName}</h3>
              <div className="mt-1 flex flex-wrap items-center gap-2 text-xs font-medium text-stone-500">
                {data.phone && (
                  <span className="inline-flex items-center gap-1">
                    <Phone size={12} />
                    {data.phone}
                  </span>
                )}
                {data.email && (
                  <span className="inline-flex items-center gap-1">
                    <Mail size={12} />
                    {data.email}
                  </span>
                )}
              </div>
              <div className="mt-2 flex flex-wrap items-center gap-1.5">
                <span className={cn('inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[11px] font-semibold ring-1', creditStatusTones[status])}>
                  Credito: {creditStatusLabels[status]}
                </span>
                {!data.isActive && (
                  <span className="rounded-full bg-stone-100 px-2 py-0.5 text-[11px] font-semibold text-stone-600 ring-1 ring-stone-200">
                    Inactivo
                  </span>
                )}
              </div>
            </div>
          </div>
          <div className="flex shrink-0 items-start gap-1">
            <Button onClick={() => setPaymentOpen(true)} size="sm" type="button">
              <WalletCards size={14} />
              Registrar abono
            </Button>
            <Button
              aria-label="Cerrar panel"
              className="lg:hidden"
              onClick={onClose}
              size="icon"
              type="button"
              variant="ghost"
            >
              <X size={16} />
            </Button>
          </div>
        </div>

        {notice && (
          <div className="mt-3 flex items-center gap-2 rounded-md bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-800 ring-1 ring-emerald-200">
            <CheckCircle2 size={15} />
            {notice}
          </div>
        )}
      </div>

      {/* Body */}
      <div className="min-h-0 flex-1 space-y-4 p-4 sm:p-6 lg:p-8">
        {/* Credit metrics */}
        <div className="grid gap-2 sm:grid-cols-3">
          <Metric
            icon={<Wallet size={15} />}
            label="Balance pendiente"
            value={summary ? formatMoney(summary.currentBalance) : '—'}
            tone={summary && summary.currentBalance > 0 ? 'amber' : 'stone'}
          />
          <Metric
            icon={<CreditCard size={15} />}
            label="Limite de credito"
            value={summary ? (summary.creditLimit === 0 ? 'Sin limite' : formatMoney(summary.creditLimit)) : '—'}
            tone="stone"
          />
          <Metric
            icon={isBlocked ? <Ban size={15} /> : <CheckCircle2 size={15} />}
            label="Estado"
            value={creditStatusLabels[status]}
            tone={isBlocked ? 'red' : 'emerald'}
          />
        </div>

        {/* Profile */}
        <Section description="Nombre y datos de contacto del cliente." title="Datos del cliente">
          <div className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-1.5">
                <Label htmlFor="fullName">Nombre completo</Label>
                <Input
                  id="fullName"
                  onChange={(e) => setFullName(e.target.value)}
                  value={fullName}
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="phone">Telefono</Label>
                <Input
                  id="phone"
                  onChange={(e) => setPhone(e.target.value)}
                  placeholder="8090000000"
                  value={phone}
                />
              </div>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="email">Correo electronico</Label>
              <Input
                id="email"
                onChange={(e) => setEmail(e.target.value)}
                placeholder="cliente@correo.com"
                type="email"
                value={email}
              />
            </div>
            {formError && (
              <p className="text-sm font-semibold text-red-700">{formError}</p>
            )}
            <div className="flex justify-end gap-2 border-t border-stone-100 pt-3">
              <Button
                disabled={updateCustomer.isPending || !isDirty}
                onClick={() => {
                  setFullName(data.fullName)
                  setPhone(data.phone ?? '')
                  setEmail(data.email ?? '')
                  setFormError(null)
                }}
                size="sm"
                type="button"
                variant="ghost"
              >
                Descartar
              </Button>
              <Button
                disabled={updateCustomer.isPending || !isDirty}
                onClick={handleSaveProfile}
                size="sm"
                type="button"
              >
                {updateCustomer.isPending ? 'Guardando...' : 'Guardar cambios'}
              </Button>
            </div>
          </div>
        </Section>

        {/* Credit history */}
        <Section
          description={`${movements.data?.totalItems ?? 0} movimientos registrados.`}
          title="Historial de credito"
        >
          <div className="-mx-4 sm:-mx-5">
            <CustomerCreditMovementsTable
              isError={movements.isError}
              isLoading={movements.isLoading}
              movements={movements.data?.items ?? []}
            />
          </div>
        </Section>

        {/* Danger zone */}
        <Section
          description="Acciones que afectan la operacion del cliente."
          icon={<AlertTriangle className="text-amber-600" size={16} />}
          title="Acciones avanzadas"
        >
          <div className="grid gap-2 sm:grid-cols-2">
            <ActionRow
              description={isBlocked ? 'Restaura la posibilidad de generar fiados.' : 'Impide nuevos fiados al cliente.'}
              icon={isBlocked ? <RotateCcw size={16} /> : <Ban size={16} />}
              label={isBlocked ? 'Desbloquear credito' : 'Bloquear credito'}
              tone={isBlocked ? 'default' : 'warning'}
            >
              <Button
                disabled={blockCredit.isPending || unblockCredit.isPending}
                onClick={handleToggleBlock}
                size="sm"
                type="button"
                variant={isBlocked ? 'secondary' : 'destructive'}
              >
                {isBlocked ? 'Desbloquear' : 'Bloquear'}
              </Button>
            </ActionRow>
            <ActionRow
              description="Marca el cliente como inactivo sin eliminar su historial."
              icon={<ShieldOff size={16} />}
              label="Desactivar cliente"
              tone="danger"
            >
              <Button
                disabled={deactivateCustomer.isPending || !data.isActive}
                onClick={() => setConfirmDeactivate(true)}
                size="sm"
                type="button"
                variant="destructive"
              >
                {data.isActive ? 'Desactivar' : 'Inactivo'}
              </Button>
            </ActionRow>
          </div>
        </Section>
      </div>

      <RegisterCustomerPaymentDialog
        isOpen={paymentOpen}
        isSubmitting={registerPayment.isPending}
        onClose={() => setPaymentOpen(false)}
        onSubmit={(amount, note) => {
          registerPayment.mutate(
            { amount, customerId, note },
            {
              onSuccess: () => {
                setPaymentOpen(false)
                setNotice(`Abono de ${formatMoney(amount)} registrado.`)
              },
            },
          )
        }}
      />

      <AlertDialog onOpenChange={setConfirmBlock} open={confirmBlock}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Bloquear credito</AlertDialogTitle>
            <AlertDialogDescription>
              <strong>{data.fullName}</strong> no podra generar nuevos fiados hasta que desbloquees el credito.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              disabled={blockCredit.isPending}
              onClick={confirmBlockCredit}
              variant="destructive"
            >
              Bloquear
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog onOpenChange={setConfirmDeactivate} open={confirmDeactivate}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Desactivar cliente</AlertDialogTitle>
            <AlertDialogDescription>
              <strong>{data.fullName}</strong> dejara de aparecer en la lista activa, pero su historial se conservara.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              disabled={deactivateCustomer.isPending}
              onClick={handleConfirmDeactivate}
              variant="destructive"
            >
              Desactivar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

// ── Section wrapper ──────────────────────────────────────────────────────────

function Section({
  children,
  description,
  icon,
  title,
}: Readonly<{
  children: React.ReactNode
  description?: string
  icon?: React.ReactNode
  title: string
}>) {
  return (
    <section className="rounded-md bg-white p-4 ring-1 ring-stone-200 sm:p-5">
      <header className="mb-4 flex items-start gap-2.5">
        {icon && <span className="mt-0.5 shrink-0">{icon}</span>}
        <div className="min-w-0">
          <h4 className="text-sm font-semibold text-stone-950">{title}</h4>
          {description && (
            <p className="mt-0.5 text-xs font-medium text-stone-500">{description}</p>
          )}
        </div>
      </header>
      {children}
    </section>
  )
}

// ── Metric chip ──────────────────────────────────────────────────────────────

const metricTones = {
  amber: 'bg-amber-50 ring-amber-200 text-amber-800',
  emerald: 'bg-emerald-50 ring-emerald-200 text-emerald-800',
  red: 'bg-red-50 ring-red-200 text-red-800',
  stone: 'bg-white ring-stone-200 text-stone-900',
} as const

type MetricTone = keyof typeof metricTones

function Metric({
  icon,
  label,
  tone = 'stone',
  value,
}: Readonly<{ icon: React.ReactNode; label: string; tone?: MetricTone; value: string }>) {
  return (
    <div className={cn('rounded-md p-3 ring-1', metricTones[tone])}>
      <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide">
        {icon}
        {label}
      </p>
      <p className="mt-2 text-lg font-semibold tabular-nums">{value}</p>
    </div>
  )
}

// ── Action row ───────────────────────────────────────────────────────────────

type ActionRowProps = {
  children: React.ReactNode
  description: string
  icon: React.ReactNode
  label: string
  tone?: 'default' | 'warning' | 'danger'
}

function ActionRow({ children, description, icon, label, tone = 'default' }: Readonly<ActionRowProps>) {
  const toneClass =
    tone === 'danger'
      ? 'border-red-200 bg-red-50/40'
      : tone === 'warning'
      ? 'border-amber-200 bg-amber-50/40'
      : 'border-stone-200 bg-stone-50/60'

  const iconClass =
    tone === 'danger'
      ? 'bg-red-100 text-red-700 ring-red-200'
      : tone === 'warning'
      ? 'bg-amber-100 text-amber-700 ring-amber-200'
      : 'bg-white text-stone-700 ring-stone-200'

  return (
    <div className={cn('flex flex-col gap-3 rounded-md border p-3', toneClass)}>
      <div className="flex items-start gap-2.5">
        <span className={cn('mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-md ring-1', iconClass)}>
          {icon}
        </span>
        <div className="min-w-0 flex-1">
          <p className="text-sm font-semibold text-stone-900">{label}</p>
          <p className="mt-0.5 text-xs font-medium text-stone-500">{description}</p>
        </div>
      </div>
      <div className="flex justify-end">{children}</div>
    </div>
  )
}

// ── Skeleton ─────────────────────────────────────────────────────────────────

function DetailSkeleton() {
  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-center gap-3">
          <div className="h-12 w-12 shrink-0 rounded-full bg-stone-100" />
          <div className="min-w-0 flex-1 space-y-2">
            <div className="h-4 w-1/3 rounded bg-stone-100" />
            <div className="h-3 w-1/2 rounded bg-stone-100" />
          </div>
        </div>
      </div>
      <div className="space-y-4 p-4 sm:p-6 lg:p-8">
        <div className="grid gap-2 sm:grid-cols-3">
          <div className="h-20 rounded-md bg-white ring-1 ring-stone-200" />
          <div className="h-20 rounded-md bg-white ring-1 ring-stone-200" />
          <div className="h-20 rounded-md bg-white ring-1 ring-stone-200" />
        </div>
        <div className="h-48 rounded-md bg-white ring-1 ring-stone-200" />
        <div className="h-48 rounded-md bg-white ring-1 ring-stone-200" />
      </div>
    </div>
  )
}
