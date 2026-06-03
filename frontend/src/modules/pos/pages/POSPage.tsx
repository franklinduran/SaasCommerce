import { useCallback, useEffect, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, Wallet } from 'lucide-react'
import { Link } from 'react-router-dom'
import { z } from 'zod'
import { useCurrentCashSession } from '@/modules/cash/hooks/useCash'
import { CustomerSelector } from '@/modules/pos/components/CustomerSelector'
import { PaymentMethodSelector } from '@/modules/pos/components/PaymentMethodSelector'
import { POSCart } from '@/modules/pos/components/POSCart'
import { ProductGrid } from '@/modules/pos/components/ProductGrid'
import { ProductSearch } from '@/modules/pos/components/ProductSearch'
import { SaleStatusPanel } from '@/modules/pos/components/SaleStatusPanel'
import { SaleSummary } from '@/modules/pos/components/SaleSummary'
import { useCreateSaleMutation } from '@/modules/pos/hooks/useCreateSale'
import { useCurrentBranchForPOS } from '@/modules/pos/hooks/useCurrentBranchForPOS'
import { useCustomersForPOS } from '@/modules/pos/hooks/useCustomersForPOS'
import { usePOSCart } from '@/modules/pos/hooks/usePOSCart'
import { useProductsForPOS } from '@/modules/pos/hooks/useProductsForPOS'
import { useSaleStatusSync } from '@/modules/pos/hooks/useSaleStatusSync'
import { useSaleStatusSubscription } from '@/modules/pos/hooks/useSaleStatusSubscription'
import { createSaleSchema } from '@/modules/pos/services/salesApi'
import type {
  CreateSaleRequest,
  PaymentMethod,
  POSProduct,
  SaleResponse,
  SaleStatus,
  SaleStatusChangedNotification,
} from '@/modules/pos/types/posTypes'
import { useAuthStore } from '@/modules/auth/authStore'
import { HttpClientError } from '@/shared/services/httpClient'
import { offRealtimeEvent, onRealtimeEvent } from '@/shared/services/signalrClient'

type CurrentSale = {
  reason: string | null
  saleId: string
  status: SaleStatus
  total: number | null
}

export function POSPage() {
  const session = useAuthStore((state) => state.session)
  const currentBranch = useCurrentBranchForPOS(Boolean(session && !session.user.branchId))
  const branchId = session?.user.branchId ?? currentBranch.data?.branchId ?? null
  const { data: cashSession, isLoading: cashLoading } = useCurrentCashSession()
  const hasOpenCashSession = !cashLoading && cashSession !== null && cashSession !== undefined
  const queryClient = useQueryClient()
  const [productQuery, setProductQuery] = useState('')
  const [customerQuery, setCustomerQuery] = useState('')
  const [selectedCustomerId, setSelectedCustomerId] = useState<string | null>(null)
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Cash')
  const [validationMessage, setValidationMessage] = useState<string | null>(null)
  const [saleErrorMessage, setSaleErrorMessage] = useState<string | null>(null)
  const [currentSale, setCurrentSale] = useState<CurrentSale | null>(null)
  const products = useProductsForPOS(productQuery)
  const customers = useCustomersForPOS(customerQuery)
  const createSaleMutation = useCreateSaleMutation()
  const cart = usePOSCart()
  const clearCart = cart.clearCart

  const applySaleStatus = useCallback(
    (payload: SaleStatusChangedNotification | SaleResponse) => {
      const reason =
        'reason' in payload ? payload.reason : payload.failureReason ?? payload.cancellationReason

      setCurrentSale((current) => {
        const total = getSaleTotal(payload, current)

        return {
          reason,
          saleId: payload.saleId,
          status: payload.status,
          total,
        }
      })

      if (payload.status === 'Completed') {
        clearCart()
        setSaleErrorMessage(null)
        queryClient.invalidateQueries({ queryKey: ['pos-products'] })
      }

      if (payload.status === 'Failed') {
        setSaleErrorMessage(reason ?? 'La venta fallo durante el procesamiento.')
      }
    },
    [clearCart, queryClient],
  )

  useSaleStatusSubscription({
    onStatusChanged: applySaleStatus,
    saleId: currentSale?.saleId ?? null,
  })

  useSaleStatusSync({
    enabled: Boolean(currentSale && !isTerminalSaleStatus(currentSale.status)),
    onSaleLoaded: applySaleStatus,
    saleId: currentSale?.saleId ?? null,
  })

  useEffect(() => {
    if (!session?.user.businessId) {
      return undefined
    }

    const handler = (payload: { businessId?: string }) => {
      if (payload.businessId === session.user.businessId) {
        queryClient.invalidateQueries({ queryKey: ['pos-products'] })
      }
    }

    onRealtimeEvent('inventory.updated', handler)

    return () => {
      offRealtimeEvent('inventory.updated', handler)
    }
  }, [queryClient, session?.user.businessId])

  function handleAddProduct(product: POSProduct) {
    cart.addItem({
      name: product.name,
      productId: product.id,
      quantity: 1,
      sku: product.sku,
      unitPrice: product.salePrice,
    })
    if (validationMessage === 'El carrito esta vacio.') {
      setValidationMessage(null)
    }
    setSaleErrorMessage(null)
  }

  async function handleProcessSale() {
    setValidationMessage(null)
    setSaleErrorMessage(null)

    if (!branchId) {
      setValidationMessage('No hay sucursal activa para procesar la venta.')
      return
    }

    const request: CreateSaleRequest = {
      branchId,
      customerId: selectedCustomerId,
      items: cart.items.map((item) => ({
        productId: item.productId,
        quantity: item.quantity,
      })),
      paymentMethod,
    }

    const selectedCustomer = customersForPOS.find((customer) => customer.id === selectedCustomerId)

    if (paymentMethod === 'Credit' && !selectedCustomer) {
      setValidationMessage('Selecciona un cliente para vender fiado.')
      return
    }

    if (paymentMethod === 'Credit' && selectedCustomer?.creditStatus === 'Blocked') {
      setValidationMessage('El cliente tiene el credito bloqueado.')
      return
    }

    if (
      paymentMethod === 'Credit' &&
      selectedCustomer &&
      (selectedCustomer.creditLimit ?? 0) > 0 &&
      (selectedCustomer.currentBalance ?? 0) + cart.subtotal > (selectedCustomer.creditLimit ?? 0)
    ) {
      setValidationMessage('La venta supera el limite de credito del cliente.')
      return
    }

    const validation = createSaleSchema.safeParse(request)

    if (!validation.success) {
      setValidationMessage(getValidationMessage(validation.error))
      return
    }

    try {
      const sale = await createSaleMutation.mutateAsync(request)

      setCurrentSale({
        reason: sale.failureReason ?? sale.cancellationReason,
        saleId: sale.saleId,
        status: sale.status,
        total: sale.total,
      })
    } catch (error) {
      setSaleErrorMessage(getSaleErrorMessage(error))
    }
  }

  const panelStatus = createSaleMutation.isPending
    ? 'Submitting'
    : currentSale?.status ?? 'Idle'
  const productsForPOS = products.data?.items ?? []
  const customersForPOS = customers.data?.items ?? []

  return (
    <section className="min-h-full bg-background p-4 sm:p-5 lg:p-6">
      <div className="mx-auto grid w-full max-w-[1680px] min-w-0 gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(360px,420px)] xl:gap-5">
        <div className="space-y-5">
          <div className="flex min-w-0 flex-col justify-between gap-3 rounded-2xl border border-border bg-card px-5 py-5 shadow-sm sm:flex-row sm:items-center">
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                Punto de Venta
              </p>
              <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">POS</h1>
              <p className="mt-1 text-[13px] text-muted-foreground">
                {session?.user.fullName ?? 'Usuario'} · {branchId ? 'Sucursal activa' : 'Sin sucursal'}
              </p>
            </div>
            <div className="shrink-0 rounded-xl border border-border bg-muted px-4 py-2 text-[13px] font-semibold text-foreground">
              {cart.itemCount} art. · {formatMoney(cart.subtotal)}
            </div>
          </div>

          <ProductSearch
            isFetching={products.isFetching}
            onQueryChange={setProductQuery}
            onRefresh={() => products.refetch()}
            query={productQuery}
          />

          <ProductGrid
            isError={products.isError}
            isLoading={products.isLoading}
            onAddProduct={handleAddProduct}
            onRetry={() => products.refetch()}
            products={productsForPOS}
          />
        </div>

        <aside className="space-y-5 xl:sticky xl:top-5 xl:self-start">
          <POSCart
            itemCount={cart.itemCount}
            items={cart.items}
            onDecrease={cart.decreaseQuantity}
            onIncrease={cart.increaseQuantity}
            onRemove={cart.removeItem}
            subtotal={cart.subtotal}
          />

          <CustomerSelector
            customers={customersForPOS}
            isCreditPayment={paymentMethod === 'Credit'}
            isError={customers.isError}
            isLoading={customers.isLoading}
            onCustomerChange={setSelectedCustomerId}
            onQueryChange={setCustomerQuery}
            query={customerQuery}
            selectedCustomerId={selectedCustomerId}
          />

          <PaymentMethodSelector onChange={setPaymentMethod} value={paymentMethod} />

          {!cashLoading && !hasOpenCashSession && <NoCashSessionBanner />}

          <SaleSummary
            disabled={createSaleMutation.isPending || !branchId || !hasOpenCashSession}
            isSubmitting={createSaleMutation.isPending}
            itemCount={cart.itemCount}
            onProcessSale={handleProcessSale}
            subtotal={cart.subtotal}
            validationMessage={validationMessage}
          />

          <SaleStatusPanel
            errorMessage={saleErrorMessage}
            reason={currentSale?.reason}
            saleId={currentSale?.saleId ?? null}
            status={panelStatus}
            total={currentSale?.total}
          />
        </aside>
      </div>
    </section>
  )
}

function getValidationMessage(error: z.ZodError<CreateSaleRequest>) {
  const issue = error.issues[0]

  if (issue?.path[0] === 'items') {
    return 'El carrito esta vacio.'
  }

  if (issue?.path[0] === 'branchId') {
    return 'No hay sucursal activa para procesar la venta.'
  }

  return 'Revisa los datos de la venta.'
}

// Fixed Spanish messages — never overridden by API (predictable UX)
const SALE_ERROR_MESSAGES: Record<string, string> = {
  CASH_SESSION_NOT_OPEN: 'No hay caja abierta. Ve a Caja y abre una sesion primero.',
  NO_OPEN_CASH_SESSION: 'No hay caja abierta. Ve a Caja y abre una sesion primero.',
  SUBSCRIPTION_EXPIRED: 'Tu suscripcion ha expirado. Renovela para continuar vendiendo.',
  SUBSCRIPTION_SUSPENDED: 'Tu cuenta esta suspendida. Contacta a soporte.',
}

function getSaleErrorMessage(error: unknown): string {
  if (!(error instanceof HttpClientError)) return 'No se pudo crear la venta.'

  const { status } = error
  const code = error.error?.code ?? ''

  if (status === 401) return 'La sesion expiro. Inicia sesion nuevamente.'
  if (status === 403) return 'Sin permiso para esta operacion. Verifica tu suscripcion o permisos.'
  if (status === 404) return 'Uno de los productos no esta disponible.'
  if (code in SALE_ERROR_MESSAGES) return SALE_ERROR_MESSAGES[code]
  if (code === 'SUBSCRIPTION_LIMIT_REACHED') return error.error?.message ?? 'Has alcanzado el limite de tu plan.'
  if (status === 400 || status === 422) return error.error?.message ?? error.message ?? 'Los datos de la venta no son validos.'

  return error.message ?? 'No se pudo crear la venta.'
}

function isTerminalSaleStatus(status: SaleStatus) {
  return status === 'Completed' || status === 'Failed' || status === 'Cancelled'
}

function getSaleTotal(
  payload: SaleStatusChangedNotification | SaleResponse,
  current: CurrentSale | null,
) {
  if ('total' in payload) {
    return payload.total
  }

  if (current?.saleId === payload.saleId) {
    return current.total
  }

  return null
}

function NoCashSessionBanner() {
  return (
    <div className="flex items-start gap-3 rounded-2xl border border-amber-200 bg-amber-50 p-4">
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-amber-100">
        <AlertTriangle aria-hidden="true" className="text-amber-700" size={15} strokeWidth={2} />
      </div>
      <div className="min-w-0 flex-1">
        <p className="text-[13.5px] font-semibold text-amber-900">Caja cerrada</p>
        <p className="mt-0.5 text-[12.5px] text-amber-700">
          No hay sesion de caja abierta. Debes abrir una caja antes de registrar ventas.
        </p>
        <Link
          className="mt-2 inline-flex h-7 items-center gap-1.5 rounded-lg border border-amber-300 bg-white px-3 text-[12px] font-medium text-amber-800 transition-colors hover:bg-amber-50"
          to="/cash"
        >
          <Wallet aria-hidden="true" size={12} />
          Ir a Caja
        </Link>
      </div>
    </div>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
