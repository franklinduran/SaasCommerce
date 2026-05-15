import {
  BadgeDollarSign,
  Boxes,
  CalendarDays,
  CircleDollarSign,
  Package,
  PackageCheck,
  Plus,
  ReceiptText,
  Search,
  SlidersHorizontal,
  Wifi,
} from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'

const metrics = [
  {
    label: 'Ingresos hoy',
    value: 'RD$ 0.00',
    detail: '+0.0% vs ayer',
    tone: 'text-foreground',
    icon: CircleDollarSign,
    surface: 'bg-white',
    iconBox: 'bg-stone-100 text-stone-900',
    detailTone: 'text-stone-600',
  },
  {
    label: 'Ordenes completas',
    value: '0',
    detail: '+0.0% vs ayer',
    tone: 'text-foreground',
    icon: ReceiptText,
    surface: 'bg-white',
    iconBox: 'bg-stone-900 text-white',
    detailTone: 'text-stone-700',
  },
  {
    label: 'Clientes recurrentes',
    value: '0',
    detail: '+0.0% vs ayer',
    tone: 'text-foreground',
    icon: Boxes,
    surface: 'bg-white',
    iconBox: 'bg-sky-50 text-sky-700',
    detailTone: 'text-stone-700',
  },
]

const operations = [
  { label: 'Caja', value: 'Lista', icon: BadgeDollarSign },
  { label: 'Catalogo', value: 'Sincronizado', icon: PackageCheck },
  { label: 'Inventario', value: 'Pendiente', icon: Boxes },
  { label: 'Facturacion', value: 'Base', icon: ReceiptText },
]

const topSales = [
  { label: 'Telefonos', value: '300 unidades', color: 'bg-stone-950' },
  { label: 'Computadoras', value: '190 unidades', color: 'bg-sky-500' },
  { label: 'Televisores', value: '237 unidades', color: 'bg-stone-500' },
]

const products = [
  {
    name: 'Playstation 4 Limited Edition',
    price: 'RD$ 14.81',
    stock: '883',
    revenue: 'RD$ 349.00',
    status: 'En stock',
  },
  {
    name: 'Silla gamer, retiro local',
    price: 'RD$ 5.22',
    stock: '453',
    revenue: 'RD$ 354.00',
    status: 'En stock',
  },
]

export function DashboardPage() {
  return (
    <section className="min-h-full bg-surface-subtle p-4 lg:p-6">
      <div className="flex w-full flex-col gap-5">
        <div className="flex flex-col gap-3 pb-2 lg:flex-row lg:items-center lg:justify-between">
          <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">Inicio / Resumen</p>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
            <div className="flex h-9 min-w-[260px] items-center gap-2 rounded-md bg-white px-3 text-sm text-stone-600 shadow-sm ring-1 ring-stone-200">
              <Search aria-hidden="true" size={15} />
              <span>Buscar...</span>
            </div>
            <Button size="sm" variant="outline">
              <CalendarDays size={15} />
              Periodo actual
            </Button>
            <Button size="sm">Exportar</Button>
          </div>
        </div>

        <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-normal text-foreground">
              Tu tienda de un vistazo
            </h1>
            <p className="mt-1 text-sm text-stone-600">
              Resumen operativo de ingresos, ordenes y clientes.
            </p>
          </div>
          <div className="flex items-center gap-2 rounded-md bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-700 ring-1 ring-emerald-200">
            <Wifi aria-hidden="true" size={17} />
            En linea
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          {metrics.map((metric) => (
            <Card className={`${metric.surface} shadow-none`} key={metric.label}>
              <CardHeader>
                <div className="flex items-center justify-between gap-3">
                  <p className="text-sm font-semibold">{metric.label}</p>
                  <span className={`flex h-8 w-8 items-center justify-center rounded-md ${metric.iconBox}`}>
                    <metric.icon aria-hidden="true" size={16} />
                  </span>
                </div>
              </CardHeader>
              <CardContent>
                <p className={`text-3xl font-semibold ${metric.tone}`}>{metric.value}</p>
                <p className={`mt-2 text-sm font-semibold ${metric.detailTone}`}>{metric.detail}</p>
              </CardContent>
            </Card>
          ))}
        </section>

        <section className="grid gap-4 xl:grid-cols-[1.55fr_0.75fr]">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <h2 className="text-base font-semibold text-foreground">Analisis de ordenes</h2>
                <div className="mt-2 flex gap-4 text-xs font-medium text-steel">
                  <span className="flex items-center gap-1.5">
                    <span className="h-2 w-2 rounded-full bg-stone-900" />
                    Ingresos
                  </span>
                  <span className="flex items-center gap-1.5">
                    <span className="h-2 w-2 rounded-full bg-primary" />
                    Gastos
                  </span>
                </div>
              </div>
              <Button size="sm" variant="outline">
                <CalendarDays size={15} />
                Periodo actual
              </Button>
            </CardHeader>
            <CardContent>
              <div className="relative min-h-[220px] overflow-hidden rounded-md bg-muted p-3">
                <svg
                  className="h-full min-h-[220px] w-full"
                  preserveAspectRatio="none"
                  role="img"
                  viewBox="0 0 700 240"
                >
                  <title>Ingresos y gastos por mes</title>
                  <g stroke="#e7e5e4" strokeWidth="1">
                    {[44, 88, 132, 176].map((y) => (
                      <line key={y} x1="28" x2="672" y1={y} y2={y} />
                    ))}
                  </g>
                  <path
                    d="M32 150 C90 154 110 190 158 183 C204 176 216 88 270 95 C322 103 342 185 394 177 C450 167 472 92 524 110 C577 128 594 178 668 144"
                    fill="none"
                    stroke="#1c1917"
                    strokeLinecap="round"
                    strokeWidth="3"
                  />
                  <path
                    d="M32 134 C88 142 120 184 166 178 C223 169 217 70 272 68 C334 66 334 169 397 168 C457 166 462 114 517 98 C578 81 606 162 668 112"
                    fill="none"
                    stroke="#78716c"
                    strokeLinecap="round"
                    strokeWidth="3"
                  />
                  <g fill="#57534e" fontSize="12">
                    {['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago'].map((month, index) => (
                      <text key={month} textAnchor="middle" x={48 + index * 86} y="224">
                        {month}
                      </text>
                    ))}
                  </g>
                </svg>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <h2 className="text-base font-semibold text-foreground">Productos mas vendidos</h2>
              <Button size="sm" variant="outline">
                Ver detalles
              </Button>
            </CardHeader>
            <CardContent>
              <div className="mb-5">
                <p className="text-3xl font-semibold text-foreground">10,432</p>
                <p className="text-sm font-medium text-steel">+512 esta semana</p>
              </div>
              <div className="mb-5 grid grid-cols-[repeat(24,minmax(0,1fr))] gap-1">
                {Array.from({ length: 24 }).map((_, index) => (
                  <span
                    className={`h-16 rounded-sm ${index % 3 === 0 ? 'bg-stone-950' : index % 3 === 1 ? 'bg-stone-700' : 'bg-stone-400'}`}
                    key={index}
                  />
                ))}
              </div>
              <div className="space-y-3">
                {topSales.map((sale) => (
                  <div className="flex items-center justify-between text-sm" key={sale.label}>
                    <span className="flex items-center gap-2 font-medium text-foreground">
                      <span className={`h-2.5 w-2.5 rounded-sm ${sale.color}`} />
                      {sale.label}
                    </span>
                    <span className="text-steel">{sale.value}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </section>

        <section className="grid gap-4 xl:grid-cols-[1fr_340px]">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <h2 className="text-base font-semibold text-foreground">Productos</h2>
              <div className="flex items-center gap-2">
                <div className="hidden h-9 items-center gap-2 rounded-md bg-muted px-3 text-sm font-medium text-stone-700 sm:flex">
                  <Search aria-hidden="true" size={15} />
                  Buscar producto...
                </div>
                <Button size="sm" variant="outline">
                  <SlidersHorizontal size={15} />
                  Estado
                </Button>
                <Button size="sm">
                  <Plus size={15} />
                  Agregar
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto rounded-md bg-white shadow-control">
                <div className="min-w-[760px]">
                  <div className="grid grid-cols-[1.4fr_110px_100px_120px_110px] bg-muted px-4 py-3 text-xs font-semibold text-steel">
                    <span>Producto</span>
                    <span>Precio</span>
                    <span>Stock</span>
                    <span>Ingresos</span>
                    <span>Estado</span>
                  </div>
                  {products.map((product) => (
                    <div
                      className="grid grid-cols-[1.4fr_110px_100px_120px_110px] items-center bg-white px-4 py-3 text-sm shadow-[0_-1px_0_rgb(231_229_228_/_0.85)]"
                      key={product.name}
                    >
                      <span className="flex items-center gap-3 font-medium text-foreground">
                        <span className="flex h-9 w-9 items-center justify-center rounded-md bg-muted text-accent">
                          <Package aria-hidden="true" size={17} />
                        </span>
                        {product.name}
                      </span>
                      <span className="font-medium text-stone-700">{product.price}</span>
                      <span className="font-medium text-stone-700">{product.stock}</span>
                      <span className="font-medium text-stone-700">{product.revenue}</span>
                      <span className="w-fit rounded-md bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200">
                        {product.status}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <h2 className="text-base font-semibold text-foreground">Operaciones</h2>
            </CardHeader>
            <CardContent>
              <div className="grid gap-3">
                {operations.map((operation) => (
                  <div
                    className="flex items-center justify-between rounded-md bg-muted p-3"
                    key={operation.label}
                  >
                    <div className="flex items-center gap-3">
                      <span className="flex h-9 w-9 items-center justify-center rounded-md bg-white text-primary">
                        <operation.icon aria-hidden="true" size={18} />
                      </span>
                      <span className="font-medium">{operation.label}</span>
                    </div>
                    <span className="text-sm font-semibold text-foreground">{operation.value}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </section>
      </div>
    </section>
  )
}
