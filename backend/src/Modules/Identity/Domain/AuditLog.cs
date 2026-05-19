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

  public AuditLog( // NOSONAR S107 — audit log requires all fields for immutable record
    Guid id,
    BusinessId businessId,
    Guid? userId,
    string action,
    string entityName,
    Guid? entityId,
    string? description,
    string? ipAddress,
    DateTimeOffset createdAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(action);
    ArgumentException.ThrowIfNullOrWhiteSpace(entityName);

    Id = id;
    BusinessId = businessId;
    UserId = userId;
    Action = action;
    EntityName = entityName;
    EntityId = entityId;
    Description = description;
    IpAddress = ipAddress;
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
