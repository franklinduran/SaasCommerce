namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;

/// <summary>
/// Writes audit entries for critical business actions.
/// Implementations must never store sensitive data such as passwords or tokens.
/// </summary>
public interface IAuditLogWriter
{
  Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
