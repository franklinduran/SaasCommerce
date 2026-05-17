import { ArrowLeft, Ban, CreditCard, RotateCcw, WalletCards } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { useState } from 'react'
import { CustomerCreditMovementsTable } from '@/modules/customers/components/CustomerCreditMovementsTable'
import { CustomerCreditSummary } from '@/modules/customers/components/CustomerCreditSummary'
import { CustomerForm } from '@/modules/customers/components/CustomerForm'
import { RegisterCustomerPaymentDialog } from '@/modules/customers/components/RegisterCustomerPaymentDialog'
import {
  useBlockCustomerCredit,
  useCustomerCredit,
  useCustomerCreditMovements,
  useUnblockCustomerCredit,
} from '@/modules/customers/hooks/useCustomerCredit'
import { useCustomerDetail } from '@/modules/customers/hooks/useCustomerDetail'
import {
  useCustomerCreditInvalidation,
  useDeactivateCustomer,
  useUpdateCustomer,
} from '@/modules/customers/hooks/useCustomers'
import { useRegisterCustomerPayment } from '@/modules/customers/hooks/useRegisterCustomerPayment'
import { Button } from '@/shared/components/ui/button'
import { Card, CardHeader } from '@/shared/components/ui/card'

export function CustomerDetailPage() {
  const { customerId } = useParams<{ customerId: string }>()
  const [isPaymentOpen, setIsPaymentOpen] = useState(false)
  const customer = useCustomerDetail(customerId)
  const credit = useCustomerCredit(customerId)
  const movements = useCustomerCreditMovements(customerId)
  const updateCustomer = useUpdateCustomer(customerId ?? '')
  const deactivateCustomer = useDeactivateCustomer()
  const registerPayment = useRegisterCustomerPayment(customerId ?? '')
  const blockCredit = useBlockCustomerCredit(customerId ?? '')
  const unblockCredit = useUnblockCustomerCredit(customerId ?? '')

  useCustomerCreditInvalidation(customerId)

  if (!customerId) {
    return null
  }

  const summary = credit.data
  const isBlocked = summary?.status === 'Blocked'

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <Link className="inline-flex items-center gap-2 text-sm font-semibold text-stone-600 hover:text-stone-950" to="/customers">
            <ArrowLeft size={16} />
            Clientes
          </Link>
          <h2 className="mt-2 text-2xl font-semibold text-stone-950">
            {customer.data?.fullName ?? 'Cliente'}
          </h2>
          <p className="mt-2 text-sm font-medium text-stone-600">
            {customer.data?.phone ?? 'Sin telefono'} | {customer.data?.email ?? 'Sin email'}
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button onClick={() => setIsPaymentOpen(true)} type="button">
            <WalletCards size={16} />
            Registrar abono
          </Button>
          <Button
            disabled={blockCredit.isPending || unblockCredit.isPending}
            onClick={() => {
              if (isBlocked) {
                unblockCredit.mutate()
              } else {
                blockCredit.mutate()
              }
            }}
            type="button"
            variant="secondary"
          >
            {isBlocked ? <RotateCcw size={16} /> : <Ban size={16} />}
            {isBlocked ? 'Desbloquear' : 'Bloquear'}
          </Button>
        </div>
      </div>

      <CustomerCreditSummary isLoading={credit.isLoading} summary={summary} />

      <Card>
        <CardHeader>
          <div className="mb-4 flex items-center gap-2">
            <CreditCard className="text-stone-500" size={18} />
            <h3 className="text-base font-semibold text-stone-950">Datos del cliente</h3>
          </div>
          <CustomerForm
            customer={customer.data}
            isSubmitting={updateCustomer.isPending}
            onSubmit={(request) => updateCustomer.mutate(request)}
          />
          <div className="mt-4">
            <Button
              disabled={deactivateCustomer.isPending || !customer.data?.isActive}
              onClick={() => deactivateCustomer.mutate(customerId)}
              type="button"
              variant="ghost"
            >
              Desactivar cliente
            </Button>
          </div>
        </CardHeader>
      </Card>

      <Card className="overflow-hidden">
        <CardHeader className="border-b border-stone-200">
          <h3 className="text-base font-semibold text-stone-950">Historial de credito</h3>
          <p className="mt-1 text-sm font-medium text-stone-600">{movements.data?.totalItems ?? 0} movimientos</p>
        </CardHeader>
        <CustomerCreditMovementsTable
          isError={movements.isError}
          isLoading={movements.isLoading}
          movements={movements.data?.items ?? []}
        />
      </Card>

      <RegisterCustomerPaymentDialog
        isOpen={isPaymentOpen}
        isSubmitting={registerPayment.isPending}
        onClose={() => setIsPaymentOpen(false)}
        onSubmit={(amount, note) => {
          registerPayment.mutate(
            { amount, customerId, note },
            { onSuccess: () => setIsPaymentOpen(false) },
          )
        }}
      />
    </section>
  )
}
