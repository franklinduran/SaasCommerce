export function BranchStatusBadge({ isActive, isMain }: Readonly<{ isActive: boolean; isMain: boolean }>) {
  if (isMain) {
    return (
      <span className="rounded-full bg-blue-50 px-2.5 py-1 text-xs font-semibold text-blue-700 ring-1 ring-blue-200">
        Principal
      </span>
    )
  }

  if (isActive) {
    return (
      <span className="rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200">
        Activa
      </span>
    )
  }

  return (
    <span className="rounded-full bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-500 ring-1 ring-stone-200">
      Inactiva
    </span>
  )
}
