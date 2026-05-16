namespace SaasCommerce.Api.Endpoints;

internal sealed class GetProductsEndpointRequest
{
  public string? Query { get; init; }

  public string? ProductType { get; init; }

  public Guid? CategoryId { get; init; }

  public bool? IsActive { get; init; }

  public int? Page { get; init; }

  public int? PageSize { get; init; }
}
