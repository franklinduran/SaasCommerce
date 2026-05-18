namespace SaasCommerce.Modules.Billing.Contracts.Responses;

public sealed record InvoiceListResponse(
  IReadOnlyCollection<InvoiceResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
