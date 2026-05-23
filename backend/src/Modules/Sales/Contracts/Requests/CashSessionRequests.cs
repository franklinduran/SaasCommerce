namespace SaasCommerce.Modules.Sales.Contracts.Requests;

public sealed record OpenCashSessionRequest(
  decimal OpeningBalance,
  string? Notes);

public sealed record CloseCashSessionRequest(
  decimal ClosingBalance);

public sealed record RegisterCashMovementRequest(
  string Type,
  decimal Amount,
  string Description);
