import {
  ArrowUpRight,
  BadgeDollarSign,
  Boxes,
  PackageCheck,
  ReceiptText,
  Wifi,
} from 'lucide-react'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

const metrics = [
  { label: 'Ventas hoy', value: 'RD$ 0.00', detail: '0 tickets', tone: 'text-primary' },
  { label: 'Inventario', value: '0', detail: 'productos activos', tone: 'text-accent' },
  { label: 'Facturas', value: '0', detail: 'pendientes', tone: 'text-warning' },
  { label: 'Clientes', value: '0', detail: 'registrados', tone: 'text-danger' },
]

const operations = [
  { label: 'Caja', value: 'Lista', icon: BadgeDollarSign },
  { label: 'Catalogo', value: 'Sincronizado', icon: PackageCheck },
  { label: 'Inventario', value: 'Pendiente', icon: Boxes },
  { label: 'Facturacion', value: 'Base', icon: ReceiptText },
]

export function DashboardPage() {
  return (
    <main className="min-h-screen bg-background p-5 lg:p-8">
      <div className="mx-auto flex max-w-7xl flex-col gap-6">
        <header className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm font-semibold text-accent">SaasCommerce RD</p>
            <h1 className="mt-1 text-2xl font-semibold text-foreground">
              Panel operativo
            </h1>
          </div>
          <div className="flex items-center gap-2 rounded-md border border-border bg-white px-3 py-2 text-sm font-medium text-primary">
            <Wifi aria-hidden="true" size={17} />
            En linea
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {metrics.map((metric) => (
            <Card key={metric.label}>
              <CardHeader>
                <p className="text-sm font-medium text-slate-600">{metric.label}</p>
              </CardHeader>
              <CardContent>
                <div className="flex items-end justify-between gap-3">
                  <div>
                    <p className={`text-2xl font-semibold ${metric.tone}`}>
                      {metric.value}
                    </p>
                    <p className="mt-1 text-sm text-slate-500">{metric.detail}</p>
                  </div>
                  <ArrowUpRight aria-hidden="true" className="text-slate-400" size={20} />
                </div>
              </CardContent>
            </Card>
          ))}
        </section>

        <section className="grid gap-4 xl:grid-cols-[1.2fr_0.8fr]">
          <Card>
            <CardHeader>
              <h2 className="text-base font-semibold text-foreground">Operaciones</h2>
            </CardHeader>
            <CardContent>
              <div className="grid gap-3 sm:grid-cols-2">
                {operations.map((operation) => (
                  <div
                    className="flex items-center justify-between rounded-md border border-border bg-muted/50 p-4"
                    key={operation.label}
                  >
                    <div className="flex items-center gap-3">
                      <operation.icon aria-hidden="true" className="text-primary" size={20} />
                      <span className="font-medium">{operation.label}</span>
                    </div>
                    <span className="text-sm text-slate-600">{operation.value}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <h2 className="text-base font-semibold text-foreground">Cola tecnica</h2>
            </CardHeader>
            <CardContent>
              <div className="space-y-3 text-sm">
                <div className="flex items-center justify-between rounded-md bg-muted px-3 py-2">
                  <span>API</span>
                  <span className="font-semibold text-primary">Base</span>
                </div>
                <div className="flex items-center justify-between rounded-md bg-muted px-3 py-2">
                  <span>Worker</span>
                  <span className="font-semibold text-primary">Base</span>
                </div>
                <div className="flex items-center justify-between rounded-md bg-muted px-3 py-2">
                  <span>SignalR</span>
                  <span className="font-semibold text-accent">Preparado</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </section>
      </div>
    </main>
  )
}
