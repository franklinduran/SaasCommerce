namespace SaasCommerce.Modules.Sales.Contracts.Requests;

public sealed record AddCartItemRequest(Guid ProductId, int Quantity);

public sealed record SetCartItemQuantityRequest(int Quantity);
