namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed class InboxMessage
{
  private InboxMessage()
  {
  }

  public InboxMessage(
    Guid eventId,
    string consumerName,
    Guid businessId,
    Guid correlationId,
    DateTimeOffset processedAt,
    DateTimeOffset createdAt)
  {
    Id = Guid.NewGuid();
    EventId = eventId;
    ConsumerName = consumerName;
    BusinessId = businessId;
    CorrelationId = correlationId;
    ProcessedAt = processedAt;
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public Guid EventId { get; private set; }

  public string ConsumerName { get; private set; } = string.Empty;

  public Guid BusinessId { get; private set; }

  public Guid CorrelationId { get; private set; }

  public DateTimeOffset ProcessedAt { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }
}
