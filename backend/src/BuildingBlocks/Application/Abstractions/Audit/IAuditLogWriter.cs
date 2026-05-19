using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;

/// <summary>
/// Writes audit entries for critical business actions.
/// Implementations must never store sensitive data such as passwords or tokens.
/// </summary>
public interface IAuditLogWriter
{
  Task WriteAsync( // NOSONAR S107 — audit interface mirrors the immutable audit record fields
    BusinessId businessId,
    Guid? userId,
    string action,
    string entityName,
    Guid? entityId,
    string? description = null,
    string? ipAddress = null,
    CancellationToken cancellationToken = default);
}
