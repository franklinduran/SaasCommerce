namespace SaasCommerce.Modules.Reporting.Contracts.Responses;

public sealed record AccountsReceivableReportResponse(
  IReadOnlyCollection<AccountsReceivableItemDto> Items,
  AccountsReceivableSummaryDto Summary,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);

public sealed record AccountsReceivableItemDto(
  Guid CustomerId,
  string CustomerName,
  string? Phone,
  decimal CreditLimit,
  decimal CurrentBalance,
  string Status,
  DateTimeOffset CreatedAt);

public sealed record AccountsReceivableSummaryDto(
  int TotalCustomers,
  decimal TotalPending,
  decimal MaxSingleDebt);
