namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

/// <summary>
/// Pure, side-effect-free calculations for daily operational closing.
/// All public members are testable without infrastructure.
/// </summary>
public static class DailyClosingCalculator
{
  public static decimal CashDifference(decimal cashCounted, decimal cashExpected)
    => cashCounted - cashExpected;

  public static decimal ExpenseRatio(decimal totalSales, decimal totalExpenses)
    => totalSales == 0
      ? 0m
      : Math.Round((totalExpenses / totalSales) * 100m, 2);

  public static decimal CreditSalesRatio(decimal totalSales, decimal creditSales)
    => totalSales == 0
      ? 0m
      : Math.Round((creditSales / totalSales) * 100m, 2);

  public static bool IsHighExpenseRatio(decimal expenseRatio, decimal threshold = 30m)
    => expenseRatio > threshold;

  public static bool IsCreditSalesHigh(decimal creditRatio, decimal threshold = 40m)
    => creditRatio > threshold;

  /// <summary>
  /// Computes the expected cash in the register:
  /// opening balance from cash sessions + all cash sales for the day.
  /// </summary>
  public static decimal ComputeCashExpected(decimal openingBalance, decimal cashSales)
    => openingBalance + cashSales;
}
