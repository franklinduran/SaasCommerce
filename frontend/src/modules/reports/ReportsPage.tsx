import {
  AlertTriangle,
  Download,
  Loader2,
  Search,
} from 'lucide-react'
import { useState } from 'react'
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

type Tab = 'sales' | 'invoices' | 'receivable' | 'lowstock' | 'purchases'

const tabs: { id: Tab; label: string }[] = [
  { id: 'sales', label: 'Ventas' },
  { id: 'invoices', label: 'Facturas' },
  { id: 'receivable', label: 'Cuentas por cobrar' },
  { id: 'lowstock', label: 'Bajo stock' },
  { id: 'purchases', label: 'Compras' },
]

const defaultDateFilters = { dateFrom: '', dateTo: '', page: 1, pageSize: 25 }

export function ReportsPage() {
  const [activeTab, setActiveTab] = useState<Tab>('sales')

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div>
        <p className="text-sm font-semibold uppercase text-stone-500">Analítica</p>
        <h2 className="mt-1 text-2xl font-semibold text-stone-950">Reportes</h2>
        <p className="mt-2 text-sm font-medium text-stone-600">
          Consulta y exporta reportes de ventas, facturas, cobros, inventario y compras.
        </p>
      </div>

      {/* Tabs */}
      <div className="flex flex-wrap gap-1 rounded-lg bg-stone-100 p-1">
        {tabs.map((tab) => (
          <button
            className={`rounded-md px-4 py-2 text-sm font-semibold transition-colors ${
              activeTab === tab.id
                ? 'bg-white text-stone-900 shadow-sm ring-1 ring-stone-200'
                : 'text-stone-600 hover:text-stone-900'
            }`}
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            type="button"
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Tab panels */}
      {activeTab === 'sales' && <SalesReportPanel />}
      {activeTab === 'invoices' && <InvoiceReportPanel />}
      {activeTab === 'receivable' && <AccountsReceivablePanel />}
      {activeTab === 'lowstock' && <LowStockPanel />}
      {activeTab === 'purchases' && <PurchasesPanel />}
    </section>
  )
}

// ── Sales ──────────────────────────────────────────────────────────────────

function SalesReportPanel() {
  const [filters, setFilters] = useState<SalesReportFilters>({
    ...defaultDateFilters,
    branchId: '',
    paymentMethod: '',
    search: '',
    status: '',
  })

  const report = useSalesReport(filters)

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <FiltersRow>
              <SearchInput
                onChange={(v) => setFilters((f) => ({ ...f, page: 1, search: v }))}
                placeholder="Buscar venta"
                value={filters.search}
              />
              <DateInput
                label="Desde"
                onChange={(v) => setFilters((f) => ({ ...f, dateFrom: v, page: 1 }))}
                value={filters.dateFrom}
              />
              <DateInput
                label="Hasta"
                onChange={(v) => setFilters((f) => ({ ...f, dateTo: v, page: 1 }))}
                value={filters.dateTo}
              />
              <SelectInput
                label="Estado"
                onChange={(v) => setFilters((f) => ({ ...f, page: 1, status: v }))}
                options={[
                  { label: 'Todos', value: '' },
                  { label: 'Completadas', value: 'Completed' },
                  { label: 'Canceladas', value: 'Cancelled' },
                  { label: 'Pendientes', value: 'Pending' },
                ]}
                value={filters.status}
              />
            </FiltersRow>
            <ExportButton href={getSalesExportUrl(filters)} label="Exportar ventas" />
          </div>
        </CardHeader>
        <CardContent className="p-0">
          {report.isLoading && <LoadingState />}
          {report.isError && <ErrorState />}
          {report.data && (
            <>
              {report.data.summary.totalCount > 0 && (
                <SummaryBar>
                  <SummaryStat label="Total ventas" value={String(report.data.summary.totalCount)} />
                  <SummaryStat label="Monto total" value={formatMoney(report.data.summary.totalAmount)} />
                  <SummaryStat label="Promedio" value={formatMoney(report.data.summary.averageAmount)} />
                </SummaryBar>
              )}
              {!report.data.items.length ? (
                <EmptyState />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[680px] text-sm">
                    <TableHead columns={['Nro. Venta', 'Sucursal', 'Cliente', 'Método', 'Estado', 'Total', 'Fecha']} />
                    <tbody>
                      {report.data.items.map((item) => (
                        <tr className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60" key={item.saleId}>
                          <td className="px-4 py-3 font-medium text-stone-900">{item.saleNumber}</td>
                          <td className="px-4 py-3 text-stone-600">{item.branchName ?? '—'}</td>
                          <td className="px-4 py-3 text-stone-600">{item.customerName ?? '—'}</td>
                          <td className="px-4 py-3 text-stone-600">{item.paymentMethod}</td>
                          <td className="px-4 py-3">
                            <StatusBadge status={item.status} />
                          </td>
                          <td className="px-4 py-3 text-right font-semibold text-stone-900">
                            {formatMoney(item.total)}
                          </td>
                          <td className="px-4 py-3 text-stone-500">{formatDate(item.createdAt)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
      {report.data && report.data.totalPages > 1 && (
        <PaginationBar
          data={report.data}
          onNext={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
          onPrev={() => setFilters((f) => ({ ...f, page: Math.max(1, f.page - 1) }))}
        />
      )}
    </div>
  )
}

// ── Invoices ───────────────────────────────────────────────────────────────

function InvoiceReportPanel() {
  const [filters, setFilters] = useState<InvoiceReportFilters>({
    ...defaultDateFilters,
    customerId: '',
    search: '',
    status: '',
  })

  const report = useInvoiceReport(filters)

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <FiltersRow>
              <SearchInput
                onChange={(v) => setFilters((f) => ({ ...f, page: 1, search: v }))}
                placeholder="Buscar factura"
                value={filters.search}
              />
              <DateInput
                label="Desde"
                onChange={(v) => setFilters((f) => ({ ...f, dateFrom: v, page: 1 }))}
                value={filters.dateFrom}
              />
              <DateInput
                label="Hasta"
                onChange={(v) => setFilters((f) => ({ ...f, dateTo: v, page: 1 }))}
                value={filters.dateTo}
              />
              <SelectInput
                label="Estado"
                onChange={(v) => setFilters((f) => ({ ...f, page: 1, status: v }))}
                options={[
                  { label: 'Todos', value: '' },
                  { label: 'Emitidas', value: 'Issued' },
                  { label: 'Canceladas', value: 'Cancelled' },
                ]}
                value={filters.status}
              />
            </FiltersRow>
            <ExportButton href={getInvoiceExportUrl(filters)} label="Exportar facturas" />
          </div>
        </CardHeader>
        <CardContent className="p-0">
          {report.isLoading && <LoadingState />}
          {report.isError && <ErrorState />}
          {report.data && (
            <>
              {report.data.summary.totalCount > 0 && (
                <SummaryBar>
                  <SummaryStat label="Total facturas" value={String(report.data.summary.totalCount)} />
                  <SummaryStat label="Monto total" value={formatMoney(report.data.summary.totalAmount)} />
                </SummaryBar>
              )}
              {!report.data.items.length ? (
                <EmptyState />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[580px] text-sm">
                    <TableHead columns={['Nro. Factura', 'Cliente', 'Estado', 'Subtotal', 'ITBIS', 'Total', 'Fecha']} />
                    <tbody>
                      {report.data.items.map((item) => (
                        <tr className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60" key={item.invoiceId}>
                          <td className="px-4 py-3 font-medium text-stone-900">{item.invoiceNumber}</td>
                          <td className="px-4 py-3 text-stone-600">{item.customerName ?? '—'}</td>
                          <td className="px-4 py-3">
                            <StatusBadge status={item.status} />
                          </td>
                          <td className="px-4 py-3 text-right text-stone-700">{formatMoney(item.subtotal)}</td>
                          <td className="px-4 py-3 text-right text-stone-700">{formatMoney(item.taxTotal)}</td>
                          <td className="px-4 py-3 text-right font-semibold text-stone-900">
                            {formatMoney(item.total)}
                          </td>
                          <td className="px-4 py-3 text-stone-500">{formatDate(item.createdAt)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
      {report.data && report.data.totalPages > 1 && (
        <PaginationBar
          data={report.data}
          onNext={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
          onPrev={() => setFilters((f) => ({ ...f, page: Math.max(1, f.page - 1) }))}
        />
      )}
    </div>
  )
}

// ── Accounts Receivable ────────────────────────────────────────────────────

function AccountsReceivablePanel() {
  const [filters, setFilters] = useState<AccountsReceivableFilters>({
    ...defaultDateFilters,
    customerId: '',
    status: '',
  })

  const report = useAccountsReceivableReport(filters)

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <FiltersRow>
              <SelectInput
                label="Estado"
                onChange={(v) => setFilters((f) => ({ ...f, page: 1, status: v }))}
                options={[
                  { label: 'Todos', value: '' },
                  { label: 'Activos', value: 'Active' },
                  { label: 'Bloqueados', value: 'Blocked' },
                ]}
                value={filters.status}
              />
            </FiltersRow>
            <ExportButton href={getArExportUrl(filters)} label="Exportar C/C" />
          </div>
        </CardHeader>
        <CardContent className="p-0">
          {report.isLoading && <LoadingState />}
          {report.isError && <ErrorState />}
          {report.data && (
            <>
              {(report.data.summary.totalCustomers > 0) && (
                <SummaryBar>
                  <SummaryStat label="Clientes con saldo" value={String(report.data.summary.totalCustomers)} />
                  <SummaryStat label="Total pendiente" value={formatMoney(report.data.summary.totalPending)} />
                  <SummaryStat label="En mora" value={formatMoney(report.data.summary.totalOverdue)} />
                </SummaryBar>
              )}
              {!report.data.items.length ? (
                <EmptyState message="Sin saldos pendientes" />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[620px] text-sm">
                    <TableHead columns={['Cliente', 'Estado', 'Límite crédito', 'Saldo actual', 'En mora', 'Último mov.']} />
                    <tbody>
                      {report.data.items.map((item) => (
                        <tr className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60" key={item.creditAccountId}>
                          <td className="px-4 py-3 font-medium text-stone-900">{item.customerName}</td>
                          <td className="px-4 py-3">
                            <StatusBadge status={item.status} />
                          </td>
                          <td className="px-4 py-3 text-right text-stone-700">{formatMoney(item.creditLimit)}</td>
                          <td className="px-4 py-3 text-right font-semibold text-amber-700">
                            {formatMoney(item.currentBalance)}
                          </td>
                          <td className="px-4 py-3 text-right text-red-700">
                            {item.overdueAmount > 0 ? formatMoney(item.overdueAmount) : '—'}
                          </td>
                          <td className="px-4 py-3 text-stone-500">
                            {item.lastMovementAt ? formatDate(item.lastMovementAt) : '—'}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
      {report.data && report.data.totalPages > 1 && (
        <PaginationBar
          data={report.data}
          onNext={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
          onPrev={() => setFilters((f) => ({ ...f, page: Math.max(1, f.page - 1) }))}
        />
      )}
    </div>
  )
}

// ── Low Stock ──────────────────────────────────────────────────────────────

function LowStockPanel() {
  const [filters, setFilters] = useState<LowStockFilters>({
    branchId: '',
    categoryId: '',
    page: 1,
    pageSize: 25,
    search: '',
  })

  const report = useLowStockReport(filters)

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <FiltersRow>
              <SearchInput
                onChange={(v) => setFilters((f) => ({ ...f, page: 1, search: v }))}
                placeholder="Buscar producto"
                value={filters.search}
              />
            </FiltersRow>
            <ExportButton href={getLowStockExportUrl(filters)} label="Exportar bajo stock" />
          </div>
        </CardHeader>
        <CardContent className="p-0">
          {report.isLoading && <LoadingState />}
          {report.isError && <ErrorState />}
          {report.data && (
            <>
              {!report.data.items.length ? (
                <EmptyState message="Sin productos en bajo stock" />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[700px] text-sm">
                    <TableHead columns={['Producto', 'SKU', 'Categoría', 'Sucursal', 'Stock actual', 'Mínimo', 'Reponer', 'Costo unit.']} />
                    <tbody>
                      {report.data.items.map((item) => (
                        <tr className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60" key={item.productId}>
                          <td className="px-4 py-3 font-medium text-stone-900">{item.productName}</td>
                          <td className="px-4 py-3 font-mono text-xs text-stone-500">{item.sku}</td>
                          <td className="px-4 py-3 text-stone-600">{item.categoryName ?? '—'}</td>
                          <td className="px-4 py-3 text-stone-600">{item.branchName ?? '—'}</td>
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
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
      {report.data && report.data.totalPages > 1 && (
        <PaginationBar
          data={report.data}
          onNext={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
          onPrev={() => setFilters((f) => ({ ...f, page: Math.max(1, f.page - 1) }))}
        />
      )}
    </div>
  )
}

// ── Purchases ──────────────────────────────────────────────────────────────

function PurchasesPanel() {
  const [filters, setFilters] = useState<PurchaseReportFilters>({
    ...defaultDateFilters,
    branchId: '',
    status: '',
    supplierId: '',
  })

  const report = usePurchaseReport(filters)

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <FiltersRow>
              <DateInput
                label="Desde"
                onChange={(v) => setFilters((f) => ({ ...f, dateFrom: v, page: 1 }))}
                value={filters.dateFrom}
              />
              <DateInput
                label="Hasta"
                onChange={(v) => setFilters((f) => ({ ...f, dateTo: v, page: 1 }))}
                value={filters.dateTo}
              />
              <SelectInput
                label="Estado"
                onChange={(v) => setFilters((f) => ({ ...f, page: 1, status: v }))}
                options={[
                  { label: 'Todos', value: '' },
                  { label: 'Pendientes', value: 'Pending' },
                  { label: 'Recibidas', value: 'Received' },
                  { label: 'Canceladas', value: 'Cancelled' },
                ]}
                value={filters.status}
              />
            </FiltersRow>
            <ExportButton href={getPurchaseExportUrl(filters)} label="Exportar compras" />
          </div>
        </CardHeader>
        <CardContent className="p-0">
          {report.isLoading && <LoadingState />}
          {report.isError && <ErrorState />}
          {report.data && (
            <>
              {report.data.summary.totalCount > 0 && (
                <SummaryBar>
                  <SummaryStat label="Total compras" value={String(report.data.summary.totalCount)} />
                  <SummaryStat label="Monto total" value={formatMoney(report.data.summary.totalAmount)} />
                </SummaryBar>
              )}
              {!report.data.items.length ? (
                <EmptyState />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[640px] text-sm">
                    <TableHead columns={['Nro. Compra', 'Proveedor', 'Estado', 'Artículos', 'Total', 'Recibida', 'Fecha']} />
                    <tbody>
                      {report.data.items.map((item) => (
                        <tr className="border-b border-stone-100 last:border-0 hover:bg-stone-50/60" key={item.purchaseId}>
                          <td className="px-4 py-3 font-medium text-stone-900">{item.purchaseNumber}</td>
                          <td className="px-4 py-3 text-stone-600">{item.supplierName ?? '—'}</td>
                          <td className="px-4 py-3">
                            <StatusBadge status={item.status} />
                          </td>
                          <td className="px-4 py-3 text-center text-stone-600">{item.itemCount}</td>
                          <td className="px-4 py-3 text-right font-semibold text-stone-900">
                            {formatMoney(item.total)}
                          </td>
                          <td className="px-4 py-3 text-stone-500">
                            {item.receivedAt ? formatDate(item.receivedAt) : '—'}
                          </td>
                          <td className="px-4 py-3 text-stone-500">{formatDate(item.createdAt)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
      {report.data && report.data.totalPages > 1 && (
        <PaginationBar
          data={report.data}
          onNext={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
          onPrev={() => setFilters((f) => ({ ...f, page: Math.max(1, f.page - 1) }))}
        />
      )}
    </div>
  )
}

// ── Shared UI primitives ───────────────────────────────────────────────────

function FiltersRow({ children }: { children: React.ReactNode }) {
  return <div className="flex flex-wrap items-center gap-2">{children}</div>
}

function SearchInput({
  value,
  onChange,
  placeholder,
}: {
  value: string
  onChange: (v: string) => void
  placeholder?: string
}) {
  return (
    <label className="relative block">
      <span className="sr-only">{placeholder ?? 'Buscar'}</span>
      <Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" size={15} />
      <input
        className={inputCn + ' pl-9 min-w-[200px]'}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder ?? 'Buscar...'}
        value={value}
      />
    </label>
  )
}

function DateInput({
  value,
  onChange,
  label,
}: {
  value: string
  onChange: (v: string) => void
  label: string
}) {
  return (
    <input
      aria-label={label}
      className={inputCn}
      onChange={(e) => onChange(e.target.value)}
      title={label}
      type="date"
      value={value}
    />
  )
}

function SelectInput({
  value,
  onChange,
  options,
  label,
}: {
  value: string
  onChange: (v: string) => void
  options: { value: string; label: string }[]
  label: string
}) {
  return (
    <select
      aria-label={label}
      className={inputCn}
      onChange={(e) => onChange(e.target.value)}
      value={value}
    >
      {options.map((opt) => (
        <option key={opt.value} value={opt.value}>
          {opt.label}
        </option>
      ))}
    </select>
  )
}

function ExportButton({ href, label }: { href: string; label: string }) {
  return (
    <PermissionGate permission={Permission.ReportsExport}>
      <a
        className="flex h-10 shrink-0 items-center gap-2 rounded-md bg-stone-900 px-4 text-sm font-semibold text-white transition-colors hover:bg-stone-700"
        download
        href={href}
      >
        <Download size={15} />
        {label}
      </a>
    </PermissionGate>
  )
}

function TableHead({ columns }: { columns: string[] }) {
  return (
    <thead>
      <tr className="border-b border-stone-100 bg-stone-50 text-xs font-semibold text-stone-500">
        {columns.map((col) => (
          <th className="px-4 py-2.5 text-left last:text-right" key={col}>
            {col}
          </th>
        ))}
      </tr>
    </thead>
  )
}

function SummaryBar({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex flex-wrap gap-6 border-b border-stone-100 bg-stone-50 px-4 py-3">
      {children}
    </div>
  )
}

function SummaryStat({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs font-medium text-stone-500">{label}</p>
      <p className="text-sm font-semibold text-stone-900">{value}</p>
    </div>
  )
}

function StatusBadge({ status }: { status: string }) {
  const map: Record<string, string> = {
    Completed: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200',
    Issued: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200',
    Received: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200',
    Active: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200',
    Cancelled: 'bg-red-50 text-red-700 ring-1 ring-red-200',
    Blocked: 'bg-red-50 text-red-700 ring-1 ring-red-200',
    Pending: 'bg-amber-50 text-amber-700 ring-1 ring-amber-200',
  }

  return (
    <span className={`rounded-md px-2 py-1 text-xs font-semibold ${map[status] ?? 'bg-stone-100 text-stone-700'}`}>
      {status}
    </span>
  )
}

function PaginationBar({
  data,
  onPrev,
  onNext,
}: {
  data: { hasPreviousPage: boolean; hasNextPage: boolean; page: number; totalPages: number }
  onPrev: () => void
  onNext: () => void
}) {
  return (
    <div className="flex items-center justify-between">
      <p className="text-sm font-medium text-stone-500">
        Página {data.page} de {data.totalPages}
      </p>
      <div className="flex items-center gap-2">
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

function LoadingState() {
  return (
    <div className="flex items-center justify-center gap-3 py-10 text-sm font-medium text-stone-500">
      <Loader2 className="animate-spin" size={18} />
      Cargando...
    </div>
  )
}

function ErrorState() {
  return (
    <div className="flex items-center justify-center gap-3 py-10 text-sm font-medium text-red-600">
      <AlertTriangle size={18} />
      Error al cargar el reporte. Intenta de nuevo.
    </div>
  )
}

function EmptyState({ message }: { message?: string }) {
  return (
    <div className="flex items-center justify-center py-10 text-sm font-medium text-stone-400">
      {message ?? 'Sin resultados para el período seleccionado'}
    </div>
  )
}

// ── Formatting ─────────────────────────────────────────────────────────────

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

const inputCn =
  'h-10 rounded-md bg-white px-3 text-sm font-medium text-stone-900 shadow-sm ring-1 ring-stone-200 outline-none focus:ring-2 focus:ring-stone-900/15'
