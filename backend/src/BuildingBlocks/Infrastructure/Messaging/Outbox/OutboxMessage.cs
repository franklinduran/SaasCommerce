namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessage
{
  private OutboxMessage()
  {
  }

  public OutboxMessage(
    Guid eventId,
    Guid correlationId,
    Guid businessId,
    string eventType,
    string payload,
    DateTimeOffset occurredAt,
    DateTimeOffset createdAt)
  {
    Id = Guid.NewGuid();
    EventId = eventId;
    CorrelationId = correlationId;
    BusinessId = businessId;
    EventType = eventType;
    Payload = payload;
    OccurredAt = occurredAt;
    CreatedAt = createdAt;
    Status = OutboxMessageStatus.Pending;
  }

  public Guid Id { get; private set; }

  public Guid EventId { get; private set; }

  public Guid CorrelationId { get; private set; }

  public Guid BusinessId { get; private set; }

  public string EventType { get; private set; } = string.Empty;

  public string Payload { get; private set; } = string.Empty;

  public DateTimeOffset OccurredAt { get; private set; }

  public DateTimeOffset? PublishedAt { get; private set; }

  public int Attempts { get; private set; }

  public string? LastError { get; private set; }

  public OutboxMessageStatus Status { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public void StartAttempt()
  {
    Attempts++;
    Status = OutboxMessageStatus.Processing;
    LastError = null;
  }

  public void MarkPublished(DateTimeOffset publishedAt)
  {
    PublishedAt = publishedAt;
    Status = OutboxMessageStatus.Published;
    LastError = null;
  }

  public void MarkFailed(string error)
  {
    LastError = string.IsNullOrWhiteSpace(error)
      ? "Event publication failed."
      : error;
    Status = OutboxMessageStatus.Failed;
  }
}
