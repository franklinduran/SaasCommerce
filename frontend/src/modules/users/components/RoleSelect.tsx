import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select'

const ROLES = [
  { value: 'Admin', label: 'Administrador' },
  { value: 'Supervisor', label: 'Supervisor' },
  { value: 'Cashier', label: 'Cajero' },
  { value: 'InventoryManager', label: 'Gerente de Inventario' },
  { value: 'PurchasingManager', label: 'Gerente de Compras' },
  { value: 'ReadOnly', label: 'Solo Lectura' },
]

interface RoleSelectProps {
  value?: string
  onValueChange?: (value: string) => void
  disabled?: boolean
}

export function RoleSelect({ value, onValueChange, disabled }: Readonly<RoleSelectProps>) {
  return (
    <Select value={value} onValueChange={onValueChange} disabled={disabled}>
      <SelectTrigger>
        <SelectValue placeholder="Selecciona un rol" />
      </SelectTrigger>
      <SelectContent>
        {ROLES.map((role) => (
          <SelectItem key={role.value} value={role.value}>
            {role.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
