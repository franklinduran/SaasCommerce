namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record CashSessionResponse(
  Guid Id,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  string Status,
  decimal OpeningBalance,
  decimal? ClosingBalance,
  decimal SystemBalance,
  string? Notes,
  DateTimeOffset OpenedAt,
  DateTimeOffset? ClosedAt,
  DateTimeOffset UpdatedAt,
  IReadOnlyCollection<CashMovementResponse> Movements);

public sealed record CashSessionListResponse(
  Guid Id,
  Guid BranchId,
  Guid UserId,
  string Status,
  decimal OpeningBalance,
  decimal? ClosingBalance,
  decimal SystemBalance,
  DateTimeOffset OpenedAt,
  DateTimeOffset? ClosedAt);

public sealed record CashClosingResultResponse(
  Guid CashSessionId,
  decimal OpeningBalance,
  decimal SystemBalance,
  decimal ClosingBalance,
  decimal Difference,
  string Outcome);
