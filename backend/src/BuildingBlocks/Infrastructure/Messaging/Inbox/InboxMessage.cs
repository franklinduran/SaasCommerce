namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed class InboxMessage
{
  private InboxMessage()
  {
  }

  public InboxMessage(
    Guid eventId,
    Guid businessId,
    string type,
    DateTimeOffset processedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(type);

    EventId = eventId;
    BusinessId = businessId;
    Type = type;
    ProcessedAt = processedAt;
  }

  public Guid EventId { get; private set; }

  public Guid BusinessId { get; private set; }

  public string Type { get; private set; } = string.Empty;

  public DateTimeOffset ProcessedAt { get; private set; }
}
