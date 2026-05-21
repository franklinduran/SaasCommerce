import { zodResolver } from '@hookform/resolvers/zod'
import { Building2, DollarSign, Package, Receipt, ShieldCheck } from 'lucide-react'
import type { ReactNode } from 'react'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  useBillingSettingsQuery,
  useBusinessSettingsQuery,
  useInventorySettingsQuery,
  useSalesSettingsQuery,
  useUpdateBillingSettingsMutation,
  useUpdateBusinessSettingsMutation,
  useUpdateInventorySettingsMutation,
  useUpdateSalesSettingsMutation,
} from '@/modules/settings/hooks/useSettings'
import type {
  BillingSettingsData,
  BusinessSettingsData,
  InventorySettingsData,
  SalesSettingsData,
} from '@/modules/settings/types'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader } from '@/shared/components/ui/card'
import { HttpClientError } from '@/shared/services/httpClient'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select'

// ── Zod schemas ──────────────────────────────────────────────────────────────

const businessSettingsSchema = z.object({
  address: z.string().optional(),
  commercialName: z.string().optional(),
  currency: z.string().min(1, 'La moneda es requerida.').max(3, 'Maximo 3 caracteres.'),
  email: z.string().optional(),
  legalName: z.string().optional(),
  logoUrl: z.string().optional(),
  phone: z.string().optional(),
  receiptFooterText: z.string().optional(),
  rnc: z.string().optional(),
  timezone: z.string().min(1, 'La zona horaria es requerida.'),
})

const salesSettingsSchema = z.object({
  allowDiscounts: z.boolean(),
  allowNegativeStock: z.boolean(),
  defaultPaymentMethod: z.string().optional(),
  enableInvoiceAutoGeneration: z.boolean(),
  enableReceiptPrintAfterSale: z.boolean(),
  requireCustomerForCreditSale: z.boolean(),
})

const inventorySettingsSchema = z.object({
  allowInventoryTransferBetweenBranches: z.boolean(),
  defaultLowStockThreshold: z.number().min(0, 'El umbral debe ser >= 0.'),
  enableLowStockAlerts: z.boolean(),
  requireReasonForInventoryAdjustment: z.boolean(),
})

const billingSettingsSchema = z.object({
  enableInvoiceAutoGeneration: z.boolean(),
  invoicePrefix: z
    .string()
    .min(1, 'El prefijo es requerido.')
    .max(10, 'Maximo 10 caracteres.'),
  invoiceSequenceStart: z.number().int().min(1, 'El inicio debe ser >= 1.'),
  receiptFooterText: z.string().optional(),
  receiptHeaderText: z.string().optional(),
  showLogoOnReceipt: z.boolean(),
  showRncOnReceipt: z.boolean(),
})

type BusinessSettingsForm = z.infer<typeof businessSettingsSchema>
type SalesSettingsForm = z.infer<typeof salesSettingsSchema>
type InventorySettingsForm = z.infer<typeof inventorySettingsSchema>
type BillingSettingsForm = z.infer<typeof billingSettingsSchema>

// ── Tabs definition ──────────────────────────────────────────────────────────

type TabId = 'business' | 'sales' | 'inventory' | 'billing'

const TAB_LABELS: Record<TabId, string> = {
  billing: 'Facturacion',
  business: 'Negocio',
  inventory: 'Inventario',
  sales: 'Ventas',
}

const TAB_IDS: ReadonlyArray<TabId> = ['business', 'sales', 'inventory', 'billing']

// ── Panel root ────────────────────────────────────────────────────────────────

export function OperationalSettingsPanel() {
  const [activeTab, setActiveTab] = useState<TabId>('business')

  return (
    <section aria-label="Configuracion operativa" className="space-y-4">
      <div>
        <p className="text-sm font-semibold uppercase tracking-wide text-stone-500">
          Parametros operativos
        </p>
        <h2 className="mt-1 text-xl font-semibold text-stone-950">Configuracion del sistema</h2>
        <p className="mt-1 text-sm font-medium text-stone-600">
          Personaliza el comportamiento del sistema para tu negocio.
        </p>
      </div>

      <div
        aria-label="Secciones de configuracion"
        className="flex max-w-full gap-1 overflow-x-auto rounded-md bg-stone-100 p-1"
        role="tablist"
      >
        {TAB_IDS.map((id) => (
          <button
            aria-controls={`ops-tab-panel-${id}`}
            aria-selected={activeTab === id}
            className={[
              'flex h-9 flex-1 shrink-0 items-center justify-center gap-1.5 rounded-md px-3 text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-stone-900/25 focus-visible:ring-offset-2',
              activeTab === id
                ? 'bg-stone-900 text-white shadow-sm ring-1 ring-stone-900 hover:bg-stone-900 hover:text-white active:bg-stone-950 active:text-white'
                : 'text-stone-700 hover:bg-stone-200 hover:text-stone-950 active:bg-stone-300',
            ].join(' ')}
            id={`ops-tab-${id}`}
            key={id}
            onClick={() => setActiveTab(id)}
            role="tab"
            type="button"
          >
            {TAB_LABELS[id]}
          </button>
        ))}
      </div>

      <div
        aria-labelledby={`ops-tab-${activeTab}`}
        id={`ops-tab-panel-${activeTab}`}
        role="tabpanel"
      >
        {activeTab === 'business' && <BusinessSettingsTab />}
        {activeTab === 'sales' && <SalesSettingsTab />}
        {activeTab === 'inventory' && <InventorySettingsTab />}
        {activeTab === 'billing' && <BillingSettingsTab />}
      </div>
    </section>
  )
}

// ── Business settings tab ─────────────────────────────────────────────────────

function BusinessSettingsTab() {
  const query = useBusinessSettingsQuery()
  const mutation = useUpdateBusinessSettingsMutation()
  const data: BusinessSettingsData | undefined = query.data

  const form = useForm<BusinessSettingsForm>({
    resolver: zodResolver(businessSettingsSchema),
    values: {
      address: data?.address ?? '',
      commercialName: data?.commercialName ?? '',
      currency: data?.currency ?? 'DOP',
      email: data?.email ?? '',
      legalName: data?.legalName ?? '',
      logoUrl: data?.logoUrl ?? '',
      phone: data?.phone ?? '',
      receiptFooterText: data?.receiptFooterText ?? '',
      rnc: data?.rnc ?? '',
      timezone: data?.timezone ?? 'America/Santo_Domingo',
    },
  })

  if (query.isLoading) return <OpsTabSkeleton />

  return (
    <OpsCard
      description="Nombre comercial, identificacion fiscal y datos de contacto."
      icon={<Building2 size={18} />}
      title="Informacion del negocio"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) =>
          mutation.mutate({
            address: emptyToNull(values.address),
            commercialName: emptyToNull(values.commercialName),
            currency: values.currency,
            email: emptyToNull(values.email),
            legalName: emptyToNull(values.legalName),
            logoUrl: emptyToNull(values.logoUrl),
            phone: emptyToNull(values.phone),
            receiptFooterText: emptyToNull(values.receiptFooterText),
            rnc: emptyToNull(values.rnc),
            timezone: values.timezone,
          }),
        )}
      >
        <div className="grid gap-4 lg:grid-cols-2">
          <OpsField error={form.formState.errors.commercialName?.message} label="Nombre comercial">
            <input className={inputCls} {...form.register('commercialName')} />
          </OpsField>
          <OpsField error={form.formState.errors.legalName?.message} label="Razon social">
            <input className={inputCls} {...form.register('legalName')} />
          </OpsField>
        </div>
        <div className="grid gap-4 lg:grid-cols-2">
          <OpsField error={form.formState.errors.rnc?.message} label="RNC / Identificacion fiscal">
            <input className={inputCls} {...form.register('rnc')} />
          </OpsField>
          <OpsField error={form.formState.errors.phone?.message} label="Telefono">
            <input className={inputCls} {...form.register('phone')} />
          </OpsField>
        </div>
        <OpsField error={form.formState.errors.email?.message} label="Correo electronico">
          <input className={inputCls} type="email" {...form.register('email')} />
        </OpsField>
        <OpsField error={form.formState.errors.address?.message} label="Direccion">
          <input className={inputCls} {...form.register('address')} />
        </OpsField>
        <div className="grid gap-4 lg:grid-cols-2">
          <OpsField error={form.formState.errors.currency?.message} label="Moneda *">
            <Controller
              control={form.control}
              name="currency"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="DOP">DOP — Peso dominicano</SelectItem>
                    <SelectItem value="USD">USD — Dolar estadounidense</SelectItem>
                    <SelectItem value="EUR">EUR — Euro</SelectItem>
                  </SelectContent>
                </Select>
              )}
            />
          </OpsField>
          <OpsField error={form.formState.errors.timezone?.message} label="Zona horaria *">
            <input
              className={inputCls}
              placeholder="America/Santo_Domingo"
              {...form.register('timezone')}
            />
          </OpsField>
        </div>
        <OpsField error={form.formState.errors.logoUrl?.message} label="URL del logo">
          <input className={inputCls} placeholder="https://..." {...form.register('logoUrl')} />
        </OpsField>
        <OpsField error={form.formState.errors.receiptFooterText?.message} label="Pie de recibo">
          <textarea
            className={`${inputCls} h-20 resize-none py-2`}
            {...form.register('receiptFooterText')}
          />
        </OpsField>
        <OpsFormFooter mutation={mutation} />
      </form>
    </OpsCard>
  )
}

// ── Sales settings tab ────────────────────────────────────────────────────────

function SalesSettingsTab() {
  const query = useSalesSettingsQuery()
  const mutation = useUpdateSalesSettingsMutation()
  const data: SalesSettingsData | undefined = query.data

  const form = useForm<SalesSettingsForm>({
    resolver: zodResolver(salesSettingsSchema),
    values: {
      allowDiscounts: data?.allowDiscounts ?? true,
      allowNegativeStock: data?.allowNegativeStock ?? false,
      defaultPaymentMethod: data?.defaultPaymentMethod ?? '',
      enableInvoiceAutoGeneration: data?.enableInvoiceAutoGeneration ?? true,
      enableReceiptPrintAfterSale: data?.enableReceiptPrintAfterSale ?? false,
      requireCustomerForCreditSale: data?.requireCustomerForCreditSale ?? true,
    },
  })

  if (query.isLoading) return <OpsTabSkeleton />

  return (
    <OpsCard
      description="Reglas operativas del punto de venta y generacion de comprobantes."
      icon={<DollarSign size={18} />}
      title="Configuracion de ventas"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) =>
          mutation.mutate({
            allowDiscounts: values.allowDiscounts,
            allowNegativeStock: values.allowNegativeStock,
            defaultPaymentMethod: emptyToNull(values.defaultPaymentMethod),
            enableInvoiceAutoGeneration: values.enableInvoiceAutoGeneration,
            enableReceiptPrintAfterSale: values.enableReceiptPrintAfterSale,
            requireCustomerForCreditSale: values.requireCustomerForCreditSale,
          }),
        )}
      >
        <div className="space-y-3">
          <OpsCheckboxRow description="Permite realizar ventas aunque no haya existencia suficiente." label="Permitir stock negativo">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('allowNegativeStock')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="Los cajeros pueden aplicar descuentos en los items de venta." label="Permitir descuentos">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('allowDiscounts')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="Exige seleccionar un cliente antes de registrar una venta a credito." label="Requerir cliente en venta a credito">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('requireCustomerForCreditSale')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="Imprime el recibo automaticamente al completar una venta." label="Imprimir recibo al finalizar venta">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('enableReceiptPrintAfterSale')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="Genera la factura fiscal de forma automatica al cerrar la venta." label="Generacion automatica de facturas">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('enableInvoiceAutoGeneration')}
            />
          </OpsCheckboxRow>
        </div>
        <OpsField
          error={form.formState.errors.defaultPaymentMethod?.message}
          label="Metodo de pago predeterminado"
        >
          <Controller
            control={form.control}
            name="defaultPaymentMethod"
            render={({ field }) => (
              <Select value={field.value ?? '_'} onValueChange={(v) => field.onChange(v === '_' ? '' : v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="_">- Sin predeterminado -</SelectItem>
                  <SelectItem value="Efectivo">Efectivo</SelectItem>
                  <SelectItem value="Tarjeta">Tarjeta</SelectItem>
                  <SelectItem value="Transferencia">Transferencia</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
        </OpsField>
        <OpsFormFooter mutation={mutation} />
      </form>
    </OpsCard>
  )
}

// ── Inventory settings tab ────────────────────────────────────────────────────

function InventorySettingsTab() {
  const query = useInventorySettingsQuery()
  const mutation = useUpdateInventorySettingsMutation()
  const data: InventorySettingsData | undefined = query.data

  const form = useForm<InventorySettingsForm>({
    resolver: zodResolver(inventorySettingsSchema),
    values: {
      allowInventoryTransferBetweenBranches: data?.allowInventoryTransferBetweenBranches ?? false,
      defaultLowStockThreshold: data?.defaultLowStockThreshold ?? 5,
      enableLowStockAlerts: data?.enableLowStockAlerts ?? true,
      requireReasonForInventoryAdjustment: data?.requireReasonForInventoryAdjustment ?? false,
    },
  })

  if (query.isLoading) return <OpsTabSkeleton />

  return (
    <OpsCard
      description="Alertas de stock, ajustes y transferencias entre sucursales."
      icon={<Package size={18} />}
      title="Configuracion de inventario"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) =>
          mutation.mutate({
            allowInventoryTransferBetweenBranches: values.allowInventoryTransferBetweenBranches,
            defaultLowStockThreshold: values.defaultLowStockThreshold,
            enableLowStockAlerts: values.enableLowStockAlerts,
            requireReasonForInventoryAdjustment: values.requireReasonForInventoryAdjustment,
          }),
        )}
      >
        <div className="space-y-3">
          <OpsCheckboxRow description="Envia notificaciones cuando el stock baja del umbral configurado." label="Alertas de stock bajo">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('enableLowStockAlerts')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="El operador debe indicar el motivo al realizar un ajuste manual." label="Requerir motivo en ajustes de inventario">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('requireReasonForInventoryAdjustment')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="Permite mover productos entre las distintas sucursales del negocio." label="Permitir transferencias entre sucursales">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('allowInventoryTransferBetweenBranches')}
            />
          </OpsCheckboxRow>
        </div>
        <OpsField
          error={form.formState.errors.defaultLowStockThreshold?.message}
          label="Umbral de stock bajo predeterminado *"
        >
          <input
            className={inputCls}
            step={1}
            type="number"
            {...form.register('defaultLowStockThreshold', { valueAsNumber: true })}
          />
        </OpsField>
        <OpsFormFooter mutation={mutation} />
      </form>
    </OpsCard>
  )
}

// ── Billing settings tab ──────────────────────────────────────────────────────

function BillingSettingsTab() {
  const query = useBillingSettingsQuery()
  const mutation = useUpdateBillingSettingsMutation()
  const data: BillingSettingsData | undefined = query.data

  const form = useForm<BillingSettingsForm>({
    resolver: zodResolver(billingSettingsSchema),
    values: {
      enableInvoiceAutoGeneration: data?.enableInvoiceAutoGeneration ?? true,
      invoicePrefix: data?.invoicePrefix ?? 'RI',
      invoiceSequenceStart: data?.invoiceSequenceStart ?? 1,
      receiptFooterText: data?.receiptFooterText ?? '',
      receiptHeaderText: data?.receiptHeaderText ?? '',
      showLogoOnReceipt: data?.showLogoOnReceipt ?? false,
      showRncOnReceipt: data?.showRncOnReceipt ?? false,
    },
  })

  if (query.isLoading) return <OpsTabSkeleton />

  return (
    <OpsCard
      description="Prefijos de factura, secuencias y contenido del recibo impreso."
      icon={<Receipt size={18} />}
      title="Configuracion de facturacion"
    >
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit((values) =>
          mutation.mutate({
            enableInvoiceAutoGeneration: values.enableInvoiceAutoGeneration,
            invoicePrefix: values.invoicePrefix,
            invoiceSequenceStart: values.invoiceSequenceStart,
            receiptFooterText: emptyToNull(values.receiptFooterText),
            receiptHeaderText: emptyToNull(values.receiptHeaderText),
            showLogoOnReceipt: values.showLogoOnReceipt,
            showRncOnReceipt: values.showRncOnReceipt,
          }),
        )}
      >
        <div className="grid gap-4 lg:grid-cols-2">
          <OpsField error={form.formState.errors.invoicePrefix?.message} label="Prefijo de factura *">
            <input
              className={`${inputCls} uppercase`}
              maxLength={10}
              {...form.register('invoicePrefix')}
            />
          </OpsField>
          <OpsField
            error={form.formState.errors.invoiceSequenceStart?.message}
            label="Inicio de secuencia *"
          >
            <input
              className={inputCls}
              step={1}
              type="number"
              {...form.register('invoiceSequenceStart', { valueAsNumber: true })}
            />
          </OpsField>
        </div>
        <div className="space-y-3">
          <OpsCheckboxRow description="Genera la factura fiscal automaticamente al completar la venta." label="Generacion automatica de facturas">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('enableInvoiceAutoGeneration')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="Incluye el logo del negocio en el recibo impreso." label="Mostrar logo en recibo">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('showLogoOnReceipt')}
            />
          </OpsCheckboxRow>
          <OpsCheckboxRow description="Imprime el RNC / identificacion fiscal en el recibo." label="Mostrar RNC en recibo">
            <input
              className={checkboxCls}
              type="checkbox"
              {...form.register('showRncOnReceipt')}
            />
          </OpsCheckboxRow>
        </div>
        <OpsField
          error={form.formState.errors.receiptHeaderText?.message}
          label="Encabezado del recibo"
        >
          <textarea
            className={`${inputCls} h-16 resize-none py-2`}
            placeholder="Texto que aparecera en la parte superior del recibo..."
            {...form.register('receiptHeaderText')}
          />
        </OpsField>
        <OpsField error={form.formState.errors.receiptFooterText?.message} label="Pie del recibo">
          <textarea
            className={`${inputCls} h-16 resize-none py-2`}
            placeholder="Texto que aparecera en la parte inferior del recibo..."
            {...form.register('receiptFooterText')}
          />
        </OpsField>
        <OpsFormFooter mutation={mutation} />
      </form>
    </OpsCard>
  )
}

// ── Shared sub-components ─────────────────────────────────────────────────────

function OpsCard({
  children,
  description,
  icon,
  title,
}: Readonly<{
  children: ReactNode
  description: string
  icon: ReactNode
  title: string
}>) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-start gap-3">
        <span className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-stone-100 text-stone-800 ring-1 ring-stone-200">
          {icon}
        </span>
        <div>
          <h3 className="text-base font-semibold text-stone-950">{title}</h3>
          <p className="mt-1 text-sm font-medium text-stone-600">{description}</p>
        </div>
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  )
}

function OpsField({
  children,
  error,
  label,
}: Readonly<{
  children: ReactNode
  error?: string
  label: string
}>) {
  return (
    <label className="block space-y-1.5">
      <span className="text-sm font-semibold text-stone-800">{label}</span>
      {children}
      {error && <span className="block text-sm font-semibold text-red-700">{error}</span>}
    </label>
  )
}

function OpsCheckboxRow({
  children,
  description,
  label,
}: Readonly<{
  children: ReactNode
  description: string
  label: string
}>) {
  return (
    <label className="flex cursor-pointer items-start gap-3 rounded-md border border-stone-200 bg-stone-50 px-4 py-3 hover:bg-stone-100">
      {children}
      <div>
        <p className="text-sm font-semibold text-stone-900">{label}</p>
        <p className="text-xs font-medium text-stone-500">{description}</p>
      </div>
    </label>
  )
}

function OpsFormFooter({
  mutation,
}: Readonly<{
  mutation: { error: Error | null; isPending: boolean; isSuccess: boolean }
}>) {
  return (
    <div className="flex flex-col gap-3 border-t border-stone-200 pt-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-h-5">
        {mutation.error && (
          <p className="text-sm font-semibold text-red-700">{toErrorMessage(mutation.error)}</p>
        )}
        {mutation.isSuccess && (
          <p className="flex items-center gap-2 text-sm font-semibold text-emerald-700">
            <ShieldCheck size={15} />
            Configuracion guardada correctamente.
          </p>
        )}
      </div>
      <Button disabled={mutation.isPending} type="submit">
        {mutation.isPending ? 'Guardando...' : 'Guardar cambios'}
      </Button>
    </div>
  )
}

function OpsTabSkeleton() {
  const skeletonRows = ['row-1', 'row-2', 'row-3']
  return (
    <Card>
      <CardContent className="space-y-4 p-6">
        {skeletonRows.map((id) => (
          <div className="h-10 rounded bg-stone-100" key={id} />
        ))}
      </CardContent>
    </Card>
  )
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function emptyToNull(value: string | undefined): string | null {
  return value?.trim() ? value.trim() : null
}

function toErrorMessage(error: Error): string {
  if (error instanceof HttpClientError) {
    return error.error?.message ?? error.message
  }
  return error.message
}

const inputCls =
  'h-10 w-full rounded-md bg-white px-3 text-sm font-semibold text-stone-900 shadow-sm ring-1 ring-stone-300 outline-none placeholder:text-stone-400 focus:ring-2 focus:ring-stone-900/20'

const checkboxCls = 'mt-0.5 h-4 w-4 cursor-pointer rounded accent-stone-900'
