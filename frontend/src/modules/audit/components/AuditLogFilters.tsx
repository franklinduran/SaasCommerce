import { RotateCcw } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import type { AuditLogFilters } from '../auditLogTypes'

const ACTION_OPTIONS = [
  { value: '_',                      label: 'Todas las acciones' },
  { value: 'auth.login_succeeded',   label: 'Login exitoso' },
  { value: 'auth.login_failed',      label: 'Login fallido' },
  { value: 'sale.created',           label: 'Venta creada' },
  { value: 'sale.completed',         label: 'Venta completada' },
  { value: 'sale.cancelled',         label: 'Venta cancelada' },
  { value: 'sale.failed',            label: 'Venta fallida' },
  { value: 'payment.registered',     label: 'Pago registrado' },
  { value: 'invoice.generated',      label: 'Factura generada' },
  { value: 'inventory.adjusted',     label: 'Inventario ajustado' },
  { value: 'purchase.received',      label: 'Compra recibida' },
  { value: 'purchase.cancelled',     label: 'Compra cancelada' },
  { value: 'user.created',           label: 'Usuario creado' },
  { value: 'user.updated',           label: 'Usuario actualizado' },
  { value: 'user.activated',         label: 'Usuario activado' },
  { value: 'user.deactivated',       label: 'Usuario desactivado' },
  { value: 'user.role_changed',      label: 'Rol cambiado' },
  { value: 'user.password_reset',    label: 'Contraseña reseteada' },
]

const ENTITY_OPTIONS = [
  { value: '_',         label: 'Todas las entidades' },
  { value: 'Auth',      label: 'Autenticación' },
  { value: 'User',      label: 'Usuarios' },
  { value: 'Sale',      label: 'Ventas' },
  { value: 'Payment',   label: 'Pagos' },
  { value: 'Invoice',   label: 'Facturas' },
  { value: 'Inventory', label: 'Inventario' },
  { value: 'Purchase',  label: 'Compras' },
  { value: 'Customer',  label: 'Clientes' },
  { value: 'Supplier',  label: 'Proveedores' },
]

interface AuditLogFiltersProps {
  filters: AuditLogFilters
  onChange: (filters: AuditLogFilters) => void
}

export function AuditLogFiltersBar({ filters, onChange }: Readonly<AuditLogFiltersProps>) {
  function set(partial: Partial<AuditLogFilters>) {
    onChange({ ...filters, ...partial, page: 1 })
  }

  return (
    <div className="grid gap-3 lg:grid-cols-[1fr_1fr_1fr_1fr_auto]">
      <div className="space-y-1.5">
        <Label className="text-xs uppercase text-stone-500">Desde</Label>
        <Input
          onChange={(e) => set({ dateFrom: e.target.value })}
          type="date"
          value={filters.dateFrom}
        />
      </div>

      <div className="space-y-1.5">
        <Label className="text-xs uppercase text-stone-500">Hasta</Label>
        <Input
          onChange={(e) => set({ dateTo: e.target.value })}
          type="date"
          value={filters.dateTo}
        />
      </div>

      <div className="space-y-1.5">
        <Label className="text-xs uppercase text-stone-500">Accion</Label>
        <Select
          value={filters.action || '_'}
          onValueChange={(v) => set({ action: v === '_' ? '' : v })}
        >
          <SelectTrigger><SelectValue /></SelectTrigger>
          <SelectContent>
            {ACTION_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-1.5">
        <Label className="text-xs uppercase text-stone-500">Entidad</Label>
        <Select
          value={filters.entityName || '_'}
          onValueChange={(v) => set({ entityName: v === '_' ? '' : v })}
        >
          <SelectTrigger><SelectValue /></SelectTrigger>
          <SelectContent>
            {ENTITY_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="flex items-end">
        <Button
          onClick={() =>
            onChange({
              action: '',
              dateFrom: '',
              dateTo: '',
              entityName: '',
              page: 1,
              pageSize: filters.pageSize,
              userId: '',
            })
          }
          type="button"
          variant="ghost"
        >
          <RotateCcw size={15} />
          Limpiar
        </Button>
      </div>
    </div>
  )
}
