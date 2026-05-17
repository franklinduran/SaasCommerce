using MassTransit;
using Microsoft.Extensions.Logging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

public abstract class IdempotentConsumer<TMessage>(
  IInboxStore inboxStore,
  IClock clock,
  ILogger? logger = null) : IConsumer<TMessage>
  where TMessage : class, IIntegrationEvent
{
  private static readonly Action<ILogger, string, Guid, Guid, Guid, Exception?> LogDuplicateIntegrationEvent =
    LoggerMessage.Define<string, Guid, Guid, Guid>(
      LogLevel.Information,
      new EventId(2100, nameof(LogDuplicateIntegrationEvent)),
      "Duplicate integration event skipped. ConsumerName={ConsumerName} EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId}");

  public Task Consume(ConsumeContext<TMessage> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return ConsumeCoreAsync(context);
  }

  private async Task ConsumeCoreAsync(ConsumeContext<TMessage> context)
  {
    var consumerName = GetType().Name;

    if (await inboxStore
        .HasProcessedAsync(context.Message.EventId, consumerName, context.CancellationToken)
        .ConfigureAwait(false))
    {
      if (logger is not null)
      {
        LogDuplicateIntegrationEvent(
          logger,
          consumerName,
          context.Message.EventId,
          context.Message.CorrelationId,
          context.Message.BusinessId,
          null);
      }

      return;
    }

    await ConsumeMessageAsync(context).ConfigureAwait(false);

    await inboxStore
      .MarkProcessedAsync(
        context.Message.EventId,
        consumerName,
        context.Message.BusinessId,
        context.Message.CorrelationId,
        clock.UtcNow,
        context.CancellationToken)
      .ConfigureAwait(false);
  }

  protected abstract Task ConsumeMessageAsync(ConsumeContext<TMessage> context);
}
