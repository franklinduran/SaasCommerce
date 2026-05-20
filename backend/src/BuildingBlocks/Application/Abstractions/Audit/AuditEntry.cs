using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;

/// <summary>
/// Captures all data for a single audit trail entry.
/// No sensitive data (passwords, tokens) should ever be included.
/// </summary>
public sealed record AuditEntry(
    BusinessId BusinessId,
    Guid? UserId,
    string Action,
    string EntityName,
    Guid? EntityId,
    string? Description = null,
    string? IpAddress = null,
    Guid? CorrelationId = null,
    string? UserAgent = null,
    string? MetadataJson = null);
