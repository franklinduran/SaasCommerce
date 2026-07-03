import { useState } from 'react'
import type { ReactNode } from 'react'
import { CashRegisterPage } from '@/modules/cash-register/pages/CashRegisterPage'
import { DailyCashRegisterSummaryPage } from '@/modules/cash-register/pages/DailyCashRegisterSummaryPage'
import { useHasPermission } from '@/shared/hooks/usePermissions'
import { Permission } from '@/shared/types/permissions'
import { cn } from '@/shared/utils/cn'

type ArqueoTab = 'avanzado' | 'diario'

// ─── Tab button ────────────────────────────────────────────────────────────────

type TabButtonProps = {
  active: boolean
  onClick: () => void
  children: ReactNode
}

function TabButton({ active, onClick, children }: Readonly<TabButtonProps>) {
  return (
    <button
      className={cn(
        'relative -mb-px border-b-2 px-1 py-2.5 text-[13.5px] font-semibold transition-colors',
        active
          ? 'border-gray-900 text-gray-900'
          : 'border-transparent text-gray-400 hover:text-gray-600',
      )}
      type="button"
      onClick={onClick}
    >
      {children}
    </button>
  )
}

// ─── Page ────────────────────────────────────────────────────────────────────

export function ArqueoPage() {
  const canViewDaily = useHasPermission(Permission.CashRegisterDailySummary)
  const [tab, setTab] = useState<ArqueoTab>('avanzado')

  const showDaily = tab === 'diario' && canViewDaily

  return (
    <div className="flex flex-col gap-5 overflow-y-auto p-6 lg:p-8">
      {/* Header */}
      <header>
        <p className="text-[11px] font-semibold uppercase tracking-[0.15em] text-muted-foreground">Caja</p>
        <h1 className="mt-1 text-2xl font-bold tracking-tight text-foreground">Arqueo</h1>
        <p className="mt-1 text-[13.5px] text-muted-foreground">
          Gestiona el turno activo y consulta el arqueo diario de cajas.
        </p>
      </header>

      {/* Tabs */}
      <div className="flex gap-6 border-b border-gray-200">
        <TabButton active={tab === 'avanzado'} onClick={() => setTab('avanzado')}>
          Caja del turno
        </TabButton>
        {canViewDaily && (
          <TabButton active={tab === 'diario'} onClick={() => setTab('diario')}>
            Arqueo diario
          </TabButton>
        )}
      </div>

      {/* Content */}
      {showDaily ? <DailyCashRegisterSummaryPage embedded /> : <CashRegisterPage embedded />}
    </div>
  )
}
