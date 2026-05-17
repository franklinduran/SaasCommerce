namespace SaasCommerce.Modules.Customers.Contracts.Responses;

public sealed record CustomerCreditMovementListResponse(
  IReadOnlyCollection<CustomerCreditMovementResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
