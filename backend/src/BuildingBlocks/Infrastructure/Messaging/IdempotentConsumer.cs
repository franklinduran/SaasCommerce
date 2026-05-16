using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

public abstract class IdempotentConsumer<TMessage>(
  IInboxStore inboxStore,
  IClock clock) : IConsumer<TMessage>
  where TMessage : class, IIntegrationEvent
{
  public Task Consume(ConsumeContext<TMessage> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return ConsumeCoreAsync(context);
  }

  private async Task ConsumeCoreAsync(ConsumeContext<TMessage> context)
  {
    if (await inboxStore
        .HasProcessedAsync(context.Message.EventId, context.CancellationToken)
        .ConfigureAwait(false))
    {
      return;
    }

    await ConsumeMessageAsync(context).ConfigureAwait(false);

    await inboxStore
      .MarkProcessedAsync(
        context.Message.EventId,
        context.Message.BusinessId,
        typeof(TMessage).FullName ?? typeof(TMessage).Name,
        clock.UtcNow,
        context.CancellationToken)
      .ConfigureAwait(false);
  }

  protected abstract Task ConsumeMessageAsync(ConsumeContext<TMessage> context);
}
