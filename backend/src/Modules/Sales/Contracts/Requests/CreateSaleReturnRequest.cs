namespace SaasCommerce.Modules.Sales.Contracts.Requests;

public sealed record CreateSaleReturnRequest(
  string Reason,
  IReadOnlyCollection<CreateSaleReturnItemRequest> Items);

public sealed record CreateSaleReturnItemRequest(Guid SaleItemId, decimal Quantity);
