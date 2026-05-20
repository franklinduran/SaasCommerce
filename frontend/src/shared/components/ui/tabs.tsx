import type { ReactNode } from 'react'
import { createContext, useContext, useMemo, useState } from 'react'

// ── Context ──────────────────────────────────────────────────────────────────

type TabsCtx = {
  activeTab: string
  setActiveTab: (tab: string) => void
}

const TabsContext = createContext<TabsCtx>({ activeTab: '', setActiveTab: () => undefined })

// ── Tabs root ─────────────────────────────────────────────────────────────────

export function Tabs({
  children,
  className = '',
  defaultValue = '',
  onValueChange,
  value,
}: Readonly<{
  children: ReactNode
  className?: string
  defaultValue?: string
  onValueChange?: (value: string) => void
  value?: string
}>) {
  const [internalTab, setInternalTab] = useState(defaultValue)
  const activeTab = value ?? internalTab

  function setActiveTab(tab: string) {
    setInternalTab(tab)
    onValueChange?.(tab)
  }

  const ctxValue = useMemo<TabsCtx>(() => ({ activeTab, setActiveTab }), [activeTab])

  return (
    <TabsContext.Provider value={ctxValue}>
      <div className={className}>{children}</div>
    </TabsContext.Provider>
  )
}

// ── TabsList ──────────────────────────────────────────────────────────────────

export function TabsList({
  children,
  className = '',
}: Readonly<{ children: ReactNode; className?: string }>) {
  return (
    <div
      className={`flex gap-1 rounded-lg bg-stone-100 p-1 ${className}`}
      role="tablist"
    >
      {children}
    </div>
  )
}

// ── TabsTrigger ───────────────────────────────────────────────────────────────

export function TabsTrigger({
  children,
  className = '',
  value,
}: Readonly<{ children: ReactNode; className?: string; value: string }>) {
  const { activeTab, setActiveTab } = useContext(TabsContext)
  const isActive = activeTab === value

  return (
    <button
      aria-selected={isActive}
      className={[
        'flex flex-1 items-center justify-center rounded-md px-3 py-2 text-sm font-semibold transition-colors',
        isActive
          ? 'bg-white text-stone-900 shadow-sm ring-1 ring-stone-200'
          : 'text-stone-600 hover:text-stone-900',
        className,
      ].join(' ')}
      onClick={() => setActiveTab(value)}
      role="tab"
      type="button"
    >
      {children}
    </button>
  )
}

// ── TabsContent ───────────────────────────────────────────────────────────────

export function TabsContent({
  children,
  className = '',
  value,
}: Readonly<{ children: ReactNode; className?: string; value: string }>) {
  const { activeTab } = useContext(TabsContext)

  if (activeTab !== value) return null

  return (
    <div className={className} role="tabpanel">
      {children}
    </div>
  )
}
