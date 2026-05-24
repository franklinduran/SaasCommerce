using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Domain;

/// <summary>
/// Represents a persisted operational alert surfaced to business staff.
/// Notifications are created by Worker consumers listening to integration events.
/// </summary>
public sealed class OperationalNotification
{
  private OperationalNotification()
  {
  }

  private OperationalNotification(
    Guid id,
    BusinessId businessId,
    Guid? branchId,
    OperationalNotificationType type,
    OperationalNotificationSeverity severity,
    string title,
    string message,
    Guid? relatedEntityId,
    string? relatedEntityType,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    Type = type;
    Severity = severity;
    Status = OperationalNotificationStatus.Unread;
    Title = title;
    Message = message;
    RelatedEntityId = relatedEntityId;
    RelatedEntityType = relatedEntityType;
    CreatedAt = createdAt;
    ReadAt = null;
    ReadByUserId = null;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  /// <summary>
  /// Optional branch scope. Null means business-wide.
  /// </summary>
  public Guid? BranchId { get; private set; }

  public OperationalNotificationType Type { get; private set; }

  public OperationalNotificationSeverity Severity { get; private set; }

  public OperationalNotificationStatus Status { get; private set; }

  public string Title { get; private set; } = string.Empty;

  public string Message { get; private set; } = string.Empty;

  /// <summary>Optional FK to the entity that triggered this notification.</summary>
  public Guid? RelatedEntityId { get; private set; }

  /// <summary>Human-readable entity type label, e.g. "Product", "Sale".</summary>
  public string? RelatedEntityType { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? ReadAt { get; private set; }

  public Guid? ReadByUserId { get; private set; }

  // ── Factory ──────────────────────────────────────────────────────────────

  public static OperationalNotification Create(
    Guid id,
    BusinessId businessId,
    Guid? branchId,
    OperationalNotificationType type,
    OperationalNotificationSeverity severity,
    string title,
    string message,
    Guid? relatedEntityId,
    string? relatedEntityType,
    DateTimeOffset createdAt)
    => new(id, businessId, branchId, type, severity, title, message,
           relatedEntityId, relatedEntityType, createdAt);

  // ── Behaviour ────────────────────────────────────────────────────────────

  public void MarkRead(Guid userId, DateTimeOffset readAt)
  {
    if (userId == Guid.Empty)
      throw new ArgumentException("User id is required.", nameof(userId));

    if (Status == OperationalNotificationStatus.Read)
      return; // idempotent

    Status = OperationalNotificationStatus.Read;
    ReadAt = readAt;
    ReadByUserId = userId;
  }
}
