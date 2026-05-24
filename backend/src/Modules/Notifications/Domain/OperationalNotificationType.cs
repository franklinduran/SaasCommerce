namespace SaasCommerce.Modules.Notifications.Domain;

public enum OperationalNotificationType
{
  LowStock,
  CashDifference,
  SaleFailed,
  InvoiceFailed,
  HighExpense,
  NegativeProfit,
  ProductMissingCost,
  DailyClosingPending,
  CustomerDebtOverdue,
}
