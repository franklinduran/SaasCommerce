import { CreditCard, ShieldAlert, WalletCards } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { CreditStatusBadge } from '@/modules/customers/components/CustomerTable'
import type { CustomerCreditSummary as Summary } from '@/modules/customers/types'
import { Card } from '@/shared/components/ui/card'

type CustomerCreditSummaryProps = {
  summary?: Summary
  isLoading: boolean
}

export function CustomerCreditSummary({ summary, isLoading }: Readonly<CustomerCreditSummaryProps>) {
  if (isLoading || !summary) {
    return (
      <div className="grid gap-4 md:grid-cols-3">
        <Metric icon={WalletCards} label="Balance pendiente" value="Cargando..." />
        <Metric icon={CreditCard} label="Limite" value="Cargando..." />
        <Metric icon={ShieldAlert} label="Estado" value="Cargando..." />
      </div>
    )
  }

  return (
    <div className="grid gap-4 md:grid-cols-3">
      <Metric icon={WalletCards} label="Balance pendiente" value={formatMoney(summary.currentBalance)} />
      <Metric
        icon={CreditCard}
        label="Limite"
        value={summary.creditLimit === 0 ? 'Sin limite' : formatMoney(summary.creditLimit)}
      />
      <Card className="p-4">
        <p className="flex items-center gap-2 text-xs font-semibold uppercase text-stone-500">
          <ShieldAlert size={16} />
          Estado
        </p>
        <div className="mt-3">
          <CreditStatusBadge status={summary.status} />
        </div>
      </Card>
    </div>
  )
}

function Metric({
  icon: Icon,
  label,
  value,
}: Readonly<{ icon: LucideIcon; label: string; value: string }>) {
  return (
    <Card className="p-4">
      <p className="flex items-center gap-2 text-xs font-semibold uppercase text-stone-500">
        <Icon size={16} />
        {label}
      </p>
      <p className="mt-2 text-xl font-semibold text-stone-950">{value}</p>
    </Card>
  )
}

function formatMoney(value: number) {
  return `RD$ ${value.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`
}
