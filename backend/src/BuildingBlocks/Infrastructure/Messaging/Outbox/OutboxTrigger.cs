using System.Threading.Channels;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Single-slot channel that wakes the outbox publisher the moment a message lands in the DB.
/// Bounded(1) with DropWrite means concurrent signals collapse into one — no queue buildup.
/// </summary>
public sealed class OutboxTrigger
{
  private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
    new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

  public ChannelReader<bool> Reader => _channel.Reader;

  public void Signal() => _channel.Writer.TryWrite(true);
}
