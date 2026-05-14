using SaasCommerce.Contracts.Messaging;
using MassTransit;

namespace SaasCommerce.Worker.Consumers;

public sealed class TechnicalPingConsumer(ILogger<TechnicalPingConsumer> logger)
  : IConsumer<TechnicalPing>
{
  private static readonly Action<ILogger, Guid, Exception?> LogTechnicalPingConsumed =
    LoggerMessage.Define<Guid>(
      LogLevel.Information,
      new EventId(2000, nameof(LogTechnicalPingConsumed)),
      "Technical ping consumed. MessageId: {MessageId}");

  public Task Consume(ConsumeContext<TechnicalPing> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    LogTechnicalPingConsumed(logger, context.Message.MessageId, null);

    return Task.CompletedTask;
  }
}
