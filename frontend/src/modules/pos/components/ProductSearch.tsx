import { RefreshCw, Search } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'

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
          className="absolute left-3 top-1/2 -translate-y-1/2 text-stone-400"
          size={18}
        />
        <input
          aria-label="Buscar productos"
          className="h-11 w-full min-w-0 rounded-md bg-white pl-10 pr-3 text-sm font-medium text-stone-900 shadow-[0_0_0_1px_rgb(214_211_209)] outline-none transition placeholder:text-stone-400 focus:shadow-[0_0_0_1px_rgb(28_25_23)] focus:ring-2 focus:ring-stone-900/15"
          onChange={(event) => onQueryChange(event.target.value)}
          placeholder="Buscar por nombre o SKU"
          value={query}
        />
      </label>
      <Button disabled={isFetching} onClick={onRefresh} type="button" variant="secondary">
        <RefreshCw aria-hidden="true" size={16} />
        Actualizar
      </Button>
    </div>
  )
}
