namespace SaasCommerce.Modules.Sales.Application.Profitability;

/// <summary>
/// Pure, side-effect-free profitability formulas. All public members are testable without infrastructure.
/// </summary>
public static class ProfitabilityCalculator
{
  public static decimal GrossProfit(decimal totalSales, decimal totalCost)
    => totalSales - totalCost;

  public static decimal EstimatedNetProfit(decimal grossProfit, decimal operatingExpenses)
    => grossProfit - operatingExpenses;

  public static decimal GrossMarginPercent(decimal totalSales, decimal grossProfit)
    => totalSales == 0
      ? 0m
      : Math.Round((grossProfit / totalSales) * 100m, 2);

  public static decimal NetMarginPercent(decimal totalSales, decimal netProfit)
    => totalSales == 0
      ? 0m
      : Math.Round((netProfit / totalSales) * 100m, 2);

  public static decimal ProductMarginPercent(decimal totalSales, decimal totalCost)
    => totalSales == 0
      ? 0m
      : Math.Round(((totalSales - totalCost) / totalSales) * 100m, 2);

  public static bool IsNegativeMargin(decimal totalSales, decimal totalCost)
    => totalSales > 0 && totalCost > totalSales;

  public static bool IsLowMargin(decimal marginPercent, decimal threshold = 10m)
    => marginPercent >= 0m && marginPercent < threshold;
}
