import {
  AlertTriangle,
  Building2,
  CheckCircle2,
  Mail,
  MapPin,
  Pencil,
  Phone,
  ShieldCheck,
  ShieldOff,
  X,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { SupplierFormDialog } from '@/modules/suppliers/components/SupplierFormDialog'
import { useUpdateSupplier } from '@/modules/suppliers/hooks/useSuppliers'
import type { Supplier } from '@/modules/suppliers/types'
import { Button } from '@/shared/components/ui/button'
import { cn } from '@/shared/utils/cn'

type SupplierDetailPanelProps = {
  supplier: Supplier | null
  onClose: () => void
}

function initials(name: string) {
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('') || '?'
}

function formatDate(value: string | null | undefined) {
  if (!value) return 'Sin registro'
  try {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(new Date(value))
  } catch {
    return value
  }
}

export function SupplierDetailPanel({ supplier, onClose }: Readonly<SupplierDetailPanelProps>) {
  const updateSupplier = useUpdateSupplier()
  const [editOpen, setEditOpen] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)

  useEffect(() => {
    if (!notice) return
    const t = window.setTimeout(() => setNotice(null), 3500)
    return () => window.clearTimeout(t)
  }, [notice])

  if (!supplier) {
    return null
  }

  function handleToggleStatus() {
    if (!supplier) return
    updateSupplier.mutate(
      {
        request: {
          address: supplier.address,
          email: supplier.email,
          isActive: !supplier.isActive,
          name: supplier.name,
          phone: supplier.phone,
          rnc: supplier.rnc,
        },
        supplierId: supplier.id,
      },
      {
        onSuccess: () =>
          setNotice(supplier.isActive ? 'Proveedor desactivado.' : 'Proveedor reactivado.'),
      },
    )
  }

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="sticky top-0 z-10 border-b border-stone-200 bg-white px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-center gap-3">
            <div className="relative shrink-0">
              <div className="flex h-12 w-12 items-center justify-center rounded-md bg-stone-900 text-base font-semibold text-white shadow-sm">
                {initials(supplier.name)}
              </div>
              <span
                aria-hidden="true"
                className={cn(
                  'absolute -bottom-0.5 -right-0.5 h-3.5 w-3.5 rounded-full ring-2 ring-white',
                  supplier.isActive ? 'bg-emerald-500' : 'bg-stone-300',
                )}
              />
            </div>
            <div className="min-w-0">
              <h3 className="truncate text-lg font-semibold text-stone-950">{supplier.name}</h3>
              <div className="mt-1 flex flex-wrap items-center gap-2 text-xs font-medium text-stone-500">
                {supplier.phone && (
                  <span className="inline-flex items-center gap-1">
                    <Phone size={12} />
                    {supplier.phone}
                  </span>
                )}
                {supplier.email && (
                  <span className="inline-flex items-center gap-1">
                    <Mail size={12} />
                    {supplier.email}
                  </span>
                )}
              </div>
              <div className="mt-2 flex flex-wrap items-center gap-1.5">
                {supplier.rnc && (
                  <span className="rounded-full bg-stone-100 px-2 py-0.5 font-mono text-[11px] font-semibold text-stone-700 ring-1 ring-stone-200">
                    RNC {supplier.rnc}
                  </span>
                )}
                <span
                  className={cn(
                    'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[11px] font-semibold ring-1',
                    supplier.isActive
                      ? 'bg-emerald-50 text-emerald-700 ring-emerald-200'
                      : 'bg-stone-100 text-stone-600 ring-stone-200',
                  )}
                >
                  <span
                    aria-hidden="true"
                    className={cn(
                      'h-1.5 w-1.5 rounded-full',
                      supplier.isActive ? 'bg-emerald-500' : 'bg-stone-400',
                    )}
                  />
                  {supplier.isActive ? 'Activo' : 'Inactivo'}
                </span>
              </div>
            </div>
          </div>
          <div className="flex shrink-0 items-start gap-1">
            <Button onClick={() => setEditOpen(true)} size="sm" type="button">
              <Pencil size={14} />
              Editar
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

      <div className="min-h-0 flex-1 space-y-4 p-4 sm:p-6 lg:p-8">
        <Section description="Datos de contacto e identificacion fiscal." title="Informacion del proveedor">
          <dl className="grid gap-4 sm:grid-cols-2">
            <Detail icon={<Building2 size={14} />} label="Razon social" value={supplier.name} />
            <Detail icon={<Building2 size={14} />} label="RNC / Identificacion" value={supplier.rnc ?? '—'} mono />
            <Detail icon={<Phone size={14} />} label="Telefono" value={supplier.phone ?? '—'} />
            <Detail icon={<Mail size={14} />} label="Correo" value={supplier.email ?? '—'} />
            <Detail
              className="sm:col-span-2"
              icon={<MapPin size={14} />}
              label="Direccion"
              value={supplier.address ?? '—'}
            />
          </dl>
        </Section>

        <Section
          description="Acciones que afectan la disponibilidad del proveedor."
          icon={<AlertTriangle className="text-amber-600" size={16} />}
          title="Acciones"
        >
          <div className="grid gap-2 sm:grid-cols-2">
            <ActionRow
              description={supplier.isActive
                ? 'El proveedor no aparecera en compras nuevas.'
                : 'Restaura el proveedor para usarlo en compras.'}
              icon={supplier.isActive ? <ShieldOff size={16} /> : <ShieldCheck size={16} />}
              label={supplier.isActive ? 'Desactivar proveedor' : 'Reactivar proveedor'}
              tone={supplier.isActive ? 'danger' : 'default'}
            >
              <Button
                disabled={updateSupplier.isPending}
                onClick={handleToggleStatus}
                size="sm"
                type="button"
                variant={supplier.isActive ? 'destructive' : 'secondary'}
              >
                {supplier.isActive ? 'Desactivar' : 'Reactivar'}
              </Button>
            </ActionRow>
          </div>
        </Section>

        <Section description="Datos de auditoria." title="Detalles">
          <dl className="grid gap-3 sm:grid-cols-2">
            <div>
              <dt className="text-xs font-semibold uppercase text-stone-500">Creado</dt>
              <dd className="mt-1 text-sm font-medium text-stone-900">{formatDate(supplier.createdAt)}</dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-stone-500">Ultima actualizacion</dt>
              <dd className="mt-1 text-sm font-medium text-stone-900">{formatDate(supplier.updatedAt)}</dd>
            </div>
          </dl>
        </Section>
      </div>

      <SupplierFormDialog
        onOpenChange={setEditOpen}
        onSaved={() => {
          setEditOpen(false)
          setNotice('Cambios guardados.')
        }}
        open={editOpen}
        supplier={supplier}
      />
    </div>
  )
}

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

function Detail({
  className,
  icon,
  label,
  mono,
  value,
}: Readonly<{
  className?: string
  icon: React.ReactNode
  label: string
  mono?: boolean
  value: string
}>) {
  return (
    <div className={className}>
      <dt className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
        {icon}
        {label}
      </dt>
      <dd className={cn('mt-1 text-sm font-medium text-stone-900', mono && 'font-mono')}>{value}</dd>
    </div>
  )
}

function ActionRow({
  children,
  description,
  icon,
  label,
  tone = 'default',
}: Readonly<{
  children: React.ReactNode
  description: string
  icon: React.ReactNode
  label: string
  tone?: 'default' | 'danger'
}>) {
  return (
    <div
      className={cn(
        'flex flex-col gap-3 rounded-md border p-3',
        tone === 'danger' ? 'border-red-200 bg-red-50/40' : 'border-stone-200 bg-stone-50/60',
      )}
    >
      <div className="flex items-start gap-2.5">
        <span
          className={cn(
            'mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-md ring-1',
            tone === 'danger'
              ? 'bg-red-100 text-red-700 ring-red-200'
              : 'bg-white text-stone-700 ring-stone-200',
          )}
        >
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
