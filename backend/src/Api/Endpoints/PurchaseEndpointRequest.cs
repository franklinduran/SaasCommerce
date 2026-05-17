namespace SaasCommerce.Api.Endpoints;

public sealed class PurchaseEndpointRequest
{
  public Guid? SupplierId { get; init; }

  public Guid? BranchId { get; init; }

  public string? Status { get; init; }

  public string? Query { get; init; }

  public DateTimeOffset? DateFrom { get; init; }

  public DateTimeOffset? DateTo { get; init; }

  public int? Page { get; init; }

  public int? PageSize { get; init; }

  public string? SortBy { get; init; }

  public string? SortDirection { get; init; }
}
