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
import { ProductSearch, type ProductViewMode } from '@/modules/pos/components/ProductSearch'
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
  const [productPage, setProductPage] = useState(1)
  const [customerQuery, setCustomerQuery] = useState('')
  const [selectedCustomerId, setSelectedCustomerId] = useState<string | null>(null)
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Cash')
  const [validationMessage, setValidationMessage] = useState<string | null>(null)
  const [saleErrorMessage, setSaleErrorMessage] = useState<string | null>(null)
  const [currentSale, setCurrentSale] = useState<CurrentSale | null>(null)
  const [productView, setProductView] = useState<ProductViewMode>(() => {
    try {
      const saved = localStorage.getItem('pos:productView')
      return saved === 'list' ? 'list' : 'grid'
    } catch {
      return 'grid'
    }
  })

  function handleViewChange(view: ProductViewMode) {
    setProductView(view)
    try { localStorage.setItem('pos:productView', view) } catch { /* ignore */ }
  }

  function handleQueryChange(query: string) {
    setProductQuery(query)
    setProductPage(1)
  }
  const products = useProductsForPOS(productQuery, productPage)
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

      // Don't apply the API status directly: the Worker may fire a spurious
      // "Failed / not in a processable state" event moments later (race condition).
      // Instead, record the saleId in Received state so SignalR or the polling
      // hook (useSaleStatusSync) delivers the authoritative final status.
      setCurrentSale({
        saleId: sale.saleId,
        status: 'Received',
        reason: null,
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
  const cartQuantities = new Map(cart.items.map((item) => [item.productId, item.quantity]))

  return (
    <div className="flex h-full bg-background">

      {/* ── Left: productos ── */}
      <div className="flex flex-1 flex-col gap-6 overflow-y-auto p-6 lg:p-8">

        {/* Header */}
        <header>
          <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">
            Punto de Venta
          </p>
          <h1 className="mt-1.5 text-2xl font-bold tracking-tight text-foreground">POS</h1>
          <p className="mt-1 text-[13.5px] text-muted-foreground">
            {session?.user.fullName ?? 'Usuario'} · {branchId ? 'Sucursal activa' : 'Sin sucursal'}
          </p>
        </header>

        {/* Buscador + toggle de vista */}
        <ProductSearch
          isFetching={products.isFetching}
          onQueryChange={handleQueryChange}
          onRefresh={() => products.refetch()}
          onViewChange={handleViewChange}
          query={productQuery}
          view={productView}
        />

        {/* Productos */}
        <ProductGrid
          cartQuantities={cartQuantities}
          isError={products.isError}
          isLoading={products.isLoading}
          onAddProduct={handleAddProduct}
          onNextPage={() => setProductPage((p) => p + 1)}
          onPrevPage={() => setProductPage((p) => Math.max(1, p - 1))}
          onRetry={() => products.refetch()}
          page={productPage}
          products={productsForPOS}
          totalPages={products.data?.totalPages ?? 1}
          view={productView}
        />
      </div>

      {/* ── Right: panel lateral (igual que el sidebar de la app) ── */}
      <aside className="hidden w-[360px] shrink-0 flex-col divide-y divide-gray-100 overflow-y-auto border-l border-gray-200 bg-white xl:flex">

          {/* Carrito */}
          <div className="px-5 py-5">
            <POSCart
              itemCount={cart.itemCount}
              items={cart.items}
              onDecrease={cart.decreaseQuantity}
              onIncrease={cart.increaseQuantity}
              onRemove={cart.removeItem}
              onSetQuantity={cart.setQuantity}
              subtotal={cart.subtotal}
            />
          </div>

          {/* Cliente */}
          <div className="px-5 py-5">
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
          </div>

          {/* Método de pago */}
          <div className="px-5 py-5">
            <PaymentMethodSelector onChange={setPaymentMethod} value={paymentMethod} />
          </div>

          {/* Alerta caja cerrada */}
          {!cashLoading && !hasOpenCashSession && (
            <div className="px-5 py-4">
              <NoCashSessionBanner />
            </div>
          )}

          {/* Resumen + CTA */}
          <div className="px-5 py-5">
            <SaleSummary
              disabled={createSaleMutation.isPending || !branchId || !hasOpenCashSession}
              isSubmitting={createSaleMutation.isPending}
              itemCount={cart.itemCount}
              onProcessSale={handleProcessSale}
              subtotal={cart.subtotal}
              validationMessage={validationMessage}
            />
          </div>

          {/* Estado de venta */}
          <div className="px-5 py-5">
            <SaleStatusPanel
              errorMessage={saleErrorMessage}
              reason={currentSale?.reason}
              saleId={currentSale?.saleId ?? null}
              status={panelStatus}
              total={currentSale?.total}
            />
          </div>
      </aside>
    </div>
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
    <div className="flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 p-3.5">
      <AlertTriangle aria-hidden="true" className="mt-0.5 shrink-0 text-amber-600" size={14} strokeWidth={2} />
      <div className="min-w-0 flex-1">
        <p className="text-[12.5px] font-semibold text-amber-900">Caja cerrada</p>
        <p className="mt-0.5 text-[12px] text-amber-700">
          Abre una sesión de caja para registrar ventas.
        </p>
        <Link
          className="mt-2 inline-flex items-center gap-1 text-[12px] font-semibold text-amber-700 hover:text-amber-900 focus-visible:outline-none"
          to="/cash"
        >
          <Wallet aria-hidden="true" size={11} />
          Ir a Caja
        </Link>
      </div>
    </div>
  )
}

