namespace SaasCommerce.Api.Endpoints;

internal sealed class AuditLogEndpointRequest
{
  public DateTimeOffset? DateFrom { get; init; }

  public DateTimeOffset? DateTo { get; init; }

  public Guid? UserId { get; init; }

  public string? Action { get; init; }

  public string? EntityName { get; init; }

  public int? Page { get; init; }

  public int? PageSize { get; init; }
}
