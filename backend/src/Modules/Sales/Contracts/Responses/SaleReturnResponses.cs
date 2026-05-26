namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record SaleReturnItemResponse(
  Guid Id,
  Guid SaleItemId,
  Guid ProductId,
  string ProductName,
  string? Sku,
  decimal Quantity,
  decimal UnitPrice,
  decimal LineTotal);

public sealed record CreditNoteItemResponse(
  Guid Id,
  Guid ProductId,
  string ProductName,
  string? Sku,
  decimal Quantity,
  decimal UnitPrice,
  decimal LineTotal);

public sealed record CreditNoteResponse(
  Guid Id,
  Guid SaleId,
  Guid SaleReturnId,
  Guid? CustomerId,
  string Code,
  decimal Total,
  IReadOnlyCollection<CreditNoteItemResponse> Items,
  DateTimeOffset CreatedAt);

public sealed record SaleReturnResponse(
  Guid Id,
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  string Status,
  string Reason,
  decimal Total,
  IReadOnlyCollection<SaleReturnItemResponse> Items,
  CreditNoteResponse? CreditNote,
  DateTimeOffset RequestedAt,
  DateTimeOffset? ApprovedAt,
  DateTimeOffset? FailedAt,
  string? FailureReason);
