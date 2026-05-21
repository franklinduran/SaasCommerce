import type { LucideIcon } from 'lucide-react'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

type ModulePageProps = {
  title: string
  eyebrow: string
  icon: LucideIcon
  rows: ReadonlyArray<{ label: string; value: string; status: string }>
}

export function ModulePage({ title, eyebrow, icon: Icon, rows }: Readonly<ModulePageProps>) {
  return (
    <section className="min-h-full bg-surface-subtle p-4 lg:p-6">
      <div className="flex w-full flex-col gap-6">
        <header className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm font-semibold text-stone-600">{eyebrow}</p>
            <h1 className="mt-1 text-2xl font-semibold text-foreground">{title}</h1>
          </div>
          <div className="flex h-12 w-12 items-center justify-center rounded-md bg-stone-900 text-white shadow-sm">
            <Icon aria-hidden="true" size={24} />
          </div>
        </header>

        <Card>
          <CardHeader>
            <h2 className="text-base font-semibold text-foreground">Bandeja</h2>
          </CardHeader>
          <CardContent>
            <div className="overflow-hidden rounded-md bg-white shadow-control">
              {rows.map((row) => (
                <div
                  className="grid grid-cols-1 gap-2 bg-surface p-4 shadow-[0_-1px_0_rgb(231_229_228_/_0.85)] first:shadow-none sm:grid-cols-[1fr_160px_120px] sm:items-center"
                  key={row.label}
                >
                  <span className="font-medium text-foreground">{row.label}</span>
                  <span className="text-sm text-stone-600">{row.value}</span>
                  <span className="w-fit rounded-md bg-stone-100 px-2 py-1 text-xs font-semibold text-stone-700 ring-1 ring-stone-200">
                    {row.status}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  )
}
