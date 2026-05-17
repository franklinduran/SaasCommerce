namespace SaasCommerce.Api.Endpoints;

internal sealed class CustomerEndpointRequest
{
  public string? Query { get; init; }

  public bool? IsActive { get; init; }

  public int? Page { get; init; }

  public int? PageSize { get; init; }

  public string? SortBy { get; init; }

  public string? SortDirection { get; init; }
}
