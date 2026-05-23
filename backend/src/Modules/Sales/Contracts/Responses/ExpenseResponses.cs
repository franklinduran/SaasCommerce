namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record ExpenseCategoryResponse(
  Guid Id,
  Guid BusinessId,
  string Name,
  bool IsActive,
  DateTimeOffset CreatedAt);

public sealed record OperatingExpenseResponse(
  Guid Id,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  Guid CategoryId,
  string CategoryName,
  string Description,
  decimal Amount,
  string PaymentMethod,
  string Status,
  DateTimeOffset ExpenseDate,
  string? Notes,
  Guid? CashSessionId,
  Guid? CashMovementId,
  DateTimeOffset? PaidAt,
  Guid? PaidByUserId,
  DateTimeOffset? CancelledAt,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);

public sealed record OperatingExpenseListResponse(
  Guid Id,
  Guid BranchId,
  Guid CategoryId,
  string CategoryName,
  string Description,
  decimal Amount,
  string PaymentMethod,
  string Status,
  DateTimeOffset ExpenseDate,
  DateTimeOffset CreatedAt);

public sealed record OperatingExpensesListResult(
  IReadOnlyCollection<OperatingExpenseListResponse> Items,
  int TotalCount,
  int Page,
  int PageSize);

public sealed record ExpenseSummaryResponse(
  DateTimeOffset DateFrom,
  DateTimeOffset DateTo,
  decimal TotalAmount,
  decimal PendingAmount,
  int PaidCount,
  int PendingCount,
  int CancelledCount,
  IReadOnlyCollection<ExpenseByCategoryResponse> ByCategory,
  IReadOnlyCollection<ExpenseByPaymentMethodResponse> ByPaymentMethod);

public sealed record ExpenseByCategoryResponse(
  Guid CategoryId,
  string CategoryName,
  decimal TotalAmount,
  int Count);

public sealed record ExpenseByPaymentMethodResponse(
  string PaymentMethod,
  decimal TotalAmount,
  int Count);
