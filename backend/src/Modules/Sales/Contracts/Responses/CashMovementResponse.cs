namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record CashMovementResponse(
  Guid Id,
  Guid CashSessionId,
  Guid UserId,
  string Type,
  decimal Amount,
  string Description,
  DateTimeOffset CreatedAt);
