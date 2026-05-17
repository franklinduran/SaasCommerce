namespace SaasCommerce.Modules.Sales.Contracts.Requests;

public sealed record CreateSaleRequest(
  Guid? BranchId,
  Guid? CustomerId,
  string PaymentMethod,
  IReadOnlyCollection<CreateSaleItemRequest> Items);

public sealed record CreateSaleItemRequest(
  Guid ProductId,
  decimal Quantity);
