import { Banknote, Clock, ExternalLink, Loader2 } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useCurrentCashSession } from '@/modules/cash/hooks/useCash'
import { cn } from '@/shared/utils/cn'

function formatMoney(amount: number) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(amount)
}

function formatTime(dateString: string) {
  return new Intl.DateTimeFormat('es-DO', { hour: '2-digit', minute: '2-digit' }).format(new Date(dateString))
}

export function CashSessionWidget() {
  const { data: session, isLoading } = useCurrentCashSession()

  return (
    <div>
      <div className="flex items-center gap-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
          Caja
        </p>
        <div className="h-px flex-1 bg-border" />
        <Link
          className="flex items-center gap-1 text-[11px] font-medium text-muted-foreground hover:text-foreground"
          to="/cash"
        >
          <ExternalLink size={10} />
          Gestionar
        </Link>
      </div>

      <div className="mt-3">
        {isLoading ? (
          <div className="flex items-center gap-2 text-[13px] text-muted-foreground">
            <Loader2 className="animate-spin" size={14} />
            Cargando...
          </div>
        ) : session ? (
          <OpenSession
            openedAt={session.openedAt}
            openingBalance={session.openingBalance}
            systemBalance={session.systemBalance}
          />
        ) : (
          <ClosedSession />
        )}
      </div>
    </div>
  )
}

type OpenSessionProps = {
  openedAt: string
  openingBalance: number
  systemBalance: number
}

function OpenSession({ openedAt, openingBalance, systemBalance }: Readonly<OpenSessionProps>) {
  return (
    <div className="space-y-2.5">
      <div className="flex items-center justify-between gap-2">
        <span
          className={cn(
            'inline-flex items-center gap-1.5 rounded-full px-2 py-0.5',
            'bg-emerald-50 text-[11.5px] font-semibold text-emerald-700 ring-1 ring-emerald-200',
          )}
        >
          <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
          Abierta
        </span>
        <span className="flex items-center gap-1 text-[11.5px] text-muted-foreground">
          <Clock size={11} />
          {formatTime(openedAt)}
        </span>
      </div>

      <div className="rounded-lg border border-border bg-muted/30 px-3 py-2.5 space-y-1.5">
        <div className="flex items-center justify-between gap-2">
          <span className="text-[12px] text-muted-foreground">Saldo sistema</span>
          <span className="text-[13px] font-bold tabular-nums text-foreground">
            {formatMoney(systemBalance)}
          </span>
        </div>
        <div className="flex items-center justify-between gap-2">
          <span className="text-[12px] text-muted-foreground">Apertura</span>
          <span className="text-[12px] tabular-nums text-muted-foreground">
            {formatMoney(openingBalance)}
          </span>
        </div>
      </div>
    </div>
  )
}

function ClosedSession() {
  return (
    <div className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2.5">
      <div className="flex items-start gap-2.5">
        <Banknote className="mt-0.5 shrink-0 text-amber-600" size={14} />
        <div className="min-w-0">
          <p className="text-[12.5px] font-semibold text-amber-900">Caja cerrada</p>
          <p className="mt-0.5 text-[12px] text-amber-700">
            Abre una sesión para registrar ventas.
          </p>
          <Link
            className="mt-1.5 inline-flex items-center gap-1 text-[12px] font-semibold text-amber-700 hover:text-amber-900"
            to="/cash"
          >
            <ExternalLink size={10} />
            Ir a Caja
          </Link>
        </div>
      </div>
    </div>
  )
}
