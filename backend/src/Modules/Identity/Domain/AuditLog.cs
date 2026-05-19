using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Domain;

/// <summary>
/// Immutable record of a critical action performed by a user within a business context.
/// No sensitive data (passwords, tokens) should ever be stored here.
/// </summary>
public sealed class AuditLog
{
  private AuditLog()
  {
  }

  public AuditLog(Guid id, AuditEntry entry, DateTimeOffset createdAt)
  {
    ArgumentNullException.ThrowIfNull(entry);
    ArgumentException.ThrowIfNullOrWhiteSpace(entry.Action);
    ArgumentException.ThrowIfNullOrWhiteSpace(entry.EntityName);

    Id = id;
    BusinessId = entry.BusinessId;
    UserId = entry.UserId;
    Action = entry.Action;
    EntityName = entry.EntityName;
    EntityId = entry.EntityId;
    Description = entry.Description;
    IpAddress = entry.IpAddress;
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid? UserId { get; private set; }

  /// <summary>Short action code, e.g. "sale.cancelled", "inventory.adjusted".</summary>
  public string Action { get; private set; } = string.Empty;

  /// <summary>Aggregate/entity type, e.g. "Sale", "StockItem", "Invoice".</summary>
  public string EntityName { get; private set; } = string.Empty;

  public Guid? EntityId { get; private set; }

  public string? Description { get; private set; }

  public string? IpAddress { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }
}
