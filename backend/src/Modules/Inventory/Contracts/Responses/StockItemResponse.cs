namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record StockItemResponse(
  Guid Id,
  Guid BusinessId,
  Guid BranchId,
  Guid ProductId,
  decimal Quantity);
