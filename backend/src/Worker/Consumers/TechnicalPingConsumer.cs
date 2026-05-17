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
  : IdempotentConsumer<TechnicalPingIntegrationEventV1>(inboxStore, clock, logger)
{
  private static readonly Action<ILogger, Guid, Guid, Guid, string, Exception?> LogTechnicalPingConsumed =
    LoggerMessage.Define<Guid, Guid, Guid, string>(
      LogLevel.Information,
      new EventId(2000, nameof(LogTechnicalPingConsumed)),
      "Technical ping consumed. EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId} Message={Message}");

  protected override Task ConsumeMessageAsync(ConsumeContext<TechnicalPingIntegrationEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    LogTechnicalPingConsumed(
      logger,
      context.Message.EventId,
      context.Message.CorrelationId,
      context.Message.BusinessId,
      context.Message.Message,
      null);

    return Task.CompletedTask;
  }
}
