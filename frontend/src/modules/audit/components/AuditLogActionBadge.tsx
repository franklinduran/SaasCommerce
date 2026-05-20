const ACTION_LABELS: Record<string, { label: string; className: string }> = {
  'auth.login_succeeded': { label: 'Login OK', className: 'bg-emerald-50 text-emerald-700 border-emerald-200' },
  'auth.login_failed': { label: 'Login fallido', className: 'bg-red-50 text-red-700 border-red-200' },
  'sale.created': { label: 'Venta creada', className: 'bg-blue-50 text-blue-700 border-blue-200' },
  'sale.completed': { label: 'Venta completada', className: 'bg-emerald-50 text-emerald-700 border-emerald-200' },
  'sale.cancelled': { label: 'Venta cancelada', className: 'bg-red-50 text-red-700 border-red-200' },
  'sale.failed': { label: 'Venta fallida', className: 'bg-red-50 text-red-700 border-red-200' },
  'payment.registered': { label: 'Pago registrado', className: 'bg-violet-50 text-violet-700 border-violet-200' },
  'invoice.generated': { label: 'Factura generada', className: 'bg-indigo-50 text-indigo-700 border-indigo-200' },
  'inventory.adjusted': { label: 'Inv. ajustado', className: 'bg-amber-50 text-amber-700 border-amber-200' },
  'purchase.received': { label: 'Compra recibida', className: 'bg-cyan-50 text-cyan-700 border-cyan-200' },
  'purchase.cancelled': { label: 'Compra cancelada', className: 'bg-red-50 text-red-700 border-red-200' },
  'user.created': { label: 'Usuario creado', className: 'bg-stone-100 text-stone-700 border-stone-200' },
  'user.updated': { label: 'Usuario actualizado', className: 'bg-stone-100 text-stone-700 border-stone-200' },
  'user.activated': { label: 'Usuario activado', className: 'bg-emerald-50 text-emerald-700 border-emerald-200' },
  'user.deactivated': { label: 'Usuario desactivado', className: 'bg-red-50 text-red-700 border-red-200' },
  'user.role_changed': { label: 'Rol cambiado', className: 'bg-orange-50 text-orange-700 border-orange-200' },
  'user.password_reset': { label: 'Contraseña reseteada', className: 'bg-yellow-50 text-yellow-700 border-yellow-200' },
}

interface AuditLogActionBadgeProps {
  action: string
}

export function AuditLogActionBadge({ action }: Readonly<AuditLogActionBadgeProps>) {
  const config = ACTION_LABELS[action]
  const label = config?.label ?? action
  const className = config?.className ?? 'bg-stone-100 text-stone-600 border-stone-200'

  return (
    <span className={`inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium ${className}`}>
      {label}
    </span>
  )
}
