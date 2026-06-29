namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisherOptions
{
  public int BatchSize { get; init; } = 25;

  /// <summary>Milliseconds between outbox polling cycles. Min 50ms.</summary>
  public int PollingIntervalMs { get; init; } = 500;

  // Keep for backwards-compat: if someone set PollingIntervalSeconds, honour it unless
  // PollingIntervalMs is explicitly overridden from its default.
  public int? PollingIntervalSeconds { get; init; }
}
