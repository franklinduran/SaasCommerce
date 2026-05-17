using System.Diagnostics;
using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

namespace SaasCommerce.Worker.Consumers;

public abstract class LoggingIntegrationEventConsumer<TMessage> : IdempotentConsumer<TMessage>
  where TMessage : class, IIntegrationEvent
{
  private static readonly Action<ILogger, string, string, Guid, Guid?, Guid, Guid, Exception?> LogConsumed =
    LoggerMessage.Define<string, string, Guid, Guid?, Guid, Guid>(
      LogLevel.Information,
      new EventId(2300, nameof(LogConsumed)),
      "Integration event consumed. ConsumerName={ConsumerName} EventName={EventName} EventId={EventId} MessageId={MessageId} CorrelationId={CorrelationId} BusinessId={BusinessId}");
  private static readonly Action<ILogger, string, string, Guid, Guid, Guid, long, Exception?> LogCompleted =
    LoggerMessage.Define<string, string, Guid, Guid, Guid, long>(
      LogLevel.Information,
      new EventId(2301, nameof(LogCompleted)),
      "Integration event consume completed. ConsumerName={ConsumerName} EventName={EventName} EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId} ElapsedMilliseconds={ElapsedMilliseconds}");
  private readonly ILogger logger;

  protected LoggingIntegrationEventConsumer(
    ILogger logger,
    IInboxStore inboxStore,
    IClock clock) : base(inboxStore, clock, logger)
  {
    this.logger = logger;
  }

  protected override Task ConsumeMessageAsync(ConsumeContext<TMessage> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var elapsed = Stopwatch.StartNew();
    var consumerName = GetType().Name;
    var eventName = typeof(TMessage).Name;

    LogConsumed(
      logger,
      consumerName,
      eventName,
      context.Message.EventId,
      context.MessageId,
      context.Message.CorrelationId,
      context.Message.BusinessId,
      null);

    elapsed.Stop();

    LogCompleted(
      logger,
      consumerName,
      eventName,
      context.Message.EventId,
      context.Message.CorrelationId,
      context.Message.BusinessId,
      elapsed.ElapsedMilliseconds,
      null);

    return Task.CompletedTask;
  }
}
