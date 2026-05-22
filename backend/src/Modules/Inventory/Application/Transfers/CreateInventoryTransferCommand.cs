namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public sealed record CreateInventoryTransferCommand(
  Guid SourceBranchId,
  Guid TargetBranchId,
  IReadOnlyCollection<CreateInventoryTransferItemCommand> Items,
  string? Note);

public sealed record CreateInventoryTransferItemCommand(
  Guid ProductId,
  decimal Quantity);
