using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Audit;

public sealed record AuditLogCriteria(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  Guid? UserId,
  string? Action,
  string? EntityName,
  int Page,
  int PageSize);

public interface IAuditLogReadRepository
{
  Task<AuditLogListResponse> GetAuditLogsAsync(
    BusinessId businessId,
    AuditLogCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<AuditLogDetailResponse?> GetByIdAsync(
    Guid id,
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}
