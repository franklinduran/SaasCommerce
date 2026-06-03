import { RefreshCw, Search } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
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
    <div className="flex flex-col gap-3 sm:flex-row">
      <label className="relative block min-w-0 flex-1">
        <Search
          aria-hidden="true"
          className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
          size={16}
        />
        <input
          aria-label="Buscar productos"
          className={cn(
            'h-10 w-full min-w-0 rounded-xl border border-border bg-card pl-9 pr-3 text-[13px] font-medium text-foreground shadow-sm outline-none transition',
            'placeholder:text-muted-foreground',
            'focus:border-ring focus:ring-2 focus:ring-ring/20',
          )}
          onChange={(event) => onQueryChange(event.target.value)}
          placeholder="Buscar por nombre o SKU"
          value={query}
        />
      </label>
      <Button disabled={isFetching} onClick={onRefresh} type="button" variant="secondary">
        <RefreshCw
          aria-hidden="true"
          className={cn('shrink-0', isFetching && 'animate-spin')}
          size={15}
        />
        Actualizar
      </Button>
    </div>
  )
}
