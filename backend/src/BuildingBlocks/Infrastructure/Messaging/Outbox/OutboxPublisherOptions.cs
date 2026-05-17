namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisherOptions
{
  public int BatchSize { get; init; } = 25;

  public int PollingIntervalSeconds { get; init; } = 5;
}
