import { RefreshCw, Search } from 'lucide-react'
import { cn } from '@/shared/utils/cn'

type ProductSearchProps = {
  isFetching: boolean
  onQueryChange: (query: string) => void
  onRefresh: () => void
  query: string
}

export function ProductSearch({
  isFetching,
  onQueryChange,
  onRefresh,
  query,
}: Readonly<ProductSearchProps>) {
  return (
    <div className="flex gap-2">
      <label className="relative block min-w-0 flex-1">
        <Search
          aria-hidden="true"
          className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400"
          size={15}
        />
        <input
          aria-label="Buscar productos"
          className={cn(
            'h-10 w-full min-w-0 rounded-lg border border-gray-200 bg-white pl-9 pr-3.5',
            'text-sm font-medium text-gray-900 outline-none transition',
            'placeholder:font-normal placeholder:text-gray-400',
            'hover:border-gray-300',
            'focus:border-primary focus:ring-2 focus:ring-primary/15',
          )}
          onChange={(e) => onQueryChange(e.target.value)}
          placeholder="Buscar por nombre o SKU…"
          value={query}
        />
      </label>

      <button
        aria-label="Actualizar productos"
        className={cn(
          'flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border border-gray-200 bg-white text-gray-500 transition',
          'hover:border-gray-300 hover:text-gray-900',
          'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/20',
          'disabled:cursor-not-allowed disabled:opacity-50',
        )}
        disabled={isFetching}
        onClick={onRefresh}
        type="button"
      >
        <RefreshCw
          aria-hidden="true"
          className={cn('shrink-0', isFetching && 'animate-spin')}
          size={15}
        />
      </button>
    </div>
  )
}
