import {
  AlertTriangle,
  BarChart3,
  Download,
  Loader2,
  RotateCcw,
  Search,
} from 'lucide-react'
import { useState, type ReactNode } from 'react'
import { PermissionGate } from '@/shared/components/PermissionGate'
import { Permission } from '@/shared/types/permissions'
import {
  useAccountsReceivableReport,
  useInvoiceReport,
  useLowStockReport,
  usePurchaseReport,
  useSalesReport,
} from '@/modules/reports/hooks/useReports'
import {
  getArExportUrl,
  getInvoiceExportUrl,
  getLowStockExportUrl,
  getPurchaseExportUrl,
  getSalesExportUrl,
} from '@/modules/reports/services/reportsApi'
import type {
  AccountsReceivableFilters,
  InvoiceReportFilters,
  LowStockFilters,
  PurchaseReportFilters,
  SalesReportFilters,
} from '@/modules/reports/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs'

type Tab = 'sales' | 'invoices' | 'receivable' | 'lowstock' | 'purchases'
type TableColumn = { label: string; align?: 'left' | 'center' | 'right' }
type Option = { label: string; value: string }

const tabs: { id: Tab; label: string }[] = [
  { id: 'sales', label: 'Ventas' },
  { id: 'invoices', label: 'Facturas' },
  { id: 'receivable', label: 'Cuentas por cobrar' },
  { id: 'lowstock', label: 'Bajo stock' },
  { id: 'purchases', label: 'Compras' },
]

const defaultDateFilters = { dateFrom: '', dateTo: '', page: 1, pageSize: 25 }

const statusOptions = {
  accountsReceivable: [
    { label: 'Todos', value: '' },
    { label: 'Activos', value: 'Active' },
    { label: 'Bloqueados', value: 'Blocked' },
  ],
  invoices: [
    { label: 'Todos', value: '' },
    { label: 'Emitidas', value: 'Issued' },
    { label: 'Canceladas', value: 'Cancelled' },
  ],
  purchases: [
    { label: 'Todos', value: '' },
    { label: 'Pendientes', value: 'Pending' },
    { label: 'Recibidas', value: 'Received' },
    { label: 'Canceladas', value: 'Cancelled' },
  ],
  sales: [
    { label: 'Todos', value: '' },
    { label: 'Completadas', value: 'Completed' },
    { label: 'Canceladas', value: 'Cancelled' },
    { label: 'Pendientes', value: 'Pending' },
  ],
} satisfies Record<string, Option[]>

export function ReportsPage() {
  const [activeTab, setActiveTab] = useState<Tab>('sales')

  return (
    <section className="space-y-5 p-4 sm:p-6 lg:p-8">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
        <div>
          <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-stone-500">
            <BarChart3 size={13} />
            Analitica
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Reportes</h2>
          <p className="mt-1.5 max-w-2xl text-sm font-medium text-stone-600">
            Consulta, filtra y exporta informacion operativa sin perder legibilidad.
          </p>
        </div>
      </div>

      <Tabs onValueChange={(value) => setActiveTab(value as Tab)} value={activeTab}>
        <TabsList className="w-full">
          {tabs.map((tab) => (
            <TabsTrigger className="min-w-max" key={tab.id} value={tab.id}>
              {tab.label}
            </TabsTrigger>
          ))}
        </TabsList>

        <div className="mt-5">
          <TabsContent value="sales">
            <SalesReportPanel />
          </TabsContent>
          <TabsContent value="invoices">
            <InvoiceReportPanel />
          </TabsContent>
          <TabsContent value="receivable">
            <AccountsReceivablePanel />
          </TabsContent>
          <TabsContent value="lowstock">
            <LowStockPanel />
          </TabsContent>
          <TabsContent value="purchases">
            <PurchasesPanel />
          </TabsContent>
        </div>
      </Tabs>
    </section>
  )
}

function SalesReportPanel() {
  const initialFilters: SalesReportFilters = {
    ...defaultDateFilters,
    branchId: '',
    paymentMethod: '',
    search: '',
    status: '',
  }
  const [filters, setFilters] = useState<SalesReportFilters>(initialFilters)
  const report = useSalesReport(filters)
  const hasFilters = Boolean(filters.search || filters.status || filters.dateFrom || filters.dateTo)

  function updateFilters(values: Partial<SalesReportFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <ReportCard
      canReset={hasFilters}
      description="Filtra por cliente, numero de venta, estado y rango de fechas."
      exportHref={getSalesExportUrl(filters)}
      exportLabel="Exportar ventas"
      onReset={() => setFilters(initialFilters)}
      title="Ventas"
      filters={
        <>
          <SearchField
            label="Busqueda"
            onChange={(search) => updateFilters({ search })}
            placeholder="Cliente, codigo o venta"
            value={filters.search}
          />
          <DateField label="Desde" onChange={(dateFrom) => updateFilters({ dateFrom })} value={filters.dateFrom} />
          <DateField label="Hasta" onChange={(dateTo) => updateFilters({ dateTo })} value={filters.dateTo} />
          <SelectField
            label="Estado"
            onChange={(status) => updateFilters({ status })}
            options={statusOptions.sales}
            value={filters.status}
          />
        </>
      }
    >
      {report.data && report.data.summary.totalCount > 0 && (
        <SummaryGrid>
          <SummaryStat label="Ventas" value={String(report.data.summary.totalCount)} />
          <SummaryStat label="Monto total" value={formatMoney(report.data.summary.totalAmount)} />
          <SummaryStat label="Promedio" value={formatMoney(report.data.summary.averageAmount)} />
        </SummaryGrid>
      )}
      <ReportBody
        emptyMessage="Sin ventas para los filtros seleccionados."
        isEmpty={!report.data?.items.length}
        isError={report.isError}
        isLoading={report.isLoading}
      >
        <Table
          columns={[
            { label: 'Venta' },
            { label: 'Sucursal' },
            { label: 'Cliente' },
            { label: 'Metodo' },
            { label: 'Estado' },
            { align: 'right', label: 'Total' },
            { label: 'Fecha' },
          ]}
          minWidth="760px"
        >
          {report.data?.items.map((item) => (
            <tr className="hover:bg-stone-50" key={item.saleId}>
              <td className="px-4 py-3 font-medium text-stone-950">{item.saleNumber}</td>
              <td className="px-4 py-3 text-stone-600">{item.branchName ?? '-'}</td>
              <td className="px-4 py-3 text-stone-600">{item.customerName ?? '-'}</td>
              <td className="px-4 py-3 text-stone-600">{item.paymentMethod}</td>
              <td className="px-4 py-3">
                <StatusBadge status={item.status} />
              </td>
              <td className="px-4 py-3 text-right font-semibold tabular-nums text-stone-950">
                {formatMoney(item.total)}
              </td>
              <td className="px-4 py-3 text-stone-500">{formatDate(item.createdAt)}</td>
            </tr>
          ))}
        </Table>
      </ReportBody>
      <PaginationBar
        data={report.data}
        onNext={() => updateFilters({ page: filters.page + 1 })}
        onPrev={() => updateFilters({ page: Math.max(1, filters.page - 1) })}
      />
    </ReportCard>
  )
}

function InvoiceReportPanel() {
  const initialFilters: InvoiceReportFilters = {
    ...defaultDateFilters,
    customerId: '',
    search: '',
    status: '',
  }
  const [filters, setFilters] = useState<InvoiceReportFilters>(initialFilters)
  const report = useInvoiceReport(filters)
  const hasFilters = Boolean(filters.search || filters.status || filters.dateFrom || filters.dateTo)

  function updateFilters(values: Partial<InvoiceReportFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <ReportCard
      canReset={hasFilters}
      description="Filtra facturas por numero, cliente, estado y periodo."
      exportHref={getInvoiceExportUrl(filters)}
      exportLabel="Exportar facturas"
      onReset={() => setFilters(initialFilters)}
      title="Facturas"
      filters={
        <>
          <SearchField
            label="Busqueda"
            onChange={(search) => updateFilters({ search })}
            placeholder="Factura o cliente"
            value={filters.search}
          />
          <DateField label="Desde" onChange={(dateFrom) => updateFilters({ dateFrom })} value={filters.dateFrom} />
          <DateField label="Hasta" onChange={(dateTo) => updateFilters({ dateTo })} value={filters.dateTo} />
          <SelectField
            label="Estado"
            onChange={(status) => updateFilters({ status })}
            options={statusOptions.invoices}
            value={filters.status}
          />
        </>
      }
    >
      {report.data && report.data.summary.totalCount > 0 && (
        <SummaryGrid>
          <SummaryStat label="Facturas" value={String(report.data.summary.totalCount)} />
          <SummaryStat label="Monto total" value={formatMoney(report.data.summary.totalAmount)} />
        </SummaryGrid>
      )}
      <ReportBody
        emptyMessage="Sin facturas para los filtros seleccionados."
        isEmpty={!report.data?.items.length}
        isError={report.isError}
        isLoading={report.isLoading}
      >
        <Table
          columns={[
            { label: 'Factura' },
            { label: 'Cliente' },
            { label: 'Estado' },
            { align: 'right', label: 'Subtotal' },
            { align: 'right', label: 'ITBIS' },
            { align: 'right', label: 'Total' },
            { label: 'Fecha' },
          ]}
          minWidth="760px"
        >
          {report.data?.items.map((item) => (
            <tr className="hover:bg-stone-50" key={item.invoiceId}>
              <td className="px-4 py-3 font-medium text-stone-950">{item.invoiceNumber}</td>
              <td className="px-4 py-3 text-stone-600">{item.customerName ?? '-'}</td>
              <td className="px-4 py-3">
                <StatusBadge status={item.status} />
              </td>
              <td className="px-4 py-3 text-right text-stone-700">{formatMoney(item.subtotal)}</td>
              <td className="px-4 py-3 text-right text-stone-700">{formatMoney(item.taxTotal)}</td>
              <td className="px-4 py-3 text-right font-semibold tabular-nums text-stone-950">
                {formatMoney(item.total)}
              </td>
              <td className="px-4 py-3 text-stone-500">{formatDate(item.createdAt)}</td>
            </tr>
          ))}
        </Table>
      </ReportBody>
      <PaginationBar
        data={report.data}
        onNext={() => updateFilters({ page: filters.page + 1 })}
        onPrev={() => updateFilters({ page: Math.max(1, filters.page - 1) })}
      />
    </ReportCard>
  )
}

function AccountsReceivablePanel() {
  const initialFilters: AccountsReceivableFilters = {
    ...defaultDateFilters,
    customerId: '',
    status: '',
  }
  const [filters, setFilters] = useState<AccountsReceivableFilters>(initialFilters)
  const report = useAccountsReceivableReport(filters)
  const hasFilters = Boolean(filters.status || filters.dateFrom || filters.dateTo)

  function updateFilters(values: Partial<AccountsReceivableFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <ReportCard
      canReset={hasFilters}
      description="Revisa balances pendientes, limites de credito y saldos en mora."
      exportHref={getArExportUrl(filters)}
      exportLabel="Exportar C/C"
      onReset={() => setFilters(initialFilters)}
      title="Cuentas por cobrar"
      filters={
        <>
          <DateField label="Desde" onChange={(dateFrom) => updateFilters({ dateFrom })} value={filters.dateFrom} />
          <DateField label="Hasta" onChange={(dateTo) => updateFilters({ dateTo })} value={filters.dateTo} />
          <SelectField
            label="Estado"
            onChange={(status) => updateFilters({ status })}
            options={statusOptions.accountsReceivable}
            value={filters.status}
          />
        </>
      }
    >
      {report.data && report.data.summary.totalCustomers > 0 && (
        <SummaryGrid>
          <SummaryStat label="Clientes con saldo" value={String(report.data.summary.totalCustomers)} />
          <SummaryStat label="Total pendiente" value={formatMoney(report.data.summary.totalPending)} />
          <SummaryStat label="En mora" value={formatMoney(report.data.summary.totalOverdue)} />
        </SummaryGrid>
      )}
      <ReportBody
        emptyMessage="Sin saldos pendientes para los filtros seleccionados."
        isEmpty={!report.data?.items.length}
        isError={report.isError}
        isLoading={report.isLoading}
      >
        <Table
          columns={[
            { label: 'Cliente' },
            { label: 'Estado' },
            { align: 'right', label: 'Limite credito' },
            { align: 'right', label: 'Saldo actual' },
            { align: 'right', label: 'En mora' },
            { label: 'Ultimo mov.' },
          ]}
          minWidth="720px"
        >
          {report.data?.items.map((item) => (
            <tr className="hover:bg-stone-50" key={item.creditAccountId}>
              <td className="px-4 py-3 font-medium text-stone-950">{item.customerName}</td>
              <td className="px-4 py-3">
                <StatusBadge status={item.status} />
              </td>
              <td className="px-4 py-3 text-right text-stone-700">{formatMoney(item.creditLimit)}</td>
              <td className="px-4 py-3 text-right font-semibold tabular-nums text-amber-700">
                {formatMoney(item.currentBalance)}
              </td>
              <td className="px-4 py-3 text-right text-red-700">
                {item.overdueAmount > 0 ? formatMoney(item.overdueAmount) : '-'}
              </td>
              <td className="px-4 py-3 text-stone-500">
                {item.lastMovementAt ? formatDate(item.lastMovementAt) : '-'}
              </td>
            </tr>
          ))}
        </Table>
      </ReportBody>
      <PaginationBar
        data={report.data}
        onNext={() => updateFilters({ page: filters.page + 1 })}
        onPrev={() => updateFilters({ page: Math.max(1, filters.page - 1) })}
      />
    </ReportCard>
  )
}

function LowStockPanel() {
  const initialFilters: LowStockFilters = {
    branchId: '',
    categoryId: '',
    page: 1,
    pageSize: 25,
    search: '',
  }
  const [filters, setFilters] = useState<LowStockFilters>(initialFilters)
  const report = useLowStockReport(filters)
  const hasFilters = Boolean(filters.search)

  function updateFilters(values: Partial<LowStockFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <ReportCard
      canReset={hasFilters}
      description="Identifica productos por debajo del umbral de inventario."
      exportHref={getLowStockExportUrl(filters)}
      exportLabel="Exportar bajo stock"
      onReset={() => setFilters(initialFilters)}
      title="Bajo stock"
      filters={
        <SearchField
          label="Busqueda"
          onChange={(search) => updateFilters({ search })}
          placeholder="Producto o SKU"
          value={filters.search}
        />
      }
    >
      <ReportBody
        emptyMessage="Sin productos en bajo stock para los filtros seleccionados."
        isEmpty={!report.data?.items.length}
        isError={report.isError}
        isLoading={report.isLoading}
      >
        <Table
          columns={[
            { label: 'Producto' },
            { label: 'SKU' },
            { label: 'Categoria' },
            { label: 'Sucursal' },
            { align: 'center', label: 'Actual' },
            { align: 'center', label: 'Minimo' },
            { align: 'center', label: 'Reponer' },
            { align: 'right', label: 'Costo unit.' },
          ]}
          minWidth="840px"
        >
          {report.data?.items.map((item) => (
            <tr className="hover:bg-stone-50" key={item.productId}>
              <td className="px-4 py-3 font-medium text-stone-950">{item.productName}</td>
              <td className="px-4 py-3 font-mono text-xs text-stone-500">{item.sku}</td>
              <td className="px-4 py-3 text-stone-600">{item.categoryName ?? '-'}</td>
              <td className="px-4 py-3 text-stone-600">{item.branchName ?? '-'}</td>
              <td className="px-4 py-3 text-center">
                <span className="rounded-md bg-red-50 px-2 py-1 text-xs font-semibold text-red-700 ring-1 ring-red-200">
                  {item.currentStock}
                </span>
              </td>
              <td className="px-4 py-3 text-center text-stone-600">{item.minimumStock}</td>
              <td className="px-4 py-3 text-center font-semibold text-amber-700">{item.suggestedRestock}</td>
              <td className="px-4 py-3 text-right text-stone-700">{formatMoney(item.unitCost)}</td>
            </tr>
          ))}
        </Table>
      </ReportBody>
      <PaginationBar
        data={report.data}
        onNext={() => updateFilters({ page: filters.page + 1 })}
        onPrev={() => updateFilters({ page: Math.max(1, filters.page - 1) })}
      />
    </ReportCard>
  )
}

function PurchasesPanel() {
  const initialFilters: PurchaseReportFilters = {
    ...defaultDateFilters,
    branchId: '',
    status: '',
    supplierId: '',
  }
  const [filters, setFilters] = useState<PurchaseReportFilters>(initialFilters)
  const report = usePurchaseReport(filters)
  const hasFilters = Boolean(filters.status || filters.dateFrom || filters.dateTo)

  function updateFilters(values: Partial<PurchaseReportFilters>) {
    setFilters((current) => ({ ...current, ...values, page: values.page ?? 1 }))
  }

  return (
    <ReportCard
      canReset={hasFilters}
      description="Analiza compras por estado, fecha de creacion y recepcion."
      exportHref={getPurchaseExportUrl(filters)}
      exportLabel="Exportar compras"
      onReset={() => setFilters(initialFilters)}
      title="Compras"
      filters={
        <>
          <DateField label="Desde" onChange={(dateFrom) => updateFilters({ dateFrom })} value={filters.dateFrom} />
          <DateField label="Hasta" onChange={(dateTo) => updateFilters({ dateTo })} value={filters.dateTo} />
          <SelectField
            label="Estado"
            onChange={(status) => updateFilters({ status })}
            options={statusOptions.purchases}
            value={filters.status}
          />
        </>
      }
    >
      {report.data && report.data.summary.totalCount > 0 && (
        <SummaryGrid>
          <SummaryStat label="Compras" value={String(report.data.summary.totalCount)} />
          <SummaryStat label="Monto total" value={formatMoney(report.data.summary.totalAmount)} />
        </SummaryGrid>
      )}
      <ReportBody
        emptyMessage="Sin compras para los filtros seleccionados."
        isEmpty={!report.data?.items.length}
        isError={report.isError}
        isLoading={report.isLoading}
      >
        <Table
          columns={[
            { label: 'Compra' },
            { label: 'Proveedor' },
            { label: 'Estado' },
            { align: 'center', label: 'Articulos' },
            { align: 'right', label: 'Total' },
            { label: 'Recibida' },
            { label: 'Fecha' },
          ]}
          minWidth="760px"
        >
          {report.data?.items.map((item) => (
            <tr className="hover:bg-stone-50" key={item.purchaseId}>
              <td className="px-4 py-3 font-medium text-stone-950">{item.purchaseNumber}</td>
              <td className="px-4 py-3 text-stone-600">{item.supplierName ?? '-'}</td>
              <td className="px-4 py-3">
                <StatusBadge status={item.status} />
              </td>
              <td className="px-4 py-3 text-center text-stone-600">{item.itemCount}</td>
              <td className="px-4 py-3 text-right font-semibold tabular-nums text-stone-950">
                {formatMoney(item.total)}
              </td>
              <td className="px-4 py-3 text-stone-500">{item.receivedAt ? formatDate(item.receivedAt) : '-'}</td>
              <td className="px-4 py-3 text-stone-500">{formatDate(item.createdAt)}</td>
            </tr>
          ))}
        </Table>
      </ReportBody>
      <PaginationBar
        data={report.data}
        onNext={() => updateFilters({ page: filters.page + 1 })}
        onPrev={() => updateFilters({ page: Math.max(1, filters.page - 1) })}
      />
    </ReportCard>
  )
}

function ReportCard({
  canReset,
  children,
  description,
  exportHref,
  exportLabel,
  filters,
  onReset,
  title,
}: Readonly<{
  canReset: boolean
  children: ReactNode
  description: string
  exportHref: string
  exportLabel: string
  filters: ReactNode
  onReset: () => void
  title: string
}>) {
  return (
    <Card className="overflow-hidden">
      <CardHeader className="border-b border-stone-200">
        <div className="flex flex-col gap-4">
          <div className="flex flex-col gap-3 xl:flex-row xl:items-start xl:justify-between">
            <div>
              <h3 className="text-base font-semibold text-stone-950">{title}</h3>
              <p className="mt-1 text-sm font-medium text-stone-600">{description}</p>
            </div>
            <div className="grid gap-2 sm:flex sm:items-center sm:justify-end">
              <Button
                className="w-full sm:w-auto"
                disabled={!canReset}
                onClick={onReset}
                type="button"
                variant="ghost"
              >
                <RotateCcw size={16} />
                Limpiar
              </Button>
              <ExportButton href={exportHref} label={exportLabel} />
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4 xl:items-end">
            {filters}
          </div>
        </div>
      </CardHeader>
      <CardContent className="p-0">{children}</CardContent>
    </Card>
  )
}

function SearchField({
  label,
  onChange,
  placeholder,
  value,
}: Readonly<{
  label: string
  onChange: (value: string) => void
  placeholder: string
  value: string
}>) {
  return (
    <label className="block min-w-0 space-y-1.5">
      <span className="text-sm font-semibold text-stone-800">{label}</span>
      <span className="relative block">
        <Search
          aria-hidden="true"
          className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400"
          size={15}
        />
        <input
          className={`${inputClassName} pl-9`}
          onChange={(event) => onChange(event.target.value)}
          placeholder={placeholder}
          value={value}
        />
      </span>
    </label>
  )
}

function DateField({
  label,
  onChange,
  value,
}: Readonly<{
  label: string
  onChange: (value: string) => void
  value: string
}>) {
  return (
    <label className="block min-w-0 space-y-1.5">
      <span className="text-sm font-semibold text-stone-800">{label}</span>
      <input
        className={inputClassName}
        onChange={(event) => onChange(event.target.value)}
        type="date"
        value={value}
      />
    </label>
  )
}

function SelectField({
  label,
  onChange,
  options,
  value,
}: Readonly<{
  label: string
  onChange: (value: string) => void
  options: Option[]
  value: string
}>) {
  return (
    <div className="block min-w-0 space-y-1.5">
      <span className="text-sm font-semibold text-stone-800">{label}</span>
      <Select value={value || '_'} onValueChange={(next) => onChange(next === '_' ? '' : next)}>
        <SelectTrigger>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option.value || '_'} value={option.value || '_'}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  )
}

function ExportButton({ href, label }: Readonly<{ href: string; label: string }>) {
  return (
    <PermissionGate permission={Permission.ReportsExport}>
      <Button asChild className="w-full whitespace-nowrap sm:w-auto">
        <a download href={href}>
          <Download size={16} />
          {label}
        </a>
      </Button>
    </PermissionGate>
  )
}

function SummaryGrid({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <div className="grid gap-3 border-b border-stone-200 bg-stone-50 px-4 py-3 sm:grid-cols-2 xl:grid-cols-4">
      {children}
    </div>
  )
}

function SummaryStat({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="rounded-md bg-white px-3 py-2 ring-1 ring-stone-200">
      <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">{label}</p>
      <p className="mt-1 text-sm font-semibold tabular-nums text-stone-950">{value}</p>
    </div>
  )
}

function ReportBody({
  children,
  emptyMessage,
  isEmpty,
  isError,
  isLoading,
}: Readonly<{
  children: ReactNode
  emptyMessage: string
  isEmpty: boolean
  isError: boolean
  isLoading: boolean
}>) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center gap-3 py-12 text-sm font-semibold text-stone-500">
        <Loader2 className="animate-spin" size={18} />
        Cargando reporte...
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex items-center justify-center gap-3 py-12 text-sm font-semibold text-red-700">
        <AlertTriangle size={18} />
        Error al cargar el reporte.
      </div>
    )
  }

  if (isEmpty) {
    return (
      <div className="flex items-center justify-center py-12 text-sm font-semibold text-stone-500">
        {emptyMessage}
      </div>
    )
  }

  return <>{children}</>
}

function Table({
  children,
  columns,
  minWidth,
}: Readonly<{
  children: ReactNode
  columns: TableColumn[]
  minWidth: string
}>) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm" style={{ minWidth }}>
        <thead className="bg-stone-50 text-xs font-semibold uppercase tracking-wide text-stone-500">
          <tr>
            {columns.map((column) => (
              <th className={`${alignClass(column.align)} px-4 py-3`} key={column.label}>
                {column.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200 bg-white">{children}</tbody>
      </table>
    </div>
  )
}

function StatusBadge({ status }: Readonly<{ status: string }>) {
  const classes: Record<string, string> = {
    Active: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
    Blocked: 'bg-red-50 text-red-700 ring-red-200',
    Cancelled: 'bg-red-50 text-red-700 ring-red-200',
    Completed: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
    Draft: 'bg-stone-100 text-stone-700 ring-stone-200',
    Issued: 'bg-sky-50 text-sky-700 ring-sky-200',
    Pending: 'bg-amber-50 text-amber-700 ring-amber-200',
    Received: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  }

  return (
    <span className={`inline-flex rounded-md px-2 py-1 text-xs font-semibold ring-1 ${classes[status] ?? 'bg-stone-100 text-stone-700 ring-stone-200'}`}>
      {statusLabel(status)}
    </span>
  )
}

function PaginationBar({
  data,
  onNext,
  onPrev,
}: Readonly<{
  data?: { hasPreviousPage: boolean; hasNextPage: boolean; page: number; totalPages: number }
  onNext: () => void
  onPrev: () => void
}>) {
  if (!data || data.totalPages <= 1) {
    return null
  }

  return (
    <div className="flex flex-col gap-3 border-t border-stone-200 px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm font-medium text-stone-500">
        Pagina {data.page} de {data.totalPages}
      </p>
      <div className="grid gap-2 sm:flex sm:items-center">
        <Button disabled={!data.hasPreviousPage} onClick={onPrev} type="button" variant="secondary">
          Anterior
        </Button>
        <Button disabled={!data.hasNextPage} onClick={onNext} type="button" variant="secondary">
          Siguiente
        </Button>
      </div>
    </div>
  )
}

function alignClass(align: TableColumn['align']) {
  if (align === 'center') return 'text-center'
  if (align === 'right') return 'text-right'
  return 'text-left'
}

function statusLabel(status: string) {
  const labels: Record<string, string> = {
    Active: 'Activo',
    Blocked: 'Bloqueado',
    Cancelled: 'Cancelado',
    Completed: 'Completado',
    Draft: 'Borrador',
    Issued: 'Emitido',
    Pending: 'Pendiente',
    Received: 'Recibido',
  }

  return labels[status] ?? status
}

function formatMoney(amount: number): string {
  return new Intl.NumberFormat('es-DO', {
    currency: 'DOP',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency',
  }).format(amount)
}

function formatDate(isoString: string): string {
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(isoString))
}

const inputClassName =
  'h-10 w-full min-w-0 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-300 outline-none placeholder:text-stone-400 focus:ring-2 focus:ring-stone-900/20'
