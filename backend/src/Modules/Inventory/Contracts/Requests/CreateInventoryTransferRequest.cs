namespace SaasCommerce.Modules.Inventory.Contracts.Requests;

public sealed record CreateInventoryTransferRequest(
  Guid SourceBranchId,
  Guid TargetBranchId,
  IReadOnlyCollection<CreateInventoryTransferItemRequest> Items,
  string? Note);

public sealed record CreateInventoryTransferItemRequest(
  Guid ProductId,
  decimal Quantity);
