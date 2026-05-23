namespace SaasCommerce.Modules.Sales.Contracts.Requests;

public sealed record CreateExpenseCategoryRequest(string Name);

public sealed record CreateOperatingExpenseRequest(
  Guid BranchId,
  Guid CategoryId,
  string Description,
  decimal Amount,
  string PaymentMethod,
  string Status,
  DateTimeOffset ExpenseDate,
  string? Notes);

public sealed record PayOperatingExpenseRequest(string PaymentMethod);

public sealed record CancelOperatingExpenseRequest(string? Reason);
