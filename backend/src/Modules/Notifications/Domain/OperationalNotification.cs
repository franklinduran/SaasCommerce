using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Domain;

public sealed class OperationalNotificationDraft
{
  public required Guid Id { get; init; }
  public required BusinessId BusinessId { get; init; }
  public Guid? BranchId { get; init; }
  public required OperationalNotificationType Type { get; init; }
  public required OperationalNotificationSeverity Severity { get; init; }
  public required string Title { get; init; }
  public required string Message { get; init; }
  public Guid? RelatedEntityId { get; init; }
  public string? RelatedEntityType { get; init; }
  public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Represents a persisted operational alert surfaced to business staff.
/// Notifications are created by Worker consumers listening to integration events.
/// </summary>
public sealed class OperationalNotification
{
  private OperationalNotification()
  {
  }

  private OperationalNotification(OperationalNotificationDraft draft)
  {
    if (draft.Id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(draft));

    Id = draft.Id;
    BusinessId = draft.BusinessId;
    BranchId = draft.BranchId;
    Type = draft.Type;
    Severity = draft.Severity;
    Status = OperationalNotificationStatus.Unread;
    Title = draft.Title;
    Message = draft.Message;
    RelatedEntityId = draft.RelatedEntityId;
    RelatedEntityType = draft.RelatedEntityType;
    CreatedAt = draft.CreatedAt;
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

  public static OperationalNotification Create(OperationalNotificationDraft draft)
    => new(draft);

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
