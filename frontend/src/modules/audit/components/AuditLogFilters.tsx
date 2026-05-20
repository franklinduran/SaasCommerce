import type { AuditLogFilters } from '../auditLogTypes'

const ACTION_OPTIONS = [
  { value: '', label: 'Todas las acciones' },
  { value: 'auth.login_succeeded', label: 'Login exitoso' },
  { value: 'auth.login_failed', label: 'Login fallido' },
  { value: 'sale.created', label: 'Venta creada' },
  { value: 'sale.completed', label: 'Venta completada' },
  { value: 'sale.cancelled', label: 'Venta cancelada' },
  { value: 'sale.failed', label: 'Venta fallida' },
  { value: 'payment.registered', label: 'Pago registrado' },
  { value: 'invoice.generated', label: 'Factura generada' },
  { value: 'inventory.adjusted', label: 'Inventario ajustado' },
  { value: 'purchase.received', label: 'Compra recibida' },
  { value: 'purchase.cancelled', label: 'Compra cancelada' },
  { value: 'user.created', label: 'Usuario creado' },
  { value: 'user.updated', label: 'Usuario actualizado' },
  { value: 'user.activated', label: 'Usuario activado' },
  { value: 'user.deactivated', label: 'Usuario desactivado' },
  { value: 'user.role_changed', label: 'Rol cambiado' },
  { value: 'user.password_reset', label: 'Contraseña reseteada' },
]

const ENTITY_OPTIONS = [
  { value: '', label: 'Todas las entidades' },
  { value: 'Auth', label: 'Autenticación' },
  { value: 'User', label: 'Usuarios' },
  { value: 'Sale', label: 'Ventas' },
  { value: 'Payment', label: 'Pagos' },
  { value: 'Invoice', label: 'Facturas' },
  { value: 'Inventory', label: 'Inventario' },
  { value: 'Purchase', label: 'Compras' },
  { value: 'Customer', label: 'Clientes' },
  { value: 'Supplier', label: 'Proveedores' },
]

const inputCn =
  'h-10 w-full rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'

interface AuditLogFiltersProps {
  filters: AuditLogFilters
  onChange: (filters: AuditLogFilters) => void
}

export function AuditLogFiltersBar({ filters, onChange }: Readonly<AuditLogFiltersProps>) {
  function set(partial: Partial<AuditLogFilters>) {
    onChange({ ...filters, ...partial, page: 1 })
  }

  return (
    <div className="grid gap-3 lg:grid-cols-[1fr_1fr_1fr_1fr_1fr]">
      <input
        aria-label="Fecha desde"
        className={inputCn}
        onChange={(e) => set({ dateFrom: e.target.value })}
        type="date"
        value={filters.dateFrom}
      />
      <input
        aria-label="Fecha hasta"
        className={inputCn}
        onChange={(e) => set({ dateTo: e.target.value })}
        type="date"
        value={filters.dateTo}
      />
      <select
        aria-label="Acción"
        className={inputCn}
        onChange={(e) => set({ action: e.target.value })}
        value={filters.action}
      >
        {ACTION_OPTIONS.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </select>
      <select
        aria-label="Entidad"
        className={inputCn}
        onChange={(e) => set({ entityName: e.target.value })}
        value={filters.entityName}
      >
        {ENTITY_OPTIONS.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </select>
      <button
        className="h-10 rounded-md bg-stone-200 px-4 text-sm font-medium text-stone-700 hover:bg-stone-300"
        onClick={() => onChange({ dateFrom: '', dateTo: '', userId: '', action: '', entityName: '', page: 1, pageSize: filters.pageSize })}
        type="button"
      >
        Limpiar filtros
      </button>
    </div>
  )
}
