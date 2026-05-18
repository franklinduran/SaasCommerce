namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record AuditLogListResponse(
  IReadOnlyCollection<AuditLogItemResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);

public sealed record AuditLogItemResponse(
  Guid AuditLogId,
  Guid? UserId,
  string? UserFullName,
  string Action,
  string EntityName,
  Guid? EntityId,
  string? Description,
  DateTimeOffset CreatedAt);
