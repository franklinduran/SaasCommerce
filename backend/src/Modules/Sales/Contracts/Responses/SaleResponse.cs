namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record SaleResponse(
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  Guid? CustomerId,
  string? CustomerName,
  string? BranchName,
  string Status,
  string PaymentMethod,
  decimal Total,
  IReadOnlyCollection<SaleItemResponse> Items,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt,
  DateTimeOffset? CompletedAt,
  DateTimeOffset? FailedAt,
  DateTimeOffset? CancelledAt,
  string? FailureReason,
  string? CancellationReason)
{
  public Guid Id => SaleId;

  public string Code => SaleId.ToString("N")[..8].ToUpperInvariant();
}
