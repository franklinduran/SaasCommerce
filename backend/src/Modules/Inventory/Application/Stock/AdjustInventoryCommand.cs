namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed record AdjustInventoryCommand(
  Guid ProductId,
  decimal Quantity,
  string Reason);
