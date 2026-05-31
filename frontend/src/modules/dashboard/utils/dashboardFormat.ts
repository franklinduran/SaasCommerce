export function formatMoney(amount: number): string {
  return new Intl.NumberFormat('es-DO', {
    currency: 'DOP',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency',
  }).format(amount)
}

export function formatMoneyShort(amount: number): string {
  if (amount >= 1_000_000) return `RD$${(amount / 1_000_000).toFixed(1)}M`
  if (amount >= 1_000) return `RD$${(amount / 1_000).toFixed(1)}K`
  return `RD$${amount.toFixed(0)}`
}

export function formatDate(): string {
  return new Intl.DateTimeFormat('es-DO', {
    day: 'numeric',
    month: 'long',
    weekday: 'long',
    year: 'numeric',
  }).format(new Date())
}

import type { ChartDataPoint, DashboardDailySalesPoint, DashboardDailyPurchasesPoint } from '@/modules/dashboard/types'

/**
 * Builds unified chart data merging daily sales + daily purchases.
 * Fills any missing days with zero values so charts are always continuous.
 */
export function buildChartData(
  dailySales: DashboardDailySalesPoint[],
  dailyPurchases: DashboardDailyPurchasesPoint[],
  days: number,
): ChartDataPoint[] {
  const salesMap = new Map(dailySales.map((d) => [d.date, d]))
  const purchasesMap = new Map(dailyPurchases.map((d) => [d.date, d]))

  const result: ChartDataPoint[] = []

  for (let i = days - 1; i >= 0; i--) {
    const d = new Date()
    d.setDate(d.getDate() - i)
    const key = d.toISOString().slice(0, 10)

    const label = days <= 7
      ? new Intl.DateTimeFormat('es-DO', { weekday: 'short' }).format(d)
      : new Intl.DateTimeFormat('es-DO', { day: '2-digit', month: '2-digit' }).format(d)

    result.push({
      label,
      ventas: salesMap.get(key)?.totalSales ?? 0,
      facturas: 0,
      gastos: purchasesMap.get(key)?.totalPurchases ?? 0,
    })
  }

  return result
}
