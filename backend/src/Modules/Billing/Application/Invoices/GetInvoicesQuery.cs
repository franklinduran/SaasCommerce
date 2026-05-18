namespace SaasCommerce.Modules.Billing.Application.Invoices;

public sealed record GetInvoicesQuery(
  string? Status,
  string? Query,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);
