namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryTransferResponse(
  Guid Id,
  Guid BusinessId,
  Guid SourceBranchId,
  Guid TargetBranchId,
  string Status,
  string? Note,
  string? FailureReason,
  IReadOnlyCollection<InventoryTransferItemResponse> Items,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);

public sealed record InventoryTransferItemResponse(
  Guid ProductId,
  decimal Quantity);

public sealed record InventoryTransferListResponse(
  IReadOnlyCollection<InventoryTransferResponse> Items,
  int Total,
  int Page,
  int PageSize);
