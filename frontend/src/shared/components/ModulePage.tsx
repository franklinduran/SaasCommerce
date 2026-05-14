import type { LucideIcon } from 'lucide-react'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

type ModulePageProps = {
  title: string
  eyebrow: string
  icon: LucideIcon
  rows: Array<{ label: string; value: string; status: string }>
}

export function ModulePage({ title, eyebrow, icon: Icon, rows }: ModulePageProps) {
  return (
    <main className="min-h-screen bg-background p-5 lg:p-8">
      <div className="mx-auto flex max-w-6xl flex-col gap-6">
        <header className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm font-semibold text-accent">{eyebrow}</p>
            <h1 className="mt-1 text-2xl font-semibold text-foreground">{title}</h1>
          </div>
          <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-white text-primary shadow-panel">
            <Icon aria-hidden="true" size={24} />
          </div>
        </header>

        <Card>
          <CardHeader>
            <h2 className="text-base font-semibold text-foreground">Bandeja</h2>
          </CardHeader>
          <CardContent>
            <div className="overflow-hidden rounded-md border border-border">
              {rows.map((row) => (
                <div
                  className="grid grid-cols-1 gap-2 border-b border-border bg-white p-4 last:border-b-0 sm:grid-cols-[1fr_160px_120px] sm:items-center"
                  key={row.label}
                >
                  <span className="font-medium text-foreground">{row.label}</span>
                  <span className="text-sm text-slate-600">{row.value}</span>
                  <span className="w-fit rounded-md bg-muted px-2 py-1 text-xs font-semibold text-primary">
                    {row.status}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </main>
  )
}
