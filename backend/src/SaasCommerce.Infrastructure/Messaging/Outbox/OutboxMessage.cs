namespace SaasCommerce.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessage
{
  private OutboxMessage()
  {
  }

  public OutboxMessage(
    Guid businessId,
    string type,
    string content,
    DateTimeOffset occurredOnUtc)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(type);
    ArgumentException.ThrowIfNullOrWhiteSpace(content);

    Id = Guid.NewGuid();
    BusinessId = businessId;
    Type = type;
    Content = content;
    OccurredOnUtc = occurredOnUtc;
  }

  public Guid Id { get; private set; }

  public Guid BusinessId { get; private set; }

  public string Type { get; private set; } = string.Empty;

  public string Content { get; private set; } = string.Empty;

  public DateTimeOffset OccurredOnUtc { get; private set; }

  public DateTimeOffset? ProcessedOnUtc { get; private set; }

  public void MarkProcessed(DateTimeOffset processedOnUtc)
    => ProcessedOnUtc = processedOnUtc;
}
