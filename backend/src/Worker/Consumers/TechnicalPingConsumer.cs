using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Messaging;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using MassTransit;

namespace SaasCommerce.Worker.Consumers;

public sealed class TechnicalPingConsumer(
  ILogger<TechnicalPingConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : IdempotentConsumer<TechnicalPing>(inboxStore, clock)
{
  private static readonly Action<ILogger, Guid, Exception?> LogTechnicalPingConsumed =
    LoggerMessage.Define<Guid>(
      LogLevel.Information,
      new EventId(2000, nameof(LogTechnicalPingConsumed)),
      "Technical ping consumed. EventId: {EventId}");

  protected override Task ConsumeMessageAsync(ConsumeContext<TechnicalPing> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    LogTechnicalPingConsumed(logger, context.Message.EventId, null);

    return Task.CompletedTask;
  }
}
