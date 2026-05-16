using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events.V1;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

namespace SaasCommerce.Worker.Consumers;

public sealed class TechnicalPingConsumer(
  ILogger<TechnicalPingConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : IdempotentConsumer<TechnicalPingIntegrationEventV1>(inboxStore, clock)
{
  private static readonly Action<ILogger, Guid, Exception?> LogTechnicalPingConsumed =
    LoggerMessage.Define<Guid>(
      LogLevel.Information,
      new EventId(2000, nameof(LogTechnicalPingConsumed)),
      "Technical ping consumed. EventId: {EventId}");

  protected override Task ConsumeMessageAsync(ConsumeContext<TechnicalPingIntegrationEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    LogTechnicalPingConsumed(logger, context.Message.EventId, null);

    return Task.CompletedTask;
  }
}
