namespace SaasCommerce.Api.Endpoints;

internal sealed class InvoiceEndpointRequest
{
  public string? Status { get; init; }

  public string? Query { get; init; }

  public DateTimeOffset? DateFrom { get; init; }

  public DateTimeOffset? DateTo { get; init; }

  public int? Page { get; init; }

  public int? PageSize { get; init; }
}
