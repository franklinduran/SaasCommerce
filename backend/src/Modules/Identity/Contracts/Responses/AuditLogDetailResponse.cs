namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record AuditLogDetailResponse(
  Guid AuditLogId,
  Guid? UserId,
  string? UserFullName,
  string Action,
  string EntityName,
  Guid? EntityId,
  string? Description,
  string? IpAddress,
  string? UserAgent,
  Guid? CorrelationId,
  string? MetadataJson,
  DateTimeOffset CreatedAt);
