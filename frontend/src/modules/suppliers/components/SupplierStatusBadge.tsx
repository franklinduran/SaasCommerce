export function SupplierStatusBadge({ isActive }: Readonly<{ isActive: boolean }>) {
  const className = isActive
    ? 'rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200'
    : 'rounded-full bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-700 ring-1 ring-stone-200'

  return <span className={className}>{isActive ? 'Activo' : 'Inactivo'}</span>
}
